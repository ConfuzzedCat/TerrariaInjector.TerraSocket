using HarmonyLib;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Events;

namespace TerraSocket
{
    [HarmonyPatch]
    public class Patches
    {
        public static WebSocketServerHelper _server { get; set; }

        // Start WS server at launch
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Program), nameof(Program.LaunchGame))]
        public static void StartWebSocketPreLaunch(string[] args, bool monoArgs = false)
        {
            string ipPath = Path.Combine(Directory.GetCurrentDirectory(), "wsipconfig.json");
            Logger.Info(ipPath);
            _server = new WebSocketServerHelper();
        }

        //Stop WS server on "exit"
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terraria.Social.SocialAPI), nameof(Terraria.Social.SocialAPI.Shutdown))]
        public static void CloseWebSocketPostfix()
        {
            Logger.Info("WebSocket Closing");
            _server.CloseServer();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.LoadPlayer))]
        public static void LoadPlayerPostfix(string playerPath, bool cloudSave)
        {
            Main.Achievements.OnAchievementCompleted += Achievements_OnAchievementCompleted;
        }

        private static void Achievements_OnAchievementCompleted(Terraria.Achievements.Achievement achievement)
        {
            _server.SendWSMessage(new WebSocketMessageModel("AchievementCompleted", true, new WebSocketMessageModel.ContextInfo(Main.player[Main.myPlayer].name, achievement.Name)));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.OnHit))]
        public static void OnHitPostfix(float x, float y, Entity victim, ref Player __instance)
        {
            if (victim is NPC _victim)
            {
                string npcName = _victim.FullName;
                string playerName = __instance.name;
                if (Helper.IsItemNull(__instance, out Item item))
                {
                    int damage = Helper.CalculateDamageForNPC(item.damage, _victim.defense);
                    string itemName = item.HoverName;
                    _server.SendWSMessage(new WebSocketMessageModel("NPCHit", true, new WebSocketMessageModel.ContextInfo(playerName, new WebSocketMessageModel.ContextInfo.ContextNPCDamage(npcName, null, itemName, playerName, _victim.life + damage, damage))));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.OnKillNPC))]
        public static void OnKillNPCPostfix(ref NPCKillAttempt attempt, object externalKillingBlowSource, ref Player __instance)
        {
            if (attempt.DidNPCDie())
            {
                NPC npc = attempt.npc;
                string npcName = npc.FullName;
                string playerName = __instance.name;
                Item item = externalKillingBlowSource as Item;
                int damage = Helper.CalculateDamageForNPC(item.damage, npc.defense);
                _server.SendWSMessage(new WebSocketMessageModel("NPCKill", true, new WebSocketMessageModel.ContextInfo(playerName, new WebSocketMessageModel.ContextInfo.ContextNPCKilled(npcName, item.HoverName, null, item.Name, playerName, npc.life + damage, npc.life))));
            }
        }

        [HarmonyPatch]
        public class MainMenuPatch
        {
            public static MethodBase TargetMethod()
            {
                return typeof(Main).GetMethod("DrawVersionNumber", BindingFlags.NonPublic | BindingFlags.Static);
            }
            public static void Postfix(Color menuColor)
            {
                Helper.DrawModOnMenu(menuColor);
            }
        }
        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        public static void EventsPostfix(int i)
        {            
            
            if (BirthdayParty.PartyIsUp)
            {
                if (!EventIsPartyUp)
                {
                    EventIsPartyUp = true;
                    _server.SendWSMessage(new WebSocketMessageModel("PartyEvent", true));
                }
            }
            else
            {
                EventIsPartyUp = false;
            }

            if (Sandstorm.Happening)
            {
                if (!EventIsSandstormThere)
                {
                    EventIsSandstormThere = true;
                    _server.SendWSMessage(new WebSocketMessageModel("SandstormEvent", true));
                }
            }
            else
            {
                EventIsSandstormThere = false;
            }

            if (DD2Event.Ongoing)
            {
                if (!EventIsDD2There)
                {
                    _server.SendWSMessage(new WebSocketMessageModel("DD2Event", true));
                }
            }
            else
            {
                EventIsDD2There = false;
            }

            if (Main.pumpkinMoon)
            {
                if (!EventIsPumpkinMoonThere)
                {
                    EventIsPumpkinMoonThere = true;
                    _server.SendWSMessage(new WebSocketMessageModel("PumpkinMoonEvent", true));
                }
            }
            else
            {
                EventIsPumpkinMoonThere = false;
            }

            if (Main.snowMoon)
            {
                if (!EventIsSnowMoonThere)
                {
                    EventIsSnowMoonThere = true;
                    _server.SendWSMessage(new WebSocketMessageModel("SnowMoonEvent", true));
                }
            }
            else
            {
                EventIsSnowMoonThere = false;
            }<dnsy

            if (Main.bloodMoon)
            {
                if (!EventIsBloodMoonThere)
                {
                    EventIsBloodMoonThere = true;
                    _server.SendWSMessage(new WebSocketMessageModel("BloodMoonEvent", true));
                }
            }
            else
            {
                EventIsBloodMoonThere = false;
            }
        } */

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPC), nameof(NPC.NewNPC))]
        public static void NPCSpawnPostfix(int __result, int X, int Y, int Type, int Start = 0, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f, int Target = 255)
        {
            NPC npc = Main.npc[__result];
            string npcName = npc.FullName;
            int npcLife = npc.life;
            if (npc.boss)
            {
                _server.SendWSMessage(new WebSocketMessageModel("BossSpawn", true, new WebSocketMessageModel.ContextInfo(null, new WebSocketMessageModel.ContextInfo.ContextBossSpawn(npcName, npcLife))));
            }
            if (__result == 437)
            {
                _server.SendWSMessage(new WebSocketMessageModel("CultistRitualStarted", true));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WorldGen), nameof(WorldGen.meteor))]
        public static void MeteorPostfix(int i, int j, ref bool __result)
        {
            if (__result)
            {
                _server.SendWSMessage(new WebSocketMessageModel("MeteorLanded", true));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WorldGen), nameof(WorldGen.TriggerLunarApocalypse))]
        public static void LunarApocalypsePostfix()
        {
            _server.SendWSMessage(new WebSocketMessageModel("LunarApocalypseStarted", true));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Main), nameof(Main.StartSlimeRain))]
        public static void RainSlimeEventPostfix(bool announce = true)
        {
            _server.SendWSMessage(new WebSocketMessageModel("SlimeRainEvent", true));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Main), nameof(Main.AnglerQuestSwap))]
        public static void NewAnglerQuestPostfix()
        {
            _server.SendWSMessage(new WebSocketMessageModel("AnglerQuestReset", true));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.PetAnimal))]
        public static void PetAnimalPostfix(int animalNpcIndex, ref Player __instance)
        {
            NPC pet = Main.npc[animalNpcIndex];
            string petName = pet.FullName;
            _server.SendWSMessage(new WebSocketMessageModel("PetPat", true, new WebSocketMessageModel.ContextInfo(__instance.name, petName)));
        }
    }

    [HarmonyPatch]
    class updatePatch
    {
        public static bool EventIsPartyUp { get; private set; } = false;
        public static bool EventIsSandstormThere { get; private set; } = false;
        public static bool EventIsDD2There { get; private set; } = false;
        public static bool EventIsPumpkinMoonThere { get; private set; } = false;
        public static bool EventIsSnowMoonThere { get; private set; } = false;
        public static bool EventIsBloodMoonThere { get; private set; } = false;
        static MethodBase TargetMethod()
        {
            return typeof(Main).GetMethod("Update", BindingFlags.NonPublic|BindingFlags.Instance);
        }
        static void Prefix() 
        {
            if (BirthdayParty.PartyIsUp)
            {
                if (!EventIsPartyUp)
                {
                    EventIsPartyUp = true;
                    Patches._server.SendWSMessage(new WebSocketMessageModel("PartyEvent", true));
                }
            }
            else
            {
                EventIsPartyUp = false;
            }

            if (Sandstorm.Happening)
            {
                if (!EventIsSandstormThere)
                {
                    EventIsSandstormThere = true;
                    Patches._server.SendWSMessage(new WebSocketMessageModel("SandstormEvent", true));
                }
            }
            else
            {
                EventIsSandstormThere = false;
            }

            if (DD2Event.Ongoing)
            {
                if (!EventIsDD2There)
                {
                    Patches._server.SendWSMessage(new WebSocketMessageModel("DD2Event", true));
                }
            }
            else
            {
                EventIsDD2There = false;
            }

            if (Main.pumpkinMoon)
            {
                if (!EventIsPumpkinMoonThere)
                {
                    EventIsPumpkinMoonThere = true;
                    Patches._server.SendWSMessage(new WebSocketMessageModel("PumpkinMoonEvent", true));
                }
            }
            else
            {
                EventIsPumpkinMoonThere = false;
            }

            if (Main.snowMoon)
            {
                if (!EventIsSnowMoonThere)
                {
                    EventIsSnowMoonThere = true;
                    Patches._server.SendWSMessage(new WebSocketMessageModel("SnowMoonEvent", true));
                }
            }
            else
            {
                EventIsSnowMoonThere = false;
            }

            if (Main.bloodMoon)
            {
                if (!EventIsBloodMoonThere)
                {
                    EventIsBloodMoonThere = true;
                    Patches._server.SendWSMessage(new WebSocketMessageModel("BloodMoonEvent", true));
                }
            }
            else
            {
                EventIsBloodMoonThere = false;
            }
        }
    }




    public class UpdatePatch : Game
    {
        
        protected override void Update(GameTime gameTime)
        {

            
            base.Update(gameTime);
        }
    }
}
