using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using Terraria;
using TerrariaInjector;
using Terraria.Chat;
using Terraria.Localization;

namespace TerraSocket
{
    public static class Commands
    {
        public static void CommandHandler(string response)
        {
            try
            {
                CommandModel cm = JsonConvert.DeserializeObject<CommandModel>(response);
                switch (cm.Command.ToLower())
                {
                    case "giveitem":
                        GiveItem(cm.UserName, cm.ItemID);
                        break;
                    case "killplayer":
                        KillPlayer(cm.UserName);
                        break;
                    case "healplayer":
                        HealPlayer(cm.UserName, cm.HealAmount);
                        break;
                    default:
                        throw new Exception(String.Format("Unknown command. {0}.",cm.Command));
                }
            }
            catch (Exception e)
            {
                GM.Logger.Error("Error while parsing command", e);
            }
        }

        public static void GiveItem(string sourceUser, int id)
        {
            try
            {
                Player player = Main.player[Main.myPlayer];
                Item item = new Item();
                item.netDefaults(id);
                player.GetItem(player.whoAmI, item, GetItemSettings.PickupItemFromWorld);
                NetworkText text = NetworkText.FromLiteral(String.Format("{0} has given you a {1}.",sourceUser,item.Name));
                ChatHelper.DisplayMessageOnClient(text, Color.Cyan, Main.myPlayer);
            }
            catch (Exception e)
            {
                GM.Logger.Error("Error giving player an item", e);
            }
        }

        public static void KillPlayer(string sourceUser)
        {
            try
            {
                NetworkText text = NetworkText.FromLiteral(String.Format("{0} has sent a Dungeon Guardian after you!",sourceUser));
                ChatHelper.DisplayMessageOnClient(text, Color.Red, Main.myPlayer);
                Player player = Main.player[Main.myPlayer];
                Helper.SpawnKillNpc(player.Center.X, player.Center.Y, 68);

            }
            catch (Exception e)
            {

                GM.Logger.Error("Error Killing player", e);
            }
        }
        public static void HealPlayer(string sourceUser, int amount)
        {
            amount = Math.Abs(amount);
            Player player = Main.player[Main.myPlayer];
            player.statLife += amount;
            NetworkText text = NetworkText.FromLiteral(String.Format("{0} has healed you {1} hp.", sourceUser, amount) );
            ChatHelper.DisplayMessageOnClient(text, Color.Green, Main.myPlayer);
        }
    }
}
