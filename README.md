

<p align="center">
  <img src="https://sdmntprnorthcentralus.oaiusercontent.com/files/00000000-0d04-622f-abf4-4944cfac3512/raw?se=2025-10-03T12%3A18%3A41Z&sp=r&sv=2024-08-04&sr=b&scid=8b969da3-2112-5aeb-b33a-fcbf794ace38&skoid=03727f49-62d3-42ac-8350-1c0e6559d238&sktid=a48cca56-e6da-484e-a814-9c849652bcb3&skt=2025-10-03T00%3A23%3A51Z&ske=2025-10-04T00%3A23%3A51Z&sks=b&skv=2024-08-04&sig=A/VpjEkuWhq69vvZWaoKD4N%2BzJNdQdQbpdGtpgVfKX4%3D" alt="PYRO_LAB Banner" width="900"/>
</p>

# PYRO_LAB 🎆

A Unity-based prototype for **fireworks simulation**.  
The goal is to experiment with **fireworks recipes** and test ideas for future game mechanics.  

以 Unity 製作的 **煙火模擬原型專案**。  
目標是透過不同的 **煙火配方** 進行實驗，並測試未來遊戲機制的可行性。  

---

## 🚀 Features (MVP) | 功能特色
- **ScriptableObject recipes** (lift power, fuse time, star count, color, pattern)  
  **煙火配方 ScriptableObject**（發射藥、引信時間、星火數量、顏色、圖樣）
- **Particle System** for launching & bursting  
  使用 **粒子系統** 進行發射與爆炸
- **UI controls** (sliders, dropdowns, fire button) to tweak fireworks in real time  
  **介面控制**（滑桿、下拉選單、按鈕）即時調整效果
- **GitHub version control** for safe iteration and branching  
  使用 **GitHub 版控** 安全管理版本與分支

---

## 🗂 Project Structure | 專案結構
Assets/
_Core/ # Scripts & ScriptableObjects | 核心腳本與配方
_VFX/ # Particle systems / VFX Graph | 粒子與視覺特效
_UI/ # Canvas, panels, controls | UI 介面
Scenes/ # Demo scenes | 場景


---

## ⚙️ Requirements | 系統需求
- Unity 2022.3+ (URP recommended)  
- Git LFS (for large assets)  
- 建議使用 Unity 2022.3+ (推薦 URP)  
- Git LFS（管理大型素材檔案）  

---

## 📝 Roadmap | 開發規劃
- [ ] Improve patterns (Ring, Heart, Willow, etc.)  
      改進圖樣（環形、愛心、柳枝等）  
- [ ] Add VFX Graph for large star counts  
      使用 VFX Graph 支援更多星火數量  
- [ ] JSON import/export for recipes  
      支援 JSON 匯入/匯出煙火配方  
- [ ] WebGL build for sharing online  
      建立 WebGL 版本方便線上分享  

---

## 📄 License | 授權
MIT License（或依需求選擇其他授權方式）
