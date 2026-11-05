namespace WeBoard.Rules
{
    /// <summary>Tingkat kesulitan (DC) standar.</summary>
    public static class DifficultyClass
    {
        public const int Easy = 8;
        public const int Medium = 12;
        public const int Hard = 16;
        public const int Heroic = 20;
    }

    public static class AbilityMath
    {
        /// <summary>
        /// Modifier atribut = (nilai - 10) / 2, dibulatkan ke bawah.
        /// Geser bit ke kanan pada int negatif membulatkan ke bawah, jadi 9 menghasilkan -1.
        /// </summary>
        public static int Modifier(int score)
        {
            return (score - 10) >> 1;
        }
    }
}
