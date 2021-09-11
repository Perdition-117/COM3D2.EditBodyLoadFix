# COM3D2.EditBodyLoadFix

Fixes various issues relating to use of custom bodies in edit mode.

- Loads custom bodies from presets
- When switching body:
  - Restores selected pose
  - Fixes breasts going rigid
  - Fixes animation not freezing when placing accessories
  - Fixes touch jump not working
  - Fixes VR grabbing not working

Place `COM3D2.EditBodyLoadFix.dll` in `BepInEx\plugins`.

### Building

A publicized version of `Assembly-CSharp.dll` is required.
