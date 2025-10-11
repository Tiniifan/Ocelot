using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocelot.Models;
using Ocelot.Views.Panels;
using System.Windows.Controls;
using System.Windows.Input;
using StudioElevenGUI.ViewModels;

namespace Ocelot.ViewModels
{
    public partial class OcelotViewModel : BaseViewModel
    {
        private Border _activeTab;
        private bool _isDragging = false;
        private System.Windows.Point _lastMousePosition;

        // Data containers
        public Mapenv MapEnvironment { get; set; }
        public Funcpoint FunctionPoint { get; set; }
        public Healpoint HealPoint { get; set; }
        public List<NPCBase> NPCs { get; set; }
        public List<TBoxConfig> TreasureBoxes { get; set; }
        public List<EncountInfo> Encounters { get; set; }
        public Dictionary<int, List<NPCAppear>> NPCAppearDict { get; set; }
        public Dictionary<int, List<NPCTalkConfig>> NPCTalkDict { get; set; }

        // Reusable panel instances
        private MapEnvironmentPanel _mapEnvironmentPanel;
        private NPCPanel _npcPanel;
        private TreasureBoxPanel _treasureBoxPanel;
        private TalkPanel _talkPanel;
        private FuncpointPanel _funcpointPanel;
        private HealPointPanel _healPointPanel;
        private HealAreaPanel _healAreaPanel;
        private AppearPanel _appearPanel;
        private EncounterPanel _encounterPanel;
        private EventTriggerPanel _eventTriggerPanel;
        private EventSEPanel _eventSEPanel;
        private EventMapJumpPanel _eventMapJumpPanel;

        private string _currentFolderName;

        private Dictionary<int, int> _selectedNPCAppearIndices = new Dictionary<int, int>();

        private NPCJsonData _npcJsonData;
        private Dictionary<string, int> _npcNameCounts = new Dictionary<string, int>();

        #region Commands
        public ICommand AddNpcCommand { get; }

        #endregion

        public OcelotViewModel()
        {
            // Initialize commands
            InitializeMinimapCommand();
            InitializeTabCommand();
            //AddNpcCommand = new RelayCommand(ExecuteAddNpc);

            InitializePanels();
            PopulateTreeView();

            NPCAppearDict = new Dictionary<int, List<NPCAppear>>();
            NPCTalkDict = new Dictionary<int, List<NPCTalkConfig>>();
        }

        public void SetCurrentFolderName(string folderName)
        {
            _currentFolderName = folderName;
        }
    }
}
