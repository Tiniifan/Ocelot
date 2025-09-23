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

namespace Ocelot
{
    public partial class App : Application
    {
        private string _currentFolderName;
        private string _currentFolderPath;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoadStudioElevenBaseResources();

            var menuButtonStyle = Application.Current.FindResource("MenuButtonStyle") as Style;

            var openButton = new Button
            {
                Content = "Open",
                Style = menuButtonStyle,
                Margin = new Thickness(0, 0, 5, 0)
            };
            openButton.Click += OpenButton_Click;

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
                        new Button { Content = "Save", Style = menuButtonStyle }
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
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var services = new DialogService();
                string selectedFolderPath = services.ShowSelectFolderDialog("Open Inazuma Eleven Go Map Folder");

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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening the folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadImageFile(string xiFilePath)
        {
            try
            {
                byte[] fileContent = File.ReadAllBytes(xiFilePath);
                var bitmap = IMGC.ToBitmap(fileContent);

                var mainViewModel = Current.MainWindow?.DataContext as MainViewModel;
                var ocelotMainContent = mainViewModel?.MainContent as Views.OcelotMainContent;
                ocelotMainContent?.LoadImage(bitmap);
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
                    ocelotMainContent?.LoadMapenv(mapenv);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading mapenv file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    ocelotMainContent?.LoadFuncpoint(funcpoint);
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
                    ocelotMainContent?.LoadHealpoint(healpoint);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading healpt file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                ocelotMainContent?.LoadNPCData(npcBases, npcAppearDict, npcTalkDict, _currentFolderName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading NPC file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void LoadStudioElevenBaseResources()
        {
            try
            {
                // First loads a default color theme (required for StudioElevenTheme.xaml)
                var defaultColorDict = new ResourceDictionary
                {
                    Source = new Uri("/StudioElevenGUI;component/Styles/Colors/WhiteTheme.xaml", UriKind.Relative)
                };
                Application.Current.Resources.MergedDictionaries.Add(defaultColorDict);

                // Then loads the main theme that references the colors
                var themeDict = new ResourceDictionary
                {
                    Source = new Uri("/StudioElevenGUI;component/Styles/StudioElevenTheme.xaml", UriKind.Relative)
                };
                Application.Current.Resources.MergedDictionaries.Add(themeDict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading StudioElevenGUI resources: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}