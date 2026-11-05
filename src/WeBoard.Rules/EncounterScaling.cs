using System;

namespace WeBoard.Rules
{
    /// <summary>
    /// Skala pertarungan menurut jumlah pemain (1 sampai 8), sesuai dokumen desain.
    /// Pada 1-4 pemain party diisi pendamping AI sampai 4, jadi tabelnya sama.
    /// </summary>
    public static class EncounterScaling
    {
        public const int MinPlayers = 1;
        public const int MaxPlayers = 8;

        /// <summary>Rentang jumlah musuh biasa per pertarungan.</summary>
        public static (int Min, int Max) EnemyCountRange(int players)
        {
            Validate(players);
            if (players <= 4) return (4, 6);
            if (players <= 6) return (6, 9);
            return (8, 12);
        }

        /// <summary>Rentang sisi papan heks (jumlah heks per sisi).</summary>
        public static (int Min, int Max) BoardSizeRange(int players)
        {
            Validate(players);
            if (players <= 4) return (10, 12);
            if (players <= 6) return (12, 16);
            return (14, 20);
        }

        /// <summary>HP bos dalam persen dari HP dasar: +25% per pemain di atas 4.</summary>
        public static int BossHpPercent(int players)
        {
            Validate(players);
            return 100 + 25 * Math.Max(0, players - 4);
        }

        /// <summary>Aksi tambahan bos per ronde (mulai 5 pemain).</summary>
        public static int BossExtraActionsPerRound(int players)
        {
            Validate(players);
            return players >= 5 ? 1 : 0;
        }

        /// <summary>Bos memanggil anak buah setiap N ronde; 0 berarti tidak memanggil (mulai 7 pemain).</summary>
        public static int BossSummonEveryRounds(int players)
        {
            Validate(players);
            return players >= 7 ? 2 : 0;
        }

        /// <summary>Mulai 5 pemain pertarungan memakai fase pemain dan fase musuh.</summary>
        public static bool UsesSquadInitiative(int players)
        {
            Validate(players);
            return players >= 5;
        }

        private static void Validate(int players)
        {
            if (players < MinPlayers || players > MaxPlayers)
            {
                throw new ArgumentOutOfRangeException(nameof(players), "Jumlah pemain harus 1 sampai 8.");
            }
        }
    }
}
