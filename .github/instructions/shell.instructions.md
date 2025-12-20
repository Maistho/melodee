---
description: 'Shell script conventions and best practices for Bash scripts'
applyTo: '**/*.sh'
---

# Shell Script Development

## General Guidelines

- Use `#!/usr/bin/env bash` shebang for portability
- Enable strict mode at the top of scripts: `set -euo pipefail`
- Use lowercase for local variables, UPPERCASE for environment/exported variables
- Quote all variable expansions: `"${variable}"` not `$variable`
- Use `[[ ]]` for conditionals instead of `[ ]`

## Error Handling

- Always use `set -e` to exit on error
- Use `set -u` to treat unset variables as errors
- Use `set -o pipefail` to catch errors in pipelines
- Provide meaningful error messages with context
- Use trap for cleanup: `trap cleanup EXIT`

```bash
#!/usr/bin/env bash
set -euo pipefail

cleanup() {
    rm -f "${temp_file:-}"
}
trap cleanup EXIT
```

## Functions

- Declare functions before use
- Use `local` for function-scoped variables
- Return exit codes, not strings for status
- Document complex functions with comments

```bash
process_file() {
    local file="$1"
    local output_dir="$2"
    
    if [[ ! -f "$file" ]]; then
        echo "Error: File not found: $file" >&2
        return 1
    fi
    
    # Processing logic here
}
```

## Input Validation

- Validate all user inputs and arguments
- Check if required commands exist before use
- Provide usage information for scripts with arguments

```bash
command -v docker >/dev/null 2>&1 || {
    echo "Error: docker is required but not installed" >&2
    exit 1
}
```

## Security

- Never use `eval` with user input
- Use `--` to separate options from arguments in commands
- Avoid storing secrets in scripts; use environment variables
- Be cautious with `rm -rf`; validate paths first

## Style

- Use 4-space indentation (consistent with project .editorconfig)
- Keep lines under 100 characters
- Use meaningful variable and function names
- Group related commands logically

## ShellCheck Compliance

- All scripts should pass ShellCheck without warnings
- Address or explicitly disable warnings with comments when necessary
- Run: `shellcheck script.sh` before committing
