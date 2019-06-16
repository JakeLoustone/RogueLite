using Oxide.Core;
using Oxide.Game.Rust.Libraries;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Rogue Lite", "JakeLoustone", "0.0.7")]
    [Description("Rogue Lite is a plugin that kicks and prevents players rejoining if they have no lives left.")]
    class RogueLite : RustPlugin
    {
        private Dictionary<ulong, int> PlayerDeathsDictionary;

        private void Init()
        {
            LoadDefaultMessages();
            LoadConfigVariables();
            LoadPlayerDeathsDictionary();
        }

        #region Hooks
        void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            BasePlayer player = entity.ToPlayer();

            if (player != null && !player.IsNpc)
            {
                IncrementDeath(player);

                int currentDeaths = GetDeathCount(player);

                if (currentDeaths >= configData.Options.MaxLives)
                {
                    Puts(string.Format(lang.GetMessage("PlayerKicked", this, player.UserIDString), player.displayName));

                    PrintToChat(string.Format(lang.GetMessage("PlayerKicked", this, player.UserIDString), player.displayName));

                    if (configData.Options.PlaySoundOnPlayerKick)
                    {
                        foreach (BasePlayer active in BasePlayer.activePlayerList)
                        {
                            Vector3 soundPosition = active.transform.position;
                            soundPosition.y = soundPosition.y + 3;

                            Effect.server.Run(configData.Options.SoundEffect, soundPosition, Vector3.zero, null, false);
                        }
                    }

                    if (!player.IsAdmin)
                    {
                        Player.Kick(player, lang.GetMessage("KickReason", this, player.UserIDString));
                    }
                }
            }
        }

        string CanClientLogin(Network.Connection connection)
        {
            bool IsAdmin = connection.authLevel == 2;

            if (!IsAdmin)
            {
                int deathCount = 0;

                try
                {
                    deathCount = Interface.Oxide.DataFileSystem.ReadObject<int>("RogueLite/" + connection.userid);
                }
                catch (Exception e)
                {

                }

                if (deathCount >= configData.Options.MaxLives)
                {
                    return lang.GetMessage("KickReason", this);
                }
            }

            return null;
        }

        void OnPlayerConnected(Network.Message packet)
        {
            int deathCount = 0;

            try
            {
                deathCount = Interface.Oxide.DataFileSystem.ReadObject<int>("RogueLite/" + packet.connection.userid);
            }
            catch (Exception e)
            {

            }

            PlayerDeathsDictionary.Add(packet.connection.userid, deathCount);
        }

        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            int deathCount = 0;

            PlayerDeathsDictionary.TryGetValue(player.userID, out deathCount);

            Interface.Oxide.DataFileSystem.WriteObject("RogueLite/" + player.UserIDString, deathCount);

            PlayerDeathsDictionary.Remove(player.userID);
        }

        void OnServerSave()
        {
            SavePlayerDeathsDictionary();
        }

        void OnNewSave(string filename)
        {
            if (configData.Options.ClearDeathsAfterWipe)
            {
                ClearDeaths();
            }
        }
        #endregion

        #region Commands
        [ChatCommand("setmaxlives")]
        void SetMaxLivesCommand(BasePlayer player, string command, string[] arguments)
        {
            if (player.IsAdmin)
            {
                int tempInt = 3;

                int.TryParse(arguments[0], out tempInt);

                configData.Options.MaxLives = tempInt;

                SaveConfig(configData);

                SendReply(player, string.Format(lang.GetMessage("SetMaxLives", this, player.UserIDString), tempInt));
            }
        }

        [ChatCommand("cleardeaths")]
        void ClearDeathsCommand(BasePlayer player, string command, string[] arguments)
        {
            if (player.IsAdmin)
            {
                ClearDeaths();

                SendReply(player, lang.GetMessage("ClearDeaths", this, player.UserIDString));
            }
        }

        [ConsoleCommand("cleardeaths")]
        private void ClearDeathsConsoleCommand(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin != true) { return; }

            ClearDeaths();

            Puts(lang.GetMessage("ClearDeaths", this));
        }
        #endregion

        #region Helper Functions
        private void LoadPlayerDeathsDictionary()
        {
            PlayerDeathsDictionary = new Dictionary<ulong, int>();

            foreach (BasePlayer active in BasePlayer.activePlayerList)
            {
                int deathCount = 0;

                try
                {
                    deathCount = Interface.Oxide.DataFileSystem.ReadObject<int>("RogueLite/" + active.UserIDString);
                }
                catch (Exception e)
                {

                }

                PlayerDeathsDictionary.Add(active.userID, deathCount);
            }
        }

        private void SavePlayerDeathsDictionary()
        {
            foreach (KeyValuePair<ulong, int> item in PlayerDeathsDictionary)
            {
                Interface.Oxide.DataFileSystem.WriteObject("RogueLite/" + item.Key, item.Value);
            }
        }

        private void IncrementDeath(BasePlayer player)
        {
            int deathCount = GetDeathCount(player);

            deathCount++;

            if (PlayerDeathsDictionary.ContainsKey(player.userID))
            {
                PlayerDeathsDictionary[player.userID] = deathCount;
            }
            else
            {
                PlayerDeathsDictionary.Add(player.userID, deathCount);
            }

            SendReply(player, string.Format(lang.GetMessage("KickWarning", this, player.UserIDString), (configData.Options.MaxLives - deathCount)));
        }

        private int GetDeathCount(BasePlayer player)
        {
            return GetDeathCount(player.userID);
        }

        private int GetDeathCount(ulong userID)
        {
            int result = 0;

            PlayerDeathsDictionary.TryGetValue(userID, out result);

            return result;
        }

        private void ClearDeaths()
        {
            LoadPlayerDeathsDictionary();

            string[] filesToDeleteArray = Interface.Oxide.DataFileSystem.GetFiles("RogueLite/");

            foreach (string fileString in filesToDeleteArray)
            {
                string tmpString = fileString.Replace(".json", string.Empty);

                Interface.Oxide.DataFileSystem.WriteObject(tmpString, 0);
            }
        }
        #endregion

        #region Config
        private ConfigData configData;

        class ConfigData
        {
            public Options Options = new Options();
        }

        class Options
        {
            public int MaxLives = 3;
            public bool ClearDeathsAfterWipe = true;
            public bool PlaySoundOnPlayerKick = true;
            public string SoundEffect = "assets/prefabs/tools/medical syringe/effects/pop_cap.prefab";
        }

        private void LoadConfigVariables()
        {
            configData = Config.ReadObject<ConfigData>();
            SaveConfig(configData);
        }

        protected override void LoadDefaultConfig()
        {
            ConfigData config = new ConfigData();
            SaveConfig(config);
        }

        void SaveConfig(ConfigData config)
            => Config.WriteObject(config, true);
        #endregion

        #region Localization
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["PlayerKicked"] = "<color=red>{0}</color> has expended their last life and has been kicked from the server!",
                ["KickReason"] = "☠ YOU DIED ☠",
                ["KickWarning"] = "<color=red>☠</color>\nYou have {0} lives(s) remaining!",
                ["SetMaxLives"] = "MaxLives on the server set to <color=yellow>{0}</color>!",
                ["ClearDeaths"] = "Deaths cleared."
            }, this);
        }

        #endregion
    }
}
