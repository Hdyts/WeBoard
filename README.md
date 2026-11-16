# WeBoard

Board game RPG bergaya D&D untuk 1-8 pemain (co-op, tanpa PvP), tampil semi-3D seperti miniatur di atas meja, dimainkan langsung di browser. Semua teks berbahasa Indonesia. Proyek hobi tanpa model bisnis.

## Status

**Fase 1: kerangka Unity dan build WebGL di cloud.** Pustaka aturan C# murni (Fase 0) sudah ada beserta tesnya. Proyek Unity 6 berisi uji awal: diorama papan heks, kamera snap 90 derajat, dan lemparan d20 dari pustaka aturan. Build WebGL berjalan di GitHub Actions, tanpa Unity Editor di komputer mana pun.

## Struktur

| Folder | Isi |
| --- | --- |
| `src/WeBoard.Rules` | Aturan game dalam C# murni tanpa referensi ke Unity: pengacak ber-seed, modifier atribut, cek d20, koordinat heks, skala pertarungan 1-8 pemain |
| `tests/WeBoard.Rules.Tests` | Tes xUnit untuk pustaka aturan |
| `Assets`, `Packages`, `ProjectSettings` | Proyek Unity 6 (6000.3.28f1). `Assets/Editor/BuildScript.cs` adalah titik masuk build; scene awal dibuat lewat kode saat build |
| `tools/sync-rules.sh` | Menyalin `src/WeBoard.Rules` ke `Assets/Scripts/Rules` (salinan ini tidak disimpan di git) |
| `.github/workflows` | Tes aturan dan build WebGL di GitHub Actions |

Pustaka aturan sengaja dibuat bebas dari Unity agar server dan klien memakai logika yang sama, dan agar bisa diuji tanpa membuka Unity Editor.

## Menjalankan tes

Dengan .NET 8 SDK terpasang:

```
dotnet test tests/WeBoard.Rules.Tests
```

Tanpa .NET di komputer, tes berjalan otomatis di GitHub Actions setiap push ke `main`.

## Build WebGL

Build berjalan otomatis di GitHub Actions (`Build WebGL`) dan hasilnya diunggah sebagai artifact `WeBoard-WebGL` (unduh dari halaman run). Butuh tiga secret repo: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`.

## Langkah berikutnya

1. Memastikan build WebGL pertama lulus dan jalan di browser.
2. Prototipe jaringan: room 1-8 klien lewat WebSocket.
3. Vertical slice: prolog dan bab 1.
