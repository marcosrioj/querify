#!/usr/bin/env bash
set -euo pipefail

OLD_WORD="${1:-}"
NEW_WORD="${2:-}"
TARGET_DIR="${3:-.}"
APPLY="${4:-false}"

if [[ -z "$OLD_WORD" || -z "$NEW_WORD" ]]; then
  echo "Uso:"
  echo "  ./rename-all.sh OLD_WORD NEW_WORD [TARGET_DIR] [apply]"
  echo ""
  echo "Dry-run:"
  echo "  ./rename-all.sh Querify QnA ."
  echo ""
  echo "Aplicar de verdade:"
  echo "  ./rename-all.sh Querify QnA . true"
  exit 1
fi

if [[ ! -d "$TARGET_DIR" ]]; then
  echo "Erro: diretório não encontrado: $TARGET_DIR"
  exit 1
fi

echo "Target: $TARGET_DIR"
echo "From:   $OLD_WORD"
echo "To:     $NEW_WORD"
echo "Mode:   $([[ "$APPLY" == "true" ]] && echo "APPLY" || echo "DRY-RUN")"
echo ""

SKIP_DIRS=(
  -path "*/.git/*" -o
  -path "*/node_modules/*" -o
  -path "*/bin/*" -o
  -path "*/obj/*" -o
  -path "*/dist/*" -o
  -path "*/build/*" -o
  -path "*/.next/*" -o
  -path "*/.nuxt/*" -o
  -path "*/.cache/*" -o
  -path "*/.idea/*" -o
  -path "*/.vscode/*"
)

escape_perl_replacement() {
  printf '%s' "$1" | sed 's/[\/&]/\\&/g'
}

OLD_ESCAPED="$(printf '%s' "$OLD_WORD" | perl -0777 -pe 's/([\\\/\[\]\(\)\{\}\.\+\*\?\^\$\|])/\\$1/g')"
NEW_ESCAPED="$(escape_perl_replacement "$NEW_WORD")"

echo "1) Alterando conteúdo dos arquivos..."
echo ""

find "$TARGET_DIR" \
  \( "${SKIP_DIRS[@]}" \) -prune -o \
  -type f -print0 |
while IFS= read -r -d '' file; do
  # Ignora binários
  if grep -Iq . "$file"; then
    if grep -qF "$OLD_WORD" "$file"; then
      echo "[content] $file"

      if [[ "$APPLY" == "true" ]]; then
        perl -pi -e "s/$OLD_ESCAPED/$NEW_ESCAPED/g" "$file"
      fi
    fi
  fi
done

echo ""
echo "2) Renomeando arquivos e pastas..."
echo ""

find "$TARGET_DIR" \
  \( "${SKIP_DIRS[@]}" \) -prune -o \
  -depth -name "*$OLD_WORD*" -print0 |
while IFS= read -r -d '' path; do
  dir="$(dirname "$path")"
  base="$(basename "$path")"
  new_base="${base//$OLD_WORD/$NEW_WORD}"
  new_path="$dir/$new_base"

  if [[ "$path" != "$new_path" ]]; then
    echo "[rename] $path -> $new_path"

    if [[ "$APPLY" == "true" ]]; then
      mv "$path" "$new_path"
    fi
  fi
done

echo ""

if [[ "$APPLY" == "true" ]]; then
  echo "Concluído."
else
  echo "Dry-run finalizado. Nada foi alterado."
  echo "Para aplicar:"
  echo "  ./rename-all.sh \"$OLD_WORD\" \"$NEW_WORD\" \"$TARGET_DIR\" true"
fi