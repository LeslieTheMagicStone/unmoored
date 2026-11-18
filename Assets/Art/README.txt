ART ASSETS
==========

Unity .meta files carry each asset's GUID. Always move an asset together
with its .meta file (or move it inside the Unity Editor), otherwise scene
and prefab references break.

FOLDERS
-------
Characters/Player1/  - player 1 sprites (walk, jump, stop, anchor)
Characters/Player2/  - player 2 sprites (same set, "2" prefix)
Environment/Platforms/ - platform and block sprites
Environment/Props/   - doors, elevator, pipes, switches, trampoline, checkpoint
Backgrounds/         - background layers
UI/                  - UI images; UI/Keyboard/ holds key prompt sprites
Video/               - intro video
Fonts/               - TTF sources and TMP SDF font assets

RULES
-----
- Sprites are PNG, Pixels Per Unit = 100. Keep .psd sources outside the project.
- New files: no spaces in filenames, use snake_case or PascalCase.
