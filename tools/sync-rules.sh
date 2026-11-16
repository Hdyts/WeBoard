#!/usr/bin/env bash
# Menyalin pustaka aturan C# murni (src/WeBoard.Rules) ke dalam proyek Unity.
# Satu sumber kebenaran tetap di src/, folder tujuan di Assets/ tidak disimpan di git.
set -euo pipefail

cd "$(dirname "$0")/.."

dest="Assets/Scripts/Rules"
rm -rf "$dest"
mkdir -p "$dest"
cp src/WeBoard.Rules/*.cs "$dest"/

echo "Aturan disalin ke $dest:"
ls -1 "$dest"
