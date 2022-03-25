# net.loam.utils
A series of small Unity utilities, extensions, and more to assist with development.

### Menu Items
- **'Copy Meta GUID to Clipboard'** - When selecting an asset in the project view, the meta GUID can be copied to the clopboard via a right-click menu. This helps with things like manual reference checking.

### Classes
- **WHRandom** - A predictable and portable random class that can be seeded with a different pseudorandom technique than system random or unity random. Can be used to help ensure cross-platform seeded values are consistent.
- **CoroutineObject** - A lazily instantiating game object to allow static and non-unity code to kick off coroutines.

### Extensions
- **GameObject** - Extra hierarchy management
- **CharacterController** - Warping (setting the transform) that correctly accounts for transform sync.

### Functions
- **EditorClearConsole(...)** - Function that will clear the developer console if in-editor.