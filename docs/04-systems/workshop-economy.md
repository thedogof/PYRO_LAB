# PYRO_LAB Workshop 經濟與材料指南

> **安全提醒：本文僅涵蓋遊戲／視覺模擬用途的抽象參數。嚴禁將內容套用於真實煙火製造、化學反應或任何危險操作。**

## Tier 定義與解鎖條件

| Tier | 顯示名稱 | 解鎖條件 (任一達成) | 代表特色 |
| --- | --- | --- | --- |
| T1 | Starter | 預設可用 | 單層 Peony、暖金色系、快速上手 |
| T2 | Advanced | 視覺評價 ≥ 120 或 資金 ≥ 1,000 | 增加柳尾/環形、雙色星點與較穩定 Fuse |
| T3 | Artisan | 視覺評價 ≥ 300 或 資金 ≥ 3,000 | 多層 Layered + Pistil、紅金漸變、銀色頻閃 |

經濟換算：`獲得資金 = 視覺評分 × EconomyBalance.scoreToCurrency`（預設 3）。

## 材料成本速覽

| 類別 | T1 | T2 | T3 |
| --- | --- | --- | --- |
| Paper | 厚度 0.2 / Wrap 0.4 / 層數 6 — Cost 5 | 厚度 0.3 / Wrap 0.7 / 層數 10 — Cost 9 | 厚度 0.45 / Wrap 0.9 / 層數 14 — Cost 15 |
| Fuse | Delay 1.5s / Stability 0.6 — Cost 4 | Delay 2.5s / Stability 0.8 — Cost 7 | Delay 3.5s / Stability 0.95 — Cost 12 |
| Shell | Hardness 0.3 / Mass 0.2 / Tightness 0.4 — Cost 8 | Hardness 0.55 / Mass 0.3 / Tightness 0.7 — Cost 14 | Hardness 0.8 / Mass 0.4 / Tightness 0.9 — Cost 24 |
| StarCompound | Warm Gold Intensity 2.0 / Life 1.5 / Trail 0.2 — Cost 10 | Willow Gold Intensity 2.5 / Life 2.5 / Trail 0.7 — Cost 18<br>Shimmer Blue Intensity 2.3 / Life 2.0 / Twinkle 0.5 — Cost 18 | Deep Red → Gold Shift Intensity 3.2 / Life 2.5 / Trail 0.4 — Cost 28<br>Silver Strobe Intensity 3.0 / Strobe 10Hz — Cost 28 |
| BurstCore | Peony Star 320 / Speed 8~12 / Spread 0.05 — Cost 20 | Willow Star 480 / Speed 7~10 / Spread 0.04 — Cost 28 | Layered + Pistil Star 720 / Speed 9~13 / Secondary 0.55 — Cost 45 |
| TimingTrack | Default 無事件 — Cost 0 | Default 無事件 — Cost 0 | Default 無事件 + 自動 Pistil 二段 — Cost 0 |

> `EconomyBalance.asset` 已預先將上述素材歸類於對應 Tier，可在 Project 視窗中擴充或替換。

## 預估得分（視覺複雜度）

`WorkshopManager.EstimateVisualScore()` 以以下加權計算預估值（僅供遊戲使用）：

- Paper：層數 ×0.6、包覆張力 ×12、厚度 ×8。
- StarCompound：HDR 強度總和 ×6、壽命 ×4、Twinkle ×6、Strobe 頻率 ×0.8、Trail ×5。
- BurstCore：星點數 ×0.05、Spread ×40、Secondary 另加 35。
- Shell：三項平均乘 20。
- Fuse：穩定度 ×12。

計算結果範圍經過 `Clamp(10, 500)`，並僅作為工作坊 UI 的提示值。

## 匯出／匯入 JSON

Demo_Workshop 場景中的 `WorkshopManager` 會呼叫 `FireworkRecipe.ExportToJson()` / `ImportFromJson()`。匯出的 JSON 結構包含：

- 全域尺寸、預期高度、Fuse 時間與新加入的 `launchVariance`、`burstSymmetry`、`angularSpread`、`spreadJitter`、`gravityFactor`、`trailLengthScale`。
- 每個 FireworkLayer 的 Pattern、星數、速度範圍、顏色漸層、壽命、半徑與 Modifier 描述。
- TimingTrack 事件（`SubBurst`、`Split`、`ColorShift`、`StrobeToggle`）。

## 擴充建議

1. 依照 Tier 分組 UI，拖拉 ScriptableObject 即可建立新材料。
2. 若要加入額外 Modifier，先在 `Assets/_Core/Data/Modifiers/` 擴充腳本，再於 Workshop Builder 中指定對應欄位。
3. 可以新增 `WorkshopRecipePreset` asset 來保存常用組合，並於 `WorkshopManager` 的 `presets` 陣列中設定排序。

## 再次強調

> PYRO_LAB 全部內容僅服務於視覺模擬與遊戲開發。請勿將任何資料轉作現實世界煙火或爆裂物製造用途。
