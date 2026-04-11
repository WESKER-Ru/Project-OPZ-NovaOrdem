# GitHub Automation Setup (OPZ)

This guide sets a low-friction Git flow for mobile/desktop use with PR checks.

## 1) Push branch and open PR
1. Push your working branch (`work` or `feature/*`).
2. Open Pull Request into `main` (or `master`).
3. Fill `pull_request_template.md` sections.

## 2) Configure Unity CI secrets
In GitHub repo: **Settings → Secrets and variables → Actions**.

Create:
- `UNITY_LICENSE` (required to run Unity tests in CI)
- `UNITY_EMAIL` and `UNITY_PASSWORD` (optional depending on license strategy)

Without `UNITY_LICENSE`, CI workflow will skip Unity tests and print guidance.

## 3) Enable branch protection
In GitHub repo: **Settings → Branches → Add branch protection rule**.
Recommended:
- Require a pull request before merging
- Require status checks to pass
- Require branches to be up to date before merging
- (Optional) Require review approvals
- (Optional) Automatically delete head branches

## 4) Enable auto-merge
Inside a PR:
- Click **Enable auto-merge**
- Choose **Squash and merge** (recommended for cleaner history)

Now the merge happens automatically once required checks pass.

## 5) Suggested daily flow (Desktop + Mobile)
1. Work on `feature/*` branch.
2. Commit + Push from GitHub Desktop.
3. Open PR.
4. Check CI on mobile.
5. Merge/Auto-merge on mobile.
6. Pull `main` locally.

## Troubleshooting
### "Checks not running"
- Verify workflow file exists at `.github/workflows/unity-ci.yml`
- Verify Actions are enabled in repository settings

### "Unity CI skipped"
- `UNITY_LICENSE` missing or empty

### "Can’t merge on mobile"
- Use mobile browser in desktop mode if app doesn’t show full merge controls.
