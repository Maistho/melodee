---
description: 'Entity Framework Core migration guidelines - never edit migrations, always use model-driven migrations'
applyTo: '**/Migrations/*.cs'
---

# Entity Framework Core Migration Guidelines

## CRITICAL RULES - NEVER VIOLATE

### 1. NEVER EDIT EXISTING MIGRATIONS

**ABSOLUTE RULE**: Once a migration has been created and potentially applied to ANY database (dev, staging, prod), it is **IMMUTABLE**.

❌ **NEVER DO THIS:**
```csharp
// Editing an existing migration file
public partial class AddEmailSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Adding more changes to existing migration
        migrationBuilder.InsertData(...); // WRONG!
    }
}
```

✅ **ALWAYS DO THIS:**
```bash
# Create a NEW migration
dotnet ef migrations add AddAdditionalEmailSettings --project src/Melodee.Common/Melodee.Common.csproj --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj --context MelodeeDbContext
```

**Why?**: 
- Migrations may already be applied to production databases
- Editing causes hash mismatches and migration failures
- Users may have already applied the migration
- Breaks database version control and rollback capability

### 2. NEVER HAND-ROLL MIGRATIONS

**ABSOLUTE RULE**: Migrations must be generated from EF Core model changes, NOT manually written.

❌ **NEVER DO THIS:**
```csharp
// Manually writing migration code
migrationBuilder.InsertData(
    table: "Settings",
    columns: new[] { "Id", "Key", "Value", ... },
    values: new object[] { 1509, "email.template", "value", ... }
);
```

✅ **ALWAYS DO THIS:**

**Step 1: Update the model or seed data**
```csharp
// In MelodeeDbContext.cs or appropriate DbContext configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Setting>().HasData(
        new Setting 
        { 
            Id = 1509, 
            Key = SettingRegistry.EmailResetPasswordSubject,
            Value = "Reset your password",
            // ... other properties
        }
    );
}
```

**Step 2: Generate migration from model**
```bash
dotnet ef migrations add AddEmailTemplateSettings --project src/Melodee.Common/Melodee.Common.csproj --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj --context MelodeeDbContext
```

**Why?**:
- EF Core generates correct, optimized SQL
- Migrations stay in sync with model
- Type safety and compile-time checking
- Automatic Down() method generation
- Designer file stays in sync

### 3. PROPER MIGRATION WORKFLOW

**Correct Process:**

1. **Modify the EF Core Model**
   - Update entity classes
   - Modify DbContext.OnModelCreating()
   - Update seed data in HasData() calls
   - Add/remove properties
   - Change relationships

2. **Generate Migration**
   ```bash
   dotnet ef migrations add DescriptiveName --project src/Melodee.Common/Melodee.Common.csproj --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj --context MelodeeDbContext
   ```

3. **Review Generated Migration**
   - Check Up() method looks correct
   - Verify Down() method will properly rollback
   - Ensure no data loss operations without safeguards

4. **Test Migration**
   ```bash
   # Apply to development database
   dotnet ef database update --project src/Melodee.Common/Melodee.Common.csproj --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj --context MelodeeDbContext
   
   # Test rollback
   dotnet ef database update PreviousMigrationName --project src/Melodee.Common/Melodee.Common.csproj --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj --context MelodeeDbContext
   ```

5. **Commit to Source Control**
   - Commit all three files (.cs, .Designer.cs, ModelSnapshot.cs)

### 4. ADDING SEED DATA

**WRONG WAY** - Hand-editing migration:
```csharp
❌ protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(...); // NEVER write this manually
}
```

**RIGHT WAY** - Update model configuration:
```csharp
✅ // In DbContext or IEntityTypeConfiguration
modelBuilder.Entity<Setting>().HasData(
    new Setting { Id = 1509, Key = "new.setting", Value = "value" }
);

// Then generate migration
// dotnet ef migrations add AddNewSetting ...
```

### 5. FIXING MISTAKES

**If you realize you made a mistake BEFORE committing/deploying:**

1. **Remove the bad migration:**
   ```bash
   dotnet ef migrations remove --project src/Melodee.Common/Melodee.Common.csproj --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj --context MelodeeDbContext
   ```

2. **Fix the model**

3. **Generate new migration**

**If migration is already committed/deployed:**

1. **Create a NEW corrective migration**
   ```bash
   dotnet ef migrations add FixPreviousMigrationIssue --project src/Melodee.Common/Melodee.Common.csproj --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj --context MelodeeDbContext
   ```

2. **NEVER go back and edit the old one**

### 6. COMMON SCENARIOS

#### Adding a New Setting

**Step 1: Define in SettingRegistry**
```csharp
public static class SettingRegistry
{
    public const string EmailTemplateSubject = "email.template.subject";
}
```

**Step 2: Add seed data in MelodeeDbContextSeedData or appropriate configuration**
```csharp
new Setting 
{ 
    Id = NextAvailableId, // Use proper ID sequencing
    Key = SettingRegistry.EmailTemplateSubject,
    Value = "Default Subject",
    Comment = "Email template subject line",
    CreatedAt = Instant.FromUnixTimeTicks(0),
    ApiKey = Guid.NewGuid()
}
```

**Step 3: Generate migration**
```bash
dotnet ef migrations add AddEmailTemplateSubjectSetting --project src/Melodee.Common/Melodee.Common.csproj --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj --context MelodeeDbContext
```

#### Adding a New Table

**Step 1: Create entity class**
```csharp
public class EmailTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Subject { get; set; }
    // ...
}
```

**Step 2: Add DbSet to context**
```csharp
public DbSet<EmailTemplate> EmailTemplates { get; set; }
```

**Step 3: Configure in OnModelCreating (optional)**
```csharp
modelBuilder.Entity<EmailTemplate>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
});
```

**Step 4: Generate migration**
```bash
dotnet ef migrations add AddEmailTemplatesTable --project src/Melodee.Common/Melodee.Common.csproj --startup-project src/Melodee.Blazor/Melodee.Blazor.csproj --context MelodeeDbContext
```

### 7. MIGRATION NAMING CONVENTIONS

Use descriptive, action-oriented names:

✅ **Good Names:**
- `AddUserEmailColumn`
- `CreateAuditLogTable`
- `UpdateSettingsDefaultValues`
- `RemoveObsoleteIndexes`
- `AddPasswordResetTokenFields`

❌ **Bad Names:**
- `Migration1`
- `Update`
- `FixStuff`
- `Changes`
- `NewMigration`

### 8. VERIFICATION CHECKLIST

Before committing a migration, verify:

- [ ] Migration was generated by EF Core, not hand-written
- [ ] All three files are included (.cs, .Designer.cs, ModelSnapshot.cs)
- [ ] Migration has been tested with `dotnet ef database update`
- [ ] Down() method successfully rolls back with `dotnet ef database update PreviousMigration`
- [ ] No existing migrations were modified
- [ ] Migration name is descriptive
- [ ] Seed data changes are in model configuration, not migration file
- [ ] No sensitive data (passwords, secrets) in migration

### 9. MULTI-DATABASE SUPPORT

When working with multiple database providers (SQLite, PostgreSQL, SQL Server):

1. **Test migration on all supported providers**
2. **Avoid provider-specific SQL in migrations** - let EF Core handle it
3. **Use EF Core abstractions** instead of raw SQL where possible

### 10. REFERENCES

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Migration Best Practices](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing)
- [Seed Data](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)

---

## Summary

**The Golden Rule**: 
> Migrations are generated artifacts from model changes, not manually written code. Once created and shared, they are immutable historical records of schema evolution.

**Three Cardinal Sins**:
1. ❌ Editing existing migrations
2. ❌ Hand-rolling migration code
3. ❌ Putting business logic in migrations

**Always Remember**:
- Models drive migrations, not the other way around
- Migrations are historical records, not living documents  
- When in doubt, create a new migration
