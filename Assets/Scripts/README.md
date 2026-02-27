# CarDrift - Huong dan trien khai Scripts

## Tong quan

Game drift xe mobile: xe tu dong chay thang, nguoi choi xoay vo lang de drift qua chuong ngai vat.

### Danh sach Scripts (14 files)

| Script | Chuc nang |
|--------|-----------|
| GameManager | Quan ly trang thai game (Ready/Playing/Paused/GameOver/Win) |
| GameUI | Ket noi tat ca buttons UI voi GameManager |
| AutoDriveCarController | Xe tu dong chay + nhan steering input tu vo lang |
| SteeringWheelUI | Vo lang xoay bang touch/drag tren man hinh |
| TopDownCameraFollow | Camera top-down nhin xuong theo xe |
| ObstacleCollision | Phat hien va cham container/tuong -> Game Over |
| LevelGoal | Trigger zone dich -> Win |
| ScoreManager | Tinh diem drift + combo multiplier |
| ProgressManager | Luu tien trinh unlock level (PlayerPrefs) |
| LevelManager | Quan ly danh sach level |
| LoadingScene | Man hinh loading voi progress bar |
| SelectScene | Chon map/level bang SimpleScrollSnap carousel |
| AudioManager | Quan ly Music + SFX, toggle on/off |
| SettingsPopup | Popup settings (music/sfx toggle) |

---

## Build Settings - Thu tu Scene

```
Index 0: LoadingScene
Index 1: SelectScene
Index 2: Map_1 (hoac GameScene)
Index 3: Map_2
Index 4: Map_3
...
```

## Flow game

```
LoadingScene -> SelectScene -> LoadingScene -> GameScene
                                                  |
                                          Playing (xe tu chay)
                                           /    |    \
                                      Pause  GameOver  Win
                                        |       |       |
                                     Resume  Restart  NextLevel
                                              SelectScene
```

---

## SCENE 1: LoadingScene

### Tao Scene
1. File -> New Scene -> dat ten `LoadingScene`
2. Them vao Build Settings (index 0)

### Hierarchy
```
LoadingScene
  Canvas (Screen Space - Overlay)
    Background (Image - full screen, anh nen game)
    ProgressBar (Slider)
      Background (Image - thanh nen)
      Fill Area
        Fill (Image - mau xanh/trang)
      [Xoa Handle Slide Area]
    ProgressText (Text - "Loading... 0%")
  LoadingManager (Empty GameObject)
```

### Setup
1. Tao **Canvas** -> set **UI Scale Mode** = Scale With Screen Size, Reference Resolution = 1080x1920
2. Tao **Slider** lam progress bar:
   - Xoa `Handle Slide Area` (khong can keo)
   - Set `Min Value = 0`, `Max Value = 1`
   - Bo tick `Interactable`
3. Tao **Text** cho "Loading... 0%"
4. Tao **Empty GameObject** `LoadingManager`:
   - Add Component -> **LoadingScene**
   - Keo `Slider` vao **Progress Bar**
   - Keo `Text` vao **Progress Text**
   - Set **Next Scene Name** = "SelectScene"
   - Set **Minimum Load Time** = 3 (giay)

---

## SCENE 2: SelectScene

### Tao Scene
1. File -> New Scene -> dat ten `SelectScene`
2. Them vao Build Settings (index 1)

### Hierarchy
```
SelectScene
  Main Camera
  Canvas (Screen Space - Overlay)
    ScrollRect (SimpleScrollSnap)
      Content
        Panel_Map1 (Image hoac 3D preview)
        Panel_Map2
        Panel_Map3
        ...
    SelectButton (Button - "Select")
    BackButton (Button - mui ten quay lai)
    LockIcon (GameObject - hinh khoa, an/hien)
  ProgressManager (Empty GO - DontDestroyOnLoad)
  AudioManager (Empty GO - DontDestroyOnLoad)
```

### Setup SimpleScrollSnap
1. Tao **ScrollRect** tren Canvas
2. Add Component -> **Simple Scroll Snap**
3. Them cac panel con vao **Content** (moi panel la 1 map preview)
4. Trong Simple Scroll Snap Inspector:
   - Movement Type: Fixed
   - Movement Axis: Horizontal
   - Use Automatic Layout: ON
5. **On Panel Centered** event -> keo SelectScene object -> chon `UpdateSelection`

### Setup SelectScene Script
1. Tao **Empty GameObject** hoac gan truc tiep len Canvas
2. Add Component -> **SelectScene**
3. Keo:
   - `simpleScrollSnap` -> ScrollRect co Simple Scroll Snap
   - `selectButton` -> nut Select
   - `backButton` -> nut Back
   - `goLock` -> icon khoa
4. Set:
   - `backSceneName` = "LoadingScene" (hoac "Home")
   - `totalItems` = so luong map
   - `sceneNames` -> dien ten scene: Map_1, Map_2, Map_3...

### ProgressManager (chi tao 1 lan)
1. Tao **Empty GameObject** `ProgressManager`
2. Add Component -> **ProgressManager**
3. Script tu DontDestroyOnLoad, chi can dat o scene dau tien

### AudioManager (chi tao 1 lan)
1. Tao **Empty GameObject** `AudioManager`
2. Them 2x **AudioSource** component len no:
   - AudioSource 1: cho Music (Loop = ON)
   - AudioSource 2: cho SFX (Loop = OFF)
3. Add Component -> **AudioManager**
4. Keo:
   - `musicSource` -> AudioSource 1
   - `sfxSource` -> AudioSource 2
5. Keo cac AudioClip vao: menuMusic, gameMusic, buttonClick, winSfx, loseSfx

---

## SCENE 3+: GameScene (Map_1, Map_2, ...)

### Tags can tao truoc
Edit -> Project Settings -> Tags and Layers -> Tags:
- `Player` (gan vao xe)
- `Obstacle` (gan vao container)
- `Wall` (gan vao tuong)

### Hierarchy
```
GameScene
  Directional Light
  Terrain / Ground
  Main Camera
  Prometheus (xe - tu Prefab)
  Canvas
    HUD Panel
      PauseButton
      ScoreText
      ComboText
    PausePanel (an mac dinh)
      ResumeButton
      RestartButton
      SelectMapButton
      HomeButton
    GameOverPanel (an mac dinh)
      FinalScoreText
      RestartButton
      SelectMapButton
    WinPanel (an mac dinh)
      NextLevelButton
      RestartButton
      SelectMapButton
    SteeringWheel (Image - hinh vo lang)
    SettingsPopup (xem phan Settings)
  GameManager (Empty GO)
  ScoreManager (Empty GO)
  Obstacles
    Container_1 (tag: Obstacle)
    Container_2 (tag: Obstacle)
    Wall_Left (tag: Wall)
    Wall_Right (tag: Wall)
    ...
  GoalTrigger (Empty GO voi Box Collider isTrigger)
```

### 1. Setup Xe (Prometheus)

Su dung **Prometheus.prefab** co san trong `PROMETEO - Car Controller/Prefabs/`

1. Keo Prometheus prefab vao scene
2. Set **Tag = Player**
3. Tren Prometheus, cac component:

```
[Rigidbody]          -> da co san
[PrometeoCarController] -> da co san
  - Use Touch Controls: tat (script se tu bat)
  - Use Effects: bat
  - Use Sounds: bat
[AutoDriveCarController] -> Add Component
  - Car Controller: keo chinh Prometheus vao
  - Steering Wheel: keo SteeringWheel Image vao (tao UI truoc)
  - Drift Threshold: 0.5 (xoay vo lang >50% thi auto-drift)
[ObstacleCollision] -> Add Component
  - Min Speed To Trigger: 5
```

**Luu y**: AutoDriveCarController se tu dong:
- Tat PrometeoCarController.enabled (tranh conflict WASD)
- Tao fake touch buttons
- Bat useTouchControls
- PrometeoCarController xu ly toan bo physics/drift/effects nhu binh thuong

### 2. Setup Camera

1. Chon **Main Camera**
2. **Xoa** CameraFollow (neu co)
3. Add Component -> **TopDownCameraFollow**
4. Keo Prometheus vao **Car Transform**
5. Tinh chinh:
   - Offset: X=0, Y=15, Z=-8 (dieu chinh theo y thich)
   - Look Down Angle: 65
   - Follow Speed: 5
   - Rotate Speed: 2

### 3. Setup UI Canvas

1. Tao **Canvas**:
   - Render Mode: Screen Space - Overlay
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1080 x 1920 (portrait)
   - Match: 0.5

2. **Steering Wheel**:
   - Tao UI -> Image -> dat ten `SteeringWheel`
   - Anchor: bottom center
   - Size: 300x300 (tuy chinh)
   - Them hinh vo lang (hoac de mac dinh)
   - Add Component -> **SteeringWheelUI**
   - Max Rotation Angle: 120
   - Return Speed: 10
   - Drag Range: 200

3. **HUD Panel**: (hien khi Playing)
   - PauseButton (goc tren trai)
   - ScoreText
   - ComboText

4. **Pause Panel**: (an mac dinh)
   - ResumeButton
   - RestartButton
   - SelectMapButton
   - HomeButton

5. **GameOver Panel**: (an mac dinh)
   - Text "Game Over"
   - FinalScoreText
   - RestartButton
   - SelectMapButton

6. **Win Panel**: (an mac dinh)
   - Text "Level Complete!"
   - NextLevelButton
   - RestartButton
   - SelectMapButton

### 4. Setup GameManager

1. Tao **Empty GameObject** `GameManager`
2. Add Component -> **GameManager**
3. Keo cac panel vao:
   - Ready Panel (co the null neu khong can)
   - HUD Panel
   - Pause Panel
   - GameOver Panel
   - Win Panel

### 5. Setup GameUI

1. Tao **Empty GameObject** `GameUI` (hoac gan len Canvas)
2. Add Component -> **GameUI**
3. Keo buttons:
   - HUD: pauseButton
   - Pause: resumeButton, pauseRestartButton, pauseSelectButton, pauseHomeButton
   - GameOver: gameOverRestartButton, gameOverSelectButton, gameOverHomeButton
   - Win: nextLevelButton, winRestartButton, winSelectButton, winHomeButton
4. Chi keo nhung button can thiet, bo trong neu khong dung

### 6. Setup ScoreManager

1. Tao **Empty GameObject** `ScoreManager`
2. Add Component -> **ScoreManager**
3. Keo:
   - Score Text -> Text hien diem trong HUD
   - Combo Text -> Text hien combo "x2.5"
   - Final Score Text -> Text trong GameOver/Win panel
4. Tinh chinh:
   - Drift Score Per Second: 10
   - Combo Multiplier Increase Rate: 0.5
   - Max Combo Multiplier: 5

### 7. Setup Obstacles

1. Keo **container prefabs** tu `Shipping Container Environment/Prefabs/`
2. Dat **Tag = Obstacle** cho moi container
3. Dam bao moi container co **Collider** (Box Collider hoac Mesh Collider)
4. Dat tuong bao quanh voi **Tag = Wall**

### 8. Setup Goal (Dich)

1. Tao **Empty GameObject** `GoalTrigger`
2. Add Component -> **Box Collider**
   - Tick **Is Trigger = ON**
   - Size du lon de xe chay qua
3. Add Component -> **LevelGoal**
4. Dat o vi tri dich den

### 9. Setup Settings Popup (tuy chon)

1. Tao UI panel `SettingsPanel` (an mac dinh)
   - MusicToggleButton (nut bam toggle music)
     - MusicOnState (GO - hien khi ON)
     - MusicOffState (GO - hien khi OFF)
   - SfxToggleButton (nut bam toggle sfx)
     - SfxOnState (GO - hien khi ON)
     - SfxOffState (GO - hien khi OFF)
   - CloseButton
2. Add Component -> **SettingsPopup** (len GO bat ky)
3. Keo:
   - Open Button: nut settings/gear tren HUD
   - Close Button: nut Close trong panel
   - Music Toggle / Sfx Toggle: cac nut tuong ung
   - musicOnState, musicOffState, sfxOnState, sfxOffState
   - Settings Panel: panel chinh

---

## Cach goi tu code

### Chuyen scene qua Loading
```csharp
LoadingScene.LoadScene("Map_1");
```

### Doc lua chon tu SelectScene
```csharp
int mapIndex = PlayerPrefs.GetInt("selected_map", 0);
```

### Check level unlock
```csharp
bool unlocked = ProgressManager.instance.IsLevelUnlocked(3);
```

### Unlock level khi thang
```csharp
ProgressManager.instance.CompleteLevel(currentLevel);
// Tu dong unlock level tiep theo
```

### Play audio
```csharp
AudioManager.instance.PlayMusic(clip);
AudioManager.instance.PlaySfx(clip);
AudioManager.instance.PlayButtonClick();
AudioManager.instance.ToggleMusic();
```

### Game events
```csharp
GameManager.Instance.StartGame();
GameManager.Instance.PauseGame();
GameManager.Instance.ResumeGame();
GameManager.Instance.TriggerGameOver();
GameManager.Instance.TriggerWin();
GameManager.Instance.RestartLevel();
GameManager.Instance.LoadNextLevel();
GameManager.Instance.LoadSelectScene();
```

---

## Tai su dung cho game khac

Cac script duoc thiet ke de reuse:

1. **Copy folder `Scripts/`** sang project moi
2. Thay doi:
   - `AutoDriveCarController` -> viet controller rieng cho game moi
   - `ObstacleCollision` / `LevelGoal` -> giu nguyen hoac chinh tag
3. Giu nguyen:
   - `GameManager`, `GameUI` -> flow game chung
   - `LoadingScene`, `SelectScene` -> UI scene chung
   - `AudioManager`, `SettingsPopup` -> audio chung
   - `ProgressManager`, `ScoreManager` -> save data chung
4. Chi can thay doi ten scene trong Inspector
