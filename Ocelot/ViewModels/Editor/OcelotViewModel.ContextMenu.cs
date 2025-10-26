using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Ocelot.Models;
using Ocelot.ViewModels.TreeView;
using StudioElevenGUI.Services;
using StudioElevenGUI.ViewModels;
using StudioElevenLib.Level5.Binary.Logic;
using StudioElevenLib.Tools;
using Ocelot.Models.Tags;

namespace Ocelot.ViewModels
{
    public partial class OcelotViewModel : BaseViewModel
    {
        public ICommand AddNpcCommand { get; private set; }
        public ICommand DuplicateNpcCommand { get; private set; }
        public ICommand DeleteNpcCommand { get; private set; }

        public ICommand AddTalkCommand { get; private set; }
        public ICommand DuplicateTalkCommand { get; private set; }
        public ICommand DeleteTalkCommand { get; private set; }

        public ICommand AddAppearCommand { get; private set; }
        public ICommand DuplicateAppearCommand { get; private set; }
        public ICommand DeleteAppearCommand { get; private set; }

        public ICommand AddHealPointCommand { get; private set; }

        public void InitializeContextMenuCommand()
        {
            AddNpcCommand = new RelayCommand(ExecuteAddNpc);
            DuplicateNpcCommand = new RelayCommand(ExecuteDuplicateNpc);
            DeleteNpcCommand = new RelayCommand(ExecuteDeleteNpc);

            AddTalkCommand = new RelayCommand(ExecuteAddTalk);
            DuplicateTalkCommand = new RelayCommand(ExecuteDuplicateTalk);
            DeleteTalkCommand = new RelayCommand(ExecuteDeleteTalk);

            AddAppearCommand = new RelayCommand(ExecuteAddAppear);
            DuplicateAppearCommand = new RelayCommand(ExecuteDuplicateAppear);
            DeleteAppearCommand = new RelayCommand(ExecuteDeleteAppear);

            AddHealPointCommand = new RelayCommand(ExecuteAddHealPoint);
        }

        private void ExecuteAddNpc(object parameter)
        {
            string npcIdInput = DialogService.ShowInputBox("Enter NPC ID:", "Add NPC", "");

            if (string.IsNullOrWhiteSpace(npcIdInput))
                return;

            uint crc32 = Crc32.Compute(Encoding.UTF8.GetBytes(npcIdInput));
            int crc32AsInt = unchecked((int)crc32);

            if (NPCs == null)
                NPCs = new List<NPCBase>();

            if (NPCs.Any(npc => npc.ID == crc32AsInt))
            {
                MessageBox.Show($"NPC with ID {npcIdInput} already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newNpc = new NPCBase
            {
                ID = crc32AsInt
            };

            NPCs.Add(newNpc);

            MessageBox.Show($"NPC {npcIdInput} added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            PopulateTreeView();
        }

        private void ExecuteDuplicateNpc(object parameter)
        {
            if (!(parameter is TreeViewItemViewModel treeItem))
                return;

            var tagObject = treeItem.Tag as NPCTagData;
            if (tagObject == null || tagObject.Type != "NPC")
                return;

            var originalNpc = tagObject.Data;

            // Generate a random ID
            var random = new Random();
            int newId;
            do
            {
                newId = random.Next(int.MinValue, int.MaxValue);
            } while (NPCs.Any(npc => npc.ID == newId));

            var duplicatedNpc = originalNpc.Clone();

            // Change the NPC ID to avoid duplicate
            duplicatedNpc.ID = newId;

            NPCs.Add(duplicatedNpc);

            // Duplicate appears if they exist
            if (NPCAppearDict != null && NPCAppearDict.TryGetValue(originalNpc.ID, out var appears))
            {
                NPCAppearDict[newId] = new List<NPCAppear>(appears.Select(appear => appear.Clone()));
            }

            // Duplicate talks if they exist
            if (NPCTalkDict != null && NPCTalkDict.TryGetValue(originalNpc.ID, out var talks))
            {
                NPCTalkDict[newId] = new List<NPCTalkConfig>(talks.Select(talk => talk.Clone()));
            }

            MessageBox.Show($"NPC {GetNPCName(originalNpc)} duplicated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            PopulateTreeView();
        }

        private void ExecuteDeleteNpc(object parameter)
        {
            if (!(parameter is TreeViewItemViewModel treeItem))
                return;

            var tagObject = treeItem.Tag as NPCTagData;
            if (tagObject == null || tagObject.Type != "NPC")
                return;

            var npc = tagObject.Data;
            string npcName = GetNPCName(npc);

            var result = MessageBox.Show($"Are you sure you want to delete {npcName}?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NPCs.Remove(npc);

                // Supprimer les appears associés
                if (NPCAppearDict != null && NPCAppearDict.ContainsKey(npc.ID))
                {
                    NPCAppearDict.Remove(npc.ID);
                }

                // Supprimer les talks associés
                if (NPCTalkDict != null && NPCTalkDict.ContainsKey(npc.ID))
                {
                    NPCTalkDict.Remove(npc.ID);
                }

                MessageBox.Show($"{npcName} deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                PopulateTreeView();
            }
        }

        private void ExecuteAddTalk(object parameter)
        {
            if (!(parameter is TreeViewItemViewModel treeItem))
                return;

            var tagObject = treeItem.Tag as TalksCategoryTagData;
            if (tagObject == null || tagObject.Type != "TalksCategory")
                return;

            var npc = tagObject.NPC;

            if (NPCTalkDict == null)
                NPCTalkDict = new Dictionary<int, List<NPCTalkConfig>>();

            if (!NPCTalkDict.ContainsKey(npc.ID))
                NPCTalkDict[npc.ID] = new List<NPCTalkConfig>();

            var newTalk = new NPCTalkConfig
            {
                // 
            };

            NPCTalkDict[npc.ID].Add(newTalk);

            MessageBox.Show("Talk added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            PopulateTreeView();
        }

        private void ExecuteDuplicateTalk(object parameter)
        {
            if (!(parameter is TreeViewItemViewModel treeItem))
                return;

            var tagObject = treeItem.Tag as NPCTalkTagData;
            if (tagObject == null || tagObject.Type != "NPCTalk")
                return;

            var npc = tagObject.NPC;
            var originalTalk = tagObject.Talk;

            if (NPCTalkDict != null && NPCTalkDict.TryGetValue(npc.ID, out var talks))
            {
                var duplicatedTalk = originalTalk.Clone();

                talks.Add(duplicatedTalk);

                MessageBox.Show("Talk duplicated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                PopulateTreeView();
            }
        }

        private void ExecuteDeleteTalk(object parameter)
        {
            if (!(parameter is TreeViewItemViewModel treeItem))
                return;

            var tagObject = treeItem.Tag as NPCTalkTagData;
            if (tagObject == null || tagObject.Type != "NPCTalk")
                return;

            var npc = tagObject.NPC;
            var talk = tagObject.Talk;
            int index = tagObject.Index;

            var result = MessageBox.Show($"Are you sure you want to delete Talk_{index}?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (NPCTalkDict != null && NPCTalkDict.TryGetValue(npc.ID, out var talks))
                {
                    talks.Remove(talk);

                    MessageBox.Show("Talk deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    PopulateTreeView();
                }
            }
        }

        private void ExecuteAddAppear(object parameter)
        {
            if (!(parameter is TreeViewItemViewModel treeItem))
                return;

            var tagObject = treeItem.Tag as AppearsCategoryTagData;
            if (tagObject == null || tagObject.Type != "AppearsCategory")
                return;

            var npc = tagObject.NPC;

            if (NPCAppearDict == null)
                NPCAppearDict = new Dictionary<int, List<NPCAppear>>();

            if (!NPCAppearDict.ContainsKey(npc.ID))
                NPCAppearDict[npc.ID] = new List<NPCAppear>();

            var newAppear = new NPCAppear
            {
                // 
            };

            NPCAppearDict[npc.ID].Add(newAppear);

            MessageBox.Show("Appear added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            PopulateTreeView();
        }

        private void ExecuteDuplicateAppear(object parameter)
        {
            if (!(parameter is TreeViewItemViewModel treeItem))
                return;

            var tagObject = treeItem.Tag as NPCAppearTagData;
            if (tagObject == null || tagObject.Type != "NPCAppear")
                return;

            var npc = tagObject.NPC;
            var originalAppear = tagObject.Appear;

            if (NPCAppearDict != null && NPCAppearDict.TryGetValue(npc.ID, out var appears))
            {
                var duplicatedAppear = originalAppear.Clone();

                appears.Add(duplicatedAppear);

                MessageBox.Show("Appear duplicated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                PopulateTreeView();
            }
        }

        private void ExecuteDeleteAppear(object parameter)
        {
            if (!(parameter is TreeViewItemViewModel treeItem))
                return;

            var tagObject = treeItem.Tag as NPCAppearTagData;
            if (tagObject == null || tagObject.Type != "NPCAppear")
                return;

            var npc = tagObject.NPC;
            var appear = tagObject.Appear;
            int index = tagObject.Index;

            var result = MessageBox.Show($"Are you sure you want to delete Appear_{index}?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (NPCAppearDict != null && NPCAppearDict.TryGetValue(npc.ID, out var appears))
                {
                    appears.Remove(appear);

                    MessageBox.Show("Appear deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    PopulateTreeView();
                }
            }
        }

        private void ExecuteAddHealPoint(object parameter)
        {
            string healAreaIdInput = DialogService.ShowInputBox("Enter Healpoint ID:", "Add Healpoint", "");

            if (string.IsNullOrWhiteSpace(healAreaIdInput))
                return;

            if (HealPoint == null)
                HealPoint = new Healpoint()
                {
                    MapID = _currentFolderName
                };

            if (HealPoint.HealAreas.Any(healArea => healArea.HealAreaName == healAreaIdInput))
            {
                MessageBox.Show($"Healpoint with ID {healAreaIdInput} already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newHealArea = new HealArea
            {
                HealAreaName = healAreaIdInput
            };

            HealPoint.HealAreas.Add(newHealArea);

            MessageBox.Show($"Healpoint {healAreaIdInput} added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            PopulateTreeView();
        }
    }
}