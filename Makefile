.PHONY: tour open.basics open.timing open.effects open.culture open.all

tour:
	@echo "✨ 快速導覽："
	@echo "  make open.basics    # 開啟基礎三件組"
	@echo "  make open.timing    # 開啟延時與頂點判斷＋結構對照"
	@echo "  make open.effects   # 開啟綻放樣式＋排列對照"
	@echo "  make open.culture   # 開啟文化與活動"
	@echo "  make open.all       # 一次開啟常用文件"

open.basics:
	scripts/open.sh basics

open.timing:
	scripts/open.sh timing

open.effects:
	scripts/open.sh effects

open.culture:
	scripts/open.sh culture

open.all:
	scripts/open.sh all
