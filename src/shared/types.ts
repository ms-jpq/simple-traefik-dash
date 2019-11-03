export type UUID = string
export type Colour = string

export type View =
  | {
      type: "solid-colour"
      colour: Colour
    }
  | {
      type: "image"
      uri: URL
    }

export type Bookmark = {
  id: UUID
  title: string
  uri: URL
  display: View // NON-EDITABLE
}

export type Note = {
  id: UUID
  timestamp: Date
  content: string
}

export type BookmarkCollections = {
  id: UUID
  name: string
  bookmarks: Bookmark[]
}

export type NoteCollections = {
  id: UUID
  name: string
  notes: Note[]
}

export type Dashboard = {
  path: string // UNIQUE
  title: string
  background: View
  allowReadOnly: boolean
  bookmarkCollections: UUID[] //NON-EMPTY
  noteCollections: UUID[] //NON-EMPTY
}

export type Global = {
  bookmarkCollections: BookmarkCollections[]
  noteCollections: NoteCollections[]
  dashboards: Dashboard[]
}
