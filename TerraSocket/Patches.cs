﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using TerrariaInjector;
using Terraria.ID;

namespace TerraSocket
{
    [HarmonyPatch]
    public class Patches
    {
        public static WebSocketServerHelper _server { get; set; }
        
        //Stop WS server on "exit"
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terraria.Social.SocialAPI), "Shutdown")]
        public static void CloseWebSocketPostfix()
        {
            GM.Logger.Info("WebSocket Closing");

            _server.CloseServer();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "LoadPlayer")]
        public static void LoadPlayerPostfix(string playerPath, bool cloudSave)
        {
            Main.Achievements.OnAchievementCompleted += Achievements_OnAchievementCompleted;
        }

        private static void Achievements_OnAchievementCompleted(Terraria.Achievements.Achievement achievement)
        {

            _server.SendWSMessage(new WebSocketMessageModel("AchievementCompleted", true, new WebSocketMessageModel.ContextInfo(Main.player[Main.myPlayer].name, achievement.Name)));
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Hurt")]
        public static void PlayerHurtPostfix(ref Player __instance, PlayerDeathReason damageSource, int Damage, int hitDirection, bool pvp = false, bool quiet = false, bool Crit = false, int cooldownCounter = -1)
        {
            string playerName = __instance.name;
            if(__instance.statLife <= 0)
            {
                return;
            }
            bool TryGetCausingEntity = damageSource.TryGetCausingEntity(out Entity entity);
            if(TryGetCausingEntity){
                string sourceType = string.Empty;
                string sourceName = string.Empty;
                if (Main.npc.IndexInRange(entity.whoAmI))
                {
                    NPC npc = Main.npc[entity.whoAmI];
                    sourceType = "NPC";
                    sourceName = npc.FullName;
                    _server.SendWSMessage(new WebSocketMessageModel("PlayerHurt", true, new WebSocketMessageModel.ContextInfo(playerName, new WebSocketMessageModel.ContextInfo.ContextPlayerDamage(playerName, Damage, Crit, pvp, quiet, hitDirection, sourceType, sourceName))));
                    return;
                }
                if (Main.projectile.IndexInRange(entity.whoAmI))
                {
                    Projectile projectile = Main.projectile[entity.whoAmI];
                    sourceType = "PROJECTILE";
                    sourceName = projectile.Name;
                    _server.SendWSMessage(new WebSocketMessageModel("PlayerHurt", true, new WebSocketMessageModel.ContextInfo(playerName, new WebSocketMessageModel.ContextInfo.ContextPlayerDamage(playerName, Damage, Crit, pvp, quiet, hitDirection, sourceType, sourceName))));
                    return;
                }
                if (Main.player.IndexInRange(entity.whoAmI))
                {
                    Player player = Main.player[entity.whoAmI];
                    sourceType = "PLAYER";
                    sourceName = player.name;
                    _server.SendWSMessage(new WebSocketMessageModel("PlayerHurt", true, new WebSocketMessageModel.ContextInfo(playerName, new WebSocketMessageModel.ContextInfo.ContextPlayerDamage(playerName, Damage, Crit, pvp, quiet, hitDirection, sourceType, sourceName))));
                    return;
                }
            }
            _server.SendWSMessage(new WebSocketMessageModel("PlayerHurt", true, new WebSocketMessageModel.ContextInfo(playerName, new WebSocketMessageModel.ContextInfo.ContextPlayerDamage(playerName, Damage, Crit, pvp, quiet, hitDirection, "NOTENTITY"))));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player),"KillMe")]
        public static void PlayerKilledPostFix(ref Player __instance, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp = false)
        {

            string playername = __instance.name;
            if (damageSource.TryGetCausingEntity(out Entity entity))
            {
                string sourceType = string.Empty;
                string sourceName = string.Empty;
                if (Main.npc.IndexInRange(entity.whoAmI))
                {
                    NPC npc = Main.npc[entity.whoAmI];
                    sourceType = "NPC";
                    sourceName = npc.FullName;
                    _server.SendWSMessage(new WebSocketMessageModel("PlayerKilled", true, new WebSocketMessageModel.ContextInfo(playername, new WebSocketMessageModel.ContextInfo.ContextPlayerKilled(playername, sourceType, sourceName, __instance.statLife + (int)dmg, (int)dmg, __instance.statLife))));
                    return;
                }
                if (Main.projectile.IndexInRange(entity.whoAmI))
                {
                    Projectile projectile = Main.projectile[entity.whoAmI];
                    sourceType = "PROJECTILE";
                    sourceName = projectile.Name;
                    _server.SendWSMessage(new WebSocketMessageModel("PlayerKilled", true, new WebSocketMessageModel.ContextInfo(playername, new WebSocketMessageModel.ContextInfo.ContextPlayerKilled(playername, sourceType, sourceName, __instance.statLife + (int)dmg, (int)dmg, __instance.statLife))));
                    return;
                }
                if (Main.player.IndexInRange(entity.whoAmI))
                {
                    Player player = Main.player[entity.whoAmI];
                    sourceType = "PLAYER";
                    sourceName = player.name;
                    _server.SendWSMessage(new WebSocketMessageModel("PlayerKilled", true, new WebSocketMessageModel.ContextInfo(playername, new WebSocketMessageModel.ContextInfo.ContextPlayerKilled(playername, sourceType, sourceName, __instance.statLife + (int)dmg, (int)dmg, __instance.statLife))));
                    return;
                }
            }
            _server.SendWSMessage(new WebSocketMessageModel("PlayerKilled", true, new WebSocketMessageModel.ContextInfo(playername, new WebSocketMessageModel.ContextInfo.ContextPlayerKilled(playername, "NOTENTITY", null, __instance.statLife + (int)dmg, (int)dmg, __instance.statLife))));

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.OnHit))]
        public static void OnHitPostfix(float x, float y, Entity victim, ref Player __instance)
        {
            try
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
            catch (Exception e)
            {
                GM.Logger.Error($"OnHit failed. Entity: {victim}", e);
            }
            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnKillNPC")]
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPC), "NewNPC")]
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
        [HarmonyPatch(typeof(Main), "StartSlimeRain")]
        public static void RainSlimeEventPostfix(bool announce = true)
        {
            _server.SendWSMessage(new WebSocketMessageModel("SlimeRainEvent", true));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Main), "AnglerQuestSwap")]
        public static void NewAnglerQuestPostfix()
        {
            _server.SendWSMessage(new WebSocketMessageModel("AnglerQuestReset", true));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "PetAnimal")]
        public static void PetAnimalPostfix(int animalNpcIndex, ref Player __instance)
        {
            NPC pet = Main.npc[animalNpcIndex];
            string petName = pet.FullName;
            _server.SendWSMessage(new WebSocketMessageModel("PetPat", true, new WebSocketMessageModel.ContextInfo(__instance.name, petName)));
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

    [HarmonyPatch]
    class UpdatePatch
    {
        public static bool EventIsPartyUp { get; private set; }
        public static bool EventIsSandstormThere { get; private set; }
        public static bool EventIsDD2There { get; private set; }
        public static bool EventIsPumpkinMoonThere { get; private set; }
        public static bool EventIsSnowMoonThere { get; private set; }
        public static bool EventIsBloodMoonThere { get; private set; }
        static MethodBase TargetMethod()
        {
            return typeof(Main).GetMethod("DoUpdateInWorld", BindingFlags.NonPublic|BindingFlags.Instance);
        }
        static void Prefix()
        {
            if (ModHelpers.Tools.IsLocalPlayerFreeForAction())
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
    }
}
