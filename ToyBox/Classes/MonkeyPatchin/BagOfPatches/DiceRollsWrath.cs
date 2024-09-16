﻿// borrowed shamelessly and enhanced from Bag of Tricks https://www.nexusmods.com/pathfinderkingmaker/mods/26, which is under the MIT License

using HarmonyLib;
using Kingmaker;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using ModKit;
using System;

namespace ToyBox.BagOfPatches {
    public static class DiceRollsWrath {
        public static Settings settings => Main.Settings;
        public static Player player => Game.Instance.Player;

        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.IsCriticalConfirmed), MethodType.Getter)]
        private static class HitPlayer_OnTriggerl_Patch {
            private static void Postfix(ref bool __result, RuleAttackRoll __instance) {
                if (__instance.IsHit && UnitEntityDataUtils.CheckUnitEntityData(__instance.Initiator, settings.allHitsCritical)) {
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.IsHit), MethodType.Getter)]
        private static class HitPlayer_OnTrigger2_Patch {
            private static void Postfix(ref bool __result, RuleAttackRoll __instance) {
                if (UnitEntityDataUtils.CheckUnitEntityData(__instance.Initiator, settings.allAttacksHit)) {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(RuleRollDice), nameof(RuleRollDice.Roll))]
        public static class RuleRollDice_Roll_Patch {
            private static void Postfix(RuleRollDice __instance) {
                if (__instance.DiceFormula.Dice != DiceType.D20) return;
                var initiator = __instance.Initiator;
                var result = __instance.m_Result;
                //modLogger.Log($"initiator: {initiator.CharacterName} isInCombat: {initiator.IsInCombat} alwaysRole20OutOfCombat: {settings.alwaysRoll20OutOfCombat}");
                //Mod.Debug($"initiator: {initiator.CharacterName} Initial D20Roll: " + result);
                if (UnitEntityDataUtils.CheckUnitEntityData(initiator, settings.alwaysRoll20)
                   || (UnitEntityDataUtils.CheckUnitEntityData(initiator, settings.alwaysRoll20OutOfCombat) && !initiator.IsInCombat)) {
                    result = 20;
                } else if (UnitEntityDataUtils.CheckUnitEntityData(initiator, settings.alwaysRoll10)) {
                    result = 10;
                } else if (UnitEntityDataUtils.CheckUnitEntityData(initiator, settings.alwaysRoll1)) {
                    result = 1;
                } else {
                    var min = 1;
                    var max = 21;
                    if (UnitEntityDataUtils.CheckUnitEntityData(initiator, settings.rollAtLeast10OutOfCombat) && !initiator.IsInCombat) {
                        min = 10;
                        if (result < 10) {
                            result = UnityEngine.Random.Range(min, max);
                        }
                    }
                    if (UnitEntityDataUtils.CheckUnitEntityData(initiator, settings.neverRoll1)) {
                        min = 2;
                        if (result == 1) {
                            result = UnityEngine.Random.Range(min, max);
                        }
                    }
                    if (UnitEntityDataUtils.CheckUnitEntityData(initiator, settings.neverRoll20)) {
                        max = 20;
                        if (result == 20) {
                            result = UnityEngine.Random.Range(min, max);
                        }
                    }
                    if (UnitEntityDataUtils.CheckUnitEntityData(initiator, settings.rollWithAdvantage)) {
                        result = Math.Max(result, UnityEngine.Random.Range(min, max));
                    } else if (UnitEntityDataUtils.CheckUnitEntityData(initiator, settings.rollWithDisadvantage)) {
                        result = Math.Min(result, UnityEngine.Random.Range(min, max));
                    }
                }
                //Mod.Debug("Modified D20Roll: " + result);
                __instance.m_Result = result;
            }
        }

        [HarmonyPatch(typeof(RuleInitiativeRoll), nameof(RuleInitiativeRoll.Result), MethodType.Getter)]
        public static class RuleInitiativeRoll_OnTrigger_Patch {
            private static void Postfix(RuleInitiativeRoll __instance, ref int __result) {
                if (UnitEntityDataUtils.CheckUnitEntityData(__instance.Initiator, settings.roll1Initiative)) {
                    __result = 1 + __instance.Modifier;
                    Mod.Trace("Modified InitiativeRoll: " + __result);
                } else if (UnitEntityDataUtils.CheckUnitEntityData(__instance.Initiator, settings.roll10Initiative)) {
                    __result = 10 + __instance.Modifier;
                } else if (UnitEntityDataUtils.CheckUnitEntityData(__instance.Initiator, settings.roll20Initiative)) {
                    __result = 20 + __instance.Modifier;
                    Mod.Trace("Modified InitiativeRoll: " + __result);
                }
            }
        }

        // Thanks AlterAsc - https://github.com/alterasc/CombatRelief/blob/main/CombatRelief/SkillRolls.cs
        [HarmonyPatch(typeof(RuleSkillCheck))]
        public static class RuleSkillCheck_Patch {
            [HarmonyPatch(nameof(RuleSkillCheck.RollD20))]
            [HarmonyPrefix]
            private static bool RollD20(ref RuleRollD20 __result, RuleSkillCheck __instance) {
                if (__instance.Initiator.IsInCombat) {
                    return true;
                }
                if (UnitEntityDataUtils.CheckUnitEntityData(__instance.Initiator, settings.skillsTake20)) {
                    __result = RuleRollD20.FromInt(__instance.Initiator, 20);
                    return false;
                }
                if (UnitEntityDataUtils.CheckUnitEntityData(__instance.Initiator, settings.skillsTake10)) {
                    __result = RuleRollD20.FromInt(__instance.Initiator, 10);
                    return false;
                }
                return true;
            }
            // Camping gives a repeated until failed check (with each attempt dc + 5) which grants +5 min to a buff
            // Not auto-failing that check with ToyBox cheats activated can cause this to be repeated infinitely
            [HarmonyPatch(nameof(RuleSkillCheck.Success), MethodType.Getter)]
            [HarmonyPostfix]
            private static void Success(ref bool __result, RuleSkillCheck __instance) {
                if (__instance.DC > 250) {
                    __result = false;
                }
            }
        }
    }
}
