using System;

namespace WeBoard.Rules
{
    public enum CheckOutcome
    {
        CriticalFailure,
        Failure,
        Success,
        CriticalSuccess
    }

    public readonly struct CheckResult
    {
        public int NaturalRoll { get; }
        public int Modifier { get; }
        public int Total { get; }
        public int Dc { get; }
        public CheckOutcome Outcome { get; }

        public bool Succeeded
        {
            get { return Outcome == CheckOutcome.Success || Outcome == CheckOutcome.CriticalSuccess; }
        }

        public CheckResult(int naturalRoll, int modifier, int dc, CheckOutcome outcome)
        {
            NaturalRoll = naturalRoll;
            Modifier = modifier;
            Total = naturalRoll + modifier;
            Dc = dc;
            Outcome = outcome;
        }
    }

    public static class CheckResolver
    {
        /// <summary>
        /// Aturan inti: d20 + modifier harus mencapai atau melewati DC.
        /// 20 alami selalu hasil luar biasa; 1 alami selalu kegagalan dramatis.
        /// </summary>
        public static CheckResult Resolve(int naturalRoll, int modifier, int dc)
        {
            if (naturalRoll < 1 || naturalRoll > 20)
            {
                throw new ArgumentOutOfRangeException(nameof(naturalRoll), "Hasil d20 harus 1 sampai 20.");
            }

            CheckOutcome outcome;
            if (naturalRoll == 20)
            {
                outcome = CheckOutcome.CriticalSuccess;
            }
            else if (naturalRoll == 1)
            {
                outcome = CheckOutcome.CriticalFailure;
            }
            else
            {
                outcome = (naturalRoll + modifier) >= dc ? CheckOutcome.Success : CheckOutcome.Failure;
            }

            return new CheckResult(naturalRoll, modifier, dc, outcome);
        }

        public static CheckResult Roll(SeededRandom rng, int modifier, int dc)
        {
            return Resolve(rng.RollD20(), modifier, dc);
        }
    }
}
