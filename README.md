# PYRO_LAB 🎆

> **重要聲明：本專案僅為視覺／遊戲用途的煙火效果模擬。程式碼與文件不包含、也不得用於任何真實煙火製造、化學配方或危險操作。**

PYRO_LAB 是一個以 Unity（建議 2022 LTS + Universal Render Pipeline）打造的純視覺煙火模擬原型。透過 ScriptableObject、粒子系統與數學函式，開發者可以調整各種參數，實驗日本圓形煙火風格的外觀與動態，用於遊戲或互動體驗。

## ✨ 特色
- **模組化 FireworkRecipe**：以 ScriptableObject 描述多層（Layer）煙火結構，支援拖尾、閃爍、變色、重力拖曳、分裂等 Modifier，所有內容皆為純視覺行為參數。
- **BurstPatterns 幾何取樣**：提供球殼、柳枝、環形、棕櫚、Pistil Ring、Layered Shell、2D 投影等數學採樣函式，僅生成方向向量與比例資訊。
- **TimingTrack 事件**：以 0~1 正規化時間定義二段爆、層級再觸發、Modifier 事件，快速搭建複合視覺節奏。
- **Recipe Composer GUI**：改版 Inspector 具備層級排序、Modifier 選單、Timing 編輯與預覽，並保留 JSON 匯入／匯出。
- **Demo_Modular 場景**：提供至少六組預設視覺配方，可透過鍵盤快速切換並示範 Recipe Composer 操作。

## 🗂 專案結構
```
Assets/
  _Core/
    Data/
      FireworkRecipe.cs
      FireworkLayer.cs
      TimingTrack.cs
      GradientWrapper.cs
      VisualModifier.cs
      Modifiers/
        ColorShiftModifier.cs
        FadeModifier.cs
        GravityDragModifier.cs
        SplitModifier.cs
        StrobeModifier.cs
        TrailModifier.cs
        TwinkleModifier.cs
    Runtime/
      BurstPatterns.cs
      FireworkBurst.cs
      FireworkLauncher.cs
      FireworkSpawner.cs
    Editor/
      FireworkRecipeEditor.cs
      JsonPresetExporter.cs
  Prefabs/
    PF_FireworkLauncher.prefab
  Scenes/
    Demo_Modular.unity
    Demo.unity (legacy)
ProjectSettings/
README.md
```

## 🛠 環境需求
- Unity 2022.3 LTS（或相容版本）
- Universal Render Pipeline（URP）套件

## 🚀 快速開始
1. 以 Unity Hub 匯入專案資料夾並使用 Unity 2022.3 LTS（URP）開啟。
2. 在 `Assets/Scenes/Demo_Modular.unity` 中開啟 Demo 場景（保留舊 Demo 作為對照）。
3. 播放場景後，使用 `FireworkSpawner` 監控物件，以空白鍵或等待自動輪播即可觀看多組預設煙火組合；鍵盤 `1~6` 可切換推薦配方。

## 🎨 建立自訂配方
1. 於 Project 視窗中右鍵 → **Create → PYRO → Firework Recipe**。
2. 在 Inspector 的 **Recipe Composer** 中：
   - 調整 Global 區塊（尺寸、預期高度、Fuse 時間、顏色漸層、HDR 強度）。
   - 依需求新增多個 Layer，為每層選擇幾何 Pattern、設定速度範圍與顏色漸層。
   - 透過 **Add Modifier** 選單套用拖尾、Strobe、Color Shift、Fade、Gravity Drag、Split、Twinkle 等純視覺效果。
   - 在 Timing Track 新增事件，組合二段爆或指定層的再觸發節奏。
3. 使用 **Preview** 按鈕於編輯器模式播放視覺模擬。
4. 透過 **Export JSON** / **Import JSON** 保存或載入純文字 Recipe 設定。

## 📦 JSON 匯出／匯入
- 匯出：於 Inspector 點擊 **Export JSON**，指定路徑後即可產生純文字 JSON。
- 匯入：於 Inspector 點擊 **Import JSON**，選擇先前匯出的檔案即可覆寫 ScriptableObject 參數。

## 🧪 測試（建議）
專案提供 C# 腳本與粒子設定，建議在合併 PR 前於本地使用 Unity Editor 測試以下項目：
- Demo_Modular 場景中是否能播放至少六種煙火組合並支援鍵盤切換。
- Recipe Composer 是否能增刪 Layer、Modifier 與 Timing 事件並即時預覽。
- FireworkRecipe ScriptableObject 是否能正確匯入／匯出 JSON（匯出→刪除→重新匯入→結果一致）。

## 🤝 貢獻
歡迎以功能分支的方式提交 Pull Request。請遵守安全方針：
- 僅分享視覺模擬相關程式碼與素材。
- 不得加入任何真實世界煙火製造、化學、物理配方或危險操作指南。

## 📄 授權
MIT License

## 🔬 VFX Graph 研究原型（feature/vfx-graph-research）
本分支新增了一組以 **Unity VFX Graph** 為核心的花火實驗原型，僅針對視覺與數學模擬進行研究，**嚴禁** 用於任何真實煙火製造或化學試驗。

### 檔案結構
```
Assets/_CoreVFX/
  Graphs/
    FW_Peony.vfx        # 球殼 Peony 花形
    FW_Willow.vfx       # 垂墜 Willow 柳枝
    FW_Ring.vfx         # 平面環形 Ring
  Scripts/
    VFXFireworkLauncher.cs
  Scenes/
    Demo_VFXGraph.unity
```

> `.vfx` 與 `.unity` 目前為概念性 YAML 設定，說明了需要在 Unity 編輯器內建立的 VFX Graph 節點、參數與場景佈局。開啟後依照註解重建即可得到等效的圖形。所有參數皆為 HDR 純視覺設定，不牽涉真實物質。

### Demo 操作
1. 在 Unity 2022.3 LTS + URP 中開啟 `Assets/_CoreVFX/Scenes/Demo_VFXGraph.unity`。
2. 場景內 `FireworkLauncher` 物件掛載 `VFXFireworkLauncher` 腳本，可綁定三個 VFX Graph（Peony / Willow / Ring）。
3. 播放時按下 `1 / 2 / 3` 鍵可切換不同花火；空白鍵觸發單次發射（或使用自動輪播）。
4. 在 Inspector 中調整 `starCount`、`burstRadius`、`colorGradient`、`drag`、`gravityFactor`、`trailLength`、`strobeFrequency` 等公開參數即可即時測試。

### GPU 粒子測試建議
- 在每個 VFX Graph 的 Initialize Context 中將 `Capacity` 設為 ≥ 20000。
- 使用 `Set Position (Sphere)` 的 Surface 模式確保爆炸均勻分佈，並搭配 `Set Velocity Random Direction` 控制出射能量。
- Willow 變體需提高 `Drag` 與 `Gravity`，同時啟用 Trail/Spark Strip 以模擬下垂尾巴。
- Ring 變體可將 Y 分量鎖為 0 或使用自訂 Torus 取樣節點，達成 2D 環形。

### 研究問題筆記
- **URP 下的 Bloom + HDR**：透過 URP Volume 啟用 Bloom、設定高強度閾值並使用 HDR 顏色（>1）即可讓花火在後期管線中泛光。
- **GPU 粒子上限**：VFX Graph 受限於 GPU 記憶體與 context capacity，單個系統常見上限約落在 1~4 百萬粒子；實務上為維持 60 FPS 可將 capacity 控制在 50~100k 並視顯卡調整。
- **C# 與 ScriptableObject 連動**：`VFXFireworkLauncher` 示範透過 `VisualEffect` API 以事件 (`SendEvent`) 與參數 (`SetInt/SetFloat/SetGradient`) 控制 Graph。若需資料驅動，可加上 ScriptableObject（例如 Firework 配方）並在播放前注入參數。

> **再次提醒**：所有程式碼與文件皆僅供視覺研究使用，嚴禁應用於真實煙火設計、製造或測試。
