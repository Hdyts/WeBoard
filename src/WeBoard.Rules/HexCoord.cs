using System;
using System.Collections.Generic;

namespace WeBoard.Rules
{
    /// <summary>Koordinat heksagonal aksial (q, r). Sumbu ketiga s = -q - r.</summary>
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        private static readonly HexCoord[] Directions =
        {
            new HexCoord(1, 0),
            new HexCoord(1, -1),
            new HexCoord(0, -1),
            new HexCoord(-1, 0),
            new HexCoord(-1, 1),
            new HexCoord(0, 1)
        };

        public int Q { get; }
        public int R { get; }

        public int S
        {
            get { return -Q - R; }
        }

        public HexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }

        /// <summary>Jarak dalam jumlah langkah heks.</summary>
        public static int Distance(HexCoord a, HexCoord b)
        {
            int dq = a.Q - b.Q;
            int dr = a.R - b.R;
            return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(dq + dr)) / 2;
        }

        /// <summary>Tetangga ke arah 0 sampai 5 (angka lain dibungkus).</summary>
        public HexCoord Neighbor(int direction)
        {
            HexCoord d = Directions[((direction % 6) + 6) % 6];
            return new HexCoord(Q + d.Q, R + d.R);
        }

        public HexCoord[] Neighbors()
        {
            var result = new HexCoord[6];
            for (int i = 0; i < 6; i++)
            {
                result[i] = Neighbor(i);
            }
            return result;
        }

        /// <summary>Semua heks dalam jarak paling jauh <paramref name="radius"/>, termasuk heks ini.</summary>
        public List<HexCoord> Within(int radius)
        {
            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            var result = new List<HexCoord>();
            for (int dq = -radius; dq <= radius; dq++)
            {
                int rMin = Math.Max(-radius, -dq - radius);
                int rMax = Math.Min(radius, -dq + radius);
                for (int dr = rMin; dr <= rMax; dr++)
                {
                    result.Add(new HexCoord(Q + dq, R + dr));
                }
            }
            return result;
        }

        public bool Equals(HexCoord other)
        {
            return Q == other.Q && R == other.R;
        }

        public override bool Equals(object obj)
        {
            return obj is HexCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Q * 397) ^ R;
            }
        }

        public static bool operator ==(HexCoord a, HexCoord b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(HexCoord a, HexCoord b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
            return "(" + Q + ", " + R + ")";
        }
    }
}
