using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace MinimalCensusExample
{
    public class ClickSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (index != -1)
            {
                layers.Insert(index + 1, new LegacyGameInterfaceLayer(
                    "ClickSystem: UI",
                    delegate
                    {
                        DetectNPCClickInHousing();
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }

        private void DetectNPCClickInHousing()
        {
            // In vanilla 1.4, the Housing UI is part of the Equipment tab (EquipPage == 1).
            if (Main.playerInventory && Main.EquipPage == 1)
            {
                Mod.Logger.Debug("Housing UI opened. Beginning bounding-box checks...");

                // Approximate bounding box near top-right corner (older style).
                int iconSize = 32;
                int iconPadding = 4;
                int startX = Main.screenWidth - 64 - 28; // top-right-ish
                int startY = 65;

                int drawnCount = 0;

                // Loop over NPCs
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    // Must be active, a town NPC, and not a pet
                    if (!npc.active || !npc.townNPC || NPCID.Sets.IsTownPet[npc.type])
                        continue;

                    // Mod.Logger.Debug($"Considering NPC {npc.FullName} (type={npc.type}, whoAmI={npc.whoAmI}). drawnCount={drawnCount}");

                    // Compute bounding box
                    int iconPosX = startX;
                    int iconPosY = startY + (iconSize + iconPadding) * drawnCount;

                    // Check if mouse is within that box
                    bool overIcon = Main.mouseX >= iconPosX && Main.mouseX <= iconPosX + iconSize &&
                                     Main.mouseY >= iconPosY && Main.mouseY <= iconPosY + iconSize;

                    if (overIcon)
                    {
                        Mod.Logger.Debug($"Mouse is over NPC {npc.FullName}'s icon. Checking for left-click...");

                        if (Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            // We log the click
                            Mod.Logger.Info($"Clicked NPC: {npc.FullName} (type={npc.type}, whoAmI={npc.whoAmI}).");
                        }
                        else
                        {
                            Mod.Logger.Debug("Mouse is over icon, but not clicked this frame.");
                        }
                    }

                    drawnCount++;
                }
            }
        }
    }
}
