using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace ConfigLibrary
{
    public class ListeSwitchSection : ConfigurationSection
    {
        private static readonly ConfigurationPropertyCollection proprietes;
        private static readonly ConfigurationProperty liste;

        static ListeSwitchSection()
        {
            liste = new ConfigurationProperty(string.Empty, typeof(SwitchCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);
            proprietes = new ConfigurationPropertyCollection { liste };
        }

        public SwitchCollection Listes
        {
            get { return (SwitchCollection)base[liste]; }
        }

        public new  Switch this[string SwitchName]
        {
            get { return Listes[SwitchName]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return proprietes; }
        }
    }
}
