# PYRO_LAB 使用指南（2 分鐘上手）

## 0. 快速開始
make tour
make open.basics     # Shell Structure / Star Arrangement / Star Compounds
make open.timing     # Fuse Timing (+ Shell 對照)
make open.effects    # Burst Patterns (+ Arrangement 對照)
make open.all        # 常用全部

## 1. 檔案地圖（我該看哪個？）
- docs/01-basics/shell-structure.md：殼型、球度、頂點時機與評分
- docs/01-basics/star-arrangement.md：星的幾何排列＋QA校準工具
- docs/01-basics/star-compounds.md：視覺效果語彙（不涉配方）
- docs/02-timing/fuse-timing.md：apex_bias 與時機評分
- docs/03-effects/burst-patterns.md：綻放樣式行為模型
- 其他：systems / research / culture / roadmap

## 2. 可遊戲化參數速查（跨文件）
shell_type, caliber_mm, burst_strength, apex_bias,
ring_count, stars_per_ring, angular_jitter_deg, radial_ratio,
pistil_enabled, pistil_size_ratio, layout_type, color_sequence, …
（詳細對應：各文件表格內都有）

## 3. 推薦組合
make open.basics | make open.timing | make open.effects | make open.all

## 4. 小技巧
- 在 VS Code 全域搜尋參數名（如 apex_bias / ring_count）可跳到相關段落。
- Star Arrangement 章節附有角距統計、環密度熱圖、芯偏心度的 QA 公式。
