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

namespace Ocelot.ViewModels
{
    public partial class OcelotViewModel : BaseViewModel
    {
        // Propriété pour lier la racine de l'arborescence à la vue
        private ObservableCollection<TreeViewItemViewModel> _npcTreeItems;
        public ObservableCollection<TreeViewItemViewModel> NPCTreeItems
        {
            get => _npcTreeItems;
            set => SetProperty(ref _npcTreeItems, value);
        }

        // Propriétés pour l'élément sélectionné dans l'arborescence
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

        // Propriétés pour les données du panneau droit
        private Event _selectedEvent;
        public Event SelectedEvent
        {
            get => _selectedEvent;
            set => SetProperty(ref _selectedEvent, value);
        }

        private HealArea _selectedHealArea;
        public HealArea SelectedHealArea
        {
            get => _selectedHealArea;
            set => SetProperty(ref _selectedHealArea, value);
        }

        private NPCBase _selectedNPC;
        public NPCBase SelectedNPC
        {
            get => _selectedNPC;
            set => SetProperty(ref _selectedNPC, value);
        }

        // Méthode pour peupler l'arborescence
        public void PopulateTreeView()
        {
            if (NPCTreeItems == null)
            {
                NPCTreeItems = new ObservableCollection<TreeViewItemViewModel>();
            }

            NPCTreeItems.Clear();

            if (MapEnvironment != null)
            {
                NPCTreeItems.Add(new TreeViewItemViewModel { Header = $"{MapEnvironment.MapID}_mapenv", Tag = "Mapenv" });
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
                {
                    eventNode.Children.Add(new TreeViewItemViewModel { Header = evt.EventName, Tag = new { Type = "Event", Data = evt } });
                }
                funcptItem.Children.Add(eventNode);

                var mapJumpNode = new TreeViewItemViewModel { Header = "Map Jump", Tag = new { Type = "MapJumpCategory", Data = FunctionPoint } };
                var mapJumpEvents = FunctionPoint.Events?.Where(e => e.EventName.StartsWith("MJ")).ToList() ?? new List<Event>();
                foreach (var evt in mapJumpEvents)
                {
                    mapJumpNode.Children.Add(new TreeViewItemViewModel { Header = evt.EventName, Tag = new { Type = "MapJump", Data = evt } });
                }
                funcptItem.Children.Add(mapJumpNode);

                var soundEffectNode = new TreeViewItemViewModel { Header = "Sound Effect", Tag = new { Type = "SoundEffectCategory", Data = FunctionPoint } };
                var soundEffectEvents = FunctionPoint.Events?.Where(e => e.EventName.StartsWith("MS")).ToList() ?? new List<Event>();
                foreach (var evt in soundEffectEvents)
                {
                    soundEffectNode.Children.Add(new TreeViewItemViewModel { Header = evt.EventName, Tag = new { Type = "SoundEffect", Data = evt } });
                }
                funcptItem.Children.Add(soundEffectNode);

                NPCTreeItems.Add(funcptItem);
            }

            if (HealPoint != null)
            {
                var healptItem = new TreeViewItemViewModel
                {
                    Header = $"{HealPoint.MapID}_healpt",
                    Tag = new { Type = "HealpointRoot", Data = HealPoint }
                };

                foreach (var healArea in HealPoint.HealAreas)
                {
                    healptItem.Children.Add(new TreeViewItemViewModel
                    {
                        Header = healArea.HealAreaName,
                        Tag = new { Type = "HealArea", Data = healArea }
                    });
                }

                NPCTreeItems.Add(healptItem);
            }

            if (NPCs?.Count > 0)
            {
                var npcRootItem = new TreeViewItemViewModel
                {
                    Header = $"{_currentFolderName}_npc",
                    Tag = this // Peut être utile pour le DataContext de commandes liées
                };

                // Le ContextMenu sera géré dans la vue via des styles ou des DataTemplates

                foreach (var npc in NPCs)
                {
                    var npcItem = new TreeViewItemViewModel
                    {
                        Header = GetNPCName(npc),
                        Tag = new { Type = "NPC", Data = npc }
                    };

                    var appearsNode = new TreeViewItemViewModel
                    {
                        Header = "Appears",
                        Tag = new { Type = "AppearsCategory", NPC = npc }
                    };

                    if (NPCAppearDict != null && NPCAppearDict.TryGetValue(npc.ID, out var appears))
                    {
                        for (int i = 0; i < appears.Count; i++)
                        {
                            appearsNode.Children.Add(new TreeViewItemViewModel
                            {
                                Header = $"Appear_{i}",
                                Tag = new { Type = "NPCAppear", NPC = npc, Appear = appears[i], Index = i }
                            });
                        }
                    }
                    npcItem.Children.Add(appearsNode);

                    var talksNode = new TreeViewItemViewModel
                    {
                        Header = "Talks",
                        Tag = new { Type = "TalksCategory", NPC = npc }
                    };

                    if (NPCTalkDict != null && NPCTalkDict.TryGetValue(npc.ID, out var talks))
                    {
                        for (int i = 0; i < talks.Count; i++)
                        {
                            talksNode.Children.Add(new TreeViewItemViewModel
                            {
                                Header = $"Talk_{i}",
                                Tag = new { Type = "NPCTalk", NPC = npc, Talk = talks[i], Index = i }
                            });
                        }
                    }
                    npcItem.Children.Add(talksNode);

                    npcRootItem.Children.Add(npcItem);
                }

                NPCTreeItems.Add(npcRootItem);
            }

            if (TreasureBoxes?.Count > 0)
            {
                var treasureItem = new TreeViewItemViewModel { Header = "Treasure Boxes", Tag = "TreasureBoxes" };
                foreach (var treasure in TreasureBoxes)
                {
                    treasureItem.Children.Add(new TreeViewItemViewModel { Header = treasure.TBoxID.ToString("X8"), Tag = treasure });
                }
                NPCTreeItems.Add(treasureItem);
            }

            if (Encounters?.Count > 0)
            {
                var encounterItem = new TreeViewItemViewModel { Header = "Encounters", Tag = "Encounters" };
                foreach (var encounter in Encounters)
                {
                    encounterItem.Children.Add(new TreeViewItemViewModel { Header = encounter.TeamID.ToString("X8"), Tag = encounter });
                }
                NPCTreeItems.Add(encounterItem);
            }
        }

        // Nouvelle méthode dans le ViewModel pour gérer la logique de sélection
        private void HandleSelectedItemChanged(TreeViewItemViewModel selectedItem)
        {
            if (selectedItem == null) return;

            // Réinitialiser les sélections précédentes
            SelectedEvent = null;
            SelectedHealArea = null;
            SelectedNPC = null;

            // Appeler une méthode pour mettre à jour le panneau de droite, si nécessaire
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
                    _selectedNPCAppearIndices.Remove(SelectedNPC.ID); // Utilisez SelectedNPC
                }
                else if (type == "NPCAppear")
                {
                    SelectedNPC = data.NPC;
                    _selectedNPCAppearIndices[SelectedNPC.ID] = data.Index; // Utilisez SelectedNPC
                }
                else if (type == "NPCTalk")
                {
                    if (SelectedNPC == null || SelectedNPC.ID != data.NPC.ID) // Utilisez SelectedNPC
                    {
                        SelectedNPC = data.NPC;
                        _selectedNPCAppearIndices.Remove(SelectedNPC.ID); // Utilisez SelectedNPC
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
