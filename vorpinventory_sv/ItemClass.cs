﻿namespace vorpinventory_sv
{
    public class ItemClass
    {
        private bool canRemove;
        private int count;
        private string label;
        private int limit;
        private string name;
        private string type;
        private bool usable;

        public ItemClass(int count, int limit, string label, string name, string type, bool usable, bool canRemove)
        {
            this.count = count;
            this.limit = limit;
            this.label = label;
            this.name = name;
            this.type = type;
            this.usable = usable;
            this.canRemove = canRemove;
        }

        public void setCount(int count)
        {
            this.count = count;
        }

        public int getCount()
        {
            return count;
        }

        public void addCount(int count)
        {
            if (this.count + count <= limit)
            {
                this.count += count;
            }
        }

        public void quitCount(int count)
        {
            this.count -= count;
        }

        public void setLimit(int limit)
        {
            this.limit = limit;
        }

        public int getLimit()
        {
            return limit;
        }

        public void setLabel(string label)
        {
            this.label = label;
        }

        public string getLabel()
        {
            return label;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public string getName()
        {
            return name;
        }

        public void setType(string type)
        {
            this.type = type;
        }

        public string getType()
        {
            return type;
        }

        public void setUsable(bool usable)
        {
            this.usable = usable;
        }

        public bool getUsable()
        {
            return usable;
        }

        public void setCanRemove(bool canRemove)
        {
            this.canRemove = canRemove;
        }

        public bool getCanRemove()
        {
            return canRemove;
        }
    }
}
