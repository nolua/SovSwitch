using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace ConfigLibrary
{
    public class SwitchCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }
        protected override string ElementName
        {
            get { return "Switch"; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return new ConfigurationPropertyCollection(); }
        }

        public Switch this[int index]
        {
            get { return (Switch)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new Switch this[string SwitchName]
        {
            get { return (Switch)BaseGet(SwitchName); }
        }

        public void Add(Switch item)
        {
            BaseAdd(item);
        }

        public void Remove(Switch item)
        {
            BaseRemove(item);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Switch();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Switch)element).SwitchName;
        }

    }
}
