#!/usr/bin/env bash
set -euo pipefail

# collect-and-convert.sh
# Collecte une trace .nettrace avec dotnet-trace, convertit en format speedscope
# Usage:
#   ./scripts/collect-and-convert.sh [-p PID] [-n NAME] [--gcdump] [--open]
# Exemples:
#   ./scripts/collect-and-convert.sh               # cherche le process 'tinyidp'
#   ./scripts/collect-and-convert.sh -p 12345 --open

PID=""
NAME="tinyidp"
DO_GCDUMP=0
DO_OPEN=0

print_usage(){
  cat <<EOF
Usage: $0 [-p PID] [-n NAME] [--gcdump] [--open]
  -p PID     PID du process dotnet à profiler (par défaut: cherche 'tinyidp')
  -n NAME    préfixe de nom de sortie (défaut: tinyidp)
  --gcdump   collecte aussi un heap dump via dotnet-gcdump
  --open     ouvre le fichier speedscope (macOS `open`) si possible
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -p) PID="$2"; shift 2 ;;
    -n) NAME="$2"; shift 2 ;;
    --gcdump) DO_GCDUMP=1; shift ;;
    --open) DO_OPEN=1; shift ;;
    -h|--help) print_usage; exit 0 ;;
    *) echo "Unknown arg: $1"; print_usage; exit 1 ;;
  esac
done

if [[ -z "$PID" ]]; then
  echo "Aucun PID fourni, recherche d'un process contenant 'tinyidp'..."
  PID=$(pgrep -f tinyidp | head -n1 || true)
  if [[ -z "$PID" ]]; then
    echo "Erreur: impossible de trouver le process 'tinyidp'. Passez -p <PID> manuellement." >&2
    exit 2
  fi
fi

TIMESTAMP=$(date +%Y%m%d-%H%M%S)
OUT_DIR="./traces"
mkdir -p "$OUT_DIR"
NETTRACE="$OUT_DIR/${NAME}-${TIMESTAMP}.nettrace"
SPEEDSCOPE="$OUT_DIR/${NAME}-${TIMESTAMP}.speedscope.json"

echo "Collecte de trace pour PID=$PID -> $NETTRACE"

dotnet-trace collect -p "$PID" -o "$NETTRACE"

echo "Conversion en format speedscope -> $SPEEDSCOPE"
# Certains anciens dotnet-trace utilisent l'ordre: convert -f speedscope -o out input
# On utilise la forme longue pour compatibilité
dotnet-trace convert --format speedscope -o "$SPEEDSCOPE" "$NETTRACE"

if [[ "$DO_GCDUMP" -eq 1 ]]; then
  GCDUMP="$OUT_DIR/${NAME}-${TIMESTAMP}.gcdump"
  echo "Collecte heap via dotnet-gcdump -> $GCDUMP"
  dotnet-gcdump collect -p "$PID" -o "$GCDUMP"
fi

echo "Fichiers générés:"
echo " - $NETTRACE"
echo " - $SPEEDSCOPE"
[[ "$DO_GCDUMP" -eq 1 ]] && echo " - $GCDUMP"

if [[ "$DO_OPEN" -eq 1 ]]; then
  echo "Tentative d'ouverture de $SPEEDSCOPE"
  if command -v open >/dev/null 2>&1; then
    open "$SPEEDSCOPE" || echo "Échec ouverture automatique — ouvrez $SPEEDSCOPE manuellement." >&2
  else
    echo "Commande 'open' introuvable — ouvrez $SPEEDSCOPE manuellement." >&2
  fi
fi

echo "Terminé. Vous pouvez glisser $SPEEDSCOPE sur https://www.speedscope.app pour analyser le flamegraph." 
