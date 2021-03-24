﻿using UnityModManagerNet;

namespace ToyBox {
    public class Settings : UnityModManager.ModSettings {
        // Cheap Tricks
        public bool highlightObjectsToggle = false;


        // Party Editor
        public int selectedPartyFilter = 0;

        // Blueprint Browser
        public int searchLimit = 100;
        public int selectedBPTypeFilter = 1;
        public string searchText = "";

        // Previews (Dialogs, Events ,etc)

        public bool previewEventResults = false;
        public bool previewDialogResults = false;
        public bool previewAlignmentRestrictedDialog = false;
        public bool previewRandomEncounters = false;

        // Quests
        public bool hideCompleted = true;

        // Other
        public bool settingShowDebugInfo = true;

        public override void Save(UnityModManager.ModEntry modEntry) {
            Save(this, modEntry);
        }
    }
}