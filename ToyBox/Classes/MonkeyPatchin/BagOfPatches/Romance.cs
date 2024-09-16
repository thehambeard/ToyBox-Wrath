﻿using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UI.MVVM._PCView.ActionBar;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UI.UnitSettings;
using Kingmaker.Utility;
using ModKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ToyBox.BagOfPatches {
    internal static class Romance {
        public static Settings settings => Main.Settings;
        public static Player player => Game.Instance.Player;
        public enum DLC6RomanceOverride {
            NoOverride = 0,
            Galfrey = 1,
            Daeran = 2,
            Sosiel = 3,
            Camellia = 4,
            Lann = 5,
            Wenduag = 6,
            Arueshalae = 7,
            Ulbrig = 8,
            Single = 9
        }
        // Any Gender Any Romance Overrides
        // These modify the PcFemale/PcMale conditions for specific Owner blueprints 
        internal static readonly Dictionary<string, bool> PcFemaleOverrides = new() {
            // Lann
            { "2e7f66de32ccad14bbb17855b0d125fb", true },   // Etude    LannKTC_WarCamp_LannAndFeelings
            { "d9baf40d38ceaf248bd5306f0e344bdb", true },   // Etude    LannRomance
            { "15fbf08c47b9cf34d8d535765e9a143a", true },   // Answer   Answer_0052
            { "2942d51c334b42748822ea2c48093a72", true },   // Answer   AnswersList_0201
            { "c68fad6a2d296f54f825eb1557153923", true },   // Dialog   WorldwoundEdge_GMBE
            // Sosiel 
            { "1071445514a15ec42a057a987886a0b5", false },  // Cue      Cue_0019
            { "8abf3aa7d2244f048abdcfbc48721eff", false },  // Cue      Cue_0030
            { "54fea9d1c9e0b69429bec08fb49a40d2", false },  // Cue      Cue_0235
            // Camellia 
            { "0144dcae4dc708744850d81254f28ec4", false },  // Cue      Cue_0067_NoGiefSex
            { "7fec7b3b23df5f9498083f096b09f055", false }   // Answer   Answer_0057
        };
        internal static readonly Dictionary<string, bool> PcMaleOverrides = new() {
            // Sosiel 
            { "5170dd15fdfd0094aa561e4f331c269f", true },   // Cue      Cue_0018
            { "7364becdf5cc4b94dba30a9fe7c3b790", true },   // Cue      Cue_0234
            { "e166872fc2989f548af1b3e2ba8f7156", true },   // Cue      Cue_0029
            // Camellia
            { "55c0fe80d141ecf40b49c7ad12746afb", true },   // Cue      Cue_0016
            { "789ffa9876fd92f439d4b975b16be283", true },   // Cue      Cue_0066_GiefSex
            { "f263d6ed04831f240bf2a8dce2b5ce33", true },   // Answer   Answer_0052
            { "a96fc116bb7af94488b6da41161a47c7", true },   // Answer   Answer_0060 
        };

        // Path Romance Overrides
        // These modify the EtudeStatus condition for specific Owner blueprints 
        internal static readonly Dictionary<(string, string), bool> ConditionCheckOverridesLoveIsFree = new()
        {
            // Lich Path
            { ("7f532b681d64f3741a7aa0aebba7c4db", "977f3380-2938-4cc8-a26a-448edc6f9259"), false },  // Etude CamelliaRomance_Start status is:  Not Playing;   
            { ("7f532b681d64f3741a7aa0aebba7c4db", "52632774-cbb4-4eea-ada6-37ec2708e07d"), false },  // Etude WenduagRomance_Active status is:  Not Playing;   
            { ("7f532b681d64f3741a7aa0aebba7c4db", "12928be5-97e8-4e7c-ac5c-02d704289e7f"), false },  // Etude LannRomance_Active status is:  Not Playing;      
            { ("7f532b681d64f3741a7aa0aebba7c4db", "2bbad7a7-5918-4c14-b909-f1a7bbce9248"), false },  // Etude ArueshalaeRomance_Active status is:  Not Playing;   
            { ("7f532b681d64f3741a7aa0aebba7c4db", "55fd2fa2-9644-462a-a02e-23987e05fd62"), false },  // Etude DaeranRomance_Active status is:  Not Playing;      
            { ("7f532b681d64f3741a7aa0aebba7c4db", "b0f684c1-0cc9-4ec7-a4a3-fe47e3d9847c"), false },  // Etude SosielRomance_Active status is:  Not Playing; 
        };
        internal static readonly Dictionary<string, bool> EtudeStatusOverridesLoveIsFree = new()
        {
            // Lich Path
            { "2ebd861e55143014c8067c6832cdf21c", false },  // Cue_0048
            // Vellexia
            { "81b15ede1bb2a3e4e926d5ca4be3e193", true },   // Cue_0066
            { "3aa48f68198afe14cb6de752ce80cc8f", true },   // Cue_0078
            // Queen
            { "7a160960668f2ef4180cb56edb8388e9", true },   // Cue_0044
        };

        // Multiple Romances overrides
        // This modify the EtudeStatus condition for specific Owner blueprints 
        internal static readonly Dictionary<(string, string), bool> ConditionCheckOverrides = new() {
            { ("39bd3b1e9fb6fef4c8e92674c02295df", "6a5a5f14-0531-421e-8225-f777fd22fa52"), true },  // Not Etude CamelliaRomance_Start status is:   Playing;   
            { ("39bd3b1e9fb6fef4c8e92674c02295df", "0dadef83-142b-4126-9ef3-d2b3d6ac3c00"), true },  // Not Etude WenduagRomance_Active status is:   Playing;   
            { ("39bd3b1e9fb6fef4c8e92674c02295df", "6af9fc46-b172-45f6-991b-95864d7535dd"), true },  // Not Etude LannRomance_Active status is:   Playing;      
            { ("39bd3b1e9fb6fef4c8e92674c02295df", "08562d17-c875-477d-916f-484c86d6d56b"), true },  // Not Etude ArueshalaeRomance_Active status is:   Playing;   
            { ("39bd3b1e9fb6fef4c8e92674c02295df", "eed10502-68fe-4f06-93f6-9a5a344194e1"), true },  // Not Etude DaeranRomance_Active status is:   Playing;      
            { ("39bd3b1e9fb6fef4c8e92674c02295df", "79f99282-5d3a-4b2b-8d2e-61a0dc8f033f"), true },  // Not Etude SosielRomance_Active status is:   Playing; 
        };
        internal static readonly Dictionary<string, bool> EtudeStatusOverrides = new() {
            { "f4acc1a428ffbee42965a6f13fe270ac", false },  // Cue_0058
        };
        internal static readonly Dictionary<string, bool> FlagInRangeOverrides = new() {
            { "4799a25da39295b43a6eefcd2cb2b4a7", false },  // Etude    KTC_Jealousy
        };

        internal static readonly Dictionary<string, bool> EtudeStatusOverridesFriendshipIsMagic = new()  {
            // Crusaders at large + Queen
            { "60ad7865e557c50489244e3f3f7fc6bc", false },   // GalfreyOnTheEdge_Iz_ch5_Dialog - Cue_0015
            { "70f2c9f4876257a4f8c47b5409752aef", true },   // GalfreyOnTheEdge_Iz_ch5_Dialog - Cue_0062
            { "136b18c04f144794b9c5b5d39e0e296e", false },   // GalfreyAfter_Iz_ch5_Dialog - Cue_0017
            { "d9d009964b3f0e945a0d509dc56db853", false },   // GalfreyAfter_Iz_ch5_Dialog - Cue_0063
            { "070e849ca16487340ae9641fcfabab53", true },   // Coronation_Dialogue - Cue_0211
            { "977012d051210674c91dc2c31a3fddbe", true },   // Coronation_Dialogue - Cue_0053

            { "365405cc55044874893d26759532ea07", false },   // DrezenSiege_Council_Dialogue - Cue_0107

            // Ciar (commented out due to Lich issue)
            // { "5e51a15b014a1bb4181f5feaa4f71e32", false },   // KTC_Ciar_FirstVisit - Cue_0014

            // Hand of the Inheritor
            { "4a056226dce658f4da30d99d851537ec", false },   // Herald_IvoryLabyrinth_dialog - Cue_0011
            { "0a5a3d097018c1041b1eedb6fb26c5c3", false },   // Herald_IvoryLabyrinth_dialog - Cue_0018
            { "223ea6c047133024a96a18049ffe7679", true },   // Herraxa_dialogue - Cue_0268

        };
        internal static readonly Dictionary<string, bool> FlagInRangeOverridesFriendshipIsMagic = new() {
            // Cam
            { "8c8b7f25df243dd4799da10e5683ff64", true },   // AfterHorgus_Dialogue - Cue_0024

            // Hand of the Inheritor
            { "308d6b0d4d0c1944dadccf4a4942d085", true },   // Audience_Areelu_c4_dialog - Cue_0067
            { "e3b2f7241dae25548a35c65729c6f50e", true },   // Audience_Areelu_c4_dialog - Cue_0086

        };
        internal static readonly Dictionary<string, bool> EtudeStatusOverridesAnyMythic = new() {
            // Crusade Events
            { "d9fd5839ef1a44fe81473fc2bac2078b", true },   // CrusadeEvent05
        };
        internal static readonly HashSet<string> EtudeMythicIsPlaying = new() {
            // PlayerIsLich                   , // PlayerIsAeon
            "11fc5662e0ce8074ea145a022282b879", "3a040afde22f4b742a2f607354ab17e7",
            // PlayerIsAzata                  , // PlayerIsAngel
            "d3b47e973d65c6c46af1cce815d1f6ce", "3a82aba4de71b89458ac82949ed957c4",
            // PlayerIsDevil                  , // PlayerIsDemon
            "3a82aba4de71b89458ac82949ed957c4", "9a3739370f84b0b4196d0e4d326ea3a8",
            // PlayerIsLegend                 , // PlayerIsDragon
            "c6165efcd5571c442ae38d7c0601f2df", "9b193d30c89a20b409fd3dda9bd109bf",
            // PlayerIsLocust                 , // PlayerIsTrickster
            "439e63fed37f52048887d98f99255e40", "9f486a9c0c9abfc4a952bb22e88a7e96"
        };

        internal static readonly Dictionary<string, DLC6RomanceOverride> EtudesToCheckForDLCOverride = new() {
            { "b152c93f814c4b7a9c372750d9490af9", DLC6RomanceOverride.Galfrey },
            { "0158f3bd24a543d78a7be8a18b6d17cb", DLC6RomanceOverride.Daeran },
            { "76a1b56ae4e84f79818cfceb0f6bdb07", DLC6RomanceOverride.Sosiel },
            { "e192647a55614292a9057d84f11895d7", DLC6RomanceOverride.Camellia },
            { "97cd066a25ba4002898873eb986b2aee", DLC6RomanceOverride.Lann },
            { "0814b104c5874f789a291348a399b7e1", DLC6RomanceOverride.Wenduag },
            { "74e7f47a591f471a85f14c6dab557625", DLC6RomanceOverride.Arueshalae },
            { "1ae5f88c078848d1928d9655170e6c84", DLC6RomanceOverride.Ulbrig },
            { "23982979ce2e4f40ba160b86d44a4e03", DLC6RomanceOverride.Single }
        };

        internal static readonly Dictionary<(string, string), DLC6RomanceOverride> EtudesToCheckForDLCOverrideStage = new() {
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "393fca1d-d04f-4a39-be6c-8758bb48d8d8"), DLC6RomanceOverride.Galfrey },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "a1f6b8c8-1f37-4a5a-81b1-c7908d5bf7b7"), DLC6RomanceOverride.Daeran },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "5497c710-d26d-4897-b255-0c522bea8dfa"), DLC6RomanceOverride.Daeran },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "f0760227-bd25-49d3-ae21-31ad7f417c88"), DLC6RomanceOverride.Daeran },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "c20470f1-a994-46ba-a496-7c9f10090a89"), DLC6RomanceOverride.Wenduag },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "ae24ba8c-c175-400b-b60a-b697e34818bd"), DLC6RomanceOverride.Wenduag },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "b7180f39-82bb-4c9f-baff-b3f13ed8d7f9"), DLC6RomanceOverride.Arueshalae },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "02b04d8e-afaa-4897-a95d-61af64e78738"), DLC6RomanceOverride.Arueshalae },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "b1bdc21a-3244-4664-a35d-fad6de9c509b"), DLC6RomanceOverride.Sosiel },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "1b5dc2eb-185e-481d-8a6b-5233d204b8ed"), DLC6RomanceOverride.Sosiel },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "4e4825d1-c018-4d92-bc61-94502db16347"), DLC6RomanceOverride.Sosiel },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "3d1fc3d3-4529-4e3e-9c44-5c96828bd327"), DLC6RomanceOverride.Camellia },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "c8ed32ae-ed2d-40c2-8b8e-4e36bdd20034"), DLC6RomanceOverride.Camellia },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "7b357398-2f8b-4249-bdec-8c2c948f7a70"), DLC6RomanceOverride.Lann },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "66b54e28-2964-4965-a275-b22fe7fa78cc"), DLC6RomanceOverride.Lann },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "55162c4c-c45c-47b3-be08-8f36622a8b2c"), DLC6RomanceOverride.Ulbrig },
            { ("e03e8b92fcba4f0294f20fd42e41c11d", "190482fc-55d7-4650-a650-64340d72901d"), DLC6RomanceOverride.Ulbrig }
        };

        [HarmonyPatch(typeof(PcFemale), nameof(PcFemale.CheckCondition))]
        public static class PcFemale_CheckCondition_Patch {
            public static void Postfix(PcFemale __instance, ref bool __result) {
                if (!settings.toggleAllowAnyGenderRomance || __instance?.Owner is null) return;
                Mod.Debug($"checking {__instance} guid:{__instance.AssetGuid} owner:{__instance.Owner.name} guid: {__instance.Owner.AssetGuid}) value: {__result}");
                if (PcFemaleOverrides.TryGetValue(__instance.Owner.AssetGuid.ToString(), out var value)) { Mod.Debug($"overiding {__instance.Owner.name} to {value}"); __result = value; }
            }
        }
        [HarmonyPatch(typeof(PcMale), nameof(PcMale.CheckCondition))]
        public static class PcMale_CheckCondition_Patch {
            public static void Postfix(PcMale __instance, ref bool __result) {
                if (!settings.toggleAllowAnyGenderRomance || __instance?.Owner is null) return;
                Mod.Debug($"checking {__instance} guid:{__instance.AssetGuid} owner:{__instance.Owner.name} guid: {__instance.Owner.AssetGuid}) value: {__result}");
                if (PcMaleOverrides.TryGetValue(__instance.Owner.AssetGuid.ToString(), out var value)) { Mod.Debug($"overiding {__instance.Owner.name} to {value}"); __result = value; }
            }
        }

        // This is in testing and everything but debug output should be *disabled* for users
        [HarmonyPatch(typeof(ItemsEnough), nameof(ItemsEnough.CheckCondition))]
        public static class ItemsEnough_CheckCondition_Patch {
            public static void Postfix(ItemsEnough __instance, ref bool __result) {
                if (!settings.toggleAllowAnyGenderRomance || __instance?.Owner is null) return;
                Mod.Debug($"checking {__instance} guid:{__instance.AssetGuid} owner:{__instance.Owner.name} guid: {__instance.Owner.AssetGuid}) value: {__result}");
                //   if (PcMaleOverrides.TryGetValue(__instance.Owner.AssetGuid.ToString(), out var value)) { Mod.Debug($"overiding {__instance.Owner.name} to {value}"); __result = value; }
            }
        }

        [HarmonyPatch(typeof(Condition), nameof(Condition.Check))]
        public static class Condition_Check_Patch {
            public static void Postfix(Condition __instance, ref bool __result) {
                if (__instance?.Owner is null) return;
                if (string.IsNullOrEmpty(__instance.name)) return;

                var key = (__instance.Owner.AssetGuid.ToString(), __instance.AssetGuid);
                if (settings.toggleAllowAnyGenderRomance) {
                    if (ConditionCheckOverridesLoveIsFree.TryGetValue(key, out var valueLoveIsFree)) { Mod.Debug($"overiding {(__instance.Owner.name, __instance.name)} to {valueLoveIsFree}"); __result = valueLoveIsFree; }
                }
                if (settings.toggleMultipleRomance) {
                    if (ConditionCheckOverrides.TryGetValue(key, out var value)) {
                        Mod.Debug($"overiding {(__instance.Owner.name, __instance.name)} to {value}");
                        __result = value;
                    }
                }
                if (EtudesToCheckForDLCOverride.TryGetValue(key.Item1, out var Override)) {
                    if (settings.pickedDLC6Override > 0) {
                        if ((int)Override != settings.pickedDLC6Override) {
                            __result = false;
                            Mod.Debug($"(DLC6 End Romance overiding {(__instance.Owner.name, __instance.name)} to {__result}");
                        }
                    }
                } else if (EtudesToCheckForDLCOverrideStage.TryGetValue(key, out Override)) {
                    if (settings.pickedDLC6Override > 0) {
                        if ((int)Override != settings.pickedDLC6Override) {
                            __result = false;
                            Mod.Debug($"(DLC6 End Romance overiding {(__instance.Owner.name, __instance.name)} to {__result}");
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(EtudeStatus), nameof(EtudeStatus.CheckCondition))]
        public static class EtudeStatus_CheckCondition_Patch {
            public static void Postfix(EtudeStatus __instance, ref bool __result) {
                if (__instance?.Owner is null) return;

                var key = (__instance.Owner.AssetGuid.ToString());
                if (settings.toggleAllowAnyGenderRomance) {
                    if (EtudeStatusOverridesLoveIsFree.TryGetValue(key, out var valueLoveIsFree)) { Mod.Debug($"overiding {(__instance.Owner.name)} to {valueLoveIsFree}"); __result = valueLoveIsFree; }
                }
                if (settings.toggleMultipleRomance) {
                    if (EtudeStatusOverrides.TryGetValue(key, out var value)) { Mod.Debug($"overiding {__instance.Owner.name} to {value}"); __result = value; }
                }
                if (settings.toggleFriendshipIsMagic) {
                    if (EtudeStatusOverridesFriendshipIsMagic.TryGetValue(key, out var valueFriendshipIsMagic)) { Mod.Debug($"overiding {(__instance.Owner.name)} to {valueFriendshipIsMagic}"); __result = valueFriendshipIsMagic; }
                }
                if (settings.toggleDialogRestrictionsMythic || settings.toggleDialogRestrictionsEverything) {
                    if (EtudeStatusOverridesAnyMythic.TryGetValue(key, out var valueDialogRestrictionsMythic)) { Mod.Debug($"overiding {(__instance.Owner.name)} to {valueDialogRestrictionsMythic}"); __result = valueDialogRestrictionsMythic; }
                    bool ownerIsAnswer = __instance.Owner is BlueprintAnswer bpAnswer && (bpAnswer.ShowConditions.Conditions?.Contains(__instance) ?? false);
                    bool ownerIsCueSelectionWithNoValidCue = CueSelection_Select_Patch.ignoreNextMythicRestrictions;
                    if (ownerIsAnswer || ownerIsCueSelectionWithNoValidCue) {
                        if (EtudeMythicIsPlaying.Contains(__instance.Etude.AssetGuid.ToString())) {
                            Mod.Debug($"For {__instance.Owner.name} overiding {__instance.Etude.name} to {!__instance.Not}");
                            __result = !__instance.Not;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CueSelection), nameof(CueSelection.Select))]
        public static class CueSelection_Select_Patch {
            internal static bool ignoreNextMythicRestrictions = false;
            [HarmonyPrefix]
            public static void Select_Pre(CueSelection __instance) {
                try {
                    if (settings.toggleDialogRestrictionsMythic || settings.toggleDialogRestrictionsEverything) {
                        var cc = Game.Instance.DialogController.CurrentCue;
                        if (cc == null) return;
                        var answers = ResolveBlueprintAnswerBaseReference(cc.Answers);
                        foreach (var answer in answers) {
                            if (answer.NextCue == __instance) {
                                if (__instance.Cues.Dereference().Count(nc => nc.CanShow()) == 0) {
                                    ignoreNextMythicRestrictions = true;
                                }
                            }
                        }
                    }
                } catch (Exception ex) {
                    Mod.Error(ex.ToString());
                }
            }
            [HarmonyPostfix]
            public static void Select_Post() {
                ignoreNextMythicRestrictions = false;
            }
            public static List<BlueprintAnswer> ResolveBlueprintAnswerBaseReference(List<BlueprintAnswerBaseReference> lbabr) {
                if (lbabr == null) return [];
                List<BlueprintAnswer> ret = [];
                lbabr.ForEach(babr => {
                    var answerBase = babr.Get();
                    if (answerBase is BlueprintAnswer answer) ret.Add(answer);
                    else if (answerBase is BlueprintAnswersList answerList) ret.AddRange(ResolveBlueprintAnswerBaseReference(answerList.Answers));
                });
                return ret;
            }
        }

        [HarmonyPatch(typeof(FlagInRange), nameof(FlagInRange.CheckCondition))]
        public static class FlagInRange_CheckCondition_Patch {
            public static void Postfix(FlagInRange __instance, ref bool __result) {
                if (__instance?.Owner is null) return;
                if (settings.toggleMultipleRomance) {
                    if (FlagInRangeOverrides.TryGetValue(__instance.Owner.AssetGuid.ToString(), out var value)) { Mod.Debug($"overiding {__instance.Owner.name} to {value}"); __result = value; }
                }
                if (settings.toggleFriendshipIsMagic) {
                    if (FlagInRangeOverridesFriendshipIsMagic.TryGetValue(__instance.Owner.AssetGuid.ToString(), out var valueFriendshipIsMagic)) { Mod.Debug($"overiding {__instance.Owner.name} to {valueFriendshipIsMagic}"); __result = valueFriendshipIsMagic; }
                }
            }
        }
    }
}
