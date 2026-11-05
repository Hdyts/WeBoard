using System;

namespace WeBoard.Rules
{
    /// <summary>
    /// Pengacak deterministik (SplitMix64). Dengan seed yang sama, server dan klien
    /// menghasilkan urutan angka yang sama, apa pun runtime-nya (Unity, .NET, Mono).
    /// </summary>
    public sealed class SeededRandom
    {
        private ulong _state;

        public SeededRandom(ulong seed)
        {
            _state = seed;
        }

        public ulong NextUInt64()
        {
            unchecked
            {
                _state += 0x9E3779B97F4A7C15UL;
                ulong z = _state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        /// <summary>Lempar dadu bersisi <paramref name="sides"/>; hasil 1 sampai sides (inklusif).</summary>
        public int Roll(int sides)
        {
            if (sides < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(sides), "Dadu minimal punya 2 sisi.");
            }

            ulong s = (ulong)sides;
            // Buang nilai di ujung rentang agar tidak ada bias modulo.
            ulong limit = ulong.MaxValue - (ulong.MaxValue % s);
            ulong r;
            do
            {
                r = NextUInt64();
            }
            while (r >= limit);

            return (int)(r % s) + 1;
        }

        public int RollD20()
        {
            return Roll(20);
        }
    }
}
