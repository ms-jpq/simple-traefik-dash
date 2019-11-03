# SPEC FILE

This file defines the basic requirements for our app.

## The Big Idea

Communism - all resources are public.

No historical revisionism - The entire DB of our app can be syncable to a git repo

## Roles

### Users

- Users have a name and a password
- Users have access to _Resources_
- Users can set a personal background image / colour
- Users can allow read-only access to their dash
- Users can create / remove other users

## Resources

- All resources can be pinned.
- Pinning is personal to _Users_
- Resources objects are at the top of their "container".
- Resources can be created, modified, deleted by all _Users_ ☭☭☭

### Folders

- Folders have a name and a colour
- Folders containly either ONLY _Bookmarks_ or ONLY _Stickies_
- All _Stickies_ and _Bookmarks_ belong in Folders
- Folders cannot contain other folders

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
