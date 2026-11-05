using System;
using System.Linq;
using WeBoard.Rules;
using Xunit;

namespace WeBoard.Rules.Tests
{
    public class SeededRandomTests
    {
        [Fact]
        public void SameSeedGivesSameSequence()
        {
            var a = new SeededRandom(12345);
            var b = new SeededRandom(12345);
            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(a.NextUInt64(), b.NextUInt64());
            }
        }

        [Fact]
        public void DifferentSeedsDiffer()
        {
            var a = new SeededRandom(1);
            var b = new SeededRandom(2);
            Assert.NotEqual(a.NextUInt64(), b.NextUInt64());
        }

        [Fact]
        public void SplitMix64MatchesPublishedVectorForSeedZero()
        {
            var rng = new SeededRandom(0);
            Assert.Equal(0xE220A8397B1DCDAFUL, rng.NextUInt64());
        }

        [Fact]
        public void D20StaysInRangeAndIsRoughlyUniform()
        {
            var rng = new SeededRandom(2026);
            var counts = new int[21];
            for (int i = 0; i < 10000; i++)
            {
                int r = rng.RollD20();
                Assert.InRange(r, 1, 20);
                counts[r]++;
            }
            for (int face = 1; face <= 20; face++)
            {
                Assert.InRange(counts[face], 350, 650);
            }
        }

        [Fact]
        public void RollRejectsDiceWithFewerThanTwoSides()
        {
            var rng = new SeededRandom(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => { rng.Roll(1); });
        }
    }

    public class AbilityMathTests
    {
        [Theory]
        [InlineData(10, 0)]
        [InlineData(11, 0)]
        [InlineData(12, 1)]
        [InlineData(13, 1)]
        [InlineData(14, 2)]
        [InlineData(20, 5)]
        [InlineData(9, -1)]
        [InlineData(8, -1)]
        [InlineData(7, -2)]
        [InlineData(1, -5)]
        public void ModifierRoundsDown(int score, int expected)
        {
            Assert.Equal(expected, AbilityMath.Modifier(score));
        }
    }

    public class CheckResolverTests
    {
        [Fact]
        public void TotalMeetingDcSucceeds()
        {
            var result = CheckResolver.Resolve(10, 2, DifficultyClass.Medium);
            Assert.Equal(12, result.Total);
            Assert.Equal(CheckOutcome.Success, result.Outcome);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public void TotalBelowDcFails()
        {
            var result = CheckResolver.Resolve(9, 2, DifficultyClass.Medium);
            Assert.Equal(CheckOutcome.Failure, result.Outcome);
            Assert.False(result.Succeeded);
        }

        [Fact]
        public void NaturalTwentyAlwaysCriticalSuccess()
        {
            var result = CheckResolver.Resolve(20, -5, DifficultyClass.Heroic);
            Assert.Equal(CheckOutcome.CriticalSuccess, result.Outcome);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public void NaturalOneAlwaysCriticalFailure()
        {
            var result = CheckResolver.Resolve(1, 10, DifficultyClass.Easy);
            Assert.Equal(CheckOutcome.CriticalFailure, result.Outcome);
            Assert.False(result.Succeeded);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(21)]
        public void InvalidNaturalRollThrows(int roll)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { CheckResolver.Resolve(roll, 0, 10); });
        }

        [Fact]
        public void RollIsDeterministicForSameSeed()
        {
            var a = CheckResolver.Roll(new SeededRandom(7), 3, DifficultyClass.Hard);
            var b = CheckResolver.Roll(new SeededRandom(7), 3, DifficultyClass.Hard);
            Assert.Equal(a.NaturalRoll, b.NaturalRoll);
            Assert.Equal(a.Outcome, b.Outcome);
        }
    }

    public class HexCoordTests
    {
        [Fact]
        public void DistanceMatchesKnownValues()
        {
            Assert.Equal(0, HexCoord.Distance(new HexCoord(0, 0), new HexCoord(0, 0)));
            Assert.Equal(1, HexCoord.Distance(new HexCoord(0, 0), new HexCoord(1, 0)));
            Assert.Equal(2, HexCoord.Distance(new HexCoord(0, 0), new HexCoord(2, -1)));
            Assert.Equal(3, HexCoord.Distance(new HexCoord(0, 0), new HexCoord(3, 0)));
        }

        [Fact]
        public void DistanceIsSymmetric()
        {
            var a = new HexCoord(3, -2);
            var b = new HexCoord(-1, 4);
            Assert.Equal(HexCoord.Distance(a, b), HexCoord.Distance(b, a));
        }

        [Fact]
        public void NeighborsAreSixDistinctCellsAtDistanceOne()
        {
            var center = new HexCoord(2, -3);
            var neighbors = center.Neighbors();
            Assert.Equal(6, neighbors.Length);
            Assert.Equal(6, neighbors.Distinct().Count());
            foreach (var n in neighbors)
            {
                Assert.Equal(1, HexCoord.Distance(center, n));
            }
        }

        [Fact]
        public void NeighborDirectionWrapsAround()
        {
            var c = new HexCoord(0, 0);
            Assert.Equal(c.Neighbor(0), c.Neighbor(6));
            Assert.Equal(c.Neighbor(5), c.Neighbor(-1));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 7)]
        [InlineData(2, 19)]
        [InlineData(4, 61)]
        public void WithinReturnsHexagonalNumberOfCells(int radius, int expectedCount)
        {
            var cells = new HexCoord(5, -2).Within(radius);
            Assert.Equal(expectedCount, cells.Count);
            Assert.Equal(expectedCount, cells.Distinct().Count());
            foreach (var cell in cells)
            {
                Assert.True(HexCoord.Distance(new HexCoord(5, -2), cell) <= radius);
            }
        }

        [Fact]
        public void EqualityWorks()
        {
            Assert.True(new HexCoord(1, 2) == new HexCoord(1, 2));
            Assert.True(new HexCoord(1, 2) != new HexCoord(2, 1));
        }
    }

    public class EncounterScalingTests
    {
        [Theory]
        [InlineData(1, 4, 6)]
        [InlineData(4, 4, 6)]
        [InlineData(5, 6, 9)]
        [InlineData(6, 6, 9)]
        [InlineData(7, 8, 12)]
        [InlineData(8, 8, 12)]
        public void EnemyCountFollowsDesignTable(int players, int min, int max)
        {
            var range = EncounterScaling.EnemyCountRange(players);
            Assert.Equal(min, range.Min);
            Assert.Equal(max, range.Max);
        }

        [Theory]
        [InlineData(4, 10, 12)]
        [InlineData(6, 12, 16)]
        [InlineData(8, 14, 20)]
        public void BoardSizeFollowsDesignTable(int players, int min, int max)
        {
            var range = EncounterScaling.BoardSizeRange(players);
            Assert.Equal(min, range.Min);
            Assert.Equal(max, range.Max);
        }

        [Theory]
        [InlineData(1, 100)]
        [InlineData(4, 100)]
        [InlineData(5, 125)]
        [InlineData(6, 150)]
        [InlineData(8, 200)]
        public void BossHpGrowsTwentyFivePercentPerPlayerAboveFour(int players, int percent)
        {
            Assert.Equal(percent, EncounterScaling.BossHpPercent(players));
        }

        [Fact]
        public void LargePartyRulesSwitchOnAtTheRightCounts()
        {
            Assert.False(EncounterScaling.UsesSquadInitiative(4));
            Assert.True(EncounterScaling.UsesSquadInitiative(5));
            Assert.Equal(0, EncounterScaling.BossExtraActionsPerRound(4));
            Assert.Equal(1, EncounterScaling.BossExtraActionsPerRound(5));
            Assert.Equal(0, EncounterScaling.BossSummonEveryRounds(6));
            Assert.Equal(2, EncounterScaling.BossSummonEveryRounds(7));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(9)]
        public void PlayerCountOutsideOneToEightThrows(int players)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { EncounterScaling.EnemyCountRange(players); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { EncounterScaling.BossHpPercent(players); });
        }
    }
}
