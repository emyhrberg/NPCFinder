using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace MinimalCensusExample
{
    public class HousingClickLoggerSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            Mod.Logger.Debug("ModifyInterfaceLayers called.");

            // Insert after "Vanilla: Inventory"
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (index != -1)
            {
                layers.Insert(index + 1, new LegacyGameInterfaceLayer(
                    "MinimalCensusExample: DetectHousingNPCClick",
                    delegate
                    {
                        DetectNPCClickInHousing();
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
                Mod.Logger.Debug("Successfully inserted the DetectHousingNPCClick layer.");
            }
            else
            {
                Mod.Logger.Debug("Could not find layer 'Vanilla: Inventory'. No insertion made.");
            }
        }

        private void DetectNPCClickInHousing()
        {
            Mod.Logger.Debug("DetectNPCClickInHousing: Checking if inventory is open & equip page is 1.");

            // In vanilla 1.4, the Housing UI is part of the Equipment tab (EquipPage == 1).
            if (!Main.playerInventory || Main.EquipPage != 1)
            {
                Mod.Logger.Debug($"Not in housing UI. Main.playerInventory={Main.playerInventory}, Main.EquipPage={Main.EquipPage}");
                return;
            }

            Mod.Logger.Debug("We appear to be in the housing UI. Beginning bounding-box checks...");

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

                Mod.Logger.Debug($"Considering NPC {npc.FullName} (type={npc.type}, whoAmI={npc.whoAmI}). drawnCount={drawnCount}");

                // Compute bounding box
                int iconPosX = startX;
                int iconPosY = startY + (iconSize + iconPadding) * drawnCount;

                // Check if mouse is within that box
                bool overIcon = (Main.mouseX >= iconPosX && Main.mouseX <= iconPosX + iconSize &&
                                 Main.mouseY >= iconPosY && Main.mouseY <= iconPosY + iconSize);

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

            Mod.Logger.Debug($"DetectNPCClickInHousing finished. Total drawn town NPC icons: {drawnCount}");
        }
    }
}
