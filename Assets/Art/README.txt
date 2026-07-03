ART ASSETS
==========

Place all visual/audio assets in the matching subfolder.
Unity .meta files are generated automatically — don't delete them.
Use .png for sprites, .psd sources stay outside the project.

FOLDERS
-------
Sprites/     — character sprites, props, UI icons (PNG, Pixels Per Unit = 100)
Animations/  — .anim / .controller files
Tilemaps/    — tile palettes, tile assets for level geometry
Fonts/       — custom fonts (TTF/OTF)
Materials/   — custom materials for sprites, effects

RULES
-----
- No spaces in filenames — use PascalCase or snake_case
- Player sprites: prefix with Player_  (e.g. Player_Idle.png)
- Enemy sprites:  prefix with Enemy_   (e.g. Enemy_Slime.png)
- Environment:    prefix with Env_     (e.g. Env_PlatformStone.png)
- UI elements:    prefix with UI_      (e.g. UI_ButtonNormal.png)

For the programmer:
- Placeholder sprites are currently the default Unity white square.
- Replace by dragging your sprites into corresponding slots on prefabs/scene objects.
- All SpriteRenderers use DrawMode=Tiled so tileable textures auto-repeat.
