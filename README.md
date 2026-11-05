# WeBoard

Board game RPG bergaya D&D untuk 1-8 pemain (co-op, tanpa PvP), tampil semi-3D seperti miniatur di atas meja, dimainkan langsung di browser. Semua teks berbahasa Indonesia. Proyek hobi tanpa model bisnis.

## Status

**Fase 0: fondasi aturan.** Baru ada pustaka aturan C# murni beserta tesnya. Belum ada proyek Unity.

## Struktur

| Folder | Isi |
| --- | --- |
| `src/WeBoard.Rules` | Aturan game dalam C# murni tanpa referensi ke Unity: pengacak ber-seed, modifier atribut, cek d20, koordinat heks, skala pertarungan 1-8 pemain |
| `tests/WeBoard.Rules.Tests` | Tes xUnit untuk pustaka aturan |
| `.github/workflows` | Tes otomatis di GitHub Actions |

Pustaka aturan sengaja dibuat bebas dari Unity agar server dan klien memakai logika yang sama, dan agar bisa diuji tanpa membuka Unity Editor.

## Menjalankan tes

Dengan .NET 8 SDK terpasang:

```
dotnet test tests/WeBoard.Rules.Tests
```

Tanpa .NET di komputer, tes berjalan otomatis di GitHub Actions setiap push ke `main`.

## Langkah berikutnya

1. Fase 1: proyek Unity dan build WebGL di cloud (game-ci).
2. Prototipe jaringan: room 1-8 klien lewat WebSocket.
3. Vertical slice: prolog dan bab 1.
