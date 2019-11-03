export type UUID = string
export type HASH = string
export type Colour = string

export type Background =
  | {
      type: "colour"
      colour: Colour
    }
  | {
      type: "image"
      uri: URL
    }

export type Icon =
  | {
      type: "colour"
      colour: Colour
    }
  | {
      type: "icon"
      uri: URL
    }

export type Customizations = {
  collapsedFolders: UUID[]
  showSearch: boolean
}

export type User = {
  username: string
  password: HASH
  background: Background
  allowReadOnly: boolean
  folders: UUID[]
  pinned: UUID[]
} & Customizations

export type Sticky = {
  id: UUID
  timestamp: Date
  content: string
}

export type Bookmark = {
  id: UUID
  title: string
  uri: URL
  display: Icon | undefined
}

export type Folder = { id: UUID; name: string; colour: Colour } & (
  | { type: "stickies"; items: Sticky[] }
  | { type: "bookmarks"; items: Bookmark[] })
