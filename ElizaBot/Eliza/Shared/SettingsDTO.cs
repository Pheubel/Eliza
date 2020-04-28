using System;
using System.Collections.Generic;
using System.Text;

namespace Eliza.Shared
{
    public class SettingsDTO
    {
        public string Prefix { get; set; }
        public bool CaseSensitiveComands { get; set; }
        public bool UseMentionPrefix { get; set; }
    }
}
