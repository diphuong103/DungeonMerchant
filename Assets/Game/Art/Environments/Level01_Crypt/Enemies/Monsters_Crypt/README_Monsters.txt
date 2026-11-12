HẦM MỘ VONG HỒN - QUÁI & BOSS (Top-down 3/4)
==========================================
Gồm 3 quái + 1 boss, mỗi con có 3 hướng (down / up / side - side quay phải, lật X cho trái) và đầy đủ animation.
Pixels Per Unit = 64, Filter = Bilinear, Compression = None.

QUÁI (khung 128x128)                              BOSS (khung 240x240)
  Skeleton_Warrior  Xương Binh Canh Mộ              Boss_Death_Lord  Tử Vương Vong Hồn
  Ghoul             Thi Quỷ Đào Mộ
  Wraith            Oan Hồn U Uất

ANIMATION (frame / mỗi hướng):  idle 4 | walk 6 | attack 6 | hurt 2 | death 6   (boss: death 8, thêm special 10)
FPS gợi ý: idle 6, walk 10, attack 12, hurt 10, death 8, special 10.

FILE
  <id>_<state>_<dir>.png   strip ngang 1 hàng (dùng thẳng làm Animation Clip)
  <id>_atlas.png + .json   toàn bộ trong 1 atlas, mỗi hàng = state+hướng (json ghi row & số frame)
  Slice: Grid By Cell Size = frameSize. Pivot: Bottom Center (vị trí chân ở ~ 90% chiều cao khung).

VFX/
  vfx_slash_white_4f     chém của Xương Binh / đòn thường boss (hướng phải, xoay theo hướng đánh)
  vfx_slash_claw_4f      3 vết cào của Thi Quỷ
  vfx_hit_spark_4f       tia lửa khi trúng đòn
  proj_soul_bolt_4f      đạn cầu hồn của Oan Hồn (bay sang phải, xoay theo hướng bắn)  + vfx_soul_bolt_hit_5f nổ khi trúng
  vfx_soul_wisp_6f       linh hồn bay lên khi quái chết
  vfx_boss_slam_6f       sóng đập đất khi boss vung trượng (frame 3)
  telegraph_soul_nova_4f vòng đỏ cảnh báo vùng chiêu đặc biệt (384x384, bán kính 180px = 2.8 unit), lặp nhấp nháy
  vfx_soul_nova_blast_8f vụ nổ lan rộng của chiêu đặc biệt (384x384)
  proj_soul_skull_4f     đầu lâu ma bay ra từ vụ nổ (8 viên)
  shadow_monster / shadow_boss   bóng dưới chân

CHIÊU ĐẶC BIỆT CỦA BOSS - "VONG HỒN BÙNG NỔ"
  1. Boss chuyển state special: nâng cao trượng, bay lên, vòng rune xanh + linh hồn xoay quanh (frame 0-4).
  2. Spawn telegraph_soul_nova dưới chân boss, kéo dài 1.2s (giữ frame 4).
  3. Frame 5: phát vfx_soul_nova_blast, gây sát thương trong bán kính, bắn 8 proj_soul_skull đều 8 hướng.
  4. Frame 6-9: boss hồi chiêu, đứng yên cho người chơi phản công.
  Số liệu cân bằng gợi ý nằm trong monsters.json (có thể chỉnh).

Death: skeleton để lại đống xương; ghoul để lại vết bẩn; wraith tan thành linh hồn; boss tan rồi để lại xương + vương miện.
Preview/: overview_monsters_boss.png và 4 file GIF chạy thử tất cả animation (3 hướng cạnh nhau).
Bóng: đặt shadow sprite dưới chân (Sorting thấp hơn nhân vật). Wraith/Boss bay lơ lửng nên dịch bóng xuống ~0.3 unit.
