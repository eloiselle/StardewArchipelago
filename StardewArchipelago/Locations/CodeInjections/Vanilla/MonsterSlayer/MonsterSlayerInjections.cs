﻿using System;
using System.Linq;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Goals;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer
{
    public static class MonsterSlayerInjections
    {
        public const string MONSTER_ERADICATION_AP_PREFIX = "Monster Eradication: ";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static MonsterKillList _killList;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, MonsterKillList killList)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _killList = killList;
        }

        // private void gil()
        public static bool Gil_NoMonsterSlayerRewards_Prefix(AdventureGuild __instance)
        {
            try
            {
                Game1.DrawDialogue(__instance.Gil, "Characters\\Dialogue\\Gil:Snoring");
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Gil_NoMonsterSlayerRewards_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static bool areAllMonsterSlayerQuestsComplete()
        public static bool AreAllMonsterSlayerQuestsComplete_ExcludeGingerIsland_Prefix(ref bool __result)
        {
            try
            {
                __result = _killList.AreAllGoalsComplete();
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AreAllMonsterSlayerQuestsComplete_ExcludeGingerIsland_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void showMonsterKillList()
        public static bool ShowMonsterKillList_CustomListFromAP_Prefix(AdventureGuild __instance)
        {
            try
            {
                if (!Game1.player.mailReceived.Contains("checkedMonsterBoard"))
                {
                    Game1.player.mailReceived.Add("checkedMonsterBoard");
                }

                var killListContent = _killList.GetKillListLetterContent();
                Game1.drawLetterMessage(killListContent);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ShowMonsterKillList_CustomListFromAP_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void monsterKilled(string name)
        public static void MonsterKilled_SendMonstersanityCheck_Postfix(Stats __instance, string name)
        {
            try
            {
                var category = GetCategory(name);
                switch (_archipelago.SlotData.Monstersanity)
                {
                    case Monstersanity.None:
                        return;
                    case Monstersanity.OnePerCategory:
                        CheckLocation(category);
                        return;
                    case Monstersanity.OnePerMonster:
                        CheckLocation(name);
                        return;
                    case Monstersanity.Goals:
                    case Monstersanity.ShortGoals:
                    case Monstersanity.VeryShortGoals:
                        CheckLocationIfEnoughMonstersInCategory(category);
                        return;
                    case Monstersanity.ProgressiveGoals:
                        CheckLocationIfEnoughMonstersInProgressiveCategory(category);
                        return;
                    case Monstersanity.SplitGoals:
                        CheckLocationIfEnoughMonsters(name);
                        return;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MonsterKilled_SendMonstersanityCheck_Postfix)}:\n{ex}");
                return;
            }
        }

        // public void monsterKilled(string name)
        public static void MonsterKilled_CheckGoalCompletion_Postfix(Stats __instance, string name)
        {
            try
            {
                GoalCodeInjection.CheckProtectorOfTheValleyGoalCompletion();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MonsterKilled_CheckGoalCompletion_Postfix)}:\n{ex}");
                return;
            }
        }

        // public string Name { get; set; }
        public static void GetName_SkeletonMage_Postfix(Character __instance, ref string __result)
        {
            try
            {
                if (__instance is not Skeleton skeleton)
                {
                    return;
                }

                if (skeleton.isMage.Value)
                {
                    __result = MonsterName.SKELETON_MAGE;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetName_SkeletonMage_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void CheckLocationIfEnoughMonstersInCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || !_killList.MonsterGoals.ContainsKey(category))
            {
                return;
            }

            var amountNeeded = _killList.MonsterGoals[category];
            if (_killList.GetMonstersKilledInCategory(category) >= amountNeeded)
            {
                CheckLocation(category);
            }
        }

        private static void CheckLocationIfEnoughMonsters(string monster)
        {
            if (string.IsNullOrWhiteSpace(monster) || !_killList.MonsterGoals.ContainsKey(monster))
            {
                return;
            }

            var amountNeeded = _killList.MonsterGoals[monster];
            if (_killList.GetMonstersKilled(monster) >= amountNeeded)
            {
                CheckLocation(monster);
            }
        }

        private static void CheckLocationIfEnoughMonstersInProgressiveCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || !_killList.MonsterGoals.ContainsKey(category))
            {
                return;
            }

            var lastAmountNeeded = _killList.MonsterGoals[category];
            var progressiveStep = lastAmountNeeded / 5;
            var monstersKilled = _killList.GetMonstersKilledInCategory(category);
            for (var i = progressiveStep; i <= lastAmountNeeded; i += progressiveStep)
            {
                if (monstersKilled < i)
                {
                    return;
                }

                var progressiveCategoryName = (i == lastAmountNeeded) ? category : $"{i} {category}";
                CheckLocation(progressiveCategoryName);
            }
        }

        private static string GetCategory(string name)
        {
            foreach (var (category, monsters) in _killList.MonstersByCategory)
            {
                if (monsters.Contains(name))
                {
                    return category;
                }
            }

            _logger.LogDebug($"Could not find a monster slayer category for monster {name}");
            return "";
        }

        private static void CheckLocation(string goalName)
        {
            if (string.IsNullOrEmpty(goalName))
            {
                return;
            }

            goalName = goalName.Replace("Dust Spirit", "Dust Sprite");


            var apLocation = $"{MONSTER_ERADICATION_AP_PREFIX}{goalName}";
            if (_archipelago.GetLocationId(apLocation) > -1)
            {
                _locationChecker.AddCheckedLocation(apLocation);
            }
            else
            {
                _logger.LogDebug($"Tried to check a monster slayer goal, but it doesn't exist! [{apLocation}]");
            }
        }
    }
}
