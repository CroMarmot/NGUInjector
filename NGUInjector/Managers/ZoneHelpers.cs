﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NGUInjector.Main;

namespace NGUInjector.Managers
{
    public static class ZoneHelpers
    {
        public static Dictionary<int, string> ZoneList = new Dictionary<int, string>
        {
            {-1, "Safe Zone: Awakening Site"},
            {0, "Tutorial Zone"},
            {1, "Sewers"},
            {2, "Forest"},
            {3, "Cave of Many Things"},
            {4, "The Sky"},
            {5, "High Security Base"},
            {6, "Gordon Ramsay Bolton"},
            {7, "Clock Dimension"},
            {8, "Grand Corrupted Tree"},
            {9, "The 2D Universe"},
            {10, "Ancient Battlefield"},
            {11, "Jake From Accounting"},
            {12, "A Very Strange Place"},
            {13, "Mega Lands"},
            {14, "UUG THE UNMENTIONABLE"},
            {15, "The Beardverse"},
            {16, "WALDERP"},
            {17, "Badly Drawn World"},
            {18, "Boring-Ass Earth"},
            {19, "THE BEAST"},
            {20, "Chocolate World"},
            {21, "The Evilverse"},
            {22, "Pretty Pink Princess Land"},
            {23, "GREASY NERD"},
            {24, "Meta Land"},
            {25, "Interdimensional Party"},
            {26, "THE GODMOTHER"},
            {27, "Typo Zonw"},
            {28, "The Fad-Lands"},
            {29, "JRPGVille"},
            {30, "THE EXILE"},
            {31, "The Rad-lands"},
            {32, "Back To School"},
            {33, "The West World"},
            {34, "IT HUNGERS"},
            {35, "The Breadverse"},
            {36, "That 70's Zone"},
            {37, "The Halloweenies"},
            {38, "ROCK LOBSTER"},
            {39, "Construction Zone"},
            {40, "DUCK DUCK ZONE"},
            {41, "The Nether Regions"},
            {42, "AMALGAMATE"},
            {43, "7 Aethereal Seas"},
            {44, "TIPPI THE TUTORIAL MOUSE"},
            {45, "THE TRAITOR"}
        };

        public static int[] ZoneUnlocks = new int[] {
            0, 7, 17, 37, 48, 58, 58, 66, 66, 74, 82, 82, 90, 100, 100, 108, 116, 116, 124, 132, 137, // Normal
            359, 401, 426, 459, 467, 467, 475, 483, 491, 501,                                         // Evil
            727, 752, 777, 810, 818, 826, 834, 842, 850, 850, 871, 897, 902};                         // Sadistic

        private static readonly Character _character = Main.Character;

        public static readonly int[] TitanZones = { 6, 8, 11, 14, 16, 19, 23, 26, 30, 34, 38, 42, 44, 45 };
        public const int TitanCount = 14;

        private static Dictionary<int, TitanSnapshot> _titanDetails = new Dictionary<int, TitanSnapshot>();

        private static TitanSnapshotSummary _titanSnapshotSummary = new TitanSnapshotSummary();

        public static bool ZoneIsTitan(int zone) => Array.IndexOf(TitanZones, zone) >= 0;

        public static bool IsVersionedTitan(int titanIndex) => titanIndex >= 5 && titanIndex <= 11;

        public static bool ZoneIsWalderp(int zone) => zone == 16;

        public static bool ZoneIsBeast(int zone) => zone == 19;

        public static bool ZoneIsNerd(int zone) => zone == 23;

        public static bool ZoneIsGodmother(int zone) => zone == 26;

        public static bool ZoneIsExile(int zone) => zone == 30;

        public static bool ZoneIsItHungers(int zone) => zone == 34;

        public static bool TitanSpawningSoon(int titanIndex) => _character.buttons.adventure.IsInteractable() && IsTitanSpawningSoon(titanIndex);

        public static int TitanVersion(int titanIndex)
        {
            if (!IsVersionedTitan(titanIndex))
                return 1;

            return _character.adventure.GetFieldValue<Adventure, int>($"titan{titanIndex + 1}Version") + 1;
        }

        public static void SetTitanVersion(int titanIndex, int version)
        {
            if (TitanVersion(titanIndex) == version) return;

            if (!IsVersionedTitan(titanIndex) && version != 1)
                throw new IndexOutOfRangeException();

            _character.adventure.SetFieldValue($"titan{titanIndex + 1}Version", version - 1);
        }

        public static bool AutokillAvailable(int titanIndex, int version)
        {
            if (titanIndex >= 12)
                return false;

            if (titanIndex >= 5)
                return _character.adventureController.CallMethod<AdventureController, bool>($"autokillTitan{titanIndex + 1}V{version}Achieved");

            if (version != 1)
                return false;

            switch (titanIndex)
            {
                case 0:
                    return _character.totalAdvAttack() >= 3000.0 && _character.totalAdvDefense() >= 2500.0;
                case 1:
                    return _character.totalAdvAttack() >= 9000.0 && _character.totalAdvDefense() >= 7000.0;
                case 2:
                    return _character.totalAdvAttack() >= 25000.0 && _character.totalAdvDefense() >= 15000.0;
                case 3:
                    var itemCheck = _character.inventory.itemList.itemMaxxed[135];
                    return _character.totalAdvAttack() >= 8e5 && _character.totalAdvDefense() >= 4e5 && _character.totalAdvHPRegen() >= 14000.0 && itemCheck;
                case 4:
                    var killCheck = _character.adventure.boss5Kills >= 3;
                    return _character.totalAdvAttack() >= 1.3e7 && _character.totalAdvDefense() >= 7e6 && _character.totalAdvHPRegen() >= 1.5e5 && killCheck;
            }

            return false;
        }

        public static bool AutokillAvailable(int titanIndex) => AutokillAvailable(titanIndex, TitanVersion(titanIndex));

        public static int? GetHighestSpawningTitanZone()
        {
            var spawningTitans = _titanSnapshotSummary.TitansSpawningSoon;
            if (!spawningTitans.Any())
                return null;
            return TitanZones[spawningTitans.AllMaxBy(x => x.TitanIndex).First().TitanIndex];
        }

        public static bool AnyTitansSpawningSoon() => _titanSnapshotSummary.AnySpawningSoon;

        public static bool ShouldRunGoldLoadout() => _titanSnapshotSummary.RunGoldLoadout;

        public static bool ShouldRunTitanLoadout() => _titanSnapshotSummary.RunTitanLoadout;

        public static void RefreshTitanSnapshots()
        {
            if (!Main.Character.buttons.adventure.IsInteractable())
            {
                _titanSnapshotSummary = new TitanSnapshotSummary();
                _titanDetails.Clear();
                return;
            }

            int maxZone = GetMaxReachableZone(true);
            for (int titanIndex = 0; titanIndex < TitanZones.Length; titanIndex++)
            {
                if (TitanZones[titanIndex] > maxZone)
                    continue;

                var currentSnapshot = GetTitanSnapshot(titanIndex);
                if (!_titanDetails.ContainsKey(titanIndex))
                {
                    _titanDetails[titanIndex] = currentSnapshot;
                    continue;
                }

                var oldSnapshot = _titanDetails[titanIndex];
                oldSnapshot.ShouldUseGoldLoadout = currentSnapshot.ShouldUseGoldLoadout;
                oldSnapshot.ShouldUseTitanLoadout = currentSnapshot.ShouldUseTitanLoadout;

                // The titan is active, if it has been active for over 10 minutes, stats are probably not high enough to kill
                // Disable the titan to prevent sitting with suboptimal gear forever
                if (oldSnapshot.SpawnSoonTimestamp.HasValue && currentSnapshot.SpawnSoonTimestamp.HasValue && (currentSnapshot.ShouldUseGoldLoadout || currentSnapshot.ShouldUseTitanLoadout))
                {
                    // Waiting for kill...
                    if ((currentSnapshot.SpawnSoonTimestamp.Value - oldSnapshot.SpawnSoonTimestamp.Value).TotalMinutes < 10.0)
                        continue;

                    Log($"Titan {titanIndex} still available after 300 seconds");

                    var version = TitanVersion(titanIndex);
                    var reducedVersion = false;
                    while (version-- > 1)
                    {
                        if (AutokillAvailable(titanIndex, version))
                        {
                            SetTitanVersion(titanIndex, version);

                            reducedVersion = true;

                            Log($"Reducing Titan {titanIndex} version to {version}");

                            break;
                        }
                    }

                    if (!reducedVersion)
                    {
                        if (currentSnapshot.ShouldUseGoldLoadout)
                        {
                            Log($"Disabling Titan {titanIndex} as a valid gold swap target");

                            Settings.TitanGoldTargets[titanIndex] = false;

                            currentSnapshot.ShouldUseGoldLoadout = false;
                        }
                        else
                        {
                            Log($"Disabling Titan {titanIndex} as a valid swap target");

                            Settings.TitanSwapTargets[titanIndex] = false;

                            currentSnapshot.ShouldUseTitanLoadout = false;
                        }
                        Settings.SaveSettings();
                    }

                    _titanDetails[titanIndex] = currentSnapshot;
                }
                // If the timestamp is now null, the titan has been killed, flag the kill as done if the titan was set to use a gold loadout
                else if (oldSnapshot.SpawnSoonTimestamp.HasValue && !currentSnapshot.SpawnSoonTimestamp.HasValue && currentSnapshot.ShouldUseGoldLoadout)
                {
                    // Marking titan gold swap as complete
                    Settings.TitanMoneyDone[titanIndex] = true;
                    Settings.SaveSettings();

                    _titanDetails[titanIndex] = currentSnapshot;
                }
                else
                {
                    _titanDetails[titanIndex] = currentSnapshot;
                }
            }

            _titanSnapshotSummary.TitansSpawningSoon = _titanDetails.Select(x => x.Value).Where(x => x.SpawnSoonTimestamp.HasValue && (x.ShouldUseGoldLoadout || x.ShouldUseTitanLoadout));
        }

        public static float? TimeTillTitanSpawn(int bossId)
        {
            var spawnTime = Main.Character.adventureController.CallMethod<AdventureController, float>($"boss{bossId + 1}SpawnTime");
            var spawn = Main.Character.adventure.GetFieldValue<Adventure, PlayerTime>($"boss{bossId + 1}Spawn");

            return Mathf.Max(0f, spawnTime - (float)spawn.totalseconds);
        }

        private static TitanSnapshot GetTitanSnapshot(int titanIndex)
        {
            DateTime? spawnSoonTimestamp = IsTitanSpawningSoon(titanIndex) ? (DateTime?)DateTime.Now : null;
            bool shouldUseTitanLoadout = Settings.ManageTitans && Settings.SwapTitanLoadouts && Settings.TitanSwapTargets[titanIndex];
            bool shouldUseGoldLoadout = Settings.ManageGoldLoadouts && Settings.TitanGoldTargets[titanIndex] && !Settings.TitanMoneyDone[titanIndex];

            var titanSnapshot = new TitanSnapshot(titanIndex, spawnSoonTimestamp, shouldUseTitanLoadout, shouldUseGoldLoadout);

            return titanSnapshot;
        }

        private static bool IsTitanSpawningSoon(int bossId)
        {
            var time = TimeTillTitanSpawn(bossId);

            // The way this works is that spawn.totalseconds will count up until it reaches spawnTime and then stays there
            // This triggers the boss to be available, and after killed spawn.totalseconds will reset back down to 0
            return time < 20f;
        }

        public static int GetMaxReachableZone(bool includingTitans)
        {
            int effectiveBoss = _character.effectiveBossID();
            int maxZone = Array.BinarySearch(ZoneUnlocks, _character.effectiveBossID());
            if (maxZone < 0)
                maxZone = -maxZone - 2;

            for (int i = maxZone; i >= 0; i--)
            {
                if (!ZoneIsTitan(i))
                    return i;
                if (includingTitans)
                    return i;
            }
            return -1;
        }
    }

    public class TitanSnapshotSummary
    {
        public IEnumerable<TitanSnapshot> TitansSpawningSoon { get; set; } = new List<TitanSnapshot>();

        public bool AnySpawningSoon => TitansSpawningSoon.Any();

        public bool RunTitanLoadout => TitansSpawningSoon.Any(x => x.ShouldUseTitanLoadout);

        public bool RunGoldLoadout => TitansSpawningSoon.Any(x => x.ShouldUseGoldLoadout);
    }

    public class TitanSnapshot
    {
        public int TitanIndex { get; set; }

        public DateTime? SpawnSoonTimestamp { get; set; }

        public bool ShouldUseTitanLoadout { get; set; }

        public bool ShouldUseGoldLoadout { get; set; }

        public TitanSnapshot(int titanIndex, DateTime? spawnSoonTimestamp, bool shouldUseTitanLoadout, bool shouldUseGoldLoadout)
        {
            TitanIndex = titanIndex;
            SpawnSoonTimestamp = spawnSoonTimestamp;
            ShouldUseTitanLoadout = shouldUseTitanLoadout;
            ShouldUseGoldLoadout = shouldUseGoldLoadout;
        }
    }
}
