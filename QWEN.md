# Qwen Memory - Holy Saint Order

**User:** Ocxyde | **Codename:** BetsyBoop
**Project:** `D:\travaux_Unity\CodeDotLavos` | **Working Dir:** `D:\travaux_Unity\PeuImporte`
**Unity:** 6000.3.10f1 | **IDE:** Rider | **License:** GPL v3

---

## Core Rules

1. **Read `Assets/Docs/*.md` first** - Apply all documentation
2. **Plug-in-Out Architecture** - Revise old files, never patch
3. **Run `backup.ps1`** - Remind user after ANY file/folder change
4. **Shell Execution** - Allowed when user explicitly requests (e.g., git status, backup.ps1)
5. **Backup Files** - Read-only, never modify
6. **No Codenames** - Remove all codenames from documentation; only "Author: Ocxyde" is visible (human & AI readable)

---

## Coding Standards

- C# Unity 6 conventions
- Naming: `camelCase` (private), `PascalCase` (public)
- Encoding: UTF-8 with Unix LF (no CRLF)
- **NEVER** use emojis in C# files
- New Input System for input handling

---

## File & Folder Rules

- Documentation: `Assets/Docs/`
- Tests: `Assets/Scripts/Tests/`
- Use relative paths only
- Show diffs for all changes
- Store temp diffs in `diff_tmp/` (delete >2 days old)
- Core main `.cs` file - everything pivots around it

---

## Git Workflow

- Git: `C:\PROGRA~1\Git\cmd\git.exe`
- After changes: `git add -A` → commit (no codename) → `backup.ps1`
- Remind user to commit and push regularly

---

**Motto:** "Happy coding with me : Ocxyde :)"
