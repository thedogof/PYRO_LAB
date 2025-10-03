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

## 🧭 Phase 2 擴充藍圖
- 以日本花火風格為靈感，將「紙皮層數、星粒排列、導火延遲、特效粉末」抽象為純數值參數。
- 第二階段分為三個功能分支，詳細請見 `docs/phase2-expansion-plan.md`：
  - **Workshop & Material System**：建立材料資料庫、工作坊 UI 與 JSON 匯入／匯出。
  - **Assembly & Crafting Flow**：提供積木式製作流程、視覺化製作痕跡、TimingTrack 強化。
  - **Economy & Growth**：導入材料價格、合約需求、品質評分與進程解鎖。
- Demo 場景將展示「購買材料 → 拼裝配方 → 測試 → 賣出升級」的循環，所有內容皆為視覺模擬用途。

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
