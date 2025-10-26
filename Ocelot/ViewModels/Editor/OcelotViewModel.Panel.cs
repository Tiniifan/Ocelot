using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocelot.Models;
using System.Windows.Controls;
using Ocelot.Views.Panels;
using StudioElevenGUI.ViewModels;
using System.Windows;
using Ocelot.ViewModels.TreeView;

namespace Ocelot.ViewModels
{
    public partial class OcelotViewModel : BaseViewModel
    {
        private string _rightPanelHeaderText;
        /// <summary>
        /// Gets or sets the current text displayed on Right Panel Header
        /// </summary>
        public string RightPanelHeaderText
        {
            get => _rightPanelHeaderText;
            set => SetProperty(ref _rightPanelHeaderText, value);
        }

        private object _rightPanelContent;
        /// <summary>
        /// Gets or sets the current content displayed on Right Panel
        /// </summary>
        public object RightPanelContent
        {
            get => _rightPanelContent;
            set => SetProperty(ref _rightPanelContent, value);
        }

        private void InitializePanels()
        {
            _mapEnvironmentPanel = new MapEnvironmentPanel();
            _npcPanel = new NPCPanel();
            _treasureBoxPanel = new TreasureBoxPanel();
            _talkPanel = new TalkPanel();
            _funcpointPanel = new FuncpointPanel();
            _healPointPanel = new HealPointPanel();
            _healAreaPanel = new HealAreaPanel();
            _appearPanel = new AppearPanel();
            _encounterPanel = new EncounterPanel();
            _eventTriggerPanel = new EventTriggerPanel();
            _eventSEPanel = new EventSEPanel();
            _eventMapJumpPanel = new EventMapJumpPanel();
        }

        private void UpdateRightPanel(TreeViewItemViewModel selectedItem)
        {
            var tag = selectedItem.Tag;

            if (tag is string stringTag)
            {
                switch (stringTag)
                {
                    case "Mapenv": ShowMapEnvironmentPanel(); break;
                    default: ShowDefaultPanel(); break;
                }
            }
            else if (tag != null && tag.GetType().GetProperty("Type") != null)
            {
                dynamic data = tag;
                string type = data.Type;

                switch (type)
                {
                    case "FuncpointRoot":
                        ShowFuncpointPanel(data.Data);
                        break;
                    case "HealpointRoot":
                        ShowHealPointPanel(data.Data);
                        break;
                    case "HealArea":
                        ShowHealAreaPanel(data.Data);
                        break;
                    case "Event":
                        ShowEventPanel(data.Data);
                        break;
                    case "MapJump":
                        ShowEventMapJumpPanel(data.Data);
                        break;
                    case "SoundEffect":
                        ShowEventSEPanel(data.Data);
                        break;
                    case "NPCRoot":
                        ShowDefaultPanel();
                        break;
                    case "NPC":
                        ShowNPCPanel(data.Data);
                        break;
                    case "NPCAppear":
                        ShowAppearPanel(data.NPC, data.Appear, data.Index);
                        break;
                    case "NPCTalk":
                        ShowTalkPanel(data.NPC, data.Talk, data.Index);
                        break;
                    case "EventCategory":
                    case "MapJumpCategory":
                    case "SoundEffectCategory":
                    case "AppearsCategory":
                    case "TalksCategory":
                        ShowDefaultPanel();
                        break;
                }
            }
            else if (tag is NPCBase npc)
            {
                ShowNPCPanel(npc);
            }
            else if (tag != null && tag.GetType().GetProperty("Appear") != null)
            {
                dynamic data = tag;
                ShowAppearPanel(data.NPC, data.Appear, data.Index);
            }
            else if (tag != null && tag.GetType().GetProperty("Talk") != null)
            {
                dynamic data = tag;
                ShowTalkPanel(data.NPC, data.Talk, data.Index);
            }
            else if (tag is TBoxConfig treasureBox)
            {
                ShowTreasureBoxPanel(treasureBox);
            }
            else if (tag is EncountInfo encounter)
            {
                ShowEncounterPanel(encounter);
            }
            else
            {
                ShowDefaultPanel();
            }
        }

        #region Panel Display Methods
        private void ShowMapEnvironmentPanel()
        {
            RightPanelHeaderText = "Map Environment";
            _mapEnvironmentPanel.LoadData(MapEnvironment);
            RightPanelContent = _mapEnvironmentPanel;
        }

        private void ShowFuncpointPanel(Funcpoint funcpoint)
        {
            RightPanelHeaderText = $"{funcpoint.MapID}_funcpt";
            _funcpointPanel.LoadData(funcpoint);
            RightPanelContent = _funcpointPanel;
        }

        private void ShowEventPanel(Event eventData)
        {
            RightPanelHeaderText = $"Event: {eventData.EventName}";
            _eventTriggerPanel.LoadData(eventData);
            RightPanelContent = _eventTriggerPanel;
        }

        private void ShowEventMapJumpPanel(Event eventData)
        {
            RightPanelHeaderText = $"Map Jump: {eventData.EventName}";
            _eventMapJumpPanel.LoadData(eventData);
            RightPanelContent = _eventMapJumpPanel;
        }

        private void ShowEventSEPanel(Event eventData)
        {
            RightPanelHeaderText = $"Sound Effect: {eventData.EventName}";
            _eventSEPanel.LoadData(eventData);
            RightPanelContent = _eventSEPanel;
        }

        private void ShowNPCPanel(NPCBase npc)
        {
            RightPanelHeaderText = $"NPC: {npc.ID:X8}";
            _npcPanel.LoadData(npc);
            RightPanelContent = _npcPanel;
        }

        private void ShowAppearPanel(NPCBase npc, NPCAppear appear, int index)
        {
            RightPanelHeaderText = $"Appear {index} for NPC: {npc.ID:X8}";
            _appearPanel.LoadData(appear);
            RightPanelContent = _appearPanel;
        }

        private void ShowTalkPanel(NPCBase npc, NPCTalkConfig talk, int index)
        {
            RightPanelHeaderText = $"Talk {index} for NPC: {npc.ID:X8}";
            _talkPanel.LoadData(talk);
            RightPanelContent = _talkPanel;
        }

        private void ShowTreasureBoxPanel(TBoxConfig treasureBox)
        {
            RightPanelHeaderText = $"Treasure Box: {treasureBox.TBoxID:X8}";
            _treasureBoxPanel.LoadData(treasureBox);
            RightPanelContent = _treasureBoxPanel;
        }

        private void ShowHealPointPanel(Healpoint healpoint)
        {
            RightPanelHeaderText = $"{healpoint.MapID}_healpt";
            _healPointPanel.LoadData(healpoint);
            RightPanelContent = _healPointPanel;
        }

        private void ShowHealAreaPanel(HealArea healArea)
        {
            RightPanelHeaderText = $"Heal Area: {healArea.HealAreaName}";
            _healAreaPanel.LoadData(healArea);
            _healAreaPanel.SetViewModel(this);
            RightPanelContent = _healAreaPanel;
        }

        private void ShowEncounterPanel(EncountInfo encounter)
        {
            RightPanelHeaderText = $"Encounter: {encounter.TeamID:X8}";
            _encounterPanel.LoadData(encounter);
            RightPanelContent = _encounterPanel;
        }

        private void ShowDefaultPanel()
        {
            RightPanelHeaderText = "Properties";

            var defaultText = new TextBlock
            {
                Text = "Select an item from the tree",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("Theme.Text.SecondaryBrush"),
                FontStyle = FontStyles.Italic,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(20)
            };

            RightPanelContent = defaultText;
        }
        #endregion
    }
}
