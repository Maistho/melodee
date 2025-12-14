namespace Melodee.Common.Models;

public sealed record TopItemStat(string Label, double Value, Guid? ApiKey = null, int? Id = null, string? Extra = null);
