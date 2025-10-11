using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocelot.Models;
using StudioElevenGUI.ViewModels;

namespace Ocelot.ViewModels
{
    public partial class OcelotViewModel : BaseViewModel
    {
        public void SetNPCJsonData(NPCJsonData npcJsonData)
        {
            _npcJsonData = npcJsonData;
        }

        private string GetNPCName(NPCBase npcBase)
        {
            string defaultID = npcBase.ID.ToString("X8");

            if (_npcJsonData?.chara_type == null) return defaultID;

            int npcType = npcBase.Type;
            int modelHead = npcBase.ModelHead;

            Dictionary<string, string> targetDict = null;

            if (npcType == (int)NPCType.Player)
                targetDict = _npcJsonData.chara_type.player;
            else if (npcType == (int)NPCType.NPC)
                targetDict = _npcJsonData.chara_type.npc;
            else if (npcType == (int)NPCType.NPCOther)
                targetDict = _npcJsonData.chara_type.other;

            if (targetDict != null && targetDict.TryGetValue(modelHead.ToString(), out string name))
            {
                // Manage duplicates
                string key = $"{name}_{npcType}";
                if (_npcNameCounts.ContainsKey(key))
                {
                    _npcNameCounts[key]++;
                    return $"{name} ({_npcNameCounts[key]})";
                }
                else
                {
                    _npcNameCounts[key] = 1;
                    return name;
                }
            }

            return defaultID;
        }
    }
}
