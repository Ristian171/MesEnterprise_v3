# Pull Request Creation Guide

## Overview

This document provides instructions for creating a pull request to merge the `combined-features` branch into `main`.

## Background

The `combined-features` branch was created from `main` and then used to merge the following branches in sequence:

1. `copilot/add-live-scan-feature`
2. `copilot/expand-mesentprise-v3-implementation`
3. `copilot/prepare-app-for-production`

The goal is to merge all these consolidated changes into `main` via a single pull request.

## Merge Status

✅ **No conflicts detected** - The `combined-features` branch can be merged cleanly into `main`.

## Method 1: Using the Automated Script

A helper script `create-pr.sh` has been provided to automate the PR creation process.

### Prerequisites

- GitHub CLI (`gh`) must be installed
- You must be authenticated with GitHub (`gh auth login`)

### Steps

1. Ensure you have the necessary permissions for the repository
2. Authenticate with GitHub CLI:
   ```bash
   gh auth login
   ```

3. Run the script:
   ```bash
   ./create-pr.sh
   ```

The script will create a PR with:
- **Base branch:** `main`
- **Compare branch:** `combined-features`
- **Title:** "Merge combined-features into main"
- **Description:** A summary of all consolidated branches

## Method 2: Using GitHub CLI Manually

If you prefer to create the PR manually using GitHub CLI:

```bash
gh pr create \
  --base main \
  --head combined-features \
  --title "Merge combined-features into main" \
  --body "This PR consolidates work from copilot/add-live-scan-feature, copilot/expand-mesentprise-v3-implementation, and copilot/prepare-app-for-production" \
  --repo Ristian171/MesEnterprise_v3
```

## Method 3: Using GitHub Web Interface

1. Go to: https://github.com/Ristian171/MesEnterprise_v3
2. Click on "Pull requests" tab
3. Click "New pull request"
4. Set:
   - **Base:** `main`
   - **Compare:** `combined-features`
5. Click "Create pull request"
6. Use the title: **Merge combined-features into main**
7. Add the following description:

```
This pull request merges the `combined-features` branch into `main`.

## Summary

The `combined-features` branch consolidates work from the following branches that were developed in parallel:

- `copilot/add-live-scan-feature`
- `copilot/expand-mesentprise-v3-implementation`
- `copilot/prepare-app-for-production`

## Merge Status

This PR has been verified to merge cleanly with no conflicts.
```

8. Click "Create pull request"

## Method 4: Using GitHub API

You can also create the PR using the GitHub REST API:

```bash
curl -X POST \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  https://api.github.com/repos/Ristian171/MesEnterprise_v3/pulls \
  -d '{
    "title": "Merge combined-features into main",
    "body": "This PR consolidates work from copilot/add-live-scan-feature, copilot/expand-mesentprise-v3-implementation, and copilot/prepare-app-for-production",
    "head": "combined-features",
    "base": "main"
  }'
```

## Verification

After creating the PR, you should verify:

1. ✅ The PR base is `main`
2. ✅ The PR head is `combined-features`
3. ✅ There are no merge conflicts
4. ✅ The PR description mentions all consolidated branches
5. ✅ The PR can be merged successfully

## Next Steps

Once the PR is created:

1. Review the changes in the GitHub UI
2. Request reviews from team members if needed
3. Run any CI/CD checks
4. Merge the PR once approved

## Support

If you encounter any issues during PR creation, please check:

- GitHub CLI is properly authenticated
- You have write access to the repository
- The branches `main` and `combined-features` exist
- There are no network issues preventing GitHub access
