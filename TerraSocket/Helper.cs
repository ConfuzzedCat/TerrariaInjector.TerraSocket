using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace TerraSocket
{
    public static class Helper
    {
        public static bool IsItemNull(Player player, out Item item)
        {
            item = player.inventory[player.selectedItem];
            return !(item is null);
        }
        public static int CalculateDamageForNPC(int Damage, int Defense)
        {
            double num = Damage - Defense * 0.5;
            if (num < 1.0)
            {
                num = 1.0;
            }
            return (int)num;
        }
        public static void DrawModOnMenu(Color menuColor)
        {
            string text = string.Format("TerraSocket - v{0}",new WebSocketMessageModel.SemVersion().ToStringDot());
            Vector2 vector = FontAssets.MouseText.Value.MeasureString(text);
            vector.X *= 0.5f;
            vector.Y *= 0.5f;
            for (int i = 0; i < 5; i++)
            {
                Color color = Color.Black;
                if (i == 4)
                {
                    color = menuColor;
                    color.R = (byte)((byte.MaxValue + color.R) / 2);
                    color.G = (byte)((byte.MaxValue + color.R) / 2);
                    color.B = (byte)((byte.MaxValue + color.R) / 2);
                }
                color.A = (byte)((float)color.A * 0.3f);
                int num = 0;
                int num2 = 0;
                if (i == 0)
                {
                    num = -2;
                }
                if (i == 1)
                {
                    num = 2;
                }
                if (i == 2)
                {
                    num2 = -2;
                }
                if (i == 3)
                {
                    num2 = 2;
                }
                Main.spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(Main.screenWidth - vector.X + (float)num - 10f, (float)Main.screenHeight - vector.Y + (float)num2 - 2f), color, 0f, vector, 1f, SpriteEffects.None, 0f);
            }
        }
        public static int SpawnKillNpc(float X, float Y, int Type, int Start = 0)
        {
            int num = -1;
            if (Type == 222 || Type == 245)
            {
                for (int num2 = 199; num2 >= 0; num2--)
                {
                    if (!Main.npc[num2].active)
                    {
                        num = num2;
                        break;
                    }
                }
            }
            else
            {
                for (int i = Start; i < 200; i++)
                {
                    if (!Main.npc[i].active)
                    {
                        num = i;
                        break;
                    }
                }
            }
            if (num >= 0)
            {
                Main.npc[num] = new NPC();
                Main.npc[num].SetDefaults(Type, default(NPCSpawnParams));
                Main.npc[num].damage = 99999;
                Main.npc[num].whoAmI = num;
                Main.npc[num].position.X = (Main.player[Main.myPlayer].Center.X - Main.npc[num].width / 2);
                Main.npc[num].position.Y = Main.player[Main.myPlayer].Center.Y - Main.npc[num].height;
                Main.npc[num].active = true;
                Main.npc[num].timeLeft = 1;
                Main.npc[num].wet = Collision.WetCollision(Main.npc[num].position, Main.npc[num].width, Main.npc[num].height);
                Main.npc[num].ai[0] = 0;
                Main.npc[num].ai[1] = 0;
                Main.npc[num].ai[2] = 0;
                Main.npc[num].ai[3] = 0;
                Main.npc[num].target = 255;
                return num;
            }
            return 200;
        }
    }
}