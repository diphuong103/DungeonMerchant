HẦM MỘ VONG HỒN - BỘ ASSET MÔI TRƯỜNG (Top-down 3/4)
=====================================================
Ô lưới: 64x64 px  |  Pixels Per Unit = 64  (1 tile = 1 unit)
Import: Sprite (2D and UI), Filter Mode = Bilinear, Compression = None. Tileset: Sprite Mode = Multiple -> Slice Grid By Cell Size 64x64.

THƯ MỤC
Tilesets/
  tileset_floor.png               16 ô nền (8 cột x 2 hàng): stone_a-d, cracked_a/b, mossy, cobble, dirt_a/b/deadgrass, bones, rune, blood, obsidian, ornate
  tileset_wall_top_autotile16.png 16 ô nóc tường autotile. Chỉ số = N(1)+E(2)+S(4)+W(8), bit=1 khi ô láng giềng đó là TƯỜNG (kể cả ô mặt tường). cap_00 = trụ đứng riêng lẻ, cap_15 = ô giữa khối tường.
  tileset_wall_face.png           8 mặt tường: plain_a/b, cracked, moss, skull_niche, torch, chains, cobweb  (đặt ở ô tường có sàn ngay phía Nam)
  tileset_chasm_autotile16.png    16 ô vực thẳm autotile (bit=1 khi láng giềng cũng là vực). Rìa phía Bắc là vách đá, các rìa còn lại là gờ đá.
  *.json                          bản đồ tên -> chỉ số ô
Structures/  Công trình lớn: gate_crypt/gate_iron (đóng/mở 2x2), stairs_down/up (2x2), altar_ritual, sarcophagus (h/v, đóng/mở),
             mausoleum (3x3), statue_reaper, statue_weeping_angel, pillar, pillar_broken, obelisk_rune, tombstone x4, grave_mound, grave_open,
             dead_tree, door_wood (đóng/mở), chest (thường/ornate, đóng/mở), bridge_h/v, fence_h/v
Props/       Vật trang trí: bone_pile, skull_pile, candles, crate, barrel, urn(+broken), cobweb_corner_tl (lật ngang cho góc phải), spikes on/off,
             pressure_plate on/off, lever on/off, chains_hanging, rubble a/b, soul_crystal, glow_mushrooms, dead_bush, skeleton_lying, lantern_hanging, brazier_teal
Animated/    Sprite strip ngang, mỗi frame cách đều: anim_brazier_teal_4f (64x96), anim_wall_torch_teal_4f (64x64, thay ô face_torch),
             anim_portal_soul_4f (128x128), anim_soul_pool_tile_4f (64x64, ô nước linh hồn lặp 4 frame)  -> Animation 8-10 fps, loop
Decals/      Đặt lên nền: blood a/b, crack a/b, moss_patch, bone_scatter, scorch, puddle, ritual_circle_3x3 (192x192), shadow_under_wall (đặt dưới mặt tường), shadow_blob s/m/l (bóng dưới vật thể)
Lighting/    light_teal/warm/purple_soft (cookie cho Light 2D / sprite cộng sáng), overlay_vignette_dark
Preview/     Ảnh ghép thử căn hầm + bảng tổng hợp
asset_manifest.json   kích thước, pivot gợi ý, số frame của từng sprite

CÁCH XÂY MAP (Tilemap)
 Layer 0 Ground      : tileset_floor + chasm
 Layer 1 Ground Decal: decals (máu, vết nứt, rêu, vòng ritual)
 Layer 2 Walls       : nóc tường (autotile) + mặt tường. Quy tắc: ô tường có sàn ở phía Nam và tường ở phía Bắc -> dùng ô MẶT TƯỜNG; còn lại dùng NÓC TƯỜNG.
 Layer 3 Props       : Sprite Renderer thường, Sort Order theo Y. Công trình/vật cao dùng Pivot Bottom Center, vật phẳng dùng Center.
 Collision           : Tilemap Collider 2D trên Walls + chasm; vật thể dùng Polygon/Box Collider đặt ở phần chân.
Bóng: shadow_blob đặt dưới chân vật thể (Sorting thấp hơn vật), shadow_under_wall đặt ngay phía Nam mỗi ô mặt tường.
Ánh sáng: dùng Light 2D (URP 2D) màu #56EED0 cho lửa ma/portal/pha lê, cộng thêm overlay_vignette_dark phủ toàn màn hình.
Bảng màu chủ đạo: đá xanh xám (#484E6A), xanh ngọc linh hồn (#56EED0), xương (#E4E2D0), tím u tối (#543486), máu (#801622).
