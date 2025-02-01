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
        // 1 Include town slimes
        // 2 Full name, short name, type name
        // 3 Draw arrow

        [Header("Config")]

        [Label("NPC Name Format")]
        [Tooltip("Choose the format of the NPC name, e.g " +
            "Full Name: Amy the Nurse, Short Name: Amy, Type Name: Nurse.")]
        [DrawTicks]
        [OptionStrings(["Full Name", "Short Name", "Type Name"])]
        [DefaultValue("Type Name")]
        public string NPCNameFormat;

        [Label("Draw Arrow")]
        [Tooltip("Draw an arrow to the selected NPC.")]
        [DefaultValue(true)]
        public bool DrawArrow { get; set; }
    }
}