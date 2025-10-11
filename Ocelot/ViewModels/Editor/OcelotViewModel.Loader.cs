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
        public void LoadMapenv(Mapenv mapenv)
        {
            MapEnvironment = mapenv;

            UpdateBoundingBoxDisplay();

            PopulateTreeView();

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }

        public void LoadFuncpoint(Funcpoint funcpoint)
        {
            FunctionPoint = funcpoint;

            PopulateTreeView();

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }

        public void LoadHealpoint(Healpoint healpoint)
        {
            HealPoint = healpoint;

            PopulateTreeView();

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }

        public void LoadNPCData(List<NPCBase> npcBases, Dictionary<int, List<NPCAppear>> npcAppearDict, Dictionary<int, List<NPCTalkConfig>> npcTalkDict, string folderName)
        {
            NPCs = npcBases;
            NPCAppearDict = npcAppearDict;
            NPCTalkDict = npcTalkDict;
            _currentFolderName = folderName;

            PopulateTreeView();

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }
    }
}
