using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace FindMyNPCs.Core
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // Config ideas:
        // 1. Include town slimes
        // 2. Full Name, Short Name, or Type Name for NPC display
        // 3. Draw arrow

        [Header("Config")]

        [DrawTicks]
        [OptionStrings(["Full Name", "Given Name", "Type Name"])]
        [DefaultValue("Type Name")]
        [Tooltip("How should the NPC name be displayed? Full = 'Andrew the Guide', Given = 'Andrew', Type = 'Guide'")]
        public string NPCNameFormat;

        [DefaultValue(false)]
        [Tooltip("Should the arrow be disabled?")]
        public bool DisableArrow { get; set; }

        // If the player is within 10 tiles (approximately 10ft) of the NPC,
        // this option determines whether or not the target NPC should continue to be tracked.
        // When false (default), the target NPC is removed once the player gets close.
        [Tooltip("If the player is within 10 tiles of the NPC, should the target NPC continue to be tracked?")]
        [DefaultValue(false)]
        public bool KeepShowingNPCAfterFound { get; set; }
    }
}
