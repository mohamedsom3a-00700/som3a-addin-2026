```text id="6m2wxq"
You are a senior DevOps, Git, and .NET engineer.

Your task is to fully publish the current Visual Studio solution to GitHub safely and professionally.

Project type:
- C#
- .NET
- WPF
- VSTO Addin
- Visual Studio solution

Requirements:

1. Analyze the entire solution structure before making changes.

2. Create or improve `.gitignore` with proper rules for:
   - Visual Studio
   - WPF
   - VSTO
   - NuGet
   - Build artifacts
   - Temp files
   - Logs
   - Certificates
   - Secrets

3. Ensure the following are excluded:
   - bin/
   - obj/
   - .vs/
   - packages/
   - TestResults/
   - publish/
   - *.user
   - *.suo
   - *.cache
   - *.log
   - *.pdb
   - *.tmp
   - *.bak
   - *.pfx
   - *.snk
   - app secrets
   - API keys
   - local databases if needed

4. Detect and remove already tracked sensitive files from git tracking safely.

5. Verify git repository status.

6. If git is not initialized:
   - initialize git
   - create initial branch

7. Create professional commits with clean messages.

8. Generate a professional README.md containing:
   - Project name
   - Overview
   - Features
   - Technologies
   - Installation
   - Usage
   - Architecture
   - Notes

9. If GitHub CLI (gh) is installed:
   - authenticate if needed
   - create a PRIVATE GitHub repository
   - use repository name:
     Som3a-Addin-2026
   - set remote origin
   - push all commits and branches

10. Verify:
   - remote configured correctly
   - push succeeded
   - current branch synced with origin

11. Show every command before executing it.

12. Never upload:
   - secrets
   - certificates
   - temporary keys
   - compiled binaries
   - sensitive configs

13. If any dangerous action is required:
   - explain it first
   - ask for confirmation only for destructive operations

Goal:
Publish the full project professionally to GitHub following enterprise-level best practices.
```
