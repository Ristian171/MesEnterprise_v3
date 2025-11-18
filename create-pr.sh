#!/bin/bash
# Script to create a pull request merging combined-features into main
# This script requires GitHub CLI (gh) to be installed and authenticated

set -e

PR_TITLE="Merge combined-features into main"
PR_BODY="This pull request merges the \`combined-features\` branch into \`main\`.

## Summary

The \`combined-features\` branch consolidates work from the following branches that were developed in parallel:

- \`copilot/add-live-scan-feature\`
- \`copilot/expand-mesentprise-v3-implementation\`
- \`copilot/prepare-app-for-production\`

## Merge Status

This PR has been verified to merge cleanly with no conflicts.

## Changes

All changes from the above branches have been consolidated into \`combined-features\` and are ready to be merged into \`main\`."

# Create the pull request
gh pr create \
  --base main \
  --head combined-features \
  --title "$PR_TITLE" \
  --body "$PR_BODY" \
  --repo Ristian171/MesEnterprise_v3

echo "Pull request created successfully!"
