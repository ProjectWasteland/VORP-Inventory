using System.Collections.Generic;

namespace vorpinventory_sv
{
    public class Items
    {
        private readonly bool can_remove;
        private readonly string item;
        private readonly string label;
        private readonly int limit;
        private string type;
        private bool usable;

        public Items(string item, string label, int limit, bool can_remove, string type, bool usable)
        {
            this.item = item;
            this.limit = limit;
            this.can_remove = can_remove;
            this.label = label;
            this.type = type;
            this.usable = usable;
        }

        public bool getUsable()
        {
            return usable;
        }

        public void setUsable(bool usable)
        {
            this.usable = usable;
        }

        public string getType()
        {
            return type;
        }

        public void setType(string type)
        {
            this.type = type;
        }

        public string getName()
        {
            return item;
        }

        public string getLabel()
        {
            return label;
        }

        public int getLimit()
        {
            return limit;
        }

        public bool getCanRemove()
        {
            return can_remove;
        }

        public Dictionary<string, object> getItemDictionary()
        {
            var dictionary = new Dictionary<string, object>
            {
                    { "name", item },
                    { "label", label },
                    { "limit", limit },
                    { "can_remove", can_remove },
                    { "type", type },
                    { "usabel", usable }
            };
            return dictionary;
        }
    }
}
