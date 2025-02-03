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

namespace NPCFinder.Core
{
    public class ArrowSystem : ModSystem
    {
        private Texture2D _arrowTexture;
        private NPC _targetNPC;

        // Fields for fading out after the NPC is found.
        private bool isFadingOut = false;
        private float fadeOutTimer = 0f;
        private const float fadeDuration = 1f; // 1 second fade-out duration

        public override void Load()
        {
            _arrowTexture = ModContent.Request<Texture2D>("NPCFinder/Assets/Arrow", AssetRequestMode.ImmediateLoad).Value;
        }

        public override void Unload()
        {
            _arrowTexture = null;
            _targetNPC = null;
        }

        // Update the fade-out timer.
        public override void PostUpdateEverything()
        {
            if (isFadingOut)
            {
                // Use the elapsed game time to update the fade timer.
                fadeOutTimer += (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
                if (fadeOutTimer >= fadeDuration)
                {
                    // Once the fade duration is reached, clear the target.
                    ClearTargetNPC();
                    isFadingOut = false;
                }
            }
        }

        public override void PostDrawInterface(SpriteBatch sb)
        {
            if (_targetNPC != null && _arrowTexture != null && _targetNPC.active)
            {
                DrawArrowToNPC(sb);
            }
        }

        public void SetTargetNPC(NPC npc)
        {
            _targetNPC = npc;
            // Reset the fade-out state when a new target is set.
            isFadingOut = false;
            fadeOutTimer = 0f;
        }

        public void ClearTargetNPC()
        {
            _targetNPC = null;
            isFadingOut = false;
            fadeOutTimer = 0f;
        }

        private void DrawArrowToNPC(SpriteBatch sb)
        {
            if (_targetNPC == null || !_targetNPC.active)
                return; // Avoid drawing if NPC is not active

            Player player = Main.LocalPlayer;
            Vector2 screenCenter = new(Main.screenWidth / 2f, Main.screenHeight / 2f);

            // Calculate distance and direction from player to NPC.
            Vector2 offset = _targetNPC.Center - player.Center;
            float worldDistance = offset.Length();
            offset.Normalize();

            Config c = ModContent.GetInstance<Config>();

            // When the player is within 10 tiles (160 pixels) and the config says to stop showing the NPC,
            // start the fade-out process (if not already started).
            if (worldDistance < 10f * 16f && !c.KeepShowingNPCAfterFound)
            {
                if (!isFadingOut)
                {
                    isFadingOut = true;
                    fadeOutTimer = 0f;
                }
            }

            // Compute the base alpha based on distance.
            float distanceAlpha = MathHelper.Clamp(worldDistance / (10f * 16f), 0f, 1f);
            // If fading out, override the alpha to linearly fade to 0 over 1 second.
            float finalAlpha = isFadingOut ? MathHelper.Clamp(1f - fadeOutTimer / fadeDuration, 0f, 1f) : distanceAlpha;

            // Calculate arrow distance from the player.
            float minDist = 0f;   // Minimum arrow distance from player
            float maxDist = 200f; // Maximum arrow distance from player
            float minRange = 15f * 16f; // Range (15 tiles) at which we begin to adjust the arrow's distance
            float arrowDist = maxDist;

            if (worldDistance < minRange)
            {
                // Gradually reduce arrow distance as player gets closer.
                float t = worldDistance / minRange;
                arrowDist = MathHelper.Lerp(minDist, maxDist, t);
                // Log distance for debugging.
                Mod.Logger.Debug($"Distance: {worldDistance / 16f:0} ft | Arrow Distance: {arrowDist}");
            }

            // Compute arrow position.
            Vector2 arrowPos = screenCenter + offset * arrowDist;

            // Draw the arrow if not disabled by config.
            if (c.ShowArrow)
            {
                sb.Draw(_arrowTexture, arrowPos, null, Color.White * finalAlpha, offset.ToRotation(), _arrowTexture.Size() / 2f, 1f, SpriteEffects.None, 0f);
            }

            // Draw the NPC name text below the arrow.
            string npcName = SetNPCNameByFormat(_targetNPC);
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Vector2 textSize = font.MeasureString(npcName);
            Vector2 npcNamePos = new(arrowPos.X - textSize.X / 2f, arrowPos.Y + 20f);

            // Draw text outline.
            Color outlineColor = Color.Black;
            Vector2[] offsets = {
                new Vector2(-1, -1), new Vector2(1, -1),
                new Vector2(-1, 1), new Vector2(1, 1)
            };
            foreach (var outlineOffset in offsets)
            {
                sb.DrawString(font, npcName, npcNamePos + outlineOffset, outlineColor * finalAlpha);
            }

            // Draw the main NPC name text.
            sb.DrawString(font, npcName, npcNamePos, Color.White * finalAlpha);

            // Draw the NPC head icon below the NPC name.
            int headID = TownNPCProfiles.GetHeadIndexSafe(_targetNPC);
            if (headID >= 0 && headID < TextureAssets.NpcHead.Length && TextureAssets.NpcHead[headID].IsLoaded)
            {
                Texture2D npcHeadTexture = TextureAssets.NpcHead[headID].Value;
                if (npcHeadTexture != null)
                {
                    // Position for the head icon.
                    Vector2 headPos = new(npcNamePos.X - 20f, npcNamePos.Y + 20f);
                    sb.Draw(npcHeadTexture, headPos, Color.White * finalAlpha);

                    // Draw the tile distance text to the right of the head icon.
                    string distanceText = $"({worldDistance / 16f:0} ft)";
                    textSize = font.MeasureString(distanceText);
                    Vector2 tileDistancePos = new(headPos.X + npcHeadTexture.Width + 5f, headPos.Y);

                    // Draw outline for the distance text.
                    foreach (var outlineOffset in offsets)
                    {
                        sb.DrawString(font, distanceText, tileDistancePos + outlineOffset, outlineColor * finalAlpha);
                    }
                    // Draw the main distance text.
                    sb.DrawString(font, distanceText, tileDistancePos, Color.White * finalAlpha);
                }
            }
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
