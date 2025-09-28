using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.Models
{
    public class NPCJsonData
    {
        public CharaTypes chara_type { get; set; }
    }

    public class CharaTypes
    {
        public Dictionary<string, string> player { get; set; }
        public Dictionary<string, string> npc { get; set; }
        public Dictionary<string, string> other { get; set; }
    }
}
