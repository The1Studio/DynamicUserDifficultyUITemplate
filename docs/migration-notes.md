# Migration Notes

## Branch Migration: main → master

**Date**: November 4, 2025
**Migration Type**: Default Branch Change
**Status**: Completed ✅

### Overview

This repository has been migrated from using `main` as the default branch to `master` as the default branch.

### Changes Made

1. **Branch Creation**: Created `master` branch from existing `main` branch
2. **Remote Push**: Pushed `master` branch to GitHub remote
3. **GitHub Settings**: Updated default branch setting on GitHub from `main` to `master`
4. **Local Checkout**: Switched local working directory to `master` branch

### Current State

- **Default Branch**: `master`
- **Legacy Branch**: `main` (still exists for backward compatibility)
- **Remote Configuration**: Both branches available on origin
- **GitHub Setting**: Default branch set to `master`

### Branch Status

```bash
# Current branches
* master                           # New default branch (currently checked out)
  main                             # Legacy default branch
  remotes/origin/HEAD -> origin/main  # Will be updated to master
  remotes/origin/main              # Legacy remote branch
  remotes/origin/master            # New default remote branch
```

### For Developers

#### New Clones

New clones of this repository will automatically checkout the `master` branch:

```bash
git clone git@github.com:The1Studio/DynamicUserDifficultyUITemplate.git
cd DynamicUserDifficultyUITemplate
# You are now on master branch by default
```

#### Existing Clones

If you have an existing clone using `main` branch:

```bash
# Fetch latest changes
git fetch origin

# Switch to master branch
git checkout master

# Pull latest changes
git pull origin master

# Optional: Set master as default tracking branch
git branch --set-upstream-to=origin/master master

# Optional: Delete local main branch (if no longer needed)
git branch -d main
```

#### Updating Submodules

If this package is used as a submodule in another project:

```bash
# In the parent project
cd path/to/parent/project

# Update submodule reference
cd Packages/DynamicUserDifficultyUITemplate
git checkout master
git pull

# Commit the submodule change in parent project
cd ../..
git add Packages/DynamicUserDifficultyUITemplate
git commit -m "Update DynamicUserDifficultyUITemplate submodule to master branch"
```

### Unity Package Manager Integration

No changes required for Unity Package Manager (UPM) integration. The package.json references the repository URL, and GitHub will automatically serve the new default branch.

### Rationale for Migration

The migration to `master` branch aligns with:
- Traditional Git conventions
- TheOne Studio's standard practices
- Industry-standard naming for default branches
- Better compatibility with legacy tooling

### Backward Compatibility

- The `main` branch will remain available for backward compatibility
- Existing references to `main` branch will continue to work
- Gradual migration is supported - no breaking changes

### Timeline

- **2025-11-04**: Migration completed
- **Next 30 days**: Transition period for existing clones
- **After 30 days**: `main` branch may be archived or removed

### Support

For questions or issues related to this migration, contact:
- **Email**: support@theonestudio.com
- **Repository Issues**: https://github.com/The1Studio/DynamicUserDifficultyUITemplate/issues

---

**Last Updated**: November 4, 2025
**Migration Status**: ✅ Completed
