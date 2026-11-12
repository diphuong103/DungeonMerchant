WORLDMAP ASSET PACK - Unity 2D
================================
Background/  (tileable, 2048x1152 - dung Sprite Renderer Draw Mode = Tiled hoac lap 2-3 ban canh nhau)
  bg_layer0_base.png         Nen tim/xanh nebula       -> parallax 0.10
  bg_layer1_fog_a.png        Suong mu tim (alpha)      -> parallax 0.25, tu troi cham (scroll auto)
  bg_layer1_fog_b.png        Suong mu xanh ngoc        -> parallax 0.35, troi nguoc huong fog_a
  bg_layer2_decor.png        Cay/da/nam/co             -> parallax 0.60
  bg_layer3_particles.png    Dom dom/hat sang          -> parallax 0.85 (hoac dung Particle System voi decor_firefly)
  overlay_vignette.png       Phu toi vien man hinh     -> gan vao Canvas, KHONG di chuyen theo camera
Decor/     sprite le: pine, rock, mushroom, grass, firefly, glow vung (purple/teal/green), emblem watermark
Nodes/     node_unlocked / node_locked / node_cleared, glow selected/cleared, pulse ring
Icons/     icon trang tri node: dragon, tree, gem, cave, swamp, ruins, spring, sword, skull_boss, crown, lock, check
Path/      path_dot_* (rai cham doc day noi), path_dash_tile_* (Line Renderer, Texture Mode = Tile), path_dot_glow
UI/        label_plate (9-slice), badge level, star filled/empty
Preview/   anh ghep thu

IMPORT SETTINGS
- Texture Type: Sprite (2D and UI), Pixels Per Unit = 100, Filter: Bilinear, Compression: None (hoac High Quality)
- Background layer: Wrap Mode = Repeat, Mesh Type = Full Rect
- Node_*.png: 256px -> scale ~0.5 trong scene (~1.3 unit). Icon 128px dat chinh giua node, scale 0.7
- UI label_plate: Sprite Editor -> Border L/R = 24, T/B = 24 de dung 9-slice

PHAN LOP (Sorting Order, thap -> cao)
 0 bg_layer0 | 1 fog | 2 decor | 3 particles | 10 path | 11 region emblem (vung khoa) |
 12 node glow | 13 node frame | 14 icon | 15 label | 100 vignette (UI)

TRANG THAI NODE
 Locked   : node_locked + icon_lock + path_dot_locked, an label hoac lam toi (alpha 0.5)
 Unlocked : node_unlocked + icon rieng + node_glow_selected (nhap nhay scale 0.95-1.05)
 Cleared  : node_cleared + badge/star; path_dot_cleared
 Doan duong noi A->B: unlocked khi B mo; locked khi con khoa.

KEO MAP BANG CHUOT
 Camera Orthographic; khi giu chuot: camera.x -= deltaMouse; Clamp theo gioi han map.
 Layer parallax: layer.x = camera.x * (1 - parallaxFactor).

DUNGEONS/ (them moi)
 dungeonN_*_icon.png          Icon 256 trong suot co halo (dat de len khung node bat ky)
 dungeonN_*_node_unlocked.png Node thanh pham: khung vang + hinh dac trung (mau tuoi)
 dungeonN_*_node_cleared.png  Node da hoan thanh (vien xanh + dau tick)
 dungeonN_*_glow.png          Glow rieng theo mau hang
 node_locked_with_lock.png    Node khoa dung chung cho ca 5 hang
 dungeons.json                Ten, mo ta, level yeu cau, ten sprite -> import vao ScriptableObject
Logic: Locked -> node_locked_with_lock | Unlocked -> *_node_unlocked + glow | Cleared -> *_node_cleared
