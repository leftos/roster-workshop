#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

namespace RosterWorkshop
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;

    using LeftosCommonLibrary;

    using Microsoft.Win32;

    using TreeViewWithCheckBoxesLib;

    #endregion

    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : Window
    {
        public static bool OnlyShowCurrentMatchesForCurrent;
        public static bool NoConflictForMatchingTeamID;
        public static bool PreferUnhidden;
        public static int ConflictResult;
        public static Dictionary<string, string> TeamPairs = new Dictionary<string, string>();
        private static readonly string UpdateFileLocalPath = App.AppDocsPath + @"rwversion.txt";

        private readonly Dictionary<string, Dictionary<string, bool?>> _mergeSettings =
            new Dictionary<string, Dictionary<string, bool?>>();

        public MainWindow()
        {
            InitializeComponent();

            Height = Tools.GetRegistrySetting("Height", (int) Height);
            Width = Tools.GetRegistrySetting("Width", (int) Width);
            Left = Tools.GetRegistrySetting("Left", 0);
            Top = Tools.GetRegistrySetting("Top", 0);
        }

        private ObservableCollection<string> rostersToMerge { get; set; }
        protected List<FooViewModel> root { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Tools.AppName = App.AppName;
            Tools.AppRegistryKey = App.AppRegistryKey;
            Tools.OpenRegistryKey(true);

            var w = new BackgroundWorker();
            w.DoWork += (o, args) => CheckForUpdates();
            w.RunWorkerAsync();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var columnsTree = new List<FooViewModel>();
            var lines = Tools.SplitLinesToList(Properties.Resources.REDColumns).ToList();
            for (var i = 0; i < lines.Count; i++)
            {
                var category = new FooViewModel(lines[i]);
                category.Children = new List<FooViewModel>();
                columnsTree.Add(category);
                if (i == lines.Count - 1)
                {
                    break;
                }
                while (i != lines.Count - 1 && lines[++i].StartsWith("\t"))
                {
                    var line = lines[i];
                    var item = new FooViewModel(line.Replace("\t", ""));
                    category.Children.Add(item);
                    if (i == lines.Count - 1)
                    {
                        break;
                    }
                    while (i != lines.Count - 1 && lines[++i].StartsWith("\t\t"))
                    {
                        item.Children.Add(new FooViewModel(lines[i].Replace("\t", "")));
                    }
                    i--;
                }
                if (i == lines.Count - 1)
                {
                    break;
                }
                i--;
            }
            var rootItem = new FooViewModel("All");
            rootItem.Children.AddRange(columnsTree);
            rootItem.Initialize();
            root = new List<FooViewModel> { rootItem };

            prepareExpanded(root);

            trvColumns.ItemsSource = root;

            rbPlayersCurrent.IsChecked = true;
            rbTeamsNone.IsChecked = true;

            rostersToMerge = new ObservableCollection<string>();
            lstRostersToMerge.ItemsSource = rostersToMerge;

            Title += " v" + Assembly.GetExecutingAssembly().GetName().Version + " - by Lefteris \"Leftos\" Aslanoglou";
        }

        /// <summary>Checks for software updates asynchronously.</summary>
        /// <param name="showMessage">
        ///     if set to <c>true</c>, a message will be shown even if no update is found.
        /// </param>
        public static void CheckForUpdates(bool showMessage = false)
        {
            //showUpdateMessage = showMessage;
            try
            {
                var webClient = new WebClient();
                const string UpdateUri = "http://www.nba-live.com/leftos/rwversion.txt";
                if (!showMessage)
                {
                    webClient.DownloadFileCompleted += checkForUpdatesCompleted;
                    webClient.DownloadFileAsync(new Uri(UpdateUri), UpdateFileLocalPath);
                }
                else
                {
                    webClient.DownloadFile(new Uri(UpdateUri), UpdateFileLocalPath);
                    checkForUpdatesCompleted(null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to check for updates: {0}", ex.Message);
            }
        }

        /// <summary>Checks the downloaded version file to see if there's a newer version, and displays a message if needed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="AsyncCompletedEventArgs" /> instance containing the event data.
        /// </param>
        private static void checkForUpdatesCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string[] updateInfo;
            string[] versionParts;
            try
            {
                updateInfo = File.ReadAllLines(UpdateFileLocalPath);
                versionParts = updateInfo[0].Split('.');
            }
            catch
            {
                return;
            }
            var curVersionParts = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
            var iVP = new int[versionParts.Length];
            var iCVP = new int[versionParts.Length];
            for (var i = 0; i < versionParts.Length; i++)
            {
                iVP[i] = Convert.ToInt32(versionParts[i]);
                iCVP[i] = Convert.ToInt32(curVersionParts[i]);
                if (iCVP[i] > iVP[i])
                {
                    break;
                }
                if (iVP[i] > iCVP[i])
                {
                    var changelog = "\n\nVersion " + String.Join(".", versionParts);
                    try
                    {
                        for (var j = 2; j < updateInfo.Length; j++)
                        {
                            changelog += "\n" + updateInfo[j].Replace('\t', ' ');
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception thrown while trying to check for updates: {0}", ex.Message);
                    }
                    var mbr = MessageBox.Show(
                        "A new version is available! Would you like to download it?" + changelog,
                        App.AppName,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        Process.Start(updateInfo[1]);
                        break;
                    }
                    return;
                }
            }
        }

        private void prepareExpanded(IEnumerable<FooViewModel> list)
        {
            foreach (var item in list)
            {
                item.IsExpanded = item.Name == "All" || item.Name == "Players" || item.Name == "Teams";
                prepareExpanded(item.Children);
            }
        }

        private void btnOpenRosterBase_Click(object sender, RoutedEventArgs e)
        {
            var path = Tools.GetRegistrySetting("BaseRosterPath", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            var ofd = new OpenFileDialog
                {
                    Title = "Select the Players.csv exported from REditor",
                    Filter = "Players.csv|Players.csv",
                    InitialDirectory = path
                };
            ofd.ShowDialog();

            if (Path.GetFileName(ofd.FileName) != "Players.csv")
            {
                return;
            }

            txtRosterBase.Text = Path.GetDirectoryName(ofd.FileName);
            Tools.SetRegistrySetting("BaseRosterPath", Path.GetDirectoryName(ofd.FileName));
        }

        private void btnRTMAdd_Click(object sender, RoutedEventArgs e)
        {
            var path = Tools.GetRegistrySetting("MergeRosterPath", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            var ofd = new OpenFileDialog
                {
                    Title = "Select the Players.csv exported from REditor",
                    Filter = "Players.csv|Players.csv",
                    InitialDirectory = path
                };
            ofd.ShowDialog();

            if (Path.GetFileName(ofd.FileName) != "Players.csv")
            {
                return;
            }

            rostersToMerge.Add(Path.GetDirectoryName(ofd.FileName));
            Tools.SetRegistrySetting("MergeRosterPath", Path.GetDirectoryName(ofd.FileName));

            if (lstRostersToMerge.SelectedIndex == -1)
            {
                lstRostersToMerge.SelectedIndex = 0;
            }
        }

        private void btnRTMRemove_Click(object sender, RoutedEventArgs e)
        {
            var list = new string[lstRostersToMerge.SelectedItems.Count];
            lstRostersToMerge.SelectedItems.CopyTo(list, 0);
            foreach (var item in list)
            {
                rostersToMerge.Remove(item);
            }
        }

        private void txtRosterBase_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = (TextBox) sender;
            s.ScrollToHorizontalOffset(s.GetRectFromCharacterIndex(s.Text.Length).Right);
        }

        private void lstRostersToMerge_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstRostersToMerge.SelectedIndex == -1)
            {
                return;
            }

            if (e.RemovedItems.Count == 1)
            {
                var oldItem = e.RemovedItems[0].ToString();

                saveCurrentMergeSettings(oldItem);
            }
            var newItem = e.AddedItems[0].ToString();

            if (_mergeSettings.ContainsKey(newItem))
            {
                var dict = _mergeSettings[newItem];
                applyDict(dict, root);
            }
            else
            {
                root[0].IsChecked = false;
            }
        }

        private void saveCurrentMergeSettings(string dir)
        {
            if (!_mergeSettings.ContainsKey(dir))
            {
                _mergeSettings.Add(dir, new Dictionary<string, bool?>());
            }

            var dict = _mergeSettings[dir];
            populateDict(ref dict, root);
        }

        private void resetIsChecked(IEnumerable<FooViewModel> items)
        {
            foreach (var item in items)
            {
                item.IsChecked = false;
            }
        }

        private void applyDict(Dictionary<string, bool?> dict, IEnumerable<FooViewModel> items)
        {
            foreach (var item in items)
            {
                if (dict.ContainsKey(item.Name))
                {
                    item.IsChecked = dict[item.Name];
                }
                else
                {
                    applyDict(dict, item.Children);
                }
            }
        }

        private void populateDict(ref Dictionary<string, bool?> dict, IEnumerable<FooViewModel> items)
        {
            foreach (var item in items)
            {
                if (item.Children.Count == 0)
                {
                    dict[item.Name] = item.IsChecked;
                }
                else
                {
                    populateDict(ref dict, item.Children);
                }
            }
        }

        private Dictionary<string, bool?> getFullSettings()
        {
            var dict = new Dictionary<string, bool?>();
            foreach (var item in root)
            {
                if (item.Children.Count == 0)
                {
                    dict[item.Name] = true;
                }
                else
                {
                    getFullSettings(ref dict, item.Children);
                }
            }
            return dict;
        }

        private void getFullSettings(ref Dictionary<string, bool?> dict, List<FooViewModel> items)
        {
            foreach (var item in items)
            {
                if (item.Children.Count == 0)
                {
                    dict[item.Name] = true;
                }
                else
                {
                    getFullSettings(ref dict, item.Children);
                }
            }
        }

        // TODO: Add Jerseys handling
        // TODO: Single team/singe player
        // TODO: Replace instead of merge
        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (lstRostersToMerge.Items.Count == 0 || String.IsNullOrWhiteSpace(txtRosterBase.Text))
            {
                MessageBox.Show("You have to choose a base roster and at least one roster to merge with.");
                return;
            }
            saveCurrentMergeSettings(lstRostersToMerge.SelectedItem.ToString());

            var withError = false;
            foreach (var item in lstRostersToMerge.Items)
            {
                RepairTools.FixSorting(item.ToString());
            }
            RepairTools.FixSorting(txtRosterBase.Text);

            var doTeams = rbTeamsNone.IsChecked != true;

            #region Check Files and Initialize

            var teamsToMerge = new List<Dictionary<string, string>>();
            var teamsBase = new List<Dictionary<string, string>>();
            var freeTeamIDs = new List<string>();
            var staffToMerge = new List<Dictionary<string, string>>();
            var staffBase = new List<Dictionary<string, string>>();
            var freeStaffIDs = new List<int>();
            var jerseysToMerge = new List<Dictionary<string, string>>();
            var jerseysBase = new List<Dictionary<string, string>>();
            var freeJerseyIDs = new List<int>();
            var recordsToMerge = new List<Dictionary<string, string>>();
            var recordsBase = new List<Dictionary<string, string>>();
            var freeRecordsIDs = new List<int>();
            var teamStatsToMerge = new List<Dictionary<string, string>>();
            var teamStatsBase = new List<Dictionary<string, string>>();
            var freeTeamStatsIDs = new List<int>();
            var playerStatsToMerge = new List<Dictionary<string, string>>();
            var playerStatsBase = new List<Dictionary<string, string>>();
            var freePlayerStatsIDs = new List<int>();

            var baseDir = txtRosterBase.Text;
            var teamsBaseFile = baseDir + REDitorInfo.TeamsCSVName;

            var teamsToMergeDir = "";
            if (doTeams)
            {
                KeyValuePair<string, Dictionary<string, bool?>> settingsIncludingTeam;
                try
                {
                    settingsIncludingTeam = getSettingsIncludingTeam();
                }
                catch
                {
                    MessageBox.Show(
                        "You have chosen to merge teams, however you didn't select any (or selected more than one) file that you want "
                        + "to merge team information from. Please select one of the files in your Merge From list, then select the team "
                        + "information data you want to merge over (e.g. Rosters, Jerseys, etc.).",
                        App.AppName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                teamsToMergeDir = settingsIncludingTeam.Key;
                var teamsToMergeFile = teamsToMergeDir + REDitorInfo.TeamsCSVName;
                if (!File.Exists(teamsBaseFile) || !File.Exists(teamsToMergeFile))
                {
                    MessageBox.Show("In order to copy team rosters, you need to export the Teams tab as well.");
                    return;
                }

                teamsBase = CSV.DictionaryListFromCSVFile(teamsBaseFile);
                teamsBase = teamsBase.Where(REDitorInfo.IsValidTeam).ToList();
                teamsToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeFile);
                teamsToMerge = teamsToMerge.Where(REDitorInfo.IsValidTeam).ToList();

                if (rbTeamsAll.IsChecked == true && teamsToMerge.Count > teamsBase.Count)
                {
                    MessageBox.Show(
                        "There's not enough team entries in the base roster to fit the teams in the roster you've selected to merge.");
                    return;
                }

                if (rbTeamsCurrent.IsChecked == true)
                {
                    teamsToMerge = teamsToMerge.Where(dict => dict["ID"].ToInt32() < 30).ToList();
                }

                freeTeamIDs = teamsBase.Where(team => team["Name"].StartsWith("*")).Select(team => team["ID"]).ToList();

                staffBase = new List<Dictionary<string, string>>();
                if (!File.Exists(baseDir + REDitorInfo.StaffCSVName)
                    || _mergeSettings.Keys.Any(key => !File.Exists(key + REDitorInfo.StaffCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the Staff tab for one or more rosters, which means that any teams "
                            + "that are merged and have staff that's not in the base roster may have the wrong staff "
                            + "after the merge.\n\nAre you sure you want to continue?",
                            App.AppName,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    staffBase = CSV.DictionaryListFromCSVFile(baseDir + REDitorInfo.StaffCSVName);
                    var usedStaffIDs =
                        teamsBase.SelectMany(
                            team => team.Keys.Where(key => key.StartsWith("Staff_")).Select(key => team[key].ToInt32()).ToList())
                                 .ToList();
                    freeStaffIDs = staffBase.Select(st => st["ID"].ToInt32()).Where(id => !usedStaffIDs.Contains(id)).ToList();
                    staffToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + REDitorInfo.StaffCSVName);
                }

                jerseysBase = new List<Dictionary<string, string>>();
                if (!File.Exists(baseDir + REDitorInfo.JerseysCSVName)
                    || _mergeSettings.Keys.Any(key => !File.Exists(key + REDitorInfo.JerseysCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the Jerseys tab for one or more rosters, which means that any teams "
                            + "that are merged and have jerseys that aren't in the base roster may have the wrong jerseys "
                            + "after the merge.\n\nAre you sure you want to continue?",
                            App.AppName,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    jerseysBase = CSV.DictionaryListFromCSVFile(baseDir + REDitorInfo.JerseysCSVName);
                    var usedJerseysIDs =
                        jerseysBase.Where(je => !String.IsNullOrWhiteSpace(je["Texture"])).Select(je => je["ID"].ToInt32()).ToList();
                    freeJerseyIDs = jerseysBase.Select(st => st["ID"].ToInt32()).Where(id => !usedJerseysIDs.Contains(id)).ToList();
                    jerseysToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + REDitorInfo.JerseysCSVName);
                }

                recordsBase = new List<Dictionary<string, string>>();
                if (!File.Exists(baseDir + REDitorInfo.RecordsCSVName)
                    || _mergeSettings.Keys.Any(key => !File.Exists(key + REDitorInfo.RecordsCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the Records tab for one or more rosters, which means that any teams "
                            + "that are merged and have records that aren't in the base roster may have the wrong records "
                            + "after the merge.\n\nAre you sure you want to continue?",
                            App.AppName,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    recordsBase = CSV.DictionaryListFromCSVFile(baseDir + REDitorInfo.RecordsCSVName);
                    var usedRecordsIDs =
                        teamsBase.SelectMany(
                            team => team.Keys.Where(key => key.StartsWith("Record")).Select(key => team[key].ToInt32()).ToList())
                                 .ToList();
                    freeRecordsIDs = recordsBase.Select(st => st["ID"].ToInt32()).Where(id => !usedRecordsIDs.Contains(id)).ToList();
                    recordsToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + REDitorInfo.RecordsCSVName);
                }

                teamStatsBase = new List<Dictionary<string, string>>();
                if (!File.Exists(baseDir + REDitorInfo.TeamStatsCSVName)
                    || _mergeSettings.Keys.Any(key => !File.Exists(key + REDitorInfo.TeamStatsCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the TeamStats tab for one or more rosters, which means that any teams "
                            + "that are merged and have teamStats that aren't in the base roster may have the wrong teamStats "
                            + "after the merge.\n\nAre you sure you want to continue?",
                            App.AppName,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    teamStatsBase = CSV.DictionaryListFromCSVFile(baseDir + REDitorInfo.TeamStatsCSVName);
                    var usedTeamStatsIDs =
                        teamsBase.SelectMany(
                            team => team.Keys.Where(key => key.StartsWith("Stat")).Select(key => team[key].ToInt32()).ToList())
                                 .ToList();
                    freeTeamStatsIDs =
                        teamStatsBase.Select(st => st["ID"].ToInt32()).Where(id => !usedTeamStatsIDs.Contains(id)).ToList();
                    teamStatsToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + REDitorInfo.TeamStatsCSVName);
                }
            }
            else
            {
                if (File.Exists(teamsBaseFile))
                {
                    teamsBase = CSV.DictionaryListFromCSVFile(teamsBaseFile);
                }
            }

            var playersBase = CSV.DictionaryListFromCSVFile(baseDir + REDitorInfo.PlayersCSVName);

            var headshapesBase = new List<Dictionary<string, string>>();
            var freeHeadshapeIDs = new List<int>();
            if (_mergeSettings.Values.Any(dict => dict["Headshape"] == true))
            {
                if (!File.Exists(baseDir + REDitorInfo.HeadshapesCSVName)
                    || _mergeSettings.Keys.Any(dir => !File.Exists(dir + REDitorInfo.HeadshapesCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the Headshapes tab for one or more rosters, which means that any players "
                            + "that are merged and have a headshape that's not in the base roster may have the wrong headshape "
                            + "after the merge.\n\nAre you sure you want to continue?",
                            App.AppName,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    headshapesBase = CSV.DictionaryListFromCSVFile(baseDir + REDitorInfo.HeadshapesCSVName);
                    var usedHeadshapeIDs = playersBase.Select(dict => dict["HS_ID"].ToInt32()).ToList();
                    freeHeadshapeIDs =
                        headshapesBase.Select(hs => hs["ID"].ToInt32()).Where(id => !usedHeadshapeIDs.Contains(id)).ToList();
                }
            }

            var awardsBase = new List<Dictionary<string, string>>();
            var freeAwardIDs = new List<int>();
            if (_mergeSettings.Values.Any(dict => dict["Awards"] == true))
            {
                if (!File.Exists(baseDir + REDitorInfo.AwardsCSVName)
                    || _mergeSettings.Keys.Any(dir => !File.Exists(dir + REDitorInfo.AwardsCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the Awards tab for one or more rosters, which means that any players "
                            + "that are merged and have awards that's not in the base roster may have the wrong awards "
                            + "after the merge.\n\nAre you sure you want to continue?",
                            App.AppName,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    awardsBase = CSV.DictionaryListFromCSVFile(baseDir + REDitorInfo.AwardsCSVName);
                    freeAwardIDs =
                        awardsBase.Where(dict => String.IsNullOrWhiteSpace(dict["Team_Name"]))
                                  .Select(dict => dict["ID"].ToInt32())
                                  .ToList();
                }
            }

            playerStatsBase = new List<Dictionary<string, string>>();
            freePlayerStatsIDs = new List<int>();
            if (_mergeSettings.Values.Any(dict => dict["Player Stats"] == true))
            {
                if (!File.Exists(baseDir + REDitorInfo.PlayerStatsCSVName)
                    || _mergeSettings.Keys.Any(dir => !File.Exists(dir + REDitorInfo.PlayerStatsCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the Player Stats tab for one or more rosters, which means that any players "
                            + "that are merged and have player stats that's not in the base roster may have the wrong player stats "
                            + "after the merge.\n\nAre you sure you want to continue?",
                            App.AppName,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    playerStatsBase = CSV.DictionaryListFromCSVFile(baseDir + REDitorInfo.PlayerStatsCSVName);
                    var usedPlayerStatsIDs =
                        playersBase.SelectMany(
                            player => player.Keys.Where(key => key.StartsWith("Stat")).Select(key => player[key].ToInt32()).ToList())
                                   .ToList();
                    freePlayerStatsIDs =
                        playerStatsBase.Select(st => st["ID"].ToInt32()).Where(id => !usedPlayerStatsIDs.Contains(id)).ToList();
                }
            }

            #endregion Check Files and Initialize

            var validPlayersBase = playersBase.Where(REDitorInfo.IsValidPlayer).ToList();
            var shouldDoCurrentOnly = rbPlayersCurrent.IsChecked == true;
            if (shouldDoCurrentOnly)
            {
                validPlayersBase = validPlayersBase.Where(REDitorInfo.IsCurrentPlayer).ToList();
            }
            var shouldSkipFA = chkPlayersSkipFA.IsChecked == true;
            if (shouldSkipFA)
            {
                validPlayersBase = validPlayersBase.Where(player => player["IsFA"] == "1").ToList();
            }
            var shouldSkipHidden = chkPlayersSkipHidden.IsChecked == true;
            if (shouldSkipHidden)
            {
                validPlayersBase = validPlayersBase.Where(player => player["IsFA"] == "0" && player["TeamID1"] == "-1").ToList();
            }
            var invalidPlayersBase = playersBase.Where(player => !validPlayersBase.Contains(player)).ToList();
            var freePlayerIDs = invalidPlayersBase.Select(player => player["ID"].ToInt32()).ToList();

            NoConflictForMatchingTeamID = false;
            OnlyShowCurrentMatchesForCurrent = false;
            PreferUnhidden = false;

            foreach (var pair in _mergeSettings)
            {
                var dir = pair.Key;
                var dict = pair.Value;
                var allChecked = getAllChecked(dict);
                var curIsDraftClass = false;

                var playersCur = CSV.DictionaryListFromCSVFile(dir + REDitorInfo.PlayersCSVName);
                if (playersCur[0].ContainsKey("Age"))
                {
                    curIsDraftClass = true;
                }

                #region Populate lists for this file

                var teamsCur = new List<Dictionary<string, string>>();
                var curTeamsFile = dir + REDitorInfo.TeamsCSVName;
                if (File.Exists(curTeamsFile))
                {
                    teamsCur = CSV.DictionaryListFromCSVFile(curTeamsFile);
                }
                var headshapesCur = new List<Dictionary<string, string>>();
                var curHSFile = dir + REDitorInfo.HeadshapesCSVName;
                if (File.Exists(curHSFile))
                {
                    headshapesCur = CSV.DictionaryListFromCSVFile(curHSFile);
                }
                var awardsCur = new List<Dictionary<string, string>>();
                var curAwardsFile = dir + REDitorInfo.AwardsCSVName;
                if (File.Exists(curAwardsFile))
                {
                    awardsCur = CSV.DictionaryListFromCSVFile(curAwardsFile);
                }

                #endregion Populate lists for this file

                foreach (var player in validPlayersBase)
                {
                    var basePlayer = player;

                    #region Match Player

                    var matching =
                        playersCur.Where(
                            newPlayer =>
                            newPlayer["Last_Name"] == basePlayer["Last_Name"] && newPlayer["First_Name"] == basePlayer["First_Name"])
                                  .ToList();
                    if (OnlyShowCurrentMatchesForCurrent)
                    {
                        if (REDitorInfo.IsCurrentPlayer(basePlayer))
                        {
                            matching = matching.Where(REDitorInfo.IsCurrentPlayer).ToList();
                        }
                    }
                    if (NoConflictForMatchingTeamID)
                    {
                        if (matching.Count(newPlayer => newPlayer["TeamID1"] == basePlayer["TeamID1"]) == 1)
                        {
                            matching = new List<Dictionary<string, string>>
                                {
                                    matching.Single(newPlayer => newPlayer["TeamID1"] == basePlayer["TeamID1"])
                                };
                        }
                    }
                    if (PreferUnhidden)
                    {
                        var unhiddenPlayers = matching.Where(pl => pl["IsFA"] != "0" || pl["TeamID1"] != "-1").ToList();
                        if (unhiddenPlayers.Count == 1)
                        {
                            matching = new List<Dictionary<string, string>> { unhiddenPlayers[0] };
                        }
                    }

                    ConflictResult = 0;
                    if (matching.Count > 1)
                    {
                        var matchingNice = matching.Select(newPlayer => REDitorInfo.PresentPlayer(newPlayer, teamsCur)).ToList();

                        var cw = new ConflictWindow(
                            REDitorInfo.PresentPlayer(basePlayer, teamsBase), matchingNice, ConflictWindow.Mode.Players);
                        cw.ShowDialog();
                    }
                    else if (matching.Count == 0)
                    {
                        continue;
                    }

                    if (ConflictResult == -2)
                    {
                        continue;
                    }

                    #endregion Match Player

                    var pickedPlayer = playersCur.Single(newPlayer => newPlayer["ID"] == matching[ConflictResult]["ID"]);

                    #region Headshape

                    doHeadshape(
                        headshapesBase,
                        headshapesCur,
                        ref freeHeadshapeIDs,
                        playersBase,
                        basePlayer,
                        pickedPlayer,
                        allChecked,
                        ref withError);

                    #endregion Headshape

                    #region Awards

                    doAwards(awardsBase, awardsCur, ref freeAwardIDs, basePlayer, pickedPlayer, allChecked, ref withError);

                    #endregion Awards

                    #region Player Stats

                    if (
                        !doPlayerStats(
                            playerStatsBase, playerStatsToMerge, ref freePlayerStatsIDs, basePlayer, pickedPlayer, allChecked))
                    {
                        return;
                    }

                    #endregion Player Stats

                    #region Merge Player Entry

                    var notPlayerColumns = new List<string>
                        {
                            "Rosters",
                            "Headshape",
                            "Awards",
                            "Staff",
                            "Jerseys",
                            "Team Stats",
                            "Player Stats",
                            "Records"
                        };

                    var columnsToMerge = allChecked.Where(prop => !notPlayerColumns.Contains(prop)).ToList();

                    foreach (var col in columnsToMerge.Where(c => !playersBase.First().ContainsKey(c)))
                    {
                        if (!playersBase.First().ContainsKey(col))
                        {
                            App.ErrorReport(new Exception(col + " column wasn't found in the CSV."));
                            return;
                        }
                    }

                    if (allChecked.Contains("Headshape"))
                    {
                        columnsToMerge.Remove("HS_ID");
                    }
                    foreach (var property in columnsToMerge)
                    {
                        if (property.StartsWith("Stat"))
                        {
                            continue;
                        }

                        if (curIsDraftClass && property == "BirthYear")
                        {
                            basePlayer[property] = (2012 - pickedPlayer["Age"].ToInt32()).ToString();
                        }
                        else
                        {
                            basePlayer[property] = pickedPlayer[property];
                        }
                    }

                    #endregion Merge Player Entry
                }
            }

            #region Do Teams

            if (doTeams)
            {
                var teamIDReplacements = new Dictionary<string, string>();

                var rosterKeys = teamsBase[0].Keys.Where(key => key.StartsWith("Ros_")).ToList();
                var playerIDsInRosters = new List<int>();
                var playersToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + REDitorInfo.PlayersCSVName);
                var awardsToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + REDitorInfo.AwardsCSVName);
                var headshapesToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + REDitorInfo.HeadshapesCSVName);
                playerStatsToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + REDitorInfo.PlayerStatsCSVName);
                if (rbTeamsCustom.IsChecked == false)
                {
                    // The user either wants no teams to be replaced. Instead, matching teams will be merged, and missing teams will be
                    // copied over.
                    foreach (var team in teamsToMerge)
                    {
                        var newTeam = team;

                        #region Match Team

                        var matchingTeams =
                            teamsBase.Where(
                                possibleTeam => possibleTeam["Name"] == newTeam["Name"] && possibleTeam["City"] == newTeam["City"])
                                     .ToList();
                        matchingTeams = matchingTeams.Where(possibleTeam => possibleTeam["Year"] == newTeam["Year"]).ToList();

                        Dictionary<string, string> baseTeam;
                        ConflictResult = 0;
                        if (matchingTeams.Count >= 1)
                        {
                            if (matchingTeams.Count > 1)
                            {
                                var cw = new ConflictWindow(
                                    REDitorInfo.PresentTeam(newTeam),
                                    matchingTeams.Select(REDitorInfo.PresentTeam).ToList(),
                                    ConflictWindow.Mode.Teams);
                                cw.ShowDialog();

                                if (ConflictResult == -2)
                                {
                                    continue;
                                }
                            }

                            baseTeam = teamsBase.Single(te => te["ID"] == matchingTeams[ConflictResult]["ID"]);
                        }
                        else
                        {
                            if (freeTeamIDs.Count == 0)
                            {
                                MessageBox.Show(
                                    "No space available to copy missing team over without replacing one of the "
                                    + "pre-existing ones. Use the Teams > Custom feature to replace non-matching teams first.");
                                return;
                            }

                            var idToUse = freeTeamIDs.Pop();
                            baseTeam = teamsBase.Single(te => te["ID"] == idToUse);
                            foreach (var property in
                                baseTeam.Keys.Where(
                                    key =>
                                    key != "ID" && !rosterKeys.Contains(key) && !key.StartsWith("Sit_") && !key.StartsWith("Stat")
                                    && !key.StartsWith("Record") && key != "GID"))
                            {
                                baseTeam[property] = newTeam[property];
                            }
                        }

                        teamIDReplacements.Add(newTeam["ID"], baseTeam["ID"]);

                        #endregion Match Team

                        #region Rosters

                        if (_mergeSettings.Values.Any(dict => dict["Rosters"] == true))
                        {
                            if (
                                !doRoster(
                                    playersBase,
                                    validPlayersBase,
                                    playersToMerge,
                                    ref freePlayerIDs,
                                    teamsBase,
                                    teamsToMerge,
                                    baseTeam,
                                    newTeam,
                                    false,
                                    rosterKeys,
                                    awardsBase,
                                    awardsToMerge,
                                    ref freeAwardIDs,
                                    headshapesBase,
                                    headshapesToMerge,
                                    ref freeHeadshapeIDs,
                                    playerStatsBase,
                                    playerStatsToMerge,
                                    ref freePlayerStatsIDs,
                                    ref withError))
                            {
                                return;
                            }
                        }

                        #endregion Rosters

                        #region Staff

                        if (_mergeSettings.Values.Any(dict => dict["Staff"] == true))
                        {
                            doStaff(staffBase, staffToMerge, ref freeStaffIDs, teamsBase, baseTeam, newTeam, false);
                        }

                        #endregion Staff

                        #region Other

                        if (!doJerseys(jerseysBase, jerseysToMerge, ref freeJerseyIDs, teamsBase, teamsToMerge, baseTeam, newTeam))
                        {
                            return;
                        }

                        if (!doRecords(recordsBase, recordsToMerge, ref freeRecordsIDs, baseTeam, newTeam))
                        {
                            return;
                        }

                        if (!doTeamStats(teamStatsBase, teamStatsToMerge, ref freeTeamStatsIDs, baseTeam, newTeam))
                        {
                            return;
                        }

                        #endregion Other
                    }

                    var playersBaseNotInRoster =
                        playersBase.Where(pl => !playerIDsInRosters.Contains(pl["ID"].ToInt32()))
                                   .Where(REDitorInfo.IsValidPlayer)
                                   .ToList();

                    foreach (var player in playersBaseNotInRoster)
                    {
                        player["IsFA"] = "1";
                        player["TeamID1"] = "-1";
                        player["TeamID2"] = "-1";
                    }
                }
                else
                {
                    // The user has selected custom <baseTeam, newTeam> pairs, so instead of merging, we replace the
                    // baseTeam with the newTeam.
                    foreach (var pair in TeamPairs)
                    {
                        var baseTeamID = pair.Key.Split(new[] { "(ID: " }, StringSplitOptions.None)[1].Split(')')[0];
                        var newTeamID = pair.Value.Split(new[] { "(ID: " }, StringSplitOptions.None)[1].Split(')')[0];

                        var baseTeam = teamsBase.Single(team => team["ID"] == baseTeamID);
                        var newTeam = teamsToMerge.Single(team => team["ID"] == newTeamID);

                        foreach (var property in
                            baseTeam.Keys.Where(
                                key =>
                                key != "ID" && !rosterKeys.Contains(key) && !key.StartsWith("Sit_") && !key.StartsWith("Stat")
                                && !key.StartsWith("Record") && key != "GID"))
                        {
                            baseTeam[property] = newTeam[property];
                        }

                        teamIDReplacements.Add(newTeam["ID"], baseTeam["ID"]);

                        #region Rosters

                        if (_mergeSettings.Values.Any(dict => dict["Rosters"] == true))
                        {
                            if (
                                !doRoster(
                                    playersBase,
                                    validPlayersBase,
                                    playersToMerge,
                                    ref freePlayerIDs,
                                    teamsBase,
                                    teamsToMerge,
                                    baseTeam,
                                    newTeam,
                                    true,
                                    rosterKeys,
                                    awardsBase,
                                    awardsToMerge,
                                    ref freeAwardIDs,
                                    headshapesBase,
                                    headshapesToMerge,
                                    ref freeHeadshapeIDs,
                                    playerStatsBase,
                                    playerStatsToMerge,
                                    ref freePlayerStatsIDs,
                                    ref withError))
                            {
                                return;
                            }
                        }

                        #endregion Rosters

                        #region Staff

                        if (_mergeSettings.Values.Any(dict => dict["Staff"] == true))
                        {
                            doStaff(staffBase, staffToMerge, ref freeStaffIDs, teamsBase, baseTeam, newTeam, true);
                        }

                        #endregion Staff

                        #region Jerseys

                        if (!doJerseys(jerseysBase, jerseysToMerge, ref freeJerseyIDs, teamsBase, teamsToMerge, baseTeam, newTeam))
                        {
                            return;
                        }

                        if (!doRecords(recordsBase, recordsToMerge, ref freeRecordsIDs, baseTeam, newTeam))
                        {
                            return;
                        }

                        if (!doTeamStats(teamStatsBase, teamStatsToMerge, ref freeTeamStatsIDs, baseTeam, newTeam))
                        {
                            return;
                        }

                        #endregion Jerseys
                    }
                }

                doFreeAgents(playersBase, playersToMerge, ref freePlayerIDs, teamsBase, teamsToMerge);

                CSV.CSVFromDictionaryList(teamsBase, baseDir + REDitorInfo.TeamsCSVName);
                if (staffBase.Count > 0)
                {
                    CSV.CSVFromDictionaryList(staffBase, baseDir + REDitorInfo.StaffCSVName);
                }
                if (jerseysBase.Count > 0)
                {
                    // Fix Jerseys' unique JerseyIDs
                    for (var i = 0; i < jerseysBase.Count; i++)
                    {
                        var jersey = jerseysBase[i];
                        jersey["JerseyID"] = i.ToString();
                    }

                    CSV.CSVFromDictionaryList(jerseysBase, baseDir + REDitorInfo.JerseysCSVName);
                }
                if (recordsBase.Count > 0)
                {
                    CSV.CSVFromDictionaryList(recordsBase, baseDir + REDitorInfo.RecordsCSVName);
                }
                if (teamStatsBase.Count > 0)
                {
                    CSV.CSVFromDictionaryList(teamStatsBase, baseDir + REDitorInfo.TeamStatsCSVName);
                }
            }

            #endregion Do Teams

            CSV.CSVFromDictionaryList(playersBase, baseDir + REDitorInfo.PlayersCSVName);
            if (awardsBase.Count > 0)
            {
                CSV.CSVFromDictionaryList(awardsBase, baseDir + REDitorInfo.AwardsCSVName);
            }
            if (headshapesBase.Count > 0)
            {
                CSV.CSVFromDictionaryList(headshapesBase, baseDir + REDitorInfo.HeadshapesCSVName);
            }
            if (playerStatsBase.Count > 0)
            {
                CSV.CSVFromDictionaryList(playerStatsBase, baseDir + REDitorInfo.PlayerStatsCSVName);
            }

            if (!withError)
            {
                MessageBox.Show("Done!", App.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "Done but with errors. Open the tracelog.txt file located in My Documents\\Roster Workshop to find out more.",
                    App.AppName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private static void doFreeAgents(
            List<Dictionary<string, string>> playersBase,
            List<Dictionary<string, string>> playersToMerge,
            ref List<int> freePlayerIDs,
            List<Dictionary<string, string>> teamsBase,
            List<Dictionary<string, string>> teamsToMerge)
        {
            var faPlayersToMerge = playersToMerge.Where(REDitorInfo.IsValidPlayer).Where(REDitorInfo.IsFreeAgentPlayer).ToList();

            foreach (var newPlayer in faPlayersToMerge)
            {
                var playerToMerge = newPlayer;
                var matching =
                    playersBase.Where(
                        pl =>
                        pl["Last_Name"] == playerToMerge["Last_Name"] && pl["First_Name"] == playerToMerge["First_Name"]
                        && pl["IsFA"] == "1" && pl["TeamID1"] == "-1").ToList();

                ConflictResult = 0;

                if (matching.Count >= 1)
                {
                    if (matching.Count > 1)
                    {
                        var cw = new ConflictWindow(
                            REDitorInfo.PresentPlayer(playerToMerge, teamsToMerge),
                            matching.Select(pl => REDitorInfo.PresentPlayer(pl, teamsBase)).ToList(),
                            ConflictWindow.Mode.PlayersInDoTeams);
                        cw.ShowDialog();

                        if (ConflictResult == -2)
                        {
                            continue;
                        }
                    }

                    var playerToReplace = playersBase.Single(pl => pl["ID"] == matching[ConflictResult]["ID"]);
                    foreach (var key in playerToReplace.Keys.Where(name => name != "ID").ToList())
                    {
                        playerToReplace[key] = playerToMerge[key];
                    }
                }
                else
                {
                    if (freePlayerIDs.Count > 0)
                    {
                        var freePlayerID = freePlayerIDs.Pop();
                        var playerToReplace = playersBase.Single(pl => pl["ID"] == freePlayerID.ToString());
                        foreach (var key in playerToReplace.Keys.Where(name => name != "ID").ToList())
                        {
                            playerToReplace[key] = playerToMerge[key];
                        }
                        playerToReplace["TeamID1"] = "-1";
                        playerToReplace["TeamID2"] = "-1";
                    }
                    else
                    {
                        MessageBox.Show(
                            "Not enough space on the base roster to copy all missing Free Agents.\n"
                            + "The operation will continue, but some of the Free Agents from the source roster will not"
                            + "be in the destination roster.");
                        break;
                    }
                }
            }
        }

        private bool doRoster(
            List<Dictionary<string, string>> playersBase,
            List<Dictionary<string, string>> validPlayersBase,
            List<Dictionary<string, string>> playersToMerge,
            ref List<int> freePlayerIDs,
            List<Dictionary<string, string>> teamsBase,
            List<Dictionary<string, string>> teamsToMerge,
            Dictionary<string, string> baseTeam,
            Dictionary<string, string> newTeam,
            bool customTeams,
            List<string> rosterKeys,
            List<Dictionary<string, string>> awardsBase,
            List<Dictionary<string, string>> awardsToMerge,
            ref List<int> freeAwardIDs,
            List<Dictionary<string, string>> headshapesBase,
            List<Dictionary<string, string>> headshapesToMerge,
            ref List<int> freeHeadshapeIDs,
            List<Dictionary<string, string>> playerStatsBase,
            List<Dictionary<string, string>> playerStatsToMerge,
            ref List<int> freePlayerStatsIDs,
            ref bool withError)
        {
            foreach (var rosterSpot in rosterKeys)
            {
                if (newTeam[rosterSpot] == "-1")
                {
                    baseTeam[rosterSpot] = "-1";
                    continue;
                }

                var newPlayer = playersToMerge.Single(player => player["ID"] == newTeam[rosterSpot]);

                var matchingPlayers =
                    validPlayersBase.Where(
                        basePlayer =>
                        basePlayer["Last_Name"] == newPlayer["Last_Name"] && basePlayer["First_Name"] == newPlayer["First_Name"])
                                    .ToList();

                if (OnlyShowCurrentMatchesForCurrent)
                {
                    if (REDitorInfo.IsCurrentPlayer(newPlayer))
                    {
                        matchingPlayers = matchingPlayers.Where(REDitorInfo.IsCurrentPlayer).ToList();
                    }
                }
                if (NoConflictForMatchingTeamID)
                {
                    if (matchingPlayers.Count(basePlayer => basePlayer["TeamID1"] == newPlayer["TeamID1"]) == 1)
                    {
                        matchingPlayers = new List<Dictionary<string, string>>
                            {
                                matchingPlayers.Single(basePlayer => basePlayer["TeamID1"] == newPlayer["TeamID1"])
                            };
                    }
                }
                if (PreferUnhidden)
                {
                    var unhiddenPlayers = matchingPlayers.Where(pl => pl["IsFA"] != "0" || pl["TeamID1"] != "-1").ToList();
                    if (unhiddenPlayers.Count == 1)
                    {
                        matchingPlayers = new List<Dictionary<string, string>> { unhiddenPlayers[0] };
                    }
                }

                ConflictResult = 0;

                if (matchingPlayers.Count >= 1)
                {
                    if (matchingPlayers.Count > 1)
                    {
                        var cw = new ConflictWindow(
                            REDitorInfo.PresentPlayer(newPlayer, teamsToMerge),
                            matchingPlayers.Select(player => REDitorInfo.PresentPlayer(player, teamsBase)).ToList(),
                            ConflictWindow.Mode.PlayersInDoTeams);
                        cw.ShowDialog();

                        if (ConflictResult == -2)
                        {
                            continue;
                        }
                    }
                    var basePlayer = playersBase.Single(player => player["ID"] == matchingPlayers[ConflictResult]["ID"]);

                    basePlayer["TeamID1"] = baseTeam["ID"];
                    basePlayer["TeamID2"] = baseTeam["ID"];
                    basePlayer["IsFA"] = "0";
                    baseTeam[rosterSpot] = basePlayer["ID"];
                }
                else if (matchingPlayers.Count == 0)
                {
                    if (customTeams)
                    {
                        var rosterCountForBasePlayer =
                            teamsBase.Where(REDitorInfo.IsValidTeam)
                                     .Sum(team => rosterKeys.Count(key => team[key] == baseTeam[rosterSpot]));
                        if (rosterCountForBasePlayer == 1)
                        {
                            freePlayerIDs.Insert(0, baseTeam[rosterSpot].ToInt32());
                        }
                    }

                    if (freePlayerIDs.Count == 0)
                    {
                        MessageBox.Show(
                            "Not enough space on the base roster to copy a required player. Operation aborted.",
                            App.AppName,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return false;
                    }

                    var freeID = freePlayerIDs.Pop();

                    var playerToReplace = playersBase.Single(player => player["ID"] == freeID.ToString());

                    var fullSettings = getAllChecked(getFullSettings());
                    doAwards(awardsBase, awardsToMerge, ref freeAwardIDs, playerToReplace, newPlayer, fullSettings, ref withError);

                    doHeadshape(
                        headshapesBase,
                        headshapesToMerge,
                        ref freeHeadshapeIDs,
                        playersBase,
                        playerToReplace,
                        newPlayer,
                        fullSettings,
                        ref withError);

                    updatePlayerStatsTeamIDs(playerToReplace, newPlayer, playerStatsToMerge, newTeam, baseTeam);

                    doPlayerStats(
                        playerStatsBase, playerStatsToMerge, ref freePlayerStatsIDs, playerToReplace, newPlayer, fullSettings);

                    foreach (var property in playerToReplace.Keys.Where(key => key != "ID").ToList())
                    {
                        playerToReplace[property] = newPlayer[property];
                    }
                    baseTeam[rosterSpot] = freeID.ToString();
                }
                foreach (var sitProp in
                    newTeam.Keys.Where(key => key.StartsWith("Sit_")).Where(sitProp => newTeam[sitProp] == newPlayer["ID"]))
                {
                    baseTeam[sitProp] = baseTeam[rosterSpot];
                }
            }
            return true;
        }

        private static void doStaff(
            List<Dictionary<string, string>> staffBase,
            List<Dictionary<string, string>> staffToMerge,
            ref List<int> freeStaffIDs,
            List<Dictionary<string, string>> teamsBase,
            Dictionary<string, string> baseTeam,
            Dictionary<string, string> newTeam,
            bool customTeams)
        {
            var staffKeys = newTeam.Keys.Where(key => key.StartsWith("Staff_")).ToList();
            foreach (var staffSpot in staffKeys)
            {
                var staffIDToMerge = newTeam[staffSpot];
                var staffMemberToMerge = staffToMerge.Single(st => st["ID"] == staffIDToMerge);

                var matchingStaff =
                    staffBase.Where(
                        baseStaffMember =>
                        baseStaffMember["Last_Name"] == staffMemberToMerge["Last_Name"]
                        && baseStaffMember["First_Name"] == staffMemberToMerge["First_Name"]).ToList();
                var matchingStaffBySType =
                    matchingStaff.Where(baseStaffMember => baseStaffMember["SType"] == staffMemberToMerge["SType"]).ToList();
                var matchingStaffByExperience =
                    matchingStaffBySType.Where(baseStaffMember => baseStaffMember["Experience"] == staffMemberToMerge["Experience"])
                                        .ToList();

                ConflictResult = 0;

                if (matchingStaff.Count >= 1)
                {
                    var finalMatchingStaff = matchingStaffByExperience.Count == 0
                                                 ? (matchingStaffBySType.Count == 0 ? matchingStaff : matchingStaffBySType)
                                                 : matchingStaffByExperience;
                    if (finalMatchingStaff.Count > 1)
                    {
                        var cw = new ConflictWindow(
                            REDitorInfo.PresentStaff(staffMemberToMerge),
                            finalMatchingStaff.Select(REDitorInfo.PresentStaff).ToList(),
                            ConflictWindow.Mode.Staff);
                        cw.ShowDialog();

                        if (ConflictResult == -2)
                        {
                            continue;
                        }
                    }
                    var staffMemberToReplace = staffBase.Single(st => st["ID"] == finalMatchingStaff[ConflictResult]["ID"]);
                    foreach (var property in staffMemberToReplace.Keys.Where(name => name != "ID").ToList())
                    {
                        staffMemberToReplace[property] = staffMemberToMerge[property];
                    }
                }
                else if (matchingStaff.Count == 0)
                {
                    if (customTeams)
                    {
                        var rosterCountForBaseStaffMember =
                            teamsBase.Sum(team => staffKeys.Count(key => team[key] == baseTeam[staffSpot]));
                        if (rosterCountForBaseStaffMember == 1)
                        {
                            freeStaffIDs.Insert(0, baseTeam[staffSpot].ToInt32());
                        }
                    }

                    if (freeStaffIDs.Count > 0)
                    {
                        var newStaffID = freeStaffIDs.Pop();

                        var staffMemberToReplace = staffBase.Single(st => st["ID"] == newStaffID.ToString());
                        foreach (var property in staffMemberToReplace.Keys.Where(name => name != "ID").ToList())
                        {
                            staffMemberToReplace[property] = staffMemberToMerge[property];
                        }
                        baseTeam[staffSpot] = newStaffID.ToString();
                    }
                }
            }
        }

        private static void updatePlayerStatsTeamIDs(
            Dictionary<string, string> playerToReplace,
            Dictionary<string, string> newPlayer,
            List<Dictionary<string, string>> playerStatsToMerge,
            Dictionary<string, string> newTeam,
            Dictionary<string, string> baseTeam)
        {
            foreach (var playerStatsKey in playerToReplace.Keys.Where(key => key.StartsWith("Stat")))
            {
                var playerStatsID = newPlayer[playerStatsKey];
                if (playerStatsID == "-1")
                {
                    continue;
                }
                var playerStatsEntry = playerStatsToMerge.Single(entry => entry["ID"] == playerStatsID);
                if (playerStatsEntry["TeamID1"] == newTeam["ID"])
                {
                    playerStatsEntry["TeamID1"] = baseTeam["ID"];
                }
                if (playerStatsEntry["TeamID2"] == newTeam["ID"])
                {
                    playerStatsEntry["TeamID"] = baseTeam["ID"];
                }
            }
        }

        private static bool doPlayerStats(
            List<Dictionary<string, string>> playerStatsBase,
            List<Dictionary<string, string>> playerStatsToMerge,
            ref List<int> freePlayerStatsIDs,
            Dictionary<string, string> basePlayer,
            Dictionary<string, string> pickedPlayer,
            List<string> allChecked)
        {
            if (playerStatsBase.Count > 0 && allChecked.Contains("Player Stats"))
            {
                foreach (var statKey in allChecked.Where(key => key.StartsWith("Stat")))
                {
                    if (pickedPlayer[statKey] == "-1")
                    {
                        basePlayer[statKey] = "-1";
                        continue;
                    }

                    var newStatEntry = playerStatsToMerge.Single(entry => entry["ID"] == pickedPlayer[statKey]);

                    Dictionary<string, string> baseStatEntry;
                    if (basePlayer[statKey] != "-1")
                    {
                        baseStatEntry = playerStatsBase.Single(entry => entry["ID"] == basePlayer[statKey]);
                    }
                    else
                    {
                        if (freePlayerStatsIDs.Count == 0)
                        {
                            MessageBox.Show("Not enough free space to merge player stats.");
                            return false;
                        }

                        var freeID = freePlayerStatsIDs.Pop();
                        baseStatEntry = playerStatsBase.Single(entry => entry["ID"] == freeID.ToString());
                    }

                    foreach (var key in playerStatsBase[0].Keys.Where(key => key != "ID"))
                    {
                        baseStatEntry[key] = newStatEntry[key];
                    }

                    // TODO: Do TeamF and TeamS work?
                }
            }
            return true;
        }

        private void doAwards(
            List<Dictionary<string, string>> awardsBase,
            List<Dictionary<string, string>> awardsCur,
            ref List<int> freeAwardIDs,
            Dictionary<string, string> basePlayer,
            Dictionary<string, string> pickedPlayer,
            List<string> allChecked,
            ref bool withError)
        {
            if (awardsBase.Count > 0 && awardsCur.Count > 0 && awardsCur.Any(award => award["Pl_ASA_ID"] == pickedPlayer["ASA_ID"])
                && allChecked.Contains("Awards"))
            {
                var pickedAwards = awardsCur.Where(award => award["Pl_ASA_ID"] == pickedPlayer["ASA_ID"]).ToList();
                var basePlayerAwards = awardsBase.Where(award => award["Pl_ASA_ID"] == basePlayer["ASA_ID"]).ToList();
                for (var i = 0; i < basePlayerAwards.Count; i++)
                {
                    var baseAward = basePlayerAwards[i];
                    eraseAward(ref baseAward);
                    freeAwardIDs.Add(baseAward["ID"].ToInt32());
                }

                foreach (var pickedAward in pickedAwards)
                {
                    var baseIDToReplace = -1;
                    if (freeAwardIDs.Count > 0)
                    {
                        baseIDToReplace = freeAwardIDs.Pop();
                    }
                    if (baseIDToReplace == -1)
                    {
                        withError = true;
                        Tools.WriteToTrace("Ran out of space while writing awards.");
                        break;
                    }

                    foreach (var property in awardsBase[0].Keys.Where(key => key != "ID").ToList())
                    {
                        if (property == "Pl_ASA_ID" && !allChecked.Contains("ASA_ID"))
                        {
                            continue;
                        }
                        awardsBase[baseIDToReplace][property] = pickedAward[property];
                    }
                }
            }
            else if (allChecked.Contains("ASA_ID") && !allChecked.Contains("Awards"))
            {
                var awardsToEdit = awardsBase.Where(aw => aw["Pl_ASA_ID"] == basePlayer["ASA_ID"]);
                foreach (var award in awardsToEdit)
                {
                    award["Pl_ASA_ID"] = pickedPlayer["ASA_ID"];
                }
            }
        }

        private static void doHeadshape(
            List<Dictionary<string, string>> headshapesBase,
            List<Dictionary<string, string>> headshapesCur,
            ref List<int> freeHeadshapeIDs,
            List<Dictionary<string, string>> playersBase,
            Dictionary<string, string> basePlayer,
            Dictionary<string, string> pickedPlayer,
            List<string> allChecked,
            ref bool withError)
        {
            if (headshapesBase.Count > 0 && headshapesCur.Count > 0 && allChecked.Contains("Headshape"))
            {
                Dictionary<string, string> baseHS = null;
                if (allChecked.Contains("HS_ID"))
                {
                    // User wants the new headshape ID, but is it available?
                    if (freeHeadshapeIDs.Contains(pickedPlayer["HS_ID"].ToInt32())
                        || (basePlayer["HS_ID"] == pickedPlayer["HS_ID"]
                            && playersBase.Count(pl => pl["HS_ID"] == basePlayer["HS_ID"]) == 1))
                    {
                        baseHS = headshapesBase.Single(hs => hs["ID"] == pickedPlayer["HS_ID"]);
                        freeHeadshapeIDs.Remove(pickedPlayer["HS_ID"].ToInt32());
                    }
                    else
                    {
                        // New headshape ID isn't free, so let's get the first one that is.
                        if (freeHeadshapeIDs.Count > 0)
                        {
                            var newHeadshapeID = freeHeadshapeIDs.Pop();
                            baseHS = headshapesBase.Single(hs => hs["ID"] == newHeadshapeID.ToString());
                            //pickedPlayer["HS_ID"] = newHeadshapeID.ToString();

                            // The base player will have a new HS_ID, so let's see if his old one should become available.
                            if (playersBase.Count(pl => pl["HS_ID"] == basePlayer["HS_ID"]) == 1)
                            {
                                // Only the base player was using it, so it's now available.
                                freeHeadshapeIDs.Add(basePlayer["HS_ID"].ToInt32());
                            }
                        }
                        else
                        {
                            // Fallback to using replaced player's Headshape ID, if he's the only one using it
                            if (playersBase.Count(pl => pl["HS_ID"] == basePlayer["HS_ID"]) == 1)
                            {
                                baseHS = headshapesBase.Single(hs => hs["ID"] == basePlayer["HS_ID"]);
                                //pickedPlayer["HS_ID"] = basePlayer["HS_ID"];
                            }
                            else
                            {
                                // No headshape ID was available.
                                withError = true;
                                Tools.WriteToTrace("Ran out of space while trying to transfer headshape.");
                            }
                        }
                    }
                }
                else
                {
                    // User doesn't care to transfer the HS_ID, so we should use the base player's one, if it's available
                    if (playersBase.Count(pl => pl["HS_ID"] == basePlayer["HS_ID"]) == 1)
                    {
                        baseHS = headshapesBase.Single(hs => hs["ID"] == basePlayer["HS_ID"]);
                    }
                    else
                    {
                        // Base player's HS_ID is used by other players, so we can't replace it.
                        // Let's try to get a free one.
                        if (freeHeadshapeIDs.Count > 0)
                        {
                            var newHeadshapeID = freeHeadshapeIDs.Pop();
                            baseHS = headshapesBase.Single(hs => hs["ID"] == newHeadshapeID.ToString());

                            // The base player will have a new HS_ID, so let's see if his old one should become available.
                            if (playersBase.Count(pl => pl["HS_ID"] == basePlayer["HS_ID"]) == 1)
                            {
                                // Only the base player was using it, so it's now available.
                                freeHeadshapeIDs.Add(basePlayer["HS_ID"].ToInt32());
                            }

                            // Replace the base player's HS_ID with the new one
                            basePlayer["HS_ID"] = newHeadshapeID.ToString();
                        }
                        else
                        {
                            // No headshape ID was available.
                            withError = true;
                            Tools.WriteToTrace("Ran out of space while trying to transfer headshape.");
                        }
                    }
                }
                // If we found a headshape that we can replace, we do so.
                if (baseHS != null)
                {
                    var pickedHS = headshapesCur.Single(hs => hs["ID"] == pickedPlayer["HS_ID"]);

                    foreach (var property in pickedHS.Keys.Where(key => key != "ID").ToList())
                    {
                        baseHS[property] = pickedHS[property];
                    }
                    basePlayer["HS_ID"] = baseHS["ID"];
                }
            }
        }

        private bool doTeamStats(
            List<Dictionary<string, string>> teamStatsBase,
            List<Dictionary<string, string>> teamStatsToMerge,
            ref List<int> freeTeamStatsIDs,
            Dictionary<string, string> baseTeam,
            Dictionary<string, string> newTeam)
        {
            if (_mergeSettings.Values.Any(dict => dict["Team Stats"] == true))
            {
                var teamStatKeys = baseTeam.Keys.Where(key => key.StartsWith("Stat")).ToList();
                foreach (var teamStatKey in teamStatKeys)
                {
                    if (baseTeam[teamStatKey] == "-1")
                    {
                        continue;
                    }
                    freeTeamStatsIDs.Add(baseTeam[teamStatKey].ToInt32());
                    baseTeam[teamStatKey] = "-1";
                }

                foreach (var teamStatKey in teamStatKeys)
                {
                    if (newTeam[teamStatKey] == "-1")
                    {
                        baseTeam[teamStatKey] = "-1";
                        continue;
                    }

                    if (freeTeamStatsIDs.Count > 0)
                    {
                        var freeID = freeTeamStatsIDs.Pop();
                        var teamStatToReplace = teamStatsBase.Single(entry => entry["ID"] == freeID.ToString());
                        var teamStatToMerge = teamStatsToMerge.Single(entry => entry["ID"] == newTeam[teamStatKey]);
                        foreach (var key in teamStatToMerge.Keys.Where(key => key != "ID"))
                        {
                            teamStatToReplace[key] = teamStatToMerge[key];
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "Not enough space on the base roster to copy a required teamStat. Operation aborted.",
                            App.AppName,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return false;
                    }
                }
            }

            return true;
        }

        private bool doRecords(
            List<Dictionary<string, string>> recordsBase,
            List<Dictionary<string, string>> recordsToMerge,
            ref List<int> freeRecordsIDs,
            Dictionary<string, string> baseTeam,
            Dictionary<string, string> newTeam)
        {
            if (_mergeSettings.Values.Any(dict => dict["Records"] == true))
            {
                var recordKeys = baseTeam.Keys.Where(key => key.StartsWith("Record")).ToList();
                foreach (var recordKey in recordKeys)
                {
                    if (baseTeam[recordKey] == "-1")
                    {
                        continue;
                    }
                    freeRecordsIDs.Add(baseTeam[recordKey].ToInt32());
                    baseTeam[recordKey] = "-1";
                }

                foreach (var recordKey in recordKeys)
                {
                    if (newTeam[recordKey] == "-1")
                    {
                        baseTeam[recordKey] = "-1";
                        continue;
                    }

                    if (freeRecordsIDs.Count > 0)
                    {
                        var freeID = freeRecordsIDs.Pop();
                        var recordToReplace = recordsBase.Single(entry => entry["ID"] == freeID.ToString());
                        var recordToMerge = recordsToMerge.Single(entry => entry["ID"] == newTeam[recordKey]);
                        foreach (var key in recordToMerge.Keys.Where(key => key != "ID"))
                        {
                            recordToReplace[key] = recordToMerge[key];
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "Not enough space on the base roster to copy a required record. Operation aborted.",
                            App.AppName,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return false;
                    }
                }
            }

            return true;
        }

        private bool doJerseys(
            List<Dictionary<string, string>> jerseysBase,
            List<Dictionary<string, string>> jerseysToMerge,
            ref List<int> freeJerseyIDs,
            List<Dictionary<string, string>> teamsBase,
            List<Dictionary<string, string>> teamsToMerge,
            Dictionary<string, string> baseTeam,
            Dictionary<string, string> newTeam)
        {
            if (_mergeSettings.Values.Any(dict => dict["Jerseys"] == true))
            {
                var dummyGID =
                    (Math.Max(teamsToMerge.Max(te => te["GID"].ToInt32()), teamsBase.Max(te => te["GID"].ToInt32())) + 1).ToString();
                var baseTeamJerseys = jerseysBase.Where(je => je["GID"] == baseTeam["GID"]);
                var newTeamJerseys = jerseysToMerge.Where(je => je["GID"] == newTeam["GID"]);
                foreach (var jersey in baseTeamJerseys)
                {
                    jersey["Texture"] = "";
                    jersey["GID"] = dummyGID;
                    freeJerseyIDs.Add(jersey["ID"].ToInt32());
                }
                foreach (var jersey in newTeamJerseys)
                {
                    var jerseyToMerge = jersey;
                    if (freeJerseyIDs.Count > 0)
                    {
                        var newJerseyID = freeJerseyIDs.Pop();
                        var jerseyToReplace = jerseysBase.Single(je => je["ID"] == newJerseyID.ToString());
                        foreach (var key in jersey.Keys.Where(name => name != "ID").ToList())
                        {
                            jerseyToReplace[key] = jerseyToMerge[key];
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "Not enough space on the base roster to copy a required jersey. Operation aborted.",
                            App.AppName,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return false;
                    }
                }
            }

            return true;
        }

        private void eraseAward(ref Dictionary<string, string> award)
        {
            award["Team_Name"] = "";
            award["Team_City"] = "";
            var keys = new List<string> { "Year", "AType", "TeamGID", "OpTeamGID", "Pl_ASA_ID", "Value", "Value2" };
            foreach (var key in keys)
            {
                award[key] = "0";
            }
        }

        private List<string> getAllChecked(Dictionary<string, bool?> dict)
        {
            return dict.Where(pair => pair.Value == true).Select(pair => pair.Key).ToList();
        }

        private void mnuRepairFixTeamIDs_Click(object sender, RoutedEventArgs e)
        {
            if (noRoster())
            {
                return;
            }

            RepairTools.FixTeamIDs(txtRosterBase.Text);

            MessageBox.Show("TeamIDs fixed!");
        }

        private bool noRoster()
        {
            return String.IsNullOrWhiteSpace(txtRosterBase.Text);
        }

        private void btnRepair_Click(object sender, RoutedEventArgs e)
        {
            mnuRepair.PlacementTarget = btnRepair;
            mnuRepair.IsOpen = true;
        }

        private void mnuRepairFixSorting_Click(object sender, RoutedEventArgs e)
        {
            if (noRoster())
            {
                return;
            }

            RepairTools.FixSorting(txtRosterBase.Text);

            MessageBox.Show("Sorting fixed!");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Tools.SetRegistrySetting("Height", Height);
            Tools.SetRegistrySetting("Width", Width);
            Tools.SetRegistrySetting("Left", Left);
            Tools.SetRegistrySetting("Top", Top);
        }

        private void mnuRepairFixASAIDs_Click(object sender, RoutedEventArgs e)
        {
            if (noRoster())
            {
                return;
            }

            RepairTools.FixASAIDs(txtRosterBase.Text);

            MessageBox.Show("ASA_IDs fixed!");
        }

        private void mnuRepairFixPlNum_Click(object sender, RoutedEventArgs e)
        {
            if (noRoster())
            {
                return;
            }

            RepairTools.FixPlayerNumberAndOrder(txtRosterBase.Text);

            MessageBox.Show("PlNum & player order in team rosters fixed!");
        }

        private void mnuRepairFixContracts_Click(object sender, RoutedEventArgs e)
        {
            if (noRoster())
            {
                return;
            }

            RepairTools.FixASAIDs(txtRosterBase.Text);

            MessageBox.Show("Contract lengths for all players and contract information for free agents fixed!");
        }

        private void rbTeamsCustom_Click(object sender, RoutedEventArgs e)
        {
            if (lstRostersToMerge.Items.Count == 0 || String.IsNullOrWhiteSpace(txtRosterBase.Text))
            {
                MessageBox.Show("You have to choose a base roster and at least one roster to merge with.");
                rbTeamsNone.IsChecked = true;
                return;
            }
            saveCurrentMergeSettings(lstRostersToMerge.SelectedItem.ToString());

            KeyValuePair<string, Dictionary<string, bool?>> settings;
            try
            {
                settings = getSettingsIncludingTeam();
            }
            catch
            {
                rbTeamsNone.IsChecked = true;
                MessageBox.Show("You haven't selected any roster to copy team information from.");
                return;
            }
            var mergeDir = settings.Key;
            var baseTeamsFile = txtRosterBase.Text + REDitorInfo.TeamsCSVName;
            var mergeTeamsFile = mergeDir + REDitorInfo.TeamsCSVName;
            if (!File.Exists(baseTeamsFile) || !File.Exists(mergeTeamsFile))
            {
                MessageBox.Show(
                    "You need the Teams tab exported both from the base roster and from the roster you've picked to merge team "
                    + "information from.");
                rbTeamsNone.IsChecked = true;
                return;
            }

            var teamsBase = CSV.DictionaryListFromCSVFile(baseTeamsFile);
            var teamsToMerge = CSV.DictionaryListFromCSVFile(mergeTeamsFile);
            var ptpw = new PickTeamPairsWindow(
                teamsBase.Where(REDitorInfo.IsValidTeam).ToList(), teamsToMerge.Where(REDitorInfo.IsValidTeam).ToList());
            ptpw.ShowDialog();
        }

        private KeyValuePair<string, Dictionary<string, bool?>> getSettingsIncludingTeam()
        {
            var teamOptions = new List<string> { "Rosters", "Staff", "Jerseys" };
            return _mergeSettings.Single(pair => teamOptions.Any(option => pair.Value[option] == true));
        }
    }
}