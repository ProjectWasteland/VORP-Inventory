﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace vorpinventory_sv
{
    public class Config : BaseScript
    {
        public static JObject config = new JObject();
        public static string ConfigString;
        public static Dictionary<string, string> lang = new Dictionary<string, string>();
        public static string resourcePath = $"{API.GetResourcePath(API.GetCurrentResourceName())}";

        public static int MaxItems;
        public static int MaxWeapons;

        public Config()
        {
            EventHandlers["vorp_NewCharacter"] += new Action<int>(itemsConfig);
            EventHandlers[$"{API.GetCurrentResourceName()}:getConfig"] += new Action<Player>(getConfig);

            if (File.Exists($"{resourcePath}/Config.json"))
            {
                ConfigString = File.ReadAllText($"{resourcePath}/Config.json", Encoding.UTF8);
                config = JObject.Parse(ConfigString);
                if (File.Exists($"{resourcePath}/languages/{config["defaultlang"]}.json"))
                {
                    var langstring = File.ReadAllText($"{resourcePath}/languages/{config["defaultlang"]}.json",
                                                      Encoding.UTF8);
                    lang = JsonConvert.DeserializeObject<Dictionary<string, string>>(langstring);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{API.GetCurrentResourceName()}: Language {config["defaultlang"]}.json loaded!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Debug.WriteLine($"{API.GetCurrentResourceName()}: {config["defaultlang"]}.json Not Found");
                }
            }

            MaxItems = config["MaxItemsInInventory"]["Items"].ToObject<int>();
            MaxWeapons = config["MaxItemsInInventory"]["Weapons"].ToObject<int>();

            if (MaxItems < 0)
            {
                MaxItems = 0;
            }

            if (MaxWeapons < 0)
            {
                MaxWeapons = 0;
            }
        }

        private void getConfig([FromSource] Player source)
        {
            source.TriggerEvent($"{API.GetCurrentResourceName()}:SendConfig", ConfigString, lang);
        }

        private async void itemsConfig(int player)
        {
            await Delay(5000);
            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];
            try
            {
                foreach (var item in (JObject)config["startItems"][0])
                {
                    TriggerEvent("vorpCore:addItem", player, item.Key, int.Parse(item.Value.ToString()));
                }

                foreach (var weapon in (JObject)config["startItems"][1])
                {
                    var wpc = config["Weapons"].FirstOrDefault(x => x["HashName"].ToString().Contains(weapon.Key));
                    var auxbullets = new List<string>();
                    var givedBullets = new Dictionary<string, int>();
                    foreach (var bullets in (JObject)wpc["AmmoHash"][0])
                    {
                        auxbullets.Add(bullets.Key);
                    }

                    foreach (var bullet in (JObject)weapon.Value[0])
                    {
                        if (auxbullets.Contains(bullet.Key))
                        {
                            givedBullets.Add(bullet.Key, int.Parse(bullet.Value.ToString()));
                        }
                    }

                    TriggerEvent("vorpCore:registerWeapon", player, weapon.Key, givedBullets);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
