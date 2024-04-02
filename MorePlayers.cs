using BepInEx;
using HarmonyLib;
using System;

using DefaultNamespace;
using System.Reflection;

namespace CWMorePlayers
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MorePlayers : BaseUnityPlugin
    {
        private const string modGUID = "x753.MorePlayers";
        private const string modName = "MorePlayers";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private void Awake()
        {
            harmony.PatchAll();

            Logger.LogInfo($"Plugin {modGUID} is loaded!");
        }

        [HarmonyPatch(typeof(InviteFriendsTerminal))]
        internal class StartOfRoundPatch
        {
            [HarmonyPatch("IsGameFull", MethodType.Getter)]
            [HarmonyPrefix]
            static bool IsGameFullPatch()
            {
                return PlayerHandler.instance.players.Count > 128;
            }
        }

        [HarmonyPatch(typeof(SteamLobbyHandler))]
        internal class SteamLobbyHandlerPatch
        {
            static FieldInfo maxPlayers = typeof(SteamLobbyHandler).GetField("m_MaxPlayers", BindingFlags.NonPublic | BindingFlags.Instance);

            [HarmonyPatch("HostMatch")]
            [HarmonyPrefix]
            static void HostMatchPatch(ref SteamLobbyHandler __instance)
            {
                maxPlayers.SetValue(__instance, 128);
            }
        }

        [HarmonyPatch(typeof(SpawnHandler))]
        internal class SpawnHandlerPatch
        {
            static FieldInfo localSpawnIndex = typeof(SpawnHandler).GetField("m_LocalSpawnIndex", BindingFlags.NonPublic | BindingFlags.Instance);

            [HarmonyPatch("FindLocalSpawnIndex")]
            [HarmonyPostfix]
            static void FindLocalSpawnIndexPatch(ref SpawnHandler __instance)
            {
                int oldValue = (int) localSpawnIndex.GetValue(__instance);
                localSpawnIndex.SetValue(__instance, oldValue % 4);
            }
        }

        [HarmonyPatch(typeof(BedBoss))]
        internal class BedBossPatch
        {
            [HarmonyPatch("AssignBed")]
            [HarmonyPrefix]
            static bool AssignBedPatch(ref BedBoss __instance)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerHandler))]
        internal class PlayerHandlerPatch
        {
            [HarmonyPatch("AllPlayersAsleep")]
            [HarmonyPrefix]
            static bool AllPlayersAsleepPatch(ref PlayerHandler __instance, out bool __result)
            {
                int sleepingPlayers = 0;
                for (int i = 0; i < __instance.playerAlive.Count; i++)
                {
                    if (__instance.playerAlive[i].data.sleepAmount > 0.85f)
                    {
                        sleepingPlayers++;
                    }
                }

                if (sleepingPlayers >= 4 || sleepingPlayers >= __instance.playerAlive.Count)
                {
                    __result = true;
                }
                else
                {
                    __result = false;
                }

                return false;
            }
        }
    }
}
