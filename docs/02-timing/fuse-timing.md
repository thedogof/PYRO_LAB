# 導火與時序 (Fuse & Timing)

Fuse 負責決定何時引爆，Timing 則控制多層煙火的先後。

## 要點
- **延遲引信**：控制外層與內層的時間差。
- **二段爆**：先小爆再大爆，形成層次感。

## 遊戲轉換
- `Fuse.cs`：`delayTime / stability`。
- `TimingTrack.cs`：用事件 (0~1 正規化時間) 觸發第二層或 Modifier。
