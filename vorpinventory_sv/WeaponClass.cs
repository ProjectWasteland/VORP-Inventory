using System.Collections.Generic;
using CitizenFX.Core;
using Newtonsoft.Json;

namespace vorpinventory_sv
{
    public class WeaponClass : BaseScript
    {
        private readonly Dictionary<string, int> ammo;
        private int charId;
        private readonly List<string> components;
        private int id;
        private readonly string name;
        private string propietary;
        private bool used;
        private bool used2;

        public WeaponClass(int id, string propietary, string name, Dictionary<string, int> ammo,
                           List<string> components, bool used, bool used2, int charid)
        {
            this.id = id;
            this.name = name;
            this.ammo = ammo;
            this.components = components;
            this.propietary = propietary;
            this.used = used;
            this.used2 = used2;
            charId = charid;
        }

        public void setUsed(bool used)
        {
            this.used = used;
        }

        public bool getUsed()
        {
            return used;
        }

        public void setUsed2(bool used2)
        {
            this.used2 = used2;
        }

        public bool getUsed2()
        {
            return used2;
        }

        public string getPropietary()
        {
            return propietary;
        }

        public int getCharId()
        {
            return charId;
        }

        public void setCharId(int charid)
        {
            charId = charid;
        }

        public void setPropietary(string propietary)
        {
            this.propietary = propietary;
        }

        public int getId()
        {
            return id;
        }

        public void setId(int id)
        {
            this.id = id;
        }

        public string getName()
        {
            return name;
        }

        public Dictionary<string, int> getAllAmmo()
        {
            return ammo;
        }

        public List<string> getAllComponents()
        {
            return components;
        }

        public void setComponent(string component)
        {
            components.Add(component);
        }

        public void quitComponent(string component)
        {
            if (components.Contains(component))
            {
                components.Remove(component);
            }
        }

        public int getAmmo(string type)
        {
            return ammo[type];
        }

        public void addAmmo(int ammo, string type)
        {
            if (this.ammo.ContainsKey(type))
            {
                this.ammo[type] += ammo;
            }
            else
            {
                this.ammo.Add(type, ammo);
            }

            Exports["ghmattimysql"]
                    .execute(
                             $"UPDATE loadout SET ammo = '{JsonConvert.SerializeObject(getAllAmmo())}' WHERE id=?",
                             new[] { id });
        }

        public void setAmmo(int ammo, string type)
        {
            if (this.ammo.ContainsKey(type))
            {
                this.ammo[type] = ammo;
            }
            else
            {
                this.ammo.Add(type, ammo);
            }

            Exports["ghmattimysql"]
                    .execute(
                             $"UPDATE loadout SET ammo = '{JsonConvert.SerializeObject(getAllAmmo())}' WHERE id=?",
                             new[] { id });
        }

        public void subAmmo(int ammo, string type)
        {
            if (this.ammo.ContainsKey(type))
            {
                this.ammo[type] -= ammo;
                if (this.ammo[type] == 0)
                {
                    this.ammo.Remove(type);
                }
            }

            Exports["ghmattimysql"]
                    .execute(
                             $"UPDATE loadout SET ammo = '{JsonConvert.SerializeObject(getAllAmmo())}' WHERE id=?",
                             new[] { id });
        }
    }
}
