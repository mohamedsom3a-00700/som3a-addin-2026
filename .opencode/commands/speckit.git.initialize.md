---
description: Initialize a Git repository with an initial commit
---


<!-- Extension: git -->
<!-- Config: .specify/extensions/git/ -->
# Initialize Git Repository

Initialize a Git repository in the current project directory if one does not already exist.

## Execution

Run the appropriate script from the project root:

- **Bash**: `.specify/extensions/git/scripts/bash/initialize-repo.sh`
- **PowerShell**: `.specify/extensions/git/scripts/powershell/initialize-repo.ps1`

If the extension scripts are not found, fall back to:
- **Bash**: `git init && git status && git add -p && git commit -m "Initial commit from Specify template"` (review staged changes before committing)
- **PowerShell**: `git init; git status; git add -p; git commit -m "Initial commit from Specify template"` (review staged changes before committing)

> [!IMPORTANT]
> Always review staged files with `git status` or `git diff --cached` before committing. Use `git add <paths>` with explicit paths instead of `git add .` to avoid staging unintended files. Ensure .gitignore is in place before the initial add.

The script handles all checks internally:
- Skips if Git is not available
- Skips if already inside a Git repository
- Runs `git init`, `git add .`, and `git commit` with an initial commit message

## Customization

Replace the script to add project-specific Git initialization steps:
- Custom `.gitignore` templates
- Default branch naming (`git config init.defaultBranch`)
- Git LFS setup
- Git hooks installation
- Commit signing configuration
- Git Flow initialization

## Output

On success:
- `✓ Git repository initialized`

## Graceful Degradation

If Git is not installed:
- Warn the user
- Skip repository initialization
- The project continues to function without Git (specs can still be created under `specs/`)

If Git is installed but `git init`, `git add .`, or `git commit` fails:
- Surface the error to the user
- Stop this command rather than continuing with a partially initialized repository