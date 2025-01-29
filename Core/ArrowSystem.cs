using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent; // For FontAssets
using Terraria.ID;
using Terraria.ModLoader;

namespace FindMyNPCs.Core
{
    public class TheSystem : ModSystem
    {
        private Texture2D _arrowTexture;

        public override void Load()
        {
            _arrowTexture = ModContent
                .Request<Texture2D>("FindMyNPCs/Assets/Arrow", AssetRequestMode.ImmediateLoad)
                .Value;
        }

        public override void Unload()
        {
            _arrowTexture = null;
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (_arrowTexture == null)
                return;

            Player player = Main.LocalPlayer;
            Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);

            // Loop over NPCs. We'll only handle town NPCs here.
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.townNPC)
                    continue;

                // Calculate distance/direction from player to NPC
                Vector2 offset = npc.Center - player.Center;
                float worldDistance = offset.Length();
                if (worldDistance < 1f)
                    continue; // skip if extremely close

                offset.Normalize();

                // Arrow is drawn ~200px from screen center (or closer if NPC is near).
                float arrowDist = 200f;
                if (worldDistance < arrowDist)
                    arrowDist = worldDistance;

                Vector2 arrowPos = screenCenter + offset * arrowDist;
                float rotation = offset.ToRotation();
                Vector2 origin = _arrowTexture.Size() / 2f;

                // Draw the arrow
                spriteBatch.Draw(
                    _arrowTexture,
                    arrowPos,
                    null,
                    Color.White,
                    rotation,
                    origin,
                    1f,
                    SpriteEffects.None,
                    0f
                );

                // Draw text showing "Name (Distance in tiles)"
                // Convert world distance (pixels) -> tile distance (16px = 1 tile)
                float tileDistance = worldDistance / 16f;
                string labelText = $"{npc.GivenName} ({tileDistance:0.0} tiles)";

                // Place text to the right of the arrow (shift by e.g. +24 in X)
                Vector2 textOffset = new Vector2(24f, -8f);
                Vector2 textPosition = arrowPos + textOffset;

                // We can use the built-in MouseText font:
                DynamicSpriteFont font = FontAssets.MouseText.Value;

                // DrawString expects color. We'll use white here:
                spriteBatch.DrawString(font, labelText, textPosition, Color.White);
            }
        }
    }
}
