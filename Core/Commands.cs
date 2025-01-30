using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

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
            if (args.Length != 1)
            {
                Main.NewText("Usage: /find <name>", Color.Yellow);
                Main.NewText("Available NPCs:", Color.Yellow);

                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && npc.townNPC && npc.type != NPCID.OldMan)
                    {
                        Main.NewText($"/find {npc.GivenName.ToLower()} ([c/FFF014:{npc.FullName}])");
                    }
                }

                // Remove arrow since no valid input
                arrowSystem.ClearTargetNPC();
                return;
            }

            string targetName = args[0].ToLower();

            // Find NPC by name
            NPC foundNPC = null;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.townNPC && npc.GivenName.ToLower() == targetName)
                {
                    foundNPC = npc;
                    break;
                }
            }

            // If NPC not found, show message
            if (foundNPC == null)
            {
                Main.NewText($"NPC with name '{args[0]}' not found.", Color.Red);
                return;
            }

            // Set the NPC target in ArrowSystem
            arrowSystem.SetTargetNPC(foundNPC);
        }
    }
}
