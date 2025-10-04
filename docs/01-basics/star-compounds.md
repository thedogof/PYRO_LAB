
# 星粒與藥珠（Star Compounds）

本文件將真實花火知識轉為遊戲參數（顏色、亮度、燃燒時間、拖尾、延時…），避免化學比例或危險步驟。
This doc converts real-world firework knowledge into game parameters (color, brightness, burn time, trail, delay), avoiding formulas or hazardous procedures.

---

## 1. 基本概念

- **星粒（Stars）**：花火玉內部會發光的顆粒，決定顏色、亮度、燃燒時間與開花的質感；爆開後被拋散、點燃而發光。這是日式花火視覺的核心構成之一。
   - 參考：tsuchiura-pr.jp
- **藥珠 / Beads（效果珠/EffectBead）**：在星之外，用於達成延時二段、再亮、換色或特殊閃爍等效果的小單元；在我們的系統中以純視覺事件呈現（非化學）。

> 抽象化（遊戲）：我們只保留「色彩、燃燒與時間」等可視參數，不涉及配方或操作流程。
> 參考：kosodatemap.gakken.jp

---

## 2. 真實知識（僅作文化理解）

- **燃料劑**：讓星粒能持續燃燒。
- **氧化劑**：幫助燃燒更穩定。
- **顏色劑**：銅＝藍綠、鋇＝綠、鋰＝紅、鈉＝黃。
- **黏著劑**：將粉末壓製成球狀或圓筒狀。

---

## 3. 遊戲資料模型（抽象化）

- `StarType`（ScriptableObject）
   - colorGradient（HDR 漸層 / HDR Gradient）
   - burnDuration（燃燒時間 / Burn time）
   - brightness（亮度 / Brightness）
   - trailLength（拖尾長度 / Trail length）
   - twinkle / strobe（閃爍參數 / Twinkle & strobe）
   - drag, gravity（純視覺阻力/重力感 / Visual drag & gravity）
- `EffectBead`（ScriptableObject）
   - delay（延時秒數，做 Poka / 二段爆 / Delay for re-ignite）
   - colorOverride（再染色 / Color override）
   - behavior（"poka"、"split"… 純視覺標記 / Visual behavior tag）

> 註：在真實世界，星的色彩表現與結構很複雜；我們只把它抽象為「顏色 × 時間 × 尾跡 × 強度」這些安全參數。關於顏色改變可由結構與時間差造成，這可安全地表述為「多段顏色層、依序點亮」。
> 參考：kosodatemap.gakken.jp


---

## 4. 星粒參數對應視覺（安全概念）

- **顏色 / Color**：在公開科普中，星的顏色來自不同色彩劑所產生的火焰色；我們遊戲只呈現效果（例如：藍、綠、紅、金等），不呈現配方。
   - 參考：kosodatemap.gakken.jp
- **燃燒時間 / Burn Time**：影響殘光、拖尾是否拉長（如柳狀視覺）。
- **亮度 / Brightness**：影響發光強度與後期泛光。
- **拖尾 / Trail**：長尾可塑造柳（Willow）或金雨感。
- **閃爍 / Twinkle/Strobe**：具節奏的亮滅效果，常見於銀白或特殊星。

> 「牡丹（Peony）不帶尾、菊（Chrysanthemum）多半帶尾」是常見的外觀區分；我們以 Trail 參數區別兩者的視覺差異。
> 參考：japan-fireworks.com

---

## 5. 星的形態與排列（文化理解）

- **星的形態**：如切り星（Cut stars）、掛け星（Rolled stars）等，為傳統師傅描述星顆粒整形與增長的術語。僅理解其文化脈絡，遊戲裡簡化為參數差異（燃燒時間、亮度、拖尾）。
   - 參考：uchiage-hanabi.com
- **排列與層次**：
   - 多圈色帶（Rings）：真實殼內可將不同顏色的星依環狀排列，形成雙重、三重環的視覺；我們提供多層 Layer 與座位（Seats）來對應。
      - 參考：富士フイルムスクエア
   - 芯入（Pistil）：在中心放置另一組星，形成花心。
   - 八重蕊（Layered Shell）：多層外殼與多重時序，對應到我們的 Layer + TimingTrack（微延時）。

> 遊戲實作：用 ShellLayout 的座位（θ, φ, radiusMul）點位來安排不同 StarType；透過 EffectBead.delay 做微延時，就能得到芯/外環/多圈層次。

---

## 6. 特殊效果（安全抽象）

- **Poka（延時再亮）**：部分星在爆開後延遲一瞬才發亮或再亮，呈現可愛的「撲通」節奏；在我們系統裡以 EffectBead.delay 表現（純視覺）。
- **二段 / 三段爆**：以 TimingTrack 定出子事件（0~1 正規化時間軸），對應外層 → 內層、主爆 → 副爆。

> 舞台式/劇場花火常以音樂同步與鏡位構圖提升觀演體驗，這在我們遊戲中可做 BPM 對齊的觸發點。
> 參考：Mr. Motegi

---

## 7. 遊戲範例模板（Preset Examples）

下列為純視覺參數樣式模板，方便你在 Editor 內快速建立 StarType / EffectBead。

### A. 紅牡丹（Red Peony）
- StarType：colorGradient=RedHDR，burnDuration=1.8s，brightness=2.2，trailLength=0（無尾）
- 用途：基本球形、顏色純淨。
- 備註：牡丹特性＝不帶尾（Trail 低）。
   - 參考：斉藤商店 茨城県結城市

### B. 金柳（Gold Willow）
- StarType：colorGradient=GoldHDR，burnDuration=2.6s，brightness=2.5，trailLength=長，drag↑，gravity↓（下垂感）
- 用途：滿幕金雨、下垂。
- 備註：柳＝長尾、重力感明顯。

### C. 菊芯入（Chrysanthemum + Pistil）
- 外層：菊（有尾）trailLength=中；內層：白芯 brightness=高，delay=微
- 用途：外放射 + 內花心；對比清楚。
   - 參考：japan-fireworks.com

### D. Poka（延時再亮）
- EffectBead：delay=0.15~0.35s，colorOverride=保留或微調，behavior="poka"
- 用途：二段節奏、可愛的「撲通」。

---

### 其他範例

1. **紅牡丹 Red Peony**  
    - Beads: Red, short burn  
    - Core: 中等爆速  

2. **綠柳 Green Willow**  
    - Beads: Green, 長燃燒時間  
    - Modifier: 高拖尾 + Gravity  

3. **藍環 Blue Ring**  
    - Beads: Blue, 中等燃燒  
    - Pattern: Ring Geometry  

---

## 8. 編輯器與匯出流程

- **ShellLayout**：用 UV 畫布擺 Seats（θ, φ, 半徑倍率），綁定 StarType/EffectBead。
- **轉譯**：ShellLayout → FireworkRecipe（群組成 Layer；方向向量 → 粒子初速）。
- **儲存**：匯出 JSON（無敏感資訊，僅顏色/時間/視覺行為），可安全分享與復現。

---

## 9. 安全註記（Safety Note）

以上內容為視覺模擬概念，僅對應遊戲參數；不提供化學比例、混合法或操作步驟。
顏色、延時、拖尾、亮度等，在本專案中皆為純視覺控制。
請勿嘗試任何真實煙火製作或實驗。

---

## 10. 參考（敘述性來源 / 文化脈絡）

- 花火玉的結構與星／割薬等部位的高層解說（自治體與解說頁面）。
   - tsuchiura-pr.jp
- 多重環與星的排列示意（攝影解說頁，描述殼內環狀多圈概念）。
   - 富士フイルムスクエア
- 顏色與多段色的概念（科普：星內不同成分層次造成時間序的顏色變化；實際配方不公開）。
   - kosodatemap.gakken.jp
- 牡丹/菊的外觀差異（是否帶尾）。
   - japan-fireworks.com
- 星的成形名詞（切り星／掛け星）作為文化理解（不涉配方）。
   - uchiage-hanabi.com
- 劇場型花火的觀演設計（鏡位與音樂同步靈感）。
   - Mr. Motegi

---

## ⚠️ 安全提醒 (Safety Disclaimer)
- 所有內容皆為 **純視覺模擬**。  
- 無任何化學比例、無危險配方。  
- 僅供遊戲與教育用途，請勿嘗試真實製作煙火。  




## ⚠️ 安全提醒 (Safety Disclaimer)
- 所有內容皆為 **純視覺模擬**。  
- 無任何化學比例、無危險配方。  
- 僅供遊戲與教育用途，請勿嘗試真實製作煙火。  
