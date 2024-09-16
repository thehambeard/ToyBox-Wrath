﻿// Copyright < 2021 > Narria (github user Cabarius) - License: MIT
using Kingmaker;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using ModKit;
using ModKit.DataViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Cheats;
using static ModKit.UI;

namespace ToyBox {
    public partial class PartyEditor {
        public static Settings Settings => Main.Settings;

        private enum ToggleChoice {
            Classes,
            Stats,
            Facts,
            Features,
            Buffs,
            Abilities,
            Spells,
            AI,
            None,
        };
        private const int NarrowIndent = 413;

        private static ToggleChoice selectedToggle = ToggleChoice.None;
        private static UnitEntityData selectedCharacter = null;
        private static UnitEntityData charToAdd = null;
        private static UnitEntityData charToRecruit = null;
        private static UnitEntityData charToRemove = null;
        private static UnitEntityData charToUnrecruit = null;
        private static bool editMultiClass = false;
        private static UnitEntityData multiclassEditCharacter = null;
        private static int respecableCount = 0;
        private static int recruitableCount = 0;
        private static int selectedSpellbook = 0;
        public static int selectedSpellbookLevel = 0;
        public static int SelectedNewSpellLvl = 0;
        private static (string, string) nameEditState = (null, null);
        private static bool editSpellbooks = false;
        private static UnitEntityData spellbookEditCharacter = null;
        private static readonly Dictionary<string, int> statEditorStorage = new();
        public static Dictionary<string, Spellbook> SelectedSpellbook = new();
        private static UnitEntityData GetEditCharacter() {
            var characterList = CharacterPicker.GetCharacterList();
            if (characterList == null || characterList.Count == 0) return null;
            if (!characterList.Contains(selectedCharacter)) return null;
            else return selectedCharacter;
        }

        public static void ResetGUI() {
            selectedCharacter = null;
            selectedSpellbook = 0;
            selectedSpellbookLevel = 0;
            CharacterPicker.PartyFilterChoices = null;
            Main.Settings.selectedPartyFilter = 0;
        }

        // This bit of kludge is added in order to tell whether our generic actions are being accessed from this screen or the Search n' Pick
        public static bool IsOnPartyEditor() => Main.Settings.selectedTab == 3;

        public static void ActionsGUI(UnitEntityData ch) {
            var player = Game.Instance.Player;
            Space(25);
            var buttonCount = 0;
            if (!player.PartyAndPets.Contains(ch) && player.AllCharacters.Contains(ch)) {
                ActionButton("Add".localize(), () => { charToAdd = ch; }, Width(150));
                Space(25);
                buttonCount++;
            } else if (player.ActiveCompanions.Contains(ch)) {
                ActionButton("Remove".localize(), () => { charToRemove = ch; }, Width(150));
                Space(25);
                buttonCount++;
            } else if (!player.AllCharacters.Contains(ch)) {
                recruitableCount++;
                ActionButton("Recruit".localize().Cyan(), () => { charToRecruit = ch; }, Width(150));
                Space(25);
                buttonCount++;
            }
            if (player.AllCharacters.Contains(ch) && !ch.IsMainCharacter && !ch.IsStoryCompanion()) {
                ActionButton("Unrecruit".Cyan(),
                             () => {
                                 charToUnrecruit = ch;
                                 charToRemove = ch;
                             },
                             Width(150));
                Space(25);
                buttonCount++;
            }
            if (ch.CanRespec()) {
                respecableCount++;
                ActionButton("Respec".localize().Cyan(), () => { Actions.ToggleModWindow(); ch.DoRespec(); }, Width(150));
            } else {
                Space(153);
            }
            if (buttonCount >= 0)
                Space(178 * (2 - buttonCount));
#if false
            Space(25);
            ActionButton("Log Caster Info", () => CasterHelpers.GetOriginalCasterLevel(ch.Descriptor()),
                AutoWidth());
#endif
            ActionButton("Kill".localize().Cyan(), () => CheatsCombat.KillUnit(ch));
            Label("", AutoWidth());
        }
        public static void OnGUI() {
            var player = Game.Instance.Player;
            if (player == null) return;
            charToAdd = null;
            charToRecruit = null;
            charToRemove = null;
            charToUnrecruit = null;
            var characterListFunc = CharacterPicker.OnFilterPickerGUI();
            var characterList = characterListFunc.func();
            var mainChar = GameHelper.GetPlayerCharacter();
            if (characterListFunc.name == "Nearby") {
                Slider("Nearby Distance".localize(), ref CharacterPicker.nearbyRange, 1f, 200, 25, 0, " meters".localize(), Width(250));
                characterList = characterList.OrderBy((ch) => ch.DistanceTo(mainChar)).ToList();
            }
            Space(20);
            var chIndex = 0;
            recruitableCount = 0;
            respecableCount = 0;
            selectedCharacter = GetEditCharacter();
            var isWide = IsWide;
            if (Main.IsInGame) {
                using (HorizontalScope()) {
                    Label($"Party Level ".localize().Cyan() + $"{Game.Instance.Player.PartyLevel}".Orange().Bold(), AutoWidth());
                    Space(110);
                    ReflectionTreeView.DetailToggle($"Inspect Party {"(for modders)".Orange()}".localize(), "All", characterList, 0);
#if false   // disabled until we fix performance
                    var encounterCR = CheatsCombat.GetEncounterCr();
                    if (encounterCR > 0) {
                        UI.Label($"Encounter CR ".cyan() + $"{encounterCR}".orange().bold(), UI.AutoWidth());
                    }
#endif
                }
            }
            ReflectionTreeView.OnDetailGUI("All");
            List<Action> todo = new();
            foreach (var ch in characterList) {
                var classData = ch.Progression.Classes;
                // TODO - understand the difference between ch.Progression and ch.Descriptor().Progression
                var progression = ch.Descriptor().Progression;
                var xpTable = progression.ExperienceTable;
                var level = progression.CharacterLevel;
                var mythicLevel = progression.MythicLevel;
                var spellbooks = ch.Spellbooks.ToList();
                var spellCount = spellbooks.Sum((sb) => sb.GetAllKnownSpells().Count());
                var isOnTeam = player.AllCharacters.Contains(ch);
                using (HorizontalScope()) {
                    var name = ch.CharacterName;
                    if (Game.Instance.Player.AllCharacters.Contains(ch)) {
                        var oldEditState = nameEditState;
                        if (isWide) {
                            if (EditableLabel(ref name, ref nameEditState, 200, n => n.Orange().Bold(), MinWidth(100), MaxWidth(400))) {
                                ch.Descriptor().CustomName = name;
                                Main.SetNeedsResetGameUI();
                            }
                        } else
                            if (EditableLabel(ref name, ref nameEditState, 200, n => n.Orange().Bold(), Width(230))) {
                            ch.Descriptor().CustomName = name;
                            Main.SetNeedsResetGameUI();
                        }
                        if (nameEditState != oldEditState) {
                            Mod.Log($"EditState changed: {oldEditState} -> {nameEditState}");
                        }
                    } else {
                        if (isWide)
                            Label(ch.CharacterName.Orange().Bold(), MinWidth(100), MaxWidth(400));
                        else
                            Label(ch.CharacterName.Orange().Bold(), Width(230));
                    }
                    Space(5);
                    var distance = mainChar.DistanceTo(ch); ;
                    Label(distance < 1 ? "" : distance.ToString("0") + "m", Width(75));
                    Space(5);
                    int nextLevel;
                    for (nextLevel = level; xpTable.HasBonusForLevel(nextLevel + 1) && progression.Experience >= xpTable.GetBonus(nextLevel + 1); nextLevel++) { }
                    if (nextLevel <= level || !isOnTeam)
                        Label((level < 10 ? "   lvl" : "   lv").Green() + $" {level}", Width(90));
                    else
                        Label((level < 10 ? "  " : "") + $"{level} > " + $"{nextLevel}".Cyan(), Width(90));
                    // Level up code adapted from Bag of Tricks https://www.nexusmods.com/pathfinderkingmaker/mods/2
                    if (player.AllCharacters.Contains(ch)) {
                        if (xpTable.HasBonusForLevel(nextLevel + 1)) {
                            ActionButton("+1", () => {
                                progression.AdvanceExperienceTo(xpTable.GetBonus(nextLevel + 1), true);
                            }, Width(63));
                        } else { Label("max".localize(), Width(63)); }
                    } else { Space(66); }
                    Space(10);
                    var nextML = progression.MythicExperience;
                    if (nextML <= mythicLevel || !isOnTeam)
                        Label((mythicLevel < 10 ? "  my" : "  my").Green() + $" {mythicLevel}", Width(90));
                    else
                        Label((level < 10 ? "  " : "") + $"{mythicLevel} > " + $"{nextML}".Cyan(), Width(90));
                    if (player.AllCharacters.Contains(ch)) {
                        if (progression.MythicExperience < 10) {
                            ActionButton("+1", () => {
                                progression.AdvanceMythicExperience(progression.MythicExperience + 1, true);
                            }, Width(63));
                        } else { Label("max".localize(), Width(63)); }
                    } else { Space(66); }
                    Space(30);
                    Wrap(IsNarrow, NarrowIndent, 0);
                    var prevSelectedChar = selectedCharacter;
                    var showClasses = ch == selectedCharacter && selectedToggle == ToggleChoice.Classes;
                    if (DisclosureToggle($"{classData.Count} " + "Classes".localize(), ref showClasses, 140)) {
                        if (showClasses) {
                            selectedCharacter = ch; selectedToggle = ToggleChoice.Classes; Mod.Trace($"selected {ch.CharacterName}");
                        } else { selectedToggle = ToggleChoice.None; }
                    }
                    var showStats = ch == selectedCharacter && selectedToggle == ToggleChoice.Stats;
                    if (DisclosureToggle("Stats".localize(), ref showStats, 95)) {
                        if (showStats) { selectedCharacter = ch; selectedToggle = ToggleChoice.Stats; } else { selectedToggle = ToggleChoice.None; }
                    }
                    //var showFacts = ch == selectedCharacter && selectedToggle == ToggleChoice.Facts;
                    //if (UI.DisclosureToggle("Facts", ref showFacts, 125)) {
                    //    if (showFacts) { selectedCharacter = ch; selectedToggle = ToggleChoice.Facts; }
                    //    else { selectedToggle = ToggleChoice.None; }
                    //}
                    var showFeatures = ch == selectedCharacter && selectedToggle == ToggleChoice.Features;
                    if (DisclosureToggle("Features".localize(), ref showFeatures, 125)) {
                        if (showFeatures) { selectedCharacter = ch; selectedToggle = ToggleChoice.Features; } else { selectedToggle = ToggleChoice.None; }
                    }
                    Wrap(!IsWide, NarrowIndent, 0);
                    var showBuffs = ch == selectedCharacter && selectedToggle == ToggleChoice.Buffs;
                    if (DisclosureToggle("Buffs".localize(), ref showBuffs, 90)) {
                        if (showBuffs) { selectedCharacter = ch; selectedToggle = ToggleChoice.Buffs; } else { selectedToggle = ToggleChoice.None; }
                    }
                    var showAbilities = ch == selectedCharacter && selectedToggle == ToggleChoice.Abilities;
                    if (DisclosureToggle("Abilities".localize(), ref showAbilities, 125)) {
                        if (showAbilities) { selectedCharacter = ch; selectedToggle = ToggleChoice.Abilities; } else { selectedToggle = ToggleChoice.None; }
                    }
                    var showSpells = ch == selectedCharacter && selectedToggle == ToggleChoice.Spells;
                    if (DisclosureToggle($"{spellCount} " + "Spells".localize(), ref showSpells, 150)) {
                        if (showSpells) { selectedCharacter = ch; selectedToggle = ToggleChoice.Spells; } else { selectedToggle = ToggleChoice.None; }
                    }
                    var showAI = ch == selectedCharacter && selectedToggle == ToggleChoice.AI;
                    ReflectionTreeView.DetailToggle("Inspect".localize(), ch, ch, 75);
                    Wrap(!isWide, NarrowIndent - 20);
                    ActionsGUI(ch);
                    if (prevSelectedChar != selectedCharacter) {
                        selectedSpellbook = 0;
                    }
                }
                if (!isWide) Div(00, 10);
                5.space();
                ReflectionTreeView.OnDetailGUI(ch);
                //if (!UI.IsWide && (selectedToggle != ToggleChoice.Stats || ch != selectedCharacter)) {
                //    UI.Div(20, 20);
                //}
                if (selectedCharacter != spellbookEditCharacter) {
                    editSpellbooks = false;
                    spellbookEditCharacter = null;
                }
                if (selectedCharacter != multiclassEditCharacter) {
                    editMultiClass = false;
                    multiclassEditCharacter = null;
                }
                if (ch == selectedCharacter) {

                    if (selectedToggle == ToggleChoice.Classes) {
                        OnClassesGUI(ch, classData, selectedCharacter);
                    } else if (selectedToggle == ToggleChoice.Stats) {
                        todo = OnStatsGUI(ch);
                    } else if (selectedToggle == ToggleChoice.Features) {
                        todo = FactsEditor.OnGUI(ch, ch.Progression.Features.Enumerable.ToList());
                    } else if (selectedToggle == ToggleChoice.Buffs) {
                        todo = FactsEditor.OnGUI(ch, ch.Descriptor().Buffs.Enumerable.ToList());
                    } else if (selectedToggle == ToggleChoice.Abilities) {
                        todo = FactsEditor.OnGUI(ch, ch.Descriptor().Abilities.Enumerable, ch.Descriptor.ActivatableAbilities.Enumerable);
                    } else if (selectedToggle == ToggleChoice.Spells) {
                        todo = OnSpellsGUI(ch, spellbooks);
                    }
                }
                chIndex += 1;
            }
            Space(25);
            if (recruitableCount > 0) {
                Label($"{recruitableCount} " + ("character(s) can be ".Orange().Bold() + "Recruited".Cyan() + ". This allows you to add non party NPCs to your party as if they were mercenaries".Green()).localize());
            }
            if (respecableCount > 0) {
                Label($"{respecableCount} " + ("character(s) can be ".Orange().Bold() + "Respecced".Cyan() + ". Pressing Respec will close the mod window and take you to character level up".Green()).localize());
                Label(("WARNING".Yellow().Bold() + " The Respec UI is ".Orange() + "Non Interruptable".Yellow().Bold() + " please save before using".Orange()).localize());
            }
            if (recruitableCount > 0 || respecableCount > 0) {
                Label(("WARNING".Yellow().Bold() + " these features are ".Orange() + "EXPERIMENTAL".Yellow().Bold() + " and uses unreleased and likely buggy code.".Orange()).localize());
                Label(("BACK UP".Yellow().Bold() + " before playing with this feature.You will lose your mythic ranks but you can restore them in this Party Editor.".Orange()).localize());
            }
            Space(25);
            foreach (var action in todo)
                action();
            if (charToAdd != null) { UnitEntityDataUtils.AddCompanion(charToAdd); }
            if (charToRecruit != null) { UnitEntityDataUtils.RecruitCompanion(charToRecruit); }
            if (charToRemove != null) { UnitEntityDataUtils.RemoveCompanion(charToRemove); }
            if (charToUnrecruit != null) {
                charToUnrecruit.Ensure<UnitPartCompanion>().SetState(CompanionState.None); charToUnrecruit.Remove<UnitPartCompanion>();
            }
        }
    }
}