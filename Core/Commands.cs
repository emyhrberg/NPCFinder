using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace FindMyNPCs.Core
{
    public class Commands : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "find";
        public override string Description => "/find <name>";
        public override string Usage => "/find <name>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            // Get ArrowSystem instance
            ArrowSystem arrowSystem = ModContent.GetInstance<ArrowSystem>();

            // If no valid arguments, show available NPCs
            if (args.Length == 0)
            {
                PrintAllNPCs();

                // Remove arrow since no valid input
                arrowSystem.ClearTargetNPC();
                return;
            }

            string targetName = string.Join(" ", args).ToLower();

            // Find NPC by name
            NPC foundNPC = null;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.townNPC && string.Equals(npc.TypeName, targetName, StringComparison.OrdinalIgnoreCase))
                {
                    foundNPC = npc;
                    break;
                }
            }

            // If NPC not found, show message
            if (foundNPC == null)
            {
                Main.NewText($"NPC of type '{args[0]}' not found.", Color.Red);
                PrintAllNPCs();
                return;
            }

            // NPC found! Add UI arrow to NPC
            // Set the NPC target in ArrowSystem
            arrowSystem.SetTargetNPC(foundNPC);
        }

        private static void PrintAllNPCs()
        {
            Main.NewText("Available NPCs:", Color.Yellow);

            // Filter and sort the active town NPCs (excluding the Old Man) by TypeName
            var validNPCs = Main.npc
                .Where(npc => npc.active && npc.townNPC && npc.type != NPCID.OldMan)
                .OrderBy(npc => npc.TypeName)
                .ToList();

            // Print each NPC with the desired formatting
            foreach (NPC npc in validNPCs)
            {
                Main.NewText($"/find {npc.TypeName.ToLower()} ([c/FFF014:{npc.FullName}])");
            }
        }
    }
}
