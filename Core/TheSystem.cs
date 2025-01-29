using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FindMyNPCs.Core
{
    public class TheSystem : ModSystem
    {
        private Texture2D _arrowTexture;

        public override void Load()
        {
            // Load (or request) your arrow texture. Must exist in your mod's folders.
            _arrowTexture = ModContent
                .Request<Texture2D>("FindMyNPCs/Assets/Arrow",
                                    AssetRequestMode.ImmediateLoad)
                .Value;
        }

        public override void Unload()
        {
            _arrowTexture = null;
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;

            // 1) Find the *closest* Arms Dealer.
            NPC armsDealer = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.type == NPCID.ArmsDealer)
                {
                    float distance = Vector2.Distance(npc.Center, player.Center);
                    if (distance < closestDist)
                    {
                        closestDist = distance;
                        armsDealer = npc;
                    }
                }
            }

            // If none found, or distance is extremely small, do nothing.
            if (armsDealer == null || closestDist < 1f)
                return;

            // 2) Compute direction from player to the Arms Dealer.
            Vector2 direction = armsDealer.Center - player.Center;
            direction.Normalize();

            // 3) Decide where on-screen to draw the arrow. We'll put it ~200 pixels
            // from the center of the screen in the direction of the Arms Dealer.
            Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);

            float arrowDistanceFromCenter = 200f;
            if (closestDist < arrowDistanceFromCenter)
            {
                // If NPC is closer than 200px in world distance, clamp it so arrow
                // doesn't overshoot the NPC itself.
                arrowDistanceFromCenter = closestDist;
            }

            Vector2 arrowDrawPos = screenCenter + direction * arrowDistanceFromCenter;

            // 4) Draw the arrow texture, rotated appropriately.
            float rotation = direction.ToRotation();
            Vector2 origin = _arrowTexture.Size() / 2f;

            spriteBatch.Draw(
                _arrowTexture,
                arrowDrawPos,
                null,
                Color.White,
                rotation,
                origin,
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
