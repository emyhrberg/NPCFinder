using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace FindMyNPCs.Core
{
    public class ArrowSystem : ModSystem
    {
        private Texture2D _arrowTexture;
        private NPC _targetNPC;

        public override void Load()
        {
            _arrowTexture = ModContent.Request<Texture2D>("FindMyNPCs/Assets/Arrow", AssetRequestMode.ImmediateLoad).Value;
        }

        public override void Unload()
        {
            _arrowTexture = null;
        }

        public override void PostDrawInterface(SpriteBatch sb)
        {
            // if no target npc, set to nurse
            // (TESTING PURPOSES ONLY)
            // if (_targetNPC == null)
            // {
            //     foreach (NPC npc in Main.npc)
            //     {
            //         if (npc.active && npc.townNPC && npc.TypeName.ToLower() == "nurse")
            //         {
            //             _targetNPC = npc;
            //             break;
            //         }
            //     }
            // }

            if (_targetNPC != null && _arrowTexture != null && _targetNPC.active)
            {
                DrawArrowToNPC(sb);
            }
        }

        public void SetTargetNPC(NPC npc)
        {
            _targetNPC = npc;
        }

        public void ClearTargetNPC()
        {
            _targetNPC = null;
        }

        private void DrawArrowToNPC(SpriteBatch sb)
        {
            if (_targetNPC == null || !_targetNPC.active)
                return; // Avoid drawing if NPC is not active

            Player player = Main.LocalPlayer;
            Vector2 screenCenter = new(Main.screenWidth / 2f, Main.screenHeight / 2f);

            // Calculate distance/direction from player to NPC
            Vector2 offset = _targetNPC.Center - player.Center;
            float worldDistance = offset.Length();

            offset.Normalize();

            // Calculate arrow distance based on NPC proximity
            float minDist = 0f;  // Minimum arrow distance from player
            float maxDist = 200f; // Maximum arrow distance from player
            float minRange = 160f; // 10ft = 160 pixels
            float arrowDist = maxDist;

            if (worldDistance < minRange)
            {
                // Gradually reduce arrow distance as player gets closer
                float t = worldDistance / minRange;
                arrowDist = MathHelper.Lerp(minDist, maxDist, t);
                // log distance
                Mod.Logger.Debug($"Distance: {worldDistance / 16f:0} ft | Arrow Distance: {arrowDist}");
            }

            // Calculate arrow pos
            Vector2 arrowPos = screenCenter + offset * arrowDist;

            // Draw arrow if config says so
            Config c = ModContent.GetInstance<Config>();
            if (c.DrawArrow)
                sb.Draw(_arrowTexture, arrowPos, null, Color.White, offset.ToRotation(), _arrowTexture.Size() / 2f, 1f, SpriteEffects.None, 0f);

            // Drawstring with NPC name
            string npcName = SetNPCNameByFormat(_targetNPC);
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Vector2 textSize = font.MeasureString(npcName);
            Vector2 npcNamePos = new(arrowPos.X - textSize.X / 2f, arrowPos.Y + 20f);

            // Draw outline
            Color outlineColor = Color.Black;
            Vector2[] offsets = {
                new Vector2(-1, -1), new Vector2(1, -1),
                new Vector2(-1, 1), new Vector2(1, 1)
            };
            foreach (var outlineOffset in offsets)
            {
                sb.DrawString(font, npcName, npcNamePos + outlineOffset, outlineColor);
            }

            // Draw main text
            sb.DrawString(font, npcName, npcNamePos, Color.White);
            // DrawDebugRectangle(sb, npcNamePos, textSize, Color.Blue * 0.5f);

            // Draw NPC head icon centered below the NPC name
            int headID = TownNPCProfiles.GetHeadIndexSafe(_targetNPC);
            if (headID >= 0 && headID < TextureAssets.NpcHead.Length && TextureAssets.NpcHead[headID].IsLoaded)
            {
                Texture2D npcHeadTexture = TextureAssets.NpcHead[headID].Value;
                if (npcHeadTexture != null)
                {
                    // position is exactly same as textPos but Y pixels below
                    Vector2 headPos = new(npcNamePos.X - 20f, npcNamePos.Y + 20f);
                    sb.Draw(npcHeadTexture, headPos, Color.White);
                    // draw transparent red rectangle around dimensions of head icon
                    // DrawDebugRectangle(sb, headPos, new Vector2(npcHeadTexture.Width, npcHeadTexture.Height), Color.Red * 0.5f);

                    // Draw tile distance to the right of head icon
                    string distanceText = $"({worldDistance / 16f:0} ft)";
                    textSize = font.MeasureString(distanceText);
                    // place it exactly to the right of headPos
                    Vector2 tileDistancePos = new(headPos.X + npcHeadTexture.Width + 5f, headPos.Y);
                    // DrawDebugRectangle(sb, tileDistancePos, textSize, Color.Green * 0.5f);

                    // Draw outline
                    foreach (var outlineOffset in offsets)
                    {
                        sb.DrawString(font, distanceText, tileDistancePos + outlineOffset, outlineColor);
                    }

                    // Draw main distance text
                    sb.DrawString(font, distanceText, tileDistancePos, Color.White);
                }
            }

            // Debug log
            // Mod.Logger.Debug($"Tracking NPC: {_targetNPC.FullName} | Head ID: {headID}");
        }

        private void DrawDebugRectangle(SpriteBatch sb, Vector2 pos, Vector2 size, Color color)
        {
            sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y), color);
        }

        private string SetNPCNameByFormat(NPC npc)
        {
            Config c = ModContent.GetInstance<Config>();
            return c.NPCNameFormat switch
            {
                "Full Name" => npc.FullName,
                "Short Name" => npc.GivenOrTypeName,
                "Type Name" => npc.TypeName,
                _ => npc.FullName
            };
        }
    }
}
