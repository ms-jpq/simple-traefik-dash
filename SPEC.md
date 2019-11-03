# SPEC FILE

This file defines the basic requirements for our app.

## Roles

### Dashboards

- Dashboards have a name, a favicon, and a background image / colour
- Dashboards have access to _Folders_
- Dashboards can be read only

## Resources

- All resources can be pinned.
- Pinning is individual to _Dashboards_
- Pinned resources are at the top of their "container".

### Folders

- Folders have a name and a colour
- Folders cannot contain other folders
- Folders contain either ONLY _Bookmarks_ or ONLY _Stickies_
- All _Stickies_ and _Bookmarks_ belong in Folders

### Stickies

- A sticky contains some plain text
- Stickies are reverse chronologically sorted

### Bookmark

- Bookmarks contain a uri, a title, and optionally, an image
- Bookmarks are always sorted by name

## Version Control

- DB must pass JSON \$schema conformance
- Invalid DB require manual user intervention
- Must sync with either empty git repo or a vaild git branch
- If DB is ahead of User, will pull from DB and erase all local changes
- Users can sync DB at any time
- DB sync may exclude select _Folders_

## Authentication

- Single user mode
