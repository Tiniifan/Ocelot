using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocelot.Models;
using System.Windows.Controls;
using System.Windows;
using StudioElevenGUI.ViewModels;
using StudioElevenLib.Collections;
using Ocelot.ViewModels.TreeView;
using System.Collections.ObjectModel;
using Ocelot.Models.Tags;

namespace Ocelot.ViewModels
{
    public partial class OcelotViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the root collection of TreeView items bound to the view.
        /// </summary>
        private ObservableCollection<TreeViewItemViewModel> _npcTreeItems;
        public ObservableCollection<TreeViewItemViewModel> NPCTreeItems
        {
            get => _npcTreeItems;
            set => SetProperty(ref _npcTreeItems, value);
        }

        /// <summary>
        /// Gets or sets the currently selected TreeView item.
        /// Invokes selection handling when changed.
        /// </summary>
        private TreeViewItemViewModel _selectedTreeViewItem;
        public TreeViewItemViewModel SelectedTreeViewItem
        {
            get => _selectedTreeViewItem;
            set
            {
                SetProperty(ref _selectedTreeViewItem, value);
                HandleSelectedItemChanged(value);
            }
        }

        /// <summary>
        /// Gets or sets the currently selected Event for the right-hand panel.
        /// </summary>
        private Event _selectedEvent;
        public Event SelectedEvent
        {
            get => _selectedEvent;
            set => SetProperty(ref _selectedEvent, value);
        }

        /// <summary>
        /// Gets or sets the currently selected HealArea for the right-hand panel.
        /// </summary>
        private HealArea _selectedHealArea;
        public HealArea SelectedHealArea
        {
            get => _selectedHealArea;
            set => SetProperty(ref _selectedHealArea, value);
        }

        /// <summary>
        /// Gets or sets the currently selected NPCBase for the right-hand panel.
        /// </summary>
        private NPCBase _selectedNPC;
        public NPCBase SelectedNPC
        {
            get => _selectedNPC;
            set => SetProperty(ref _selectedNPC, value);
        }

        /// <summary>
        /// Populates the TreeView with all relevant items including map environment, function points,
        /// heal points, NPCs (with their appears and talks), treasure boxes, and encounters.
        /// Sets up context menus for each category and expands the root node while keeping its immediate children collapsed.
        /// </summary>
        public void PopulateTreeView()
        {
            if (NPCTreeItems == null)
                NPCTreeItems = new ObservableCollection<TreeViewItemViewModel>();

            NPCTreeItems.Clear();

            if (_currentFolderName == null)
            {
                return;
            }

            var rootContextMenu = (ContextMenu)Application.Current.FindResource("RootContextMenu");
            rootContextMenu.DataContext = this;

            var rootItem = new TreeViewItemViewModel
            {
                Header = _currentFolderName,
                Tag = "Root",
                ContextMenu = rootContextMenu
            };

            if (MapEnvironment != null)
            {
                rootItem.Children.Add(new TreeViewItemViewModel { Header = $"{MapEnvironment.MapID}_mapenv", Tag = "Mapenv" });
            }

            if (FunctionPoint != null)
            {
                var funcptItem = new TreeViewItemViewModel
                {
                    Header = $"{FunctionPoint.MapID}_funcpt",
                    Tag = new { Type = "FuncpointRoot", Data = FunctionPoint }
                };

                var eventNode = new TreeViewItemViewModel { Header = "Event", Tag = new { Type = "EventCategory", Data = FunctionPoint } };
                var eventEvents = FunctionPoint.Events?.Where(e => e.EventName.StartsWith("KO") || e.EventName.StartsWith("EV")).ToList() ?? new List<Event>();
                foreach (var evt in eventEvents)
                    eventNode.Children.Add(new TreeViewItemViewModel { Header = evt.EventName, Tag = new { Type = "Event", Data = evt } });
                funcptItem.Children.Add(eventNode);

                var mapJumpNode = new TreeViewItemViewModel { Header = "Map Jump", Tag = new { Type = "MapJumpCategory", Data = FunctionPoint } };
                var mapJumpEvents = FunctionPoint.Events?.Where(e => e.EventName.StartsWith("MJ")).ToList() ?? new List<Event>();
                foreach (var evt in mapJumpEvents)
                    mapJumpNode.Children.Add(new TreeViewItemViewModel { Header = evt.EventName, Tag = new { Type = "MapJump", Data = evt } });
                funcptItem.Children.Add(mapJumpNode);

                var soundEffectNode = new TreeViewItemViewModel { Header = "Sound Effect", Tag = new { Type = "SoundEffectCategory", Data = FunctionPoint } };
                var soundEffectEvents = FunctionPoint.Events?.Where(e => e.EventName.StartsWith("MS")).ToList() ?? new List<Event>();
                foreach (var evt in soundEffectEvents)
                    soundEffectNode.Children.Add(new TreeViewItemViewModel { Header = evt.EventName, Tag = new { Type = "SoundEffect", Data = evt } });
                funcptItem.Children.Add(soundEffectNode);

                rootItem.Children.Add(funcptItem);
            }

            if (HealPoint != null)
            {
                var healPointContextMenu = (ContextMenu)Application.Current.FindResource("HealPointContextMenu");
                healPointContextMenu.DataContext = this;
                
                var healptItem = new TreeViewItemViewModel
                {
                    Header = $"{HealPoint.MapID}_healpt",
                    Tag = new { Type = "HealpointRoot", Data = HealPoint },
                    ContextMenu = healPointContextMenu
                };

                foreach (var healArea in HealPoint.HealAreas)
                {
                    var healAreaItemContextMenu = (ContextMenu)Application.Current.FindResource("HealAreaItemContextMenu");
                    healAreaItemContextMenu.DataContext = this;

                    healptItem.Children.Add(new TreeViewItemViewModel
                    {
                        Header = healArea.HealAreaName,
                        Tag = new { Type = "HealArea", Data = healArea },
                        ContextMenu = healAreaItemContextMenu
                    });
                }

                rootItem.Children.Add(healptItem);
            }

            if (NPCs?.Count > 0)
            {
                var contextMenu = (ContextMenu)Application.Current.FindResource("NpcContextMenu");
                contextMenu.DataContext = this;

                var npcItemContextMenu = (ContextMenu)Application.Current.FindResource("NpcItemContextMenu");
                npcItemContextMenu.DataContext = this;

                var talksNodeContextMenu = (ContextMenu)Application.Current.FindResource("TalksNodeContextMenu");
                talksNodeContextMenu.DataContext = this;

                var talkItemContextMenu = (ContextMenu)Application.Current.FindResource("TalkItemContextMenu");
                talkItemContextMenu.DataContext = this;

                var appearsNodeContextMenu = (ContextMenu)Application.Current.FindResource("AppearsNodeContextMenu");
                appearsNodeContextMenu.DataContext = this;

                var appearItemContextMenu = (ContextMenu)Application.Current.FindResource("AppearItemContextMenu");
                appearItemContextMenu.DataContext = this;

                var npcRootItem = new TreeViewItemViewModel
                {
                    Header = $"{_currentFolderName}_npc",
                    ContextMenu = contextMenu
                };

                foreach (var npc in NPCs)
                {
                    var npcItem = new TreeViewItemViewModel
                    {
                        Header = GetNPCName(npc),
                        Tag = new NPCTagData { Type = "NPC", Data = npc },
                        ContextMenu = npcItemContextMenu,
                    };

                    var appearsNode = new TreeViewItemViewModel
                    {
                        Header = "Appears",
                        Tag = new AppearsCategoryTagData { Type = "AppearsCategory", NPC = npc },
                        ContextMenu = appearsNodeContextMenu,
                    };

                    if (NPCAppearDict != null && NPCAppearDict.TryGetValue(npc.ID, out var appears))
                    {
                        for (int i = 0; i < appears.Count; i++)
                        {
                            appearsNode.Children.Add(new TreeViewItemViewModel
                            {
                                Header = $"Appear_{i}",
                                Tag = new NPCAppearTagData { Type = "NPCAppear", NPC = npc, Appear = appears[i], Index = i },
                                ContextMenu = appearItemContextMenu,
                            });
                        }
                    }
                    npcItem.Children.Add(appearsNode);

                    var talksNode = new TreeViewItemViewModel
                    {
                        Header = "Talks",
                        Tag = new TalksCategoryTagData { Type = "TalksCategory", NPC = npc },
                        ContextMenu = talksNodeContextMenu,
                    };

                    if (NPCTalkDict != null && NPCTalkDict.TryGetValue(npc.ID, out var talks))
                    {
                        for (int i = 0; i < talks.Count; i++)
                        {
                            talksNode.Children.Add(new TreeViewItemViewModel
                            {
                                Header = $"Talk_{i}",
                                Tag = new NPCTalkTagData { Type = "NPCTalk", NPC = npc, Talk = talks[i], Index = i },
                                ContextMenu = talkItemContextMenu,
                            });
                        }
                    }
                    npcItem.Children.Add(talksNode);

                    npcRootItem.Children.Add(npcItem);
                }

                rootItem.Children.Add(npcRootItem);
            }

            if (TreasureBoxes?.Count > 0)
            {
                var treasureItem = new TreeViewItemViewModel { Header = "Treasure Boxes", Tag = "TreasureBoxes" };
                foreach (var treasure in TreasureBoxes)
                {
                    treasureItem.Children.Add(new TreeViewItemViewModel { Header = treasure.TBoxID.ToString("X8"), Tag = treasure });
                }
                rootItem.Children.Add(treasureItem);
            }

            if (Encounters?.Count > 0)
            {
                var encounterItem = new TreeViewItemViewModel { Header = "Encounters", Tag = "Encounters" };
                foreach (var encounter in Encounters)
                {
                    encounterItem.Children.Add(new TreeViewItemViewModel { Header = encounter.TeamID.ToString("X8"), Tag = encounter });
                }
                rootItem.Children.Add(encounterItem);
            }

            rootItem.IsExpanded = true;
            foreach (var child in rootItem.Children)
            {
                child.IsExpanded = false;
            }

            NPCTreeItems.Add(rootItem);
        }

        /// <summary>
        /// Recursively updates all TreeViewItemViewModel headers in the TreeView
        /// to reflect current data without recreating nodes. Preserves selection.
        /// </summary>
        public void RefreshTreeViewNames()
        {
            if (NPCTreeItems == null || NPCTreeItems.Count == 0)
                return;

            // Save the currently selected item
            var selected = SelectedTreeViewItem;

            foreach (var root in NPCTreeItems)
            {
                UpdateNodeNames(root);
            }

            // Restore selection
            SelectedTreeViewItem = selected;
        }

        /// <summary>
        /// Recursively updates a TreeViewItemViewModel header if its underlying data has changed.
        /// Handles NPCs, HealAreas, Events, MapJumps, SoundEffects, TreasureBoxes, and Encounters.
        /// </summary>
        private void UpdateNodeNames(TreeViewItemViewModel node)
        {
            if (node.Tag != null)
            {
                var typeProperty = node.Tag.GetType().GetProperty("Type");
                if (typeProperty != null)
                {
                    string type = typeProperty.GetValue(node.Tag)?.ToString();

                    switch (type)
                    {
                        case "NPC":
                            var npcTag = node.Tag as NPCTagData;
                            if (npcTag != null)
                            {
                                string newName = GetNPCName(npcTag.Data);
                                if (node.Header != newName)
                                    node.Header = newName;
                            }
                            break;

                        case "NPCAppear":
                            dynamic appearTag = node.Tag;
                            string appearName = $"Appear_{appearTag.Index}";
                            if (node.Header != appearName)
                                node.Header = appearName;
                            break;

                        case "TalksCategory":
                            // Header remains static ("Talks")
                            break;

                        case "NPCTalk":
                            dynamic talkTag = node.Tag;
                            string talkName = $"Talk_{talkTag.Index}";
                            if (node.Header != talkName)
                                node.Header = talkName;
                            break;

                        case "HealArea":
                            dynamic healAreaTag = node.Tag;
                            if (node.Header != healAreaTag.Data.HealAreaName)
                                node.Header = healAreaTag.Data.HealAreaName;
                            break;

                        case "Event":
                        case "MapJump":
                        case "SoundEffect":
                            dynamic eventTag = node.Tag;
                            if (node.Header != eventTag.Data.EventName)
                                node.Header = eventTag.Data.EventName;
                            break;

                        case "Mapenv":
                        case "FuncpointRoot":
                        case "HealpointRoot":
                        case "TreasureBoxes":
                        case "Encounters":
                            // Header might be static, skip unless needed
                            break;
                    }
                }
            }

            // Recursively update children
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    UpdateNodeNames(child);
                }
            }
        }

        /// <summary>
        /// Handles changes in the selected TreeView item.
        /// Updates the corresponding properties for Event, HealArea, or NPC,
        /// and triggers any necessary updates to the right-hand panel and overlays.
        /// </summary>
        private void HandleSelectedItemChanged(TreeViewItemViewModel selectedItem)
        {
            if (selectedItem == null) return;

            // Reset previous selections
            SelectedEvent = null;
            SelectedHealArea = null;
            SelectedNPC = null;

            // Call a method to update the right panel, if necessary
            UpdateRightPanel(selectedItem);

            if (selectedItem.Tag != null && selectedItem.Tag.GetType().GetProperty("Type") != null)
            {
                dynamic data = selectedItem.Tag;
                string type = data.Type;

                if (type == "Event" || type == "MapJump" || type == "SoundEffect")
                {
                    SelectedEvent = data.Data;
                }
                else if (type == "HealArea")
                {
                    SelectedHealArea = data.Data;
                }
                else if (type == "NPC")
                {
                    SelectedNPC = data.Data;
                    _selectedNPCAppearIndices.Remove(SelectedNPC.ID);
                }
                else if (type == "NPCAppear")
                {
                    SelectedNPC = data.NPC;
                    _selectedNPCAppearIndices[SelectedNPC.ID] = data.Index;
                }
                else if (type == "NPCTalk")
                {
                    if (SelectedNPC == null || SelectedNPC.ID != data.NPC.ID)
                    {
                        SelectedNPC = data.NPC;
                        _selectedNPCAppearIndices.Remove(SelectedNPC.ID);
                    }
                }
            }

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }
    } 
}
