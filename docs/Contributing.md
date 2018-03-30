# Guidelines for contributing to EDDI

Here are some general guidelines and conventions for developers making contributions to the EDDI project.

## Git branch structure and tag conventions

This is based on git-flow, but more relaxed:

  * We dispense with git-flow's ceremony concerning release branches.
  * Releases are tagged in the format `Release/3.0.0-b1`. There are some older release tags in the repo that lack the `Release/` prefix but never mind.
  * `master` points to the current production release. **All unit tests and code analysis must pass.** Fast-forwarding the `master` branch is preferred versus adding a merge commit.
  * `beta` points to the current beta release. **All unit tests and code analysis must pass.** Fast-forwarding the `beta` branch is preferred versus adding a merge commit.
  * `develop` is the integration branch for work in progress. **All unit tests and code analysis must pass.**

### Work in progress conventions

Branch names for WIP typically look like `feature/47-cargo-monitor` or `hotfix/305-shipyard`. Including the ticket number saves you looking it up when you come to hashtag it in the commit message. We're not too picky as long as it's clear. We do prefer prefixes like `feature/` and `hotfix/` that create collapsible folders in GUIs.

**Important:** please **under no circumstances** merge *from* `develop` *into* your WIP branch! 

We have learned the hard way that this causes very big problems when merging the WIP branch back into `develop`, notably a very confused commit history with a great many duplicated commit objects. Git's own documentation also warns against this. Any such Pull Requests will be declined.

In general, just ignore any progress in `develop` while your WIP branch was in progress and simply create a Pull Request based on your WIP branch. We will handle the merge and liaise if necessary.

In particular, if GitHub is showing that the only potential merge conflicts are in `ChangeLog.md`, just go ahead and create a Pull Request. I'm more than happy to sort out any conflicts in `ChangeLog.md` at my end.

If your WIP branch has gotten so far behind `develop` that code and project files cannot merge cleanly, here's what to do:

  * Above all, **don't** merge from `develop` into your WIP branch!
  * If you *haven't* yet published your WIP branch, it is perfectly OK to rebase it onto `develop` and resolve any issues in the privacy and comfort of your own branch. I do this all the time.
  * If you *have* published your WIP branch, you can duplicate it and rebase *the duplicate* onto `develop`. Once you are happy with it you can delete the original branch and PR and create a new PR. We are fine with that. Please do resist the temptation to rename the duplicate back to the name of the original, as that will confuse both GitHub and humans mightily.

## Coding conventions

Please adhere to US spelling for all names in the source code, for consistency with the .NET framework and to simplify searching the code base, e.g. for all variables whose name contains "color".

## Version conventions
  * Please be aware that departing from the expected version string format may break the code that talks to the update server.
  * Version strings are parsed and compared by `Utilities.Versioning` which is 100% covered by `Tests.VersioningTests`.
    * Please take a strict test-driven approach to any changes here and keep it 100% covered.
  * Please be aware that the version string needs to be updated in both `Utilities\Constants.cs` and `Installer.iss`. It also appears in `EDDI\ChangeLog.md` obviously.

## Git commit conventions
  * Please use the standard Git convention of a short title in the first line and fuller body text in subsequent lines. However we aren't too strict about line lengths.
  * Please reference issue numbers using the "hashtag" format `#123` in your commit message wherever possible. This lets GitHub create two-way hyperlinks between the issue report and the commit.
  * If in doubt, lean towards many small commits. This makes `git bisect` much more useful.
  * Please try at all costs to avoid a "mixed-up" commit, i.e. one that addresses more than one issue at once. One thing at a time is best. 

## Change log
  * We differentiate between the user-facing change log at at `EDDI/ChangeLog.md` and the developer-facing Git log.
  * Our policy for the user-facing change log is described at https://keepachangelog.com/en/1.0.0/. Please read.
  * For any user-facing change, please add a user-facing entry in the change log.

## Wiki

The Wiki is itself a repo: https://github.com/EDCD/EDDI.wiki.git.

At the time of writing, we are trying to avoid "polluting" the wiki with version annotations, however this is under review.

To regenerate the auto-generated parts of the Wiki:

1. Clone the Wiki, say to `C:\dev\EDDI.wiki`.
2. Clone the dev project, say to `C:\dev\EDDI`.
3. Rebuild the solution and run all the tests in `Tests\GeneratorTests.cs`.
4. Overwrite `C:\dev\EDDI.wiki` with `C:\dev\EDDI\Tests\bin\Debug\Wiki`, overwriting, not merging folders.
5. Push to the Wiki.

## Build process

Building the release target will create in `bin\Installer` both the installer `EDDI-[version-number].exe` and a zip of the debug symbols called `PDBs.zip`. These debug symbols are required for symbolicating stack traces and crash dumps from users, and are tied using UUIDs to the *exact* build session that created the the executables. If they are lost, it is no good to rebuild them later from the same source code: the ones from the build session need to be uploaded to Github along with the installer.

The update server is governed by `info.json` in https://github.com/EDCD/EDDP.git.
