using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using vorpinventory_sv;

namespace vorpinventory_cl
{
    public class vorp_inventoryClient : BaseScript
    {
        public static Dictionary<string, Dictionary<string, dynamic>> citems =
                new Dictionary<string, Dictionary<string, dynamic>>();

        public static Dictionary<string, ItemClass> useritems = new Dictionary<string, ItemClass>();
        public static Dictionary<int, WeaponClass> userWeapons = new Dictionary<int, WeaponClass>();
        public static Dictionary<int, string> bulletsHash = new Dictionary<int, string>();

        public vorp_inventoryClient()
        {
            EventHandlers["vorpInventory:giveItemsTable"] += new Action<dynamic>(processItems);
            EventHandlers["vorpInventory:giveInventory"] += new Action<string>(getInventory);
            EventHandlers["vorpInventory:giveLoadout"] += new Action<dynamic>(getLoadout);
            EventHandlers["vorp:SelectedCharacter"] += new Action<int>(OnSelectedCharacter);
            EventHandlers["vorpinventory:receiveItem"] += new Action<string, int>(receiveItem);
            EventHandlers["vorpinventory:receiveWeapon"] +=
                    new Action<int, string, string, ExpandoObject, List<dynamic>>(receiveWeapon);
            Tick += updateAmmoInWeapon;
        }

        private async Task updateAmmoInWeapon()
        {
            await Delay(500);
            uint weaponHash = 0;
            if (API.GetCurrentPedWeapon(API.PlayerPedId(), ref weaponHash, false, 0, false))
            {
                var weaponName = Function.Call<string>((Hash)0x89CF5FF3D363311E, weaponHash);
                //Debug.WriteLine(weaponName);
                if (weaponName.Contains("UNARMED"))
                {
                    return;
                }

                var ammoDict = new Dictionary<string, int>();
                WeaponClass usedWeapon = null;
                foreach (var weap in userWeapons.ToList())
                {
                    if (weaponName.Contains(weap.Value.getName()) && weap.Value.getUsed())
                    {
                        ammoDict = weap.Value.getAllAmmo();
                        usedWeapon = weap.Value;
                    }
                }

                if (usedWeapon == null)
                {
                    return;
                }

                foreach (var ammo in ammoDict.ToList())
                {
                    var ammoQuantity =
                            Function.Call<int>((Hash)0x39D22031557946C1, API.PlayerPedId(), API.GetHashKey(ammo.Key));
                    if (ammoQuantity != ammo.Value)
                    {
                        usedWeapon.setAmmo(ammoQuantity, ammo.Key);
                    }
                }
            }
        } //Update weapon ammo

        private void receiveItem(string name, int count)
        {
            if (useritems.ContainsKey(name))
            {
                useritems[name].addCount(count);
            }
            else
            {
                useritems.Add(name, new ItemClass(count, citems[name]["limit"], citems[name]["label"], name,
                                                  "item_standard", true, citems[name]["can_remove"]));
            }

            NUIEvents.LoadInv();
        }

        private void receiveWeapon(int id, string propietary, string name, ExpandoObject ammo, List<dynamic> components)
        {
            var ammoaux = new Dictionary<string, int>();
            foreach (var amo in ammo)
            {
                ammoaux.Add(amo.Key, int.Parse(amo.Value.ToString()));
            }

            var auxcomponents = new List<string>();
            foreach (var comp in components)
            {
                auxcomponents.Add(comp.ToString());
            }

            var weapon = new WeaponClass(id, propietary, name, ammoaux, auxcomponents, false, false);
            if (!userWeapons.ContainsKey(weapon.getId()))
            {
                userWeapons.Add(weapon.getId(), weapon);
            }

            NUIEvents.LoadInv();
        }

        private async void OnSelectedCharacter(int charId)
        {
            API.SetNuiFocus(false, false);
            API.SendNuiMessage("{\"action\": \"hide\"}");
            Debug.WriteLine("Loading Inventory");
            TriggerServerEvent("vorpinventory:getItemsTable");
            await Delay(300);
            TriggerServerEvent("vorpinventory:getInventory");
        }

        private void processItems(dynamic items)
        {
            citems.Clear();
            foreach (var item in items)
            {
                citems.Add(item.item, new Dictionary<string, dynamic>
                {
                        ["item"] = item.item,
                        ["label"] = item.label,
                        ["limit"] = item.limit,
                        ["can_remove"] = item.can_remove,
                        ["type"] = item.type,
                        ["usable"] = item.usable
                });
            }
        }

        private void getLoadout(dynamic loadout)
        {
            Debug.WriteLine(API.PlayerPedId().ToString());
            foreach (var row in loadout)
            {
                JArray componentes = JsonConvert.DeserializeObject(row.components.ToString());
                JObject amunitions = JsonConvert.DeserializeObject(row.ammo.ToString());
                var components = new List<string>();
                var ammos = new Dictionary<string, int>();
                foreach (var componente in componentes)
                {
                    components.Add(componente.ToString());
                }

                foreach (var amunition in amunitions.Properties())
                {
                    ammos.Add(amunition.Name, int.Parse(amunition.Value.ToString()));
                }

                Debug.WriteLine(row.used.ToString());
                var auused = false;
                if (row.used == 1)
                {
                    auused = true;
                }

                var auused2 = false;
                if (row.used2 == 1)
                {
                    auused2 = true;
                }

                var auxweapon = new WeaponClass(int.Parse(row.id.ToString()), row.identifier.ToString(),
                                                row.name.ToString(), ammos, components, auused, auused2);
                userWeapons.Add(auxweapon.getId(), auxweapon);
                if (auxweapon.getUsed())
                {
                    Utils.useWeapon(auxweapon.getId());
                }
            }
        }

        private void getInventory(string inventory)
        {
            useritems.Clear();
            if (inventory != null)
            {
                var items = JsonConvert.DeserializeObject<dynamic>(inventory);
                Debug.WriteLine(items.ToString());
                foreach (var fitems in citems)
                {
                    if (items[fitems.Key] != null)
                    {
                        Debug.WriteLine(fitems.Key);
                        int cuantity = int.Parse(items[fitems.Key].ToString());
                        int limit = int.Parse(fitems.Value["limit"].ToString());
                        string label = fitems.Value["label"].ToString();
                        bool can_remove = bool.Parse(fitems.Value["can_remove"].ToString());
                        string type = fitems.Value["type"].ToString();
                        bool usable = bool.Parse(fitems.Value["usable"].ToString());
                        var item = new ItemClass(cuantity, limit, label, fitems.Key, type, usable, can_remove);
                        useritems.Add(fitems.Key, item);
                    }
                }
            }
        }
    }
}
