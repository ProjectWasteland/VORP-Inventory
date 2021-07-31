using System.Collections.Generic;

namespace vorpinventory_sv
{
    public class Item
    {
        public string Name { get; set; }

        public string Label { get; set; }

        public string Type { get; set; }

        public string Model { get; set; }

        public int Count { get; set; }

        public int Limit { get; set; }

        public double Weight { get; set; }

        public bool CanUse { get; set; }

        public bool CanRemove { get; set; }

        public bool DropOnDeath { get; set; }

        public Item(string name, string label, string type, string model, int count, int limit, double weight,
                    bool canUse, bool canRemove, bool dropOnDeath)
        {
            this.Name = name;
            this.Label = label;
            this.Type = type;
            this.Model = model;
            this.Count = count;
            this.Limit = limit;
            this.Weight = weight;
            this.CanUse = canUse;
            this.CanRemove = canRemove;
            this.DropOnDeath = dropOnDeath;
        }

        public object getItemDictionary()
        {
            var itemDic = new Dictionary<string, object>
            {
                    { "label", Label },
                    { "name", Name },
                    { "model", Model },
                    { "type", Type },
                    { "count", Count },
                    { "limit", Limit },
                    { "usable", CanUse },
                    { "weight", Weight }
            };
            return itemDic;
        }

        public void addCount(int cuantity)
        {
            Count += cuantity;
        }

        public void delCount(int cuantity)
        {
            Count -= cuantity;
        }
    }
}
