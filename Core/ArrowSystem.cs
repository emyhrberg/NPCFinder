using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FindMyNPCs.Core
{
    public class ArrowSystem : ModSystem
    {
        private Texture2D _arrowTexture;
        private NPC _targetNPC;

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
        }

        public void ClearTargetNPC()
        {
            _targetNPC = null;
        }

        private void DrawArrowToNPC(SpriteBatch sb)
        {
            Player player = Main.LocalPlayer;
            Vector2 screenCenter = new(Main.screenWidth / 2f, Main.screenHeight / 2f);

            if (_targetNPC == null || !_targetNPC.active)
            {
                return; // Avoid drawing if NPC is not active
            }

            // Calculate distance/direction from player to NPC
            Vector2 offset = _targetNPC.Center - player.Center;
            float worldDistance = offset.Length();
            if (worldDistance < 320f) // Skip if too close (less than 20 tiles)
                return;

            offset.Normalize();

            // Arrow is drawn ~200px from screen center (or closer if NPC is near)
            float arrowDist = 200f;
            if (worldDistance < arrowDist)
                arrowDist = worldDistance;

            Vector2 arrowPos = screenCenter + offset * arrowDist;
            float rotation = offset.ToRotation();
            Vector2 origin = _arrowTexture.Size() / 2f;

            // Draw the arrow
            sb.Draw(_arrowTexture, arrowPos, null, Color.White, rotation, origin, 1f, SpriteEffects.None, 0f);

            // Draw text showing "Name (Distance in tiles)"
            float tileDistance = worldDistance / 16f;
            string labelText = $"{_targetNPC.FullName} ({tileDistance:0} tiles)";
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Vector2 textOffset = new Vector2(32f, -8f);
            Vector2 textPosition = arrowPos + textOffset;
            sb.DrawString(font, labelText, textPosition, Color.White);

            // ✅ Get the correct NPC head dynamically
            int headID = Terraria.GameContent.TownNPCProfiles.GetHeadIndexSafe(_targetNPC);

            // ✅ Ensure head ID is valid before drawing
            if (headID >= 0 && headID < TextureAssets.NpcHead.Length && TextureAssets.NpcHead[headID].IsLoaded)
            {
                Texture2D npcHeadTexture = TextureAssets.NpcHead[headID].Value;
                if (npcHeadTexture != null)
                {
                    Vector2 headOffset = new Vector2(-npcHeadTexture.Width - 6, -8f); // Shift left
                    Vector2 headPosition = arrowPos + headOffset;
                    sb.Draw(npcHeadTexture, headPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }

            // Debug log
            Mod.Logger.Debug($"Tracking NPC: {_targetNPC.FullName} | Head ID: {headID}");
        }
    }
}
