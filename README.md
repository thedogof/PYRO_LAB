# PYRO_LAB 🎆

> **重要聲明：本專案僅為視覺／遊戲用途的煙火效果模擬。程式碼與文件不包含、也不得用於任何真實煙火製造、化學配方或危險操作。**

PYRO_LAB 是一個以 Unity（建議 2022 LTS + Universal Render Pipeline）打造的純視覺煙火模擬原型。透過 ScriptableObject、粒子系統與數學函式，開發者可以調整各種參數，實驗日本圓形煙火風格的外觀與動態，用於遊戲或互動體驗。

## ✨ 特色
- **FireworkRecipe ScriptableObject**：集中管理升空速度、引信時間、爆炸模式、顏色漸層、大小曲線、HDR 強度等視覺參數。
- **FireworkLauncher 組件**：依據配方驅動升空與爆炸兩個粒子系統，並提供 `Launch()` / `ResetAndLaunch()` API 方便重複播放。
- **FireworkBurst 數學取樣**：提供球形、柳枝、環形等向量生成函式，安全地以程式化方式模擬煙火星粒分佈。
- **JSON 匯入／匯出**：使用者可將視覺參數保存為 JSON 檔案，便於分享與版本管理。
- **Demo Scene**：展示至少三種視覺模式（Peony、Willow、Ring），可在場景中按鍵切換。

## 🗂 專案結構
```
Assets/
  _Core/
    FireworkRecipe.cs
    FireworkLauncher.cs
    FireworkBurst.cs
    FireworkSpawner.cs
    FireworkPresetExporter.cs
    Editor/
      FireworkRecipeEditor.cs
  Prefabs/
    PF_FireworkLauncher.prefab
  Scenes/
    Demo.unity
ProjectSettings/
README.md
```

## 🛠 環境需求
- Unity 2022.3 LTS（或相容版本）
- Universal Render Pipeline（URP）套件

## 🚀 快速開始
1. 以 Unity Hub 匯入專案資料夾並使用 Unity 2022.3 LTS（URP）開啟。
2. 在 `Assets/Scenes/Demo.unity` 中開啟 Demo 場景。
3. 播放場景後，使用 `FireworkSpawner` 監控物件，以空白鍵或等待自動輪播即可觀看三種煙火模式。

## 🎨 建立自訂配方
1. 於 Project 視窗中右鍵 → **Create → PyroLab → Firework Recipe**。
2. 在 Inspector 中調整參數，例如：星火數量、爆炸模式、顏色漸層、大小曲線等。
3. 使用 Inspector 內建的 **Preview Burst** 按鈕，即可在編輯器模式快速預覽效果。
4. 透過 **Export JSON** / **Import JSON** 按鈕保存或載入視覺參數。

## 📦 JSON 匯出／匯入
- 匯出：於 Inspector 點擊 **Export JSON**，指定路徑後即可產生純文字 JSON。
- 匯入：於 Inspector 點擊 **Import JSON**，選擇先前匯出的檔案即可覆寫 ScriptableObject 參數。

## 🧪 測試（建議）
專案提供 C# 腳本與粒子設定，建議在合併 PR 前於本地使用 Unity Editor 測試以下項目：
- Demo 場景中是否能播放三種煙火模式。
- FireworkRecipe ScriptableObject 是否能正確匯入／匯出 JSON。
- Inspector 的 Preview 功能是否正常觸發粒子播放。

## 🤝 貢獻
歡迎以功能分支的方式提交 Pull Request。請遵守安全方針：
- 僅分享視覺模擬相關程式碼與素材。
- 不得加入任何真實世界煙火製造、化學、物理配方或危險操作指南。

## 📄 授權
MIT License
