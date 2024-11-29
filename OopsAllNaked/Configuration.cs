using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using static OopsAllLalafellsSRE.Utils.Constant;

namespace OopsAllLalafellsSRE
{
    [Serializable]
    internal class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public Race SelectedRace { get; set; } = Race.UNKNOWN;
        public bool enabled { get; set; } = false;
        public bool stayOn { get; set; } = false;
        public bool nameHQ { get; set; } = false;

        public bool dontStripSelf { get; set; } = false;
        public bool dontLalaSelf { get; set; } = false;

        public bool stripHats { get; set; } = true;
        public bool stripBodies { get; set; } = true;
        public bool stripLegs { get; set; } = true;
        public bool stripGloves { get; set; } = true;
        public bool stripBoots { get; set; } = true;
        public bool stripAccessories { get; set; } = true;

        public SortedSet<string> Whitelist { get; set; } = new(StringComparer.Ordinal);

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface!.SavePluginConfig(this);
        }

        public void AddToWhitelist(string charName)
        {
            Whitelist.Add(charName);
        }

        public void RemoveFromWhitelist(string charName)
        {
            Whitelist.Remove(charName);
        }

        public bool IsWhitelisted(string charName)
        {
            return Whitelist.Contains(charName);
        }
    }
}