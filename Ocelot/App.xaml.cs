using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using StudioElevenGUI.ViewModels;
using StudioElevenGUI.Services;
using Ocelot.Models;
using StudioElevenLib.Level5.Binary.Collections;
using StudioElevenLib.Level5.Image;
using StudioElevenLib.Level5.Binary;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using StudioElevenLib.Level5.Binary.Logic;
using StudioElevenLib.Collections;
using StudioElevenLib.Level5.Binary.Mapper;

namespace Ocelot
{
    public partial class App : Application
    {
        private string _currentFolderName;
        private string _currentFolderPath;
        private Button _saveButton;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var menuButtonStyle = Application.Current.FindResource("MenuButtonStyle") as Style;

            var openButton = new Button
            {
                Content = "Open",
                Style = menuButtonStyle,
                Margin = new Thickness(0, 0, 5, 0)
            };
            openButton.Click += OpenButton_Click;

            _saveButton = new Button
            {
                Content = "Save",
                Style = menuButtonStyle,
                IsEnabled = false
            };
            _saveButton.Click += SaveButton_Click;

            var viewModel = new MainViewModel
            {
                IsLoading = false,
                Title = "Ocelot",
                MainContent = new Views.OcelotMainContent(),
                CustomTitleBarButtons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        openButton,
                        _saveButton
                    }
                }
            };

            var workingArea = SystemParameters.WorkArea;

            var mainWindow = new StudioElevenGUI.Views.MainWindow
            {
                DataContext = viewModel,
                IsMaximized = true,
                Left = workingArea.Left,
                Top = workingArea.Top,
                Width = workingArea.Width,
                Height = workingArea.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            mainWindow.Show();

            LoadNPCJsonData();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedFolderPath = DialogService.ShowSelectFolderDialog("Open Inazuma Eleven Go Map Folder");

                if (!string.IsNullOrEmpty(selectedFolderPath))
                {
                    _currentFolderPath = selectedFolderPath;
                    _currentFolderName = Path.GetFileName(selectedFolderPath);

                    // Get the minimap
                    string xiFilePath = Path.Combine(selectedFolderPath, $"{_currentFolderName}.xi");
                    if (File.Exists(xiFilePath))
                    {
                        LoadImageFile(xiFilePath);
                    }

                    // Load mapenv.bin
                    string mapenvFilePath = Path.Combine(selectedFolderPath, $"{_currentFolderName}_mapenv.bin");
                    if (File.Exists(mapenvFilePath))
                    {
                        LoadMapenvFile(mapenvFilePath);
                    }

                    // Load funcpt.bin
                    string funcptFilePath = Path.Combine(selectedFolderPath, $"{_currentFolderName}_funcpt.bin");
                    if (File.Exists(funcptFilePath))
                    {
                        LoadFuncptFile(funcptFilePath);
                    }

                    // Load healpt.bin
                    string healptFilePath = Path.Combine(selectedFolderPath, $"{_currentFolderName}_healpt.bin");
                    if (File.Exists(healptFilePath))
                    {
                        LoadHealptFile(healptFilePath);
                    }

                    // Load npc.bin
                    string npcFilePath = Path.Combine(selectedFolderPath, $"{_currentFolderName}.npc.bin");
                    if (File.Exists(npcFilePath))
                    {
                        LoadNpcFile(npcFilePath);
                    }

                    _saveButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening the folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save mapenv.bin
            string mapenvFilePath = Path.Combine(_currentFolderPath, $"{_currentFolderName}_mapenv.bin");
            SaveMapenvFile(mapenvFilePath);

            // Save healpt.bin
            string healptFilePath = Path.Combine(_currentFolderPath, $"{_currentFolderName}_healpt.bin");
            SaveHealptFile(healptFilePath);

            // Save npc.bin
            string npcFilePath = Path.Combine(_currentFolderPath, $"{_currentFolderName}.npc.bin");
            SaveNpcFile(npcFilePath);

            // Save talk.bin
            string talkFilePath = Path.Combine(_currentFolderPath, $"{_currentFolderName}.talk.bin");
            SaveTalkFile(talkFilePath);

            MessageBox.Show("Saved!");
        }

        private void LoadImageFile(string xiFilePath)
        {
            try
            {
                byte[] fileContent = File.ReadAllBytes(xiFilePath);
                var bitmap = IMGC.ToBitmap(fileContent);

                var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
                var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;
                ocelotMainContent.ViewModel?.LoadImage(bitmap);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMapenvFile(string mapenvFilePath)
        {
            try
            {
                var ptreeMapenv = new CfgBin<PtreeNode>();
                ptreeMapenv.Open(File.ReadAllBytes(mapenvFilePath));

                PtreeNode ptreeMap = ptreeMapenv.Entries.FindByHeader("MAP_ENV");
                if (ptreeMap != null)
                {
                    var mapenv = new Models.Mapenv(ptreeMap);

                    // Si pas de MMModelPos, en créer un par défaut
                    if (mapenv.MMModelPos == null)
                    {
                        mapenv.MMModelPos = new Models.MMModelPos
                        {
                            MinX = -400,
                            MinY = -340,
                            MaxX = 400,
                            MaxY = 460
                        };
                    }

                    // Trouve le MainContent et met à jour les données
                    var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
                    var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;
                    ocelotMainContent.ViewModel?.LoadMapenv(mapenv);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading mapenv file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveMapenvFile(string mapenvFilePath)
        {
            CfgBin<PtreeNode> ptreeMapenv = new CfgBin<PtreeNode>();
            ptreeMapenv.Encoding = Encoding.GetEncoding("SHIFT-JIS");

            var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
            var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;

            ptreeMapenv.Entries.AddChild(ocelotMainContent.ViewModel.MapEnvironment.ToPtreeNode());

            ptreeMapenv.Save(mapenvFilePath);
        }

        private void LoadFuncptFile(string funcptFilePath)
        {
            try
            {
                var test = new CfgBin<PtreeNode>();
                byte[] fileData = File.ReadAllBytes(funcptFilePath);
                test.Open(fileData);
                PtreeNode funcPtreeNode = test.Entries.FindByHeader("FUNCPOINT");

                if (funcPtreeNode != null)
                {
                    Funcpoint funcpoint = new Funcpoint(funcPtreeNode);

                    // Trouve le MainContent et met à jour les données
                    var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
                    var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;
                    ocelotMainContent?.ViewModel.LoadFuncpoint(funcpoint);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading the funcpt file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHealptFile(string healptFilePath)
        {
            try
            {
                var test = new CfgBin<PtreeNode>();
                byte[] fileData = File.ReadAllBytes(healptFilePath);
                test.Open(fileData);
                PtreeNode healPtreeNode = test.Entries.FindByHeader("HEALPOINT");

                if (healPtreeNode != null)
                {
                    Healpoint healpoint = new Healpoint(healPtreeNode);

                    // Trouve le MainContent et met à jour les données
                    var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
                    var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;
                    ocelotMainContent.ViewModel?.LoadHealpoint(healpoint);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading healpt file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveHealptFile(string healptFilePath)
        {
            CfgBin<PtreeNode> ptreeHealpt = new CfgBin<PtreeNode>();
            ptreeHealpt.Encoding = Encoding.GetEncoding("SHIFT-JIS");

            var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
            var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;

            ptreeHealpt.Entries.AddChild(ocelotMainContent.ViewModel.HealPoint.ToPtreeNode());

            ptreeHealpt.Save(healptFilePath);
        }

        private void LoadNpcFile(string npcFilePath)
        {
            try
            {
                var npcBin = new CfgBin<CfgTreeNode>();
                byte[] fileData = File.ReadAllBytes(npcFilePath);
                npcBin.Open(fileData);

                var npcBases = new List<NPCBase>();
                var npcPresets = new List<NPCPreset>();
                var npcAppears = new List<NPCAppear>();

                // Load NPC_BASE
                CfgTreeNode npcBaseBegin = npcBin.Entries.FindByName("NPC_BASE_BEGIN");
                if (npcBaseBegin != null)
                {
                    npcBases = npcBaseBegin.FlattenEntryToClassList<NPCBase>("NPC_BASE");
                }

                // Load NPC_PRESET
                CfgTreeNode npcPresetBegin = npcBin.Entries.FindByName("NPC_PRESET_BEGIN");
                if (npcPresetBegin != null)
                {
                    npcPresets = npcPresetBegin.FlattenEntryToClassList<NPCPreset>("NPC_PRESET");
                }

                // Load NPC_APPEAR
                CfgTreeNode npcAppearBegin = npcBin.Entries.FindByName("NPC_APPEAR_BEGIN");
                if (npcAppearBegin != null)
                {
                    npcAppears = npcAppearBegin.FlattenEntryToClassList<NPCAppear>("NPC_APPEAR");
                }

                // Create dictionaries for linking
                var npcAppearDict = new Dictionary<int, List<NPCAppear>>();

                // Link NPCBase with NPCAppear using NPCPreset
                foreach (var preset in npcPresets)
                {
                    var appears = new List<NPCAppear>();
                    for (int i = preset.NPCAppearStartIndex; i < preset.NPCAppearStartIndex + preset.NPCAppearCount && i < npcAppears.Count; i++)
                    {
                        appears.Add(npcAppears[i]);
                    }
                    npcAppearDict[preset.NPCBaseID] = appears;
                }

                // Try to load talk file
                string talkFilePath = npcFilePath.Replace(".npc.bin", ".talk.bin");
                var npcTalkDict = new Dictionary<int, List<NPCTalkConfig>>();

                if (File.Exists(talkFilePath))
                {
                    npcTalkDict = LoadTalkFile(talkFilePath);
                }

                // Pass data to main content
                var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
                var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;
                ocelotMainContent?.ViewModel.LoadNPCData(npcBases, npcAppearDict, npcTalkDict, _currentFolderName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading NPC file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveNpcFile(string npcFilePath)
        {
            try
            {
                CfgBin<CfgTreeNode> npcBin = new CfgBin<CfgTreeNode>();
                npcBin.Encoding = Encoding.GetEncoding("SHIFT-JIS");

                var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
                var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;

                if (ocelotMainContent?.ViewModel?.NPCs == null)
                {
                    Console.WriteLine("No NPC data to save.");
                    return;
                }

                var npcBases = ocelotMainContent.ViewModel.NPCs;
                var npcAppearDict = ocelotMainContent.ViewModel.NPCAppearDict ?? new Dictionary<int, List<NPCAppear>>();

                // Prepare the lists
                var npcPresets = new List<NPCPreset>();
                var npcAppears = new List<NPCAppear>();

                int currentAppearIndex = 0;

                // Build lists from NPCBase
                foreach (var npcBase in npcBases)
                {
                    // Retrieve the appears for this NPC.
                    List<NPCAppear> appears = null;
                    if (npcAppearDict.ContainsKey(npcBase.ID))
                    {
                        appears = npcAppearDict[npcBase.ID];
                    }

                    int appearCount = appears?.Count ?? 0;

                    // Create the preset
                    var preset = new NPCPreset
                    {
                        NPCBaseID = npcBase.ID,
                        NPCAppearStartIndex = currentAppearIndex,
                        NPCAppearCount = appearCount
                    };
                    npcPresets.Add(preset);

                    // Add the appears to the global list
                    if (appears != null && appears.Count > 0)
                    {
                        npcAppears.AddRange(appears);
                        currentAppearIndex += appears.Count;
                    }
                }

                // Add nodes
                npcBin.Entries.AddBoundedEntryFromClassList(npcBases, "NPC_BASE_BEGIN", "NPC_BASE");
                npcBin.Entries.AddBoundedEntryFromClassList(npcPresets, "NPC_PRESET_BEGIN", "NPC_PRESET");
                npcBin.Entries.AddBoundedEntryFromClassList(npcAppears, "NPC_APPEAR_BEGIN", "NPC_APPEAR");

                // Save the file
                npcBin.Save(npcFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving NPC file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Dictionary<int, List<NPCTalkConfig>> LoadTalkFile(string talkFilePath)
        {
            var npcTalkDict = new Dictionary<int, List<NPCTalkConfig>>();

            try
            {
                var talkBin = new CfgBin<CfgTreeNode>();
                byte[] fileData = File.ReadAllBytes(talkFilePath);
                talkBin.Open(fileData);

                var npcTalkInfos = new List<NPCTalkInfo>();
                var npcTalkConfigs = new List<NPCTalkConfig>();

                // Load TALK_INFO
                CfgTreeNode talkInfoBegin = talkBin.Entries.FindByName("TALK_INFO_BEGIN");
                if (talkInfoBegin != null)
                {
                    npcTalkInfos = talkInfoBegin.FlattenEntryToClassList<NPCTalkInfo>("TALK_INFO");
                }

                // Load TALK_CONFIG
                CfgTreeNode talkConfigBegin = talkBin.Entries.FindByName("TALK_CONFIG_BEGIN");
                if (talkConfigBegin != null)
                {
                    npcTalkConfigs = talkConfigBegin.FlattenEntryToClassList<NPCTalkConfig>("TALK_CONFIG");
                }

                // Link NPCBase with NPCTalkConfig using NPCTalkInfo
                foreach (var talkInfo in npcTalkInfos)
                {
                    var talks = new List<NPCTalkConfig>();

                    for (int i = talkInfo.TalkConfigStartIndex; i < talkInfo.TalkConfigStartIndex + talkInfo.TalkConfigCount && i < npcTalkConfigs.Count; i++)
                    {
                        talks.Add(npcTalkConfigs[i]);
                    }

                    npcTalkDict[talkInfo.NPCBaseID] = talks;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du fichier Talk : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return npcTalkDict;
        }

        private void SaveTalkFile(string talkFilePath)
        {
            try
            {
                var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
                var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;

                if (ocelotMainContent?.ViewModel?.NPCs == null || ocelotMainContent?.ViewModel?.NPCTalkDict == null)
                {
                    Console.WriteLine("No NPC Talk data to save.");
                    return;
                }

                var npcBases = ocelotMainContent.ViewModel.NPCs;
                var npcTalkDict = ocelotMainContent.ViewModel.NPCTalkDict;

                CfgBin<CfgTreeNode> talkBin = new CfgBin<CfgTreeNode>();
                talkBin.Encoding = Encoding.GetEncoding("SHIFT-JIS");

                var npcTalkInfos = new List<NPCTalkInfo>();
                var npcTalkConfigs = new List<NPCTalkConfig>();

                int currentTalkConfigIndex = 0;

                // Build lists from NPCBase and TalkDict
                foreach (var npcBase in npcBases)
                {
                    List<NPCTalkConfig> talkConfigs = null;

                    if (npcTalkDict.ContainsKey(npcBase.ID))
                        talkConfigs = npcTalkDict[npcBase.ID];

                    int talkCount = talkConfigs?.Count ?? 0;

                    var talkInfo = new NPCTalkInfo
                    {
                        NPCBaseID = npcBase.ID,
                        TalkConfigStartIndex = currentTalkConfigIndex,
                        TalkConfigCount = talkCount
                    };
                    npcTalkInfos.Add(talkInfo);

                    if (talkConfigs != null && talkConfigs.Count > 0)
                    {
                        npcTalkConfigs.AddRange(talkConfigs);
                        currentTalkConfigIndex += talkConfigs.Count;
                    }
                }

                // Add nodes
                talkBin.Entries.AddBoundedEntryFromClassList(npcTalkInfos, "TALK_INFO_BEGIN", "TALK_INFO");
                talkBin.Entries.AddBoundedEntryFromClassList(npcTalkConfigs, "TALK_CONFIG_BEGIN", "TALK_CONFIG");

                // Save to file
                talkBin.Save(talkFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving Talk file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadNPCJsonData()
        {
            var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
            var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;

            try
            {
                string npcJsonPath = "./Resources/npc.json";
                if (File.Exists(npcJsonPath))
                {
                    string jsonContent = File.ReadAllText(npcJsonPath);
                    ocelotMainContent.ViewModel.SetNPCJsonData(JsonConvert.DeserializeObject<NPCJsonData>(jsonContent));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading NPC JSON data: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}