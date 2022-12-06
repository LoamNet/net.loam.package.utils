# net.loam.utils
A series of small Unity utilities, extensions, and more to assist with development.

## Setup (to use)
1. Inside unity, head to `Window > Package Manager`
2. Select the `+` dropdown button in the upper left of the window and select `Add package from git URL`
3. Enter the git URL for this repo

## Setup (to edit)
1. Create a new unity project with the version specified in package.json (2020.3 lts)
2. Clone this repo into the packages folder at the manifest.json level. The entire contents of this repo should be within a new folder. 
3. Open/Reload the unity project. Under the Projects tab and packages subfolder, you should see 'Loam Utilities'. 

## Contents

### Menu Items
- **'Copy Meta GUID to Clipboard'** - When selecting an asset in the project view, the meta GUID can be copied to the clopboard via a right-click menu. This helps with things like manual reference checking.

### Classes
- **WHRandom** - A predictable and portable random class that can be seeded with a different pseudorandom technique than system random or unity random. Can be used to help ensure cross-platform seeded values are consistent.
- **CoroutineObject** - A lazily instantiating game object to allow static and non-unity code to kick off coroutines.

### Extensions
- **GameObject** - Extra hierarchy management
- **CharacterController** - Warping (setting the transform) that correctly accounts for transform sync.

### Static Functions
- **EditorClearConsole(...)** - Function that will clear the developer console if in-editor.

### Postmaster
A messaging system that comes with inspector integration and dispatching/tracking tools. Allows easily testing
messages by dispatching them at runtime along with usage information. Intended to be used for intermittent gameplay events, not aggressive per-update events.
