# Skill: Build Verify

Run the repository build gate script after code changes:

```bash
bash .claude/scripts/build-verify.sh
```

If build fails:

1. Fix compilation or typing errors.
2. Re-run the script.
3. Repeat until clean.

Do not finish work with failing builds.
