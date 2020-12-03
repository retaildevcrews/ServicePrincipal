# Versioning strategy

This project using a versioning strategy based on [Semantic Versioning 2.0](https://semver.org/) 

Given a version number MAJOR.MINOR.PATCH, increment the:

1. MAJOR version when you make incompatible API changes,
2. MINOR version when you add functionality in a backwards compatible manner, and
3. PATCH version when you make backwards compatible bug fixes.
Additional labels for pre-release and build metadata are available as extensions to the MAJOR.MINOR.PATCH format.

## App versioning
MAJOR.MINOR is pulled from `VersionPrefix` in the application's project file at `src/Automation/CSE.Automation/CSE.Automation.csproj`.

PATCH is the number of commits since last change to MAJOR.MINOR 

# Container versioning

Containers are versioned both when a Pull Request (PR) is created and when a PR is approved and merged. Container images are tagged in to match their version. In addition, the `latest` tag is updated to the latest merged PR.

Format:

`MAJOR.MINOR.PATCH-PR#-HASH`

`MAJOR.MINOR.PATCH-HASH`

Examples:

```
latest
0.2.20-2a2fc8a
0.2.26-PR393-e62d1e9
```
