#!/usr/bin/env bash
set -e

mode="${1:-basics}"

open_files() {
  if command -v code >/dev/null 2>&1; then
    for f in "$@"; do
      if [ -f "$f" ]; then
        code -r "$f"
      else
        echo "[skip] $f (not found)"
      fi
    done
  else
    echo "VS Code CLI 'code' 不在路徑，請手動開啟以下檔案："
    for f in "$@"; do
      [ -f "$f" ] && echo " - $f"
    done
  fi
}

case "$mode" in
  basics)
    open_files docs/01-basics/shell-structure.md \
               docs/01-basics/star-arrangement.md \
               docs/01-basics/star-compounds.md
    ;;
  timing)
    open_files docs/02-timing/fuse-timing.md \
               docs/01-basics/shell-structure.md
    ;;
  effects)
    open_files docs/03-effects/burst-patterns.md \
               docs/01-basics/star-arrangement.md
    ;;
  culture)
    open_files docs/90-culture/culture-and-events.md
    ;;
  all)
    open_files docs/README.md \
               docs/01-basics/shell-structure.md \
               docs/01-basics/star-arrangement.md \
               docs/01-basics/star-compounds.md \
               docs/02-timing/fuse-timing.md \
               docs/03-effects/burst-patterns.md \
               docs/04-systems/workshop-economy.md \
               docs/80-research/vfx-research-summary.md \
               docs/90-culture/culture-and-events.md \
               docs/98-roadmap/phase2-expansion-plan.md
    ;;
  *)
    echo "用法：scripts/open.sh [basics|timing|effects|culture|all]"
    exit 1
    ;;
esac
