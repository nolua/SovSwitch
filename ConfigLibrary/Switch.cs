using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace ConfigLibrary
{
    public class Switch : ConfigurationSection
    {
        private static readonly ConfigurationPropertyCollection _proprietes;
        private static readonly ConfigurationProperty switchName;
        private static readonly ConfigurationProperty switchIp;

        static Switch()
        {
            switchName = new ConfigurationProperty("SwitchName", typeof(string), null, ConfigurationPropertyOptions.IsKey);
            switchIp = new ConfigurationProperty("SwitchIp", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _proprietes = new ConfigurationPropertyCollection { switchName, switchIp };
        }



        [ConfigurationProperty("SwitchName", IsRequired = true)]
        public string SwitchName
        {
            get { return (string)this["SwitchName"]; }
            set { this["SwitchName"] = value; }
        }

        [ConfigurationProperty("SwitchIp", IsRequired = true)]
        public string SwitchIp
        {
            get { return (string)this["SwitchIp"]; }
            set { this["SwitchIp"] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _proprietes; }
        }
    }
 }

