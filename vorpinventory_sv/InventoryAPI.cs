﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace vorpinventory_sv
{
    public class InventoryAPI : BaseScript
    {
        public static Dictionary<string, CallbackDelegate> usableItemsFunctions =
                new Dictionary<string, CallbackDelegate>();

        public static dynamic CORE;

        public InventoryAPI()
        {
            EventHandlers["vorpCore:subWeapon"] += new Action<int, int>(subWeapon);
            EventHandlers["vorpCore:giveWeapon"] += new Action<int, int, int>(giveWeapon);
            EventHandlers["vorpCore:registerWeapon"] +=
                    new Action<int, string, ExpandoObject, ExpandoObject>(registerWeapon);
            EventHandlers["vorpCore:addItem"] += new Action<int, string, int>(addItem);
            EventHandlers["vorpCore:subItem"] += new Action<int, string, int>(subItem);
            EventHandlers["vorpCore:getItemCount"] += new Action<int, CallbackDelegate, string>(getItems);
            EventHandlers["vorpCore:getUserInventory"] += new Action<int, CallbackDelegate>(getInventory);
            EventHandlers["vorpCore:canCarryItems"] += new Action<int, int, CallbackDelegate>(canCarryAmountItem);
            EventHandlers["vorpCore:canCarryItem"] += new Action<int, string, int, CallbackDelegate>(canCarryItem);
            EventHandlers["vorpCore:canCarryWeapons"] += new Action<int, int, CallbackDelegate>(canCarryAmountWeapons);
            EventHandlers["vorpCore:subBullets"] += new Action<int, int, string, int>(subBullets);
            EventHandlers["vorpCore:addBullets"] += new Action<int, int, string, int>(addBullets);
            EventHandlers["vorpCore:getWeaponComponents"] +=
                    new Action<int, CallbackDelegate, int>(getWeaponComponents);
            EventHandlers["vorpCore:getWeaponBullets"] += new Action<int, CallbackDelegate, int>(getWeaponBullets);
            EventHandlers["vorpCore:getUserWeapons"] += new Action<int, CallbackDelegate>(getUserWeapons);
            EventHandlers["vorpCore:addComponent"] += new Action<int, int, string, CallbackDelegate>(addComponent);
            EventHandlers["vorpCore:getUserWeapon"] += new Action<int, CallbackDelegate, int>(getUserWeapon);
            EventHandlers["vorpCore:registerUsableItem"] += new Action<string, CallbackDelegate>(registerUsableItem);
            EventHandlers["vorp:use"] += new Action<Player, string, object[]>(useItem);
            TriggerEvent("getCore", new Action<dynamic>(dic => { CORE = dic; }));
        }

        public async Task SaveInventoryItemsSupport(Player source)
        {
            await Delay(1000);
            var identifier = "steam:" + source.Identifiers["steam"];
            var items = new Dictionary<string, int>();
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                foreach (var item in ItemDatabase.usersInventory[identifier])
                {
                    items.Add(item.Key, item.Value.getCount());
                }

                if (items.Count >= 0)
                {
                    var CoreUser = vorpinventory_sv.CORE.getUser(int.Parse(source.Handle)).getUsedCharacter;
                    int charIdentifier = CoreUser.charIdentifier;
                    var json = JsonConvert.SerializeObject(items);
                    Exports["ghmattimysql"]
                            .execute($"UPDATE characters SET inventory = '{json}' WHERE `identifier` = ? AND `charidentifier` = ?;",
                                     new object[] { identifier, charIdentifier });
                }
            }
        }

        private void canCarryAmountWeapons(int source, int quantity, CallbackDelegate cb)
        {
            var p = Players[source];
            var identifier = "steam:" + p.Identifiers["steam"];
            var CoreUser = vorpinventory_sv.CORE.getUser(source).getUsedCharacter;
            int charIdentifier = CoreUser.charIdentifier;
            var totalcount = getUserTotalCountWeapons(identifier, charIdentifier) + quantity;
            if (Config.MaxWeapons != -1)
            {
                if (totalcount <= Config.MaxWeapons)
                {
                    cb.Invoke(true);
                }
                else
                {
                    cb.Invoke(false);
                }
            }
            else
            {
                cb.Invoke(true);
            }
        }

        private void canCarryAmountItem(int source, int quantity, CallbackDelegate cb)
        {
            var p = Players[source];
            var identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.usersInventory.ContainsKey(identifier) && Config.MaxItems != -1)
            {
                var totalcount = getUserTotalCount(identifier) + quantity;
                if (totalcount <= Config.MaxItems)
                {
                    cb.Invoke(true);
                }
                else
                {
                    cb.Invoke(false);
                }
            }
            else
            {
                cb.Invoke(true);
            }
        }

        private void canCarryItem(int source, string itemName, int quantity, CallbackDelegate cb)
        {
            var p = Players[source];
            var identifier = "steam:" + p.Identifiers["steam"];

            var limit = ItemDatabase.svItems[itemName].getLimit();

            if (limit != -1)
            {
                if (ItemDatabase.usersInventory.ContainsKey(identifier))
                {
                    if (ItemDatabase.usersInventory[identifier].ContainsKey(itemName))
                    {
                        var count = ItemDatabase.usersInventory[identifier][itemName].getCount();

                        var total = count + quantity;

                        if (total <= limit)
                        {
                            if (Config.MaxItems != -1)
                            {
                                var totalcount = getUserTotalCount(identifier) + quantity;
                                if (totalcount <= Config.MaxItems)
                                {
                                    cb.Invoke(true);
                                }
                                else
                                {
                                    cb.Invoke(false);
                                }
                            }
                            else
                            {
                                cb.Invoke(true);
                            }
                        }
                        else
                        {
                            cb.Invoke(false);
                        }
                    }
                    else
                    {
                        if (quantity <= limit)
                        {
                            if (Config.MaxItems != -1)
                            {
                                var totalcount = getUserTotalCount(identifier) + quantity;
                                if (totalcount <= Config.MaxItems)
                                {
                                    cb.Invoke(true);
                                }
                                else
                                {
                                    cb.Invoke(false);
                                }
                            }
                            else
                            {
                                cb.Invoke(true);
                            }
                        }
                        else
                        {
                            cb.Invoke(false);
                        }
                    }
                }
                else
                {
                    if (quantity <= limit)
                    {
                        if (Config.MaxItems != -1)
                        {
                            var totalcount = quantity;
                            if (totalcount <= Config.MaxItems)
                            {
                                cb.Invoke(true);
                            }
                            else
                            {
                                cb.Invoke(false);
                            }
                        }
                        else
                        {
                            cb.Invoke(true);
                        }
                    }
                    else
                    {
                        cb.Invoke(false);
                    }
                }
            }
            else
            {
                if (Config.MaxItems != -1)
                {
                    var totalcount = getUserTotalCount(identifier) + quantity;
                    if (totalcount <= Config.MaxItems)
                    {
                        cb.Invoke(true);
                    }
                    else
                    {
                        cb.Invoke(false);
                    }
                }
                else
                {
                    cb.Invoke(true);
                }
            }
        }

        private void getInventory(int source, CallbackDelegate cb)
        {
            var p = Players[source];
            var identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                var useritems = new List<object>();

                foreach (var items in ItemDatabase.usersInventory[identifier])
                {
                    var item = new Dictionary<string, object>
                    {
                            { "label", items.Value.getLabel() },
                            { "name", items.Value.getName() },
                            { "type", items.Value.getType() },
                            { "count", items.Value.getCount() },
                            { "limit", items.Value.getLimit() },
                            { "usable", items.Value.getUsable() }
                    };
                    useritems.Add(item);
                }

                cb.Invoke(useritems);
            }
        }

        private void useItem([FromSource] Player source, string itemname, params object[] args)
        {
            var identifier = "steam:" + source.Identifiers["steam"];
            if (usableItemsFunctions.ContainsKey(itemname))
            {
                if (ItemDatabase.svItems.ContainsKey(itemname))
                {
                    var argumentos = new Dictionary<string, object>
                    {
                            { "source", int.Parse(source.Handle) },
                            { "item", ItemDatabase.svItems[itemname].getItemDictionary() },
                            { "args", args }
                    };
                    usableItemsFunctions[itemname](argumentos);
                }
                else
                {
                    Debug.WriteLine("Use Item Error");
                }
            }
        }

        private void registerUsableItem(string name, CallbackDelegate cb)
        {
            usableItemsFunctions[name] = cb;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{API.GetCurrentResourceName()}: Function callback of item: {name} registered!");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void subComponent(int player, int weaponId, string component, CallbackDelegate function)
        {
            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    ItemDatabase.userWeapons[weaponId].quitComponent(component);
                    Exports["ghmattimysql"]
                            .execute(
                                     $"UPDATE loadout SET components = '{JsonConvert.SerializeObject(ItemDatabase.userWeapons[weaponId].getAllComponents())}' WHERE id=?",
                                     new[] { weaponId });
                    function.Invoke(true);
                    p.TriggerEvent("vorpCoreClient:subComponent", weaponId, component);
                }
                else
                {
                    function.Invoke(false);
                }
            }
        }

        private void addComponent(int player, int weaponId, string component, CallbackDelegate function)
        {
            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    ItemDatabase.userWeapons[weaponId].setComponent(component);

                    Exports["ghmattimysql"]
                            .execute(
                                     $"UPDATE loadout SET components = '{JsonConvert.SerializeObject(ItemDatabase.userWeapons[weaponId].getAllComponents())}' WHERE id=?",
                                     new[] { weaponId });
                    function.Invoke(true);
                    p.TriggerEvent("vorpCoreClient:addComponent", weaponId, component);
                }
                else
                {
                    function.Invoke(false);
                }
            }
        }

        private void getUserWeapon(int player, CallbackDelegate function, int weapId)
        {
            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];

            var weapons = new Dictionary<string, dynamic>();
            var found = false;
            foreach (var weapon in ItemDatabase.userWeapons)
            {
                if (weapon.Value.getId() == weapId && !found)
                {
                    weapons.Add("name", weapon.Value.getName());
                    weapons.Add("id", weapon.Value.getId());
                    weapons.Add("propietary", weapon.Value.getPropietary());
                    weapons.Add("used", weapon.Value.getUsed());
                    weapons.Add("ammo", weapon.Value.getAllAmmo());
                    weapons.Add("components", weapon.Value.getAllComponents());
                    found = true;
                }
            }

            function.Invoke(weapons);
        }

        private void getUserWeapons(int player, CallbackDelegate function)
        {
            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];
            int charIdentifier;
            try
            {
                var CoreUser = CORE.getUser(player).getUsedCharacter;
                charIdentifier = CoreUser.charIdentifier;
            }
            catch
            {
                charIdentifier = -1;
            }

            Dictionary<string, dynamic> weapons;
            var userWeapons = new List<Dictionary<string, dynamic>>();

            foreach (var weapon in ItemDatabase.userWeapons)
            {
                if (weapon.Value.getPropietary() == identifier)
                {
                    if (weapon.Value.getCharId() == charIdentifier)
                    {
                        weapons = new Dictionary<string, dynamic>
                        {
                                ["name"] = weapon.Value.getName(),
                                ["id"] = weapon.Value.getId(),
                                ["propietary"] = weapon.Value.getPropietary(),
                                ["used"] = weapon.Value.getUsed(),
                                ["ammo"] = weapon.Value.getAllAmmo(),
                                ["components"] = weapon.Value.getAllComponents()
                        };
                        userWeapons.Add(weapons);
                    }
                }
            }

            function.Invoke(userWeapons);
        }

        private void getWeaponBullets(int player, CallbackDelegate function, int weaponId)
        {
            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    function.Invoke(ItemDatabase.userWeapons[weaponId].getAllAmmo());
                }
            }
        }

        private void getWeaponComponents(int player, CallbackDelegate function, int weaponId)
        {
            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    function.Invoke(ItemDatabase.userWeapons[weaponId].getAllComponents());
                }
            }
        }

        private void addBullets(int player, int weaponId, string bulletType, int cuantity)
        {
            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    ItemDatabase.userWeapons[weaponId].addAmmo(cuantity, bulletType);
                    p.TriggerEvent("vorpCoreClient:addBullets", weaponId, bulletType, cuantity);
                }
            }
            else
            {
                Debug.WriteLine("Weapon not found in DBa");
            }
        }

        private void subBullets(int player, int weaponId, string bulletType, int cuantity)
        {
            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];

            if (ItemDatabase.userWeapons.ContainsKey(weaponId))
            {
                if (ItemDatabase.userWeapons[weaponId].getPropietary() == identifier)
                {
                    ItemDatabase.userWeapons[weaponId].subAmmo(cuantity, bulletType);
                    p.TriggerEvent("vorpCoreClient:subBullets", weaponId, bulletType, cuantity);
                }
            }
            else
            {
                Debug.WriteLine("Weapon not found in DB");
            }
        }

        private void getItems(int source, CallbackDelegate funcion, string item)
        {
            var p = Players[source];
            var identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                if (ItemDatabase.usersInventory[identifier].ContainsKey(item))
                {
                    funcion.Invoke(ItemDatabase.usersInventory[identifier][item].getCount());
                }
                else
                {
                    funcion.Invoke(0);
                }
            }
        }

        private async void addItem(int player, string name, int cuantity)
        {
            try
            {
                if (Players[player] == null)
                {
                    return;
                }

                if (!ItemDatabase.svItems.ContainsKey(name))
                {
                    Debug.WriteLine($"Item: {name} not exist on Database please add this item on Table `Items`");
                    return;
                }

                var p = Players[player];
                var added = false;
                var identifier = "steam:" + p.Identifiers["steam"];

                if (!ItemDatabase.usersInventory.ContainsKey(identifier))
                {
                    var userinv = new Dictionary<string, ItemClass>();
                    ItemDatabase.usersInventory.Add(identifier, userinv);
                }

                if (ItemDatabase.usersInventory.ContainsKey(identifier))
                {
                    if (ItemDatabase.usersInventory[identifier].ContainsKey(name))
                    {
                        if (ItemDatabase.usersInventory[identifier][name].getCount() + cuantity <=
                            ItemDatabase.usersInventory[identifier][name].getLimit())
                        {
                            if (cuantity > 0)
                            {
                                if (Config.MaxItems != 0)
                                {
                                    var totalcount = getUserTotalCount(identifier);
                                    totalcount += cuantity;
                                    if (totalcount <= Config.MaxItems)
                                    {
                                        added = true;
                                        ItemDatabase.usersInventory[identifier][name].addCount(cuantity);
                                    }
                                }
                                else
                                {
                                    added = true;
                                    ItemDatabase.usersInventory[identifier][name].addCount(cuantity);
                                }
                            }
                        }
                        else if (ItemDatabase.usersInventory[identifier][name].getLimit() == -1)
                        {
                            if (cuantity > 0)
                            {
                                if (Config.MaxItems != 0)
                                {
                                    var totalcount = getUserTotalCount(identifier);
                                    totalcount += cuantity;
                                    if (totalcount <= Config.MaxItems)
                                    {
                                        added = true;
                                        ItemDatabase.usersInventory[identifier][name].addCount(cuantity);
                                    }
                                }
                                else
                                {
                                    added = true;
                                    ItemDatabase.usersInventory[identifier][name].addCount(cuantity);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (cuantity <= ItemDatabase.svItems[name].getLimit())
                        {
                            added = true;

                            if (Config.MaxItems != 0)
                            {
                                var totalcount = getUserTotalCount(identifier);
                                totalcount += cuantity;
                                if (totalcount <= Config.MaxItems)
                                {
                                    added = true;
                                    ItemDatabase.usersInventory[identifier]
                                                .Add(name, new ItemClass(cuantity,
                                                                         ItemDatabase.svItems[name].getLimit(),
                                                                         ItemDatabase.svItems[name].getLabel(), name,
                                                                         ItemDatabase.svItems[name].getType(), true,
                                                                         ItemDatabase.svItems[name].getCanRemove()));
                                }
                            }
                            else
                            {
                                added = true;
                                ItemDatabase.usersInventory[identifier]
                                            .Add(name, new ItemClass(cuantity, ItemDatabase.svItems[name].getLimit(),
                                                                     ItemDatabase.svItems[name].getLabel(), name,
                                                                     ItemDatabase.svItems[name].getType(), true,
                                                                     ItemDatabase.svItems[name].getCanRemove()));
                            }
                        }
                        else if (ItemDatabase.svItems[name].getLimit() == -1)
                        {
                            if (Config.MaxItems != 0)
                            {
                                var totalcount = getUserTotalCount(identifier);
                                totalcount += cuantity;
                                if (totalcount <= Config.MaxItems)
                                {
                                    added = true;
                                    ItemDatabase.usersInventory[identifier]
                                                .Add(name, new ItemClass(cuantity,
                                                                         ItemDatabase.svItems[name].getLimit(),
                                                                         ItemDatabase.svItems[name].getLabel(), name,
                                                                         ItemDatabase.svItems[name].getType(), true,
                                                                         ItemDatabase.svItems[name].getCanRemove()));
                                }
                            }
                            else
                            {
                                added = true;
                                ItemDatabase.usersInventory[identifier]
                                            .Add(name, new ItemClass(cuantity, ItemDatabase.svItems[name].getLimit(),
                                                                     ItemDatabase.svItems[name].getLabel(), name,
                                                                     ItemDatabase.svItems[name].getType(), true,
                                                                     ItemDatabase.svItems[name].getCanRemove()));
                            }
                        }
                    }

                    if (ItemDatabase.usersInventory[identifier].ContainsKey(name) && added)
                    {
                        var limit = ItemDatabase.usersInventory[identifier][name].getLimit();
                        var label = ItemDatabase.usersInventory[identifier][name].getLabel();
                        var type = ItemDatabase.usersInventory[identifier][name].getType();
                        var usable = ItemDatabase.usersInventory[identifier][name].getUsable();
                        var canRemove = ItemDatabase.usersInventory[identifier][name].getCanRemove();
                        p.TriggerEvent("vorpCoreClient:addItem", cuantity, limit, label, name, type, usable,
                                       canRemove); //Pass item to client
                        SaveInventoryItemsSupport(p);
                    }
                    else
                    {
                        TriggerClientEvent(p, "vorp:Tip", Config.lang["fullInventory"], 2000);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void subItem(int player, string name, int cuantity)
        {
            if (!ItemDatabase.svItems.ContainsKey(name))
            {
                Debug.WriteLine($"Item: {name} not exist on Database please add this item on Table `Items`");
                return;
            }

            var p = Players[player];
            var identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.usersInventory.ContainsKey(identifier))
            {
                if (ItemDatabase.usersInventory[identifier].ContainsKey(name))
                {
                    if (cuantity <= ItemDatabase.usersInventory[identifier][name].getCount())
                    {
                        ItemDatabase.usersInventory[identifier][name].quitCount(cuantity);
                        SaveInventoryItemsSupport(p);
                    }

                    p.TriggerEvent("vorpCoreClient:subItem", name,
                                   ItemDatabase.usersInventory[identifier][name].getCount());
                    if (ItemDatabase.usersInventory[identifier][name].getCount() == 0)
                    {
                        ItemDatabase.usersInventory[identifier].Remove(name);
                        SaveInventoryItemsSupport(p);
                    }
                }
            }
        }

        private void
                registerWeapon(int target, string name, ExpandoObject ammos,
                               ExpandoObject components) //Needs dirt level
        {
            Player p = null;
            var targetIsPlayer = false;
            foreach (var pla in Players)
            {
                if (int.Parse(pla.Handle) == target)
                {
                    p = Players[target];
                    targetIsPlayer = true;
                }
            }

            string identifier;
            var CoreUser = vorpinventory_sv.CORE.getUser(target).getUsedCharacter;
            int charIdentifier = CoreUser.charIdentifier;

            if (targetIsPlayer)
            {
                identifier = "steam:" + p.Identifiers["steam"];
                if (Config.MaxWeapons != 0)
                {
                    var totalcount = getUserTotalCountWeapons(identifier, charIdentifier);
                    totalcount += 1;
                    if (totalcount > Config.MaxWeapons)
                    {
                        Debug.WriteLine($"{p.Name} Can't carry more weapons");
                        return;
                    }
                }
            }
            else
            {
                identifier = target.ToString();
            }

            var ammoaux = new Dictionary<string, int>();
            if (ammos != null)
            {
                foreach (var ammo in ammos)
                {
                    ammoaux.Add(ammo.Key, int.Parse(ammo.Value.ToString()));
                }
            }

            var auxcomponents = new List<string>();
            if (components != null)
            {
                foreach (var component in components)
                {
                    auxcomponents.Add(component.Key);
                }
            }

            Exports["ghmattimysql"]
                    .execute("INSERT INTO loadout (`identifier`,`charidentifier`,`name`,`ammo`,`components`) VALUES (?,?,?,?,?)",
                             new object[]
                             {
                                     identifier, charIdentifier, name, JsonConvert.SerializeObject(ammoaux),
                                     JsonConvert.SerializeObject(auxcomponents)
                             }, new Action<dynamic>(result =>
                             {
                                 int weaponId = result.insertId;
                                 var auxWeapon = new WeaponClass(weaponId, identifier, name, ammoaux, auxcomponents,
                                                                 false, false, charIdentifier);
                                 ItemDatabase.userWeapons.Add(weaponId, auxWeapon);
                                 if (targetIsPlayer)
                                 {
                                     TriggerEvent("syn_weapons:registerWeapon", weaponId);
                                     p.TriggerEvent("vorpinventory:receiveWeapon", weaponId,
                                                    ItemDatabase.userWeapons[weaponId].getPropietary(),
                                                    ItemDatabase.userWeapons[weaponId].getName(),
                                                    ItemDatabase.userWeapons[weaponId].getAllAmmo(),
                                                    ItemDatabase.userWeapons[weaponId].getAllComponents());
                                 }
                             }));
        }

        private void giveWeapon(int player, int weapId, int target)
        {
            var p = Players[player];
            Player ptarget = null;
            var targetIsPlayer = false;
            foreach (var pla in Players)
            {
                if (int.Parse(pla.Handle) == target)
                {
                    targetIsPlayer = true;
                }
            }

            if (targetIsPlayer)
            {
                ptarget = Players[target];
            }

            var identifier = "steam:" + p.Identifiers["steam"];
            var CoreUser = vorpinventory_sv.CORE.getUser(player).getUsedCharacter;
            int charIdentifier = CoreUser.charIdentifier;

            if (Config.MaxWeapons != 0)
            {
                var totalcount = getUserTotalCountWeapons(identifier, charIdentifier);
                totalcount += 1;
                if (totalcount > Config.MaxWeapons)
                {
                    Debug.WriteLine($"{p.Name} Can't carry more weapons");
                    return;
                }
            }

            if (ItemDatabase.userWeapons.ContainsKey(weapId))
            {
                ItemDatabase.userWeapons[weapId].setPropietary(identifier);
                ItemDatabase.userWeapons[weapId].setCharId(charIdentifier);
                Exports["ghmattimysql"]
                        .execute(
                                 $"UPDATE loadout SET identifier = '{ItemDatabase.userWeapons[weapId].getPropietary()}', charidentifier = '{charIdentifier}' WHERE id=?",
                                 new object[] { weapId });
                p.TriggerEvent("vorpinventory:receiveWeapon", weapId, ItemDatabase.userWeapons[weapId].getPropietary(),
                               ItemDatabase.userWeapons[weapId].getName(),
                               ItemDatabase.userWeapons[weapId].getAllAmmo(),
                               ItemDatabase.userWeapons[weapId].getAllComponents());
                if (targetIsPlayer && ptarget != null)
                {
                    ptarget.TriggerEvent("vorpCoreClient:subWeapon", weapId);
                }
            }
        }

        private void subWeapon(int player, int weapId)
        {
            var p = Players[player];

            var CoreUser = vorpinventory_sv.CORE.getUser(player).getUsedCharacter;
            int charIdentifier = CoreUser.charIdentifier;

            var identifier = "steam:" + p.Identifiers["steam"];
            if (ItemDatabase.userWeapons.ContainsKey(weapId))
            {
                ItemDatabase.userWeapons[weapId].setPropietary("");
                Exports["ghmattimysql"]
                        .execute(
                                 $"UPDATE loadout SET identifier = '{ItemDatabase.userWeapons[weapId].getPropietary()}' , charidentifier = '{charIdentifier}' WHERE id=?",
                                 new[] { weapId });
            }

            p.TriggerEvent("vorpCoreClient:subWeapon", weapId);
        }

        public static int getUserTotalCount(string identifier)
        {
            var t_count = 0;
            foreach (var item in ItemDatabase.usersInventory[identifier].Values)
            {
                t_count += item.getCount();
            }

            return t_count;
        }

        public static int getUserTotalCountWeapons(string identifier, int charId)
        {
            var t_count = 0;
            foreach (var weapon in ItemDatabase.userWeapons.Values)
            {
                if (weapon.getPropietary().Contains(identifier) && weapon.getCharId() == charId)
                {
                    t_count += 1;
                }
            }

            return t_count;
        }
    }
}
