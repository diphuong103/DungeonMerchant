# Cấu trúc project DungeonMerchant

## Thư mục gốc

```text
DungeonMerchant/
├── Assets/                 # Tài nguyên game và nội dung Unity
├── Packages/               # Khai báo package Unity của project
├── ProjectSettings/        # Cấu hình project Unity
├── UserSettings/           # Cấu hình cá nhân của Unity Editor
├── Library/                # Dữ liệu cache Unity tự tạo
├── Logs/                   # Log của Unity
├── Temp/                   # File tạm Unity tự tạo
├── .github/                # Cấu hình GitHub và workflow
├── .vscode/                # Cấu hình Visual Studio Code
├── *.csproj                # Project C# do Unity sinh để dùng với IDE
├── DungeonMerchant.slnx    # Solution dành cho IDE
└── note.txt                # Ghi chú ở thư mục gốc
```

## Thư mục `Assets`

```text
Assets/
├── Editor/                 # Script và công cụ chỉ chạy trong Unity Editor
├── Game/                   # Nội dung chính của game
├── Scenes/                 # Scene ở cấp Assets, ngoài nhóm Game
├── Screenshots/            # Ảnh chụp màn hình tham khảo hoặc kiểm tra
├── Settings/               # Thiết lập Unity, renderer và input
└── TextMesh Pro/           # Font, shader và tài nguyên TextMesh Pro
```

## Thư mục `Assets/Game`

```text
Assets/Game/
├── Art/
│   ├── Animation/          # Animation clip và animator controller
│   ├── Characters/         # Sprite và hình ảnh nhân vật
│   ├── Environments/       # Hình ảnh/tài nguyên môi trường theo khu vực, màn chơi
│   │   ├── Level01_Crypt/          # Tài nguyên riêng màn Hầm Mộ Vong Hồn
│   │   ├── Level02_SpiderNest/     # Tài nguyên riêng màn Sào Huyệt Nhện U Minh
│   │   ├── Level03_InfernoPrison/  # Tài nguyên riêng màn Ngục Hắc Viêm
│   │   ├── Level04_BloodMoonPalace/# Tài nguyên riêng màn Điện Huyết Nguyệt
│   │   └── Level05_VoidEyeAbyss/   # Tài nguyên riêng màn Vực Ma Nhãn Hư Vô
│   ├── Material/           # Material hỗ trợ cho tài nguyên art
│   ├── Materials/          # Material gắn với hình ảnh/đối tượng game
│   ├── Preset/             # Preset cấu hình import hoặc thiết lập art
│   ├── Shader/             # Shader tùy chỉnh
│   ├── Tile Palette/       # Tile palette và tile asset dùng để dựng màn
│   │   ├── Rule Tiles/     # Rule Tile tự nối hoặc chọn biến thể theo vị trí
│   │   ├── TP Grass/       # Tile của palette cỏ
│   │   ├── TP Stone Ground/# Tile của palette nền đá
│   │   └── TP Wall/        # Tile của palette tường
│   ├── Tilesets/           # Bộ tileset và các tài nguyên liên quan
│   │   └── Materials/      # Material dành cho tileset
│   ├── UI/                 # Hình ảnh nguồn dành cho giao diện
│   │   ├── 01_panel/       # Sprite panel và khung giao diện
│   │   ├── 02_dialog/      # Sprite hộp thoại
│   │   ├── 03_avatar/      # Hình đại diện
│   │   ├── 04_slots/       # Sprite ô chứa vật phẩm/kỹ năng
│   │   ├── 05_buttons/     # Sprite nút bấm
│   │   ├── 06_bars/        # Sprite thanh trạng thái
│   │   ├── 07_icons_stats/# Icon chỉ số
│   │   ├── 08_icons_tabs/ # Icon tab
│   │   └── 09_misc/        # Hình UI khác
│   └── VFX/                # Tài nguyên hình ảnh hiệu ứng
├── Audio/
│   ├── Music/              # Nhạc nền
│   └── SFX/                # Hiệu ứng âm thanh
├── Data/
│   ├── Abilities/          # Dữ liệu kỹ năng
│   ├── Characters/         # Dữ liệu nhân vật
│   ├── Items/              # Dữ liệu vật phẩm
│   └── Quests/             # Dữ liệu nhiệm vụ
├── Materials/              # Material dùng chung cho nội dung game
├── Prefabs/
│   ├── Characters/         # Prefab nhân vật
│   ├── Enemies/            # Prefab kẻ địch
│   ├── Interactables/      # Prefab vật thể có thể tương tác
│   ├── Plant/              # Prefab cây cỏ và thực vật
│   ├── Props/               # Prefab đạo cụ môi trường
│   └── UI/                  # Prefab thành phần giao diện
├── Scenes/
│   ├── Gameplay/           # Scene gameplay chính
│   └── Samples/            # Scene mẫu và scene kiểm thử nội dung
├── Scripts/
│   ├── Combat/             # Logic chiến đấu
│   ├── Core/               # Hệ thống cốt lõi dùng chung
│   ├── Dialogue/           # Logic hội thoại
│   ├── Inventory/          # Logic túi đồ và vật phẩm
│   ├── Player/             # Điều khiển và hành vi người chơi
│   ├── Quests/             # Logic nhiệm vụ
│   ├── Save/               # Lưu và tải tiến trình
│   ├── UI/                 # Logic giao diện
│   └── World/              # Logic đối tượng và tương tác trong thế giới
├── Settings/                # Thiết lập runtime hoặc cấu hình game
└── UI/
    ├── Dialogue/           # Nội dung giao diện hội thoại
    ├── HUD/                # Nội dung giao diện trong lúc chơi
    ├── Menus/              # Nội dung menu
    └── WorldMap/           # Nội dung bản đồ thế giới
        └── WorldMapAssets/ # Sprite và hình ảnh riêng của bản đồ thế giới
```

## Chức năng các nhóm thư mục

| Thư mục | Chức năng |
|---|---|
| `Art` | Tập hợp đồ họa nguồn: sprite, animation, tileset, shader, vật liệu hình ảnh và VFX. |
| `Audio` | Chứa nhạc nền và hiệu ứng âm thanh. |
| `Data` | Chứa dữ liệu cấu hình cho kỹ năng, nhân vật, vật phẩm và nhiệm vụ. |
| `Materials` | Chứa các material dùng bởi đối tượng và hình ảnh trong game. |
| `Prefabs` | Chứa GameObject đã đóng gói để có thể tái sử dụng trong nhiều scene. |
| `Scenes` | Chứa các scene Unity, gồm scene gameplay và scene mẫu. |
| `Scripts` | Chứa mã nguồn, chia theo hệ thống mà mã nguồn đó điều khiển. |
| `Settings` | Chứa các thiết lập cấu hình cho Unity hoặc phần chạy game. |
| `UI` | Chứa tài nguyên giao diện đã nhóm theo màn hình/chức năng, như HUD, menu, hội thoại và bản đồ. |
| `Art/UI` | Chứa hình ảnh nguồn của UI, ví dụ sprite nút, khung, icon và thanh trạng thái. |
| `Game/UI` | Chứa tài nguyên UI đã nhóm theo chức năng hoặc màn; không phải nơi chứa mã điều khiển UI. |
| `Scripts/UI` | Chứa mã điều khiển, cập nhật và xử lý tương tác giao diện. |
| `Environments/LevelXX_*` | Chứa art môi trường chỉ dùng cho màn tương ứng. |

