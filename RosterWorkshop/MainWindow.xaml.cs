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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LeftosCommonLibrary;
using Microsoft.Win32;
using TreeViewWithCheckBoxesLib;

#endregion

namespace RosterWorkshop
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> rostersToMerge { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var columnsTree = new List<FooViewModel>();
            var lines = Tools.SplitLinesToList(Properties.Resources.REDColumns);
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
                    category.Children.Add(new FooViewModel(line.Replace("\t", "")));
                }
                i--;
            }
            var rootItem = new FooViewModel("All");
            rootItem.Children.AddRange(columnsTree);
            rootItem.Initialize();
            root = new List<FooViewModel> {rootItem};

            trvColumns.ItemsSource = root;

            foreach (FooViewModel item in trvColumns.Items)
            {
                item.IsExpanded = item.Name == "All";
            }

            rbPlayersCurrent.IsChecked = true;
            rbTeamsNone.IsChecked = true;

            rostersToMerge = new ObservableCollection<string>();
            lstRostersToMerge.ItemsSource = rostersToMerge;
        }

        protected List<FooViewModel> root { get; set; }

        private void btnOpenRosterBase_Click(object sender, RoutedEventArgs e)
        {
            string path = Helper.GetRegistrySetting("BaseRosterPath", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
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
            Helper.SetRegistrySetting("BaseRosterPath", Path.GetDirectoryName(ofd.FileName));
        }

        private void btnRTMAdd_Click(object sender, RoutedEventArgs e)
        {
            string path = Helper.GetRegistrySetting("MergeRosterPath", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
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
            Helper.SetRegistrySetting("MergeRosterPath", Path.GetDirectoryName(ofd.FileName));

            if (lstRostersToMerge.SelectedIndex == -1)
            {
                lstRostersToMerge.SelectedIndex = 0;
            }
        }

        private void btnRTMRemove_Click(object sender, RoutedEventArgs e)
        {
            var list = new string[lstRostersToMerge.SelectedItems.Count];
            lstRostersToMerge.SelectedItems.CopyTo(list, 0);
            foreach (string item in list)
            {
                rostersToMerge.Remove(item);
            }
        }

        private void txtRosterBase_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = (TextBox) sender;
            s.ScrollToHorizontalOffset(s.GetRectFromCharacterIndex(s.Text.Length).Right);
        }

        private readonly Dictionary<string, Dictionary<string, bool?>> _mergeSettings = new Dictionary<string, Dictionary<string, bool?>>();
        public static bool OnlyShowCurrentMatchesForCurrent;
        public static bool NoConflictForMatchingTeamID;
        public static int ConflictResult;

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
            foreach (FooViewModel item in items)
            {
                item.IsChecked = false;
            }
        }

        private void applyDict(Dictionary<string, bool?> dict, IEnumerable<FooViewModel> items)
        {
            foreach (FooViewModel item in items)
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
            foreach (FooViewModel item in items)
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

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (lstRostersToMerge.Items.Count == 0 || String.IsNullOrWhiteSpace(txtRosterBase.Text))
            {
                MessageBox.Show("You have to choose a base roster and at least one roster to merge with.");
                return;
            }
            saveCurrentMergeSettings(lstRostersToMerge.SelectedItem.ToString());

            bool withError = false;
            foreach (var item in lstRostersToMerge.Items)
            {
                RepairTools.FixSorting(item.ToString());
            }
            RepairTools.FixSorting(txtRosterBase.Text);

            var doTeams = rbTeamsNone.IsChecked != true;

            var teamsToMerge = new List<Dictionary<string, string>>();
            var teamsBase = new List<Dictionary<string, string>>();
            var staffToMerge = new List<Dictionary<string, string>>();
            var staffBase = new List<Dictionary<string, string>>();
            var freeStaffIDs = new List<int>();

            var baseDir = txtRosterBase.Text;
            const string teamsCSVName = @"\Teams.csv";
            var teamsBaseFile = baseDir + teamsCSVName;
            const string staffCSVName = @"\Staff.csv";

            string teamsToMergeDir = "";
            if (doTeams)
            {
                if (doTeams && _mergeSettings.Values.All(dict => dict["Team Rosters"] == false))
                {
                    MessageBox.Show("You haven't selected which roster to copy the Team Rosters from.");
                    return;
                }

                teamsToMergeDir = _mergeSettings.Single(pair => pair.Value["Team Rosters"] == true).Key;
                var teamsToMergeFile = teamsToMergeDir + teamsCSVName;
                if (!File.Exists(teamsBaseFile) || !File.Exists(teamsToMergeFile))
                {
                    MessageBox.Show("In order to copy team rosters, you need to export the Teams tab as well.");
                    return;
                }

                teamsBase = CSV.DictionaryListFromCSVFile(teamsBaseFile);
                teamsToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeFile);

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

                staffBase = new List<Dictionary<string, string>>();
                if (!File.Exists(baseDir + staffCSVName) || _mergeSettings.Keys.Any(key => !File.Exists(key + staffCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the Staff tab for one or more rosters, which means that any teams " +
                            "that are merged and have staff that's not in the base roster may have the wrong staff " +
                            "after the merge.\n\nAre you sure you want to continue?", App.AppName, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    staffBase = CSV.DictionaryListFromCSVFile(baseDir + staffCSVName);
                    var usedStaffIDs =
                        teamsBase.SelectMany(
                            team => team.Keys.Where(key => key.StartsWith("Staff_")).Select(key => team[key].ToInt32()).ToList()).ToList();
                    freeStaffIDs = staffBase.Select(st => st["ID"].ToInt32()).Where(id => !usedStaffIDs.Contains(id)).ToList();
                    staffToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + staffCSVName);
                }
            }
            else
            {
                if (File.Exists(teamsBaseFile))
                {
                    teamsBase = CSV.DictionaryListFromCSVFile(teamsBaseFile);
                }
            }

            const string playersCSVName = @"\Players.csv";
            var playersBase = CSV.DictionaryListFromCSVFile(baseDir + playersCSVName);

            var headshapesBase = new List<Dictionary<string, string>>();
            var freeHeadshapeIDs = new List<int>();
            const string headshapesCSVName = @"\Headshapes.csv";
            if (_mergeSettings.Values.Any(dict => dict["Headshape"] == true))
            {
                if (!File.Exists(baseDir + headshapesCSVName) || _mergeSettings.Keys.Any(dir => !File.Exists(dir + headshapesCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the Headshapes tab for one or more rosters, which means that any players " +
                            "that are merged and have a headshape that's not in the base roster may have the wrong headshape " +
                            "after the merge.\n\nAre you sure you want to continue?", App.AppName, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    headshapesBase = CSV.DictionaryListFromCSVFile(baseDir + headshapesCSVName);
                    var usedHeadshapeIDs = playersBase.Select(dict => dict["HS_ID"].ToInt32()).ToList();
                    freeHeadshapeIDs = headshapesBase.Select(hs => hs["ID"].ToInt32()).Where(id => !usedHeadshapeIDs.Contains(id)).ToList();
                }
            }

            var awardsBase = new List<Dictionary<string, string>>();
            var freeAwardIDs = new List<int>();
            const string awardsCSVName = @"\Awards.csv";
            if (_mergeSettings.Values.Any(dict => dict["Awards"] == true))
            {
                if (!File.Exists(baseDir + awardsCSVName) || _mergeSettings.Keys.Any(dir => !File.Exists(dir + awardsCSVName)))
                {
                    if (
                        MessageBox.Show(
                            "You haven't exported the Awards tab for one or more rosters, which means that any players " +
                            "that are merged and have awards that's not in the base roster may have the wrong awards " +
                            "after the merge.\n\nAre you sure you want to continue?", App.AppName, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    awardsBase = CSV.DictionaryListFromCSVFile(baseDir + awardsCSVName);
                    freeAwardIDs =
                        awardsBase.Where(dict => String.IsNullOrWhiteSpace(dict["Team_Name"])).Select(dict => dict["ID"].ToInt32()).ToList();
                }
            }

            var validPlayersBase = playersBase.Where(isValidPlayer).ToList();
            var shouldDoCurrentOnly = rbPlayersCurrent.IsChecked == true;
            if (shouldDoCurrentOnly)
            {
                validPlayersBase = validPlayersBase.Where(isCurrentPlayer).ToList();
            }
            var shouldSkipFA = chkPlayersSkipFA.IsChecked == true;
            if (shouldSkipFA)
            {
                validPlayersBase = validPlayersBase.Where(player => player["IsFA"] == "1").ToList();
            }
            var invalidPlayersBase = playersBase.Where(player => !validPlayersBase.Contains(player)).ToList();
            var freePlayerIDs = invalidPlayersBase.Select(player => player["ID"].ToInt32()).ToList();

            NoConflictForMatchingTeamID = false;
            OnlyShowCurrentMatchesForCurrent = false;

            foreach (var pair in _mergeSettings)
            {
                var dir = pair.Key;
                var dict = pair.Value;
                var allChecked = getAllChecked(dict);
                bool curIsDraftClass = false;

                var playersCur = CSV.DictionaryListFromCSVFile(dir + playersCSVName);
                if (playersCur[0].ContainsKey("Age"))
                {
                    curIsDraftClass = true;
                }

                #region Populate lists for this file

                var teamsCur = new List<Dictionary<string, string>>();
                var curTeamsFile = dir + teamsCSVName;
                if (File.Exists(curTeamsFile))
                {
                    teamsCur = CSV.DictionaryListFromCSVFile(curTeamsFile);
                }
                var headshapesCur = new List<Dictionary<string, string>>();
                var curHSFile = dir + headshapesCSVName;
                if (File.Exists(curHSFile))
                {
                    headshapesCur = CSV.DictionaryListFromCSVFile(curHSFile);
                }
                var awardsCur = new List<Dictionary<string, string>>();
                var curAwardsFile = dir + awardsCSVName;
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
                        if (isCurrentPlayer(basePlayer))
                        {
                            matching = matching.Where(isCurrentPlayer).ToList();
                        }
                    }
                    if (NoConflictForMatchingTeamID)
                    {
                        if (matching.Count(newPlayer => newPlayer["TeamID1"] == basePlayer["TeamID1"]) == 1)
                        {
                            matching = new List<Dictionary<string, string>>
                                       {
                                           matching.Single(
                                               newPlayer =>
                                               newPlayer["TeamID1"] == basePlayer["TeamID1"])
                                       };
                        }
                    }

                    ConflictResult = 0;
                    if (matching.Count > 1)
                    {
                        var matchingNice = matching.Select(newPlayer => presentPlayer(newPlayer, teamsCur)).ToList();

                        var cw = new ConflictWindow(presentPlayer(basePlayer, teamsBase), matchingNice, ConflictWindow.Mode.Players);
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

                    if (headshapesCur.Count > 0 && allChecked.Contains("Headshape"))
                    {
                        Dictionary<string, string> baseHS = null;
                        if (allChecked.Contains("HS_ID"))
                        {
                            // User wants the new headshape ID, but is it available?
                            if (freeHeadshapeIDs.Contains(pickedPlayer["HS_ID"].ToInt32()) ||
                                (basePlayer["HS_ID"] == pickedPlayer["HS_ID"] &&
                                 playersBase.Count(pl => pl["HS_ID"] == basePlayer["HS_ID"]) == 1))
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
                                    pickedPlayer["HS_ID"] = newHeadshapeID.ToString();

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
                                        pickedPlayer["HS_ID"] = basePlayer["HS_ID"];
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
                        }
                    }

                    #endregion Headshape

                    #region Awards

                    if (awardsCur.Count > 0 && awardsCur.Any(award => award["Pl_ASA_ID"] == pickedPlayer["ASA_ID"]) &&
                        allChecked.Contains("Awards"))
                    {
                        var pickedAwards = awardsCur.Where(award => award["Pl_ASA_ID"] == pickedPlayer["ASA_ID"]).ToList();
                        var baseAwards = awardsBase.Where(award => award["Pl_ASA_ID"] == basePlayer["ASA_ID"]).ToList();
                        for (int i = 0; i < baseAwards.Count; i++)
                        {
                            var baseAward = baseAwards[i];
                            eraseAward(ref baseAward);
                            freeAwardIDs.Add(baseAward["ID"].ToInt32());
                        }

                        foreach (var pickedAward in pickedAwards)
                        {
                            int baseIDToReplace = -1;
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

                            foreach (var property in baseAwards[0].Keys.Where(key => key != "ID").ToList())
                            {
                                if (property == "Pl_ASA_ID" && !allChecked.Contains("ASA_ID"))
                                {
                                    continue;
                                }
                                baseAwards[baseIDToReplace][property] = pickedAward[property];
                            }
                        }
                    }

                    #endregion Awards

                    #region Merge Player Entry

                    var notPlayerColumns = new List<string> {"Team Rosters", "Headshape", "Awards"};
                    foreach (var property in allChecked.Where(prop => !notPlayerColumns.Contains(prop)).ToList())
                    {
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
                List<int> playerIDsInRosters = new List<int>();
                var playersToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + playersCSVName);
                foreach (var team in teamsToMerge)
                {
                    var newTeam = team;

                    #region Match Team

                    var matchingTeams =
                        teamsBase.Where(possibleTeam => possibleTeam["Name"] == newTeam["Name"] && possibleTeam["City"] == newTeam["City"])
                                 .ToList();
                    matchingTeams = matchingTeams.Where(possibleTeam => possibleTeam["Year"] == newTeam["Year"]).ToList();

                    ConflictResult = 0;
                    if (matchingTeams.Count > 1)
                    {
                        var cw = new ConflictWindow(presentTeam(newTeam), matchingTeams.Select(presentTeam).ToList(),
                                                    ConflictWindow.Mode.Teams);
                        cw.ShowDialog();
                    }
                    if (ConflictResult == -2)
                    {
                        continue;
                    }

                    #endregion Match Team

                    var baseTeam = teamsBase.Single(te => te["ID"] == matchingTeams[ConflictResult]["ID"]);
                    foreach (var rosterSpot in newTeam.Keys.Where(key => key.StartsWith("Ros_")))
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
                            if (isCurrentPlayer(newPlayer))
                            {
                                matchingPlayers = matchingPlayers.Where(isCurrentPlayer).ToList();
                            }
                        }
                        if (NoConflictForMatchingTeamID)
                        {
                            if (matchingPlayers.Count(basePlayer => basePlayer["TeamID1"] == newPlayer["TeamID1"]) == 1)
                            {
                                matchingPlayers = new List<Dictionary<string, string>>
                                                  {
                                                      matchingPlayers.Single(
                                                          basePlayer =>
                                                          basePlayer["TeamID1"] == newPlayer["TeamID1"])
                                                  };
                            }
                        }

                        ConflictResult = 0;

                        if (matchingPlayers.Count >= 1)
                        {
                            if (matchingPlayers.Count > 1)
                            {
                                var cw = new ConflictWindow(presentPlayer(newPlayer, teamsToMerge),
                                                            matchingPlayers.Select(player => presentPlayer(player, teamsBase)).ToList(),
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
                            if (freePlayerIDs.Count == 0)
                            {
                                MessageBox.Show("Not enough space on the base roster to copy a required player. Operation aborted.",
                                                App.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            var freeID = freePlayerIDs.Pop();

                            var playerToReplace = playersBase.Single(player => player["ID"] == freeID.ToString());
                            foreach (var property in playerToReplace.Keys.Where(key => key != "ID").ToList())
                            {
                                playerToReplace[property] = newPlayer[property];
                            }
                            baseTeam[rosterSpot] = freeID.ToString();
                        }
                        foreach (var sitProp in team.Keys.Where(key => key.StartsWith("Sit_")).ToList())
                        {
                            if (team[sitProp] == newPlayer["ID"])
                            {
                                baseTeam[sitProp] = baseTeam[rosterSpot];
                            }
                        }
                        playerIDsInRosters.Add(baseTeam[rosterSpot].ToInt32());
                    }

                    foreach (var key in newTeam.Keys.Where(key => key.StartsWith("Staff_")))
                    {
                        var staffIDToMerge = newTeam[key];
                        var staffMemberToMerge = staffToMerge.Single(st => st["ID"] == staffIDToMerge);

                        var matchingStaff =
                            staffBase.Where(
                                baseStaffMember =>
                                baseStaffMember["Last_Name"] == staffMemberToMerge["Last_Name"] &&
                                baseStaffMember["First_Name"] == staffMemberToMerge["First_Name"]).ToList();
                        var matchingStaffByExperience =
                            matchingStaff.Where(baseStaffMember => baseStaffMember["Experience"] == staffMemberToMerge["Experience"])
                                         .ToList();

                        ConflictResult = 0;

                        if (matchingStaff.Count >= 1)
                        {
                            var finalMatchingStaff = matchingStaffByExperience.Count == 0 ? matchingStaff : matchingStaffByExperience;
                            if (finalMatchingStaff.Count > 1)
                            {
                                var cw = new ConflictWindow(presentStaff(staffMemberToMerge),
                                                            finalMatchingStaff.Select(presentStaff).ToList(), ConflictWindow.Mode.Staff);
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
                            if (freeStaffIDs.Count > 0)
                            {
                                var newStaffID = freeStaffIDs.Pop();

                                var staffMemberToReplace = staffBase.Single(st => st["ID"] == newStaffID.ToString());
                                foreach (var property in staffMemberToReplace.Keys.Where(name => name != "ID").ToList())
                                {
                                    staffMemberToReplace[property] = staffMemberToMerge[property];
                                }
                                baseTeam[key] = newStaffID.ToString();
                            }
                        }
                    }
                }

                var playersBaseNotInRoster =
                    playersBase.Where(pl => !playerIDsInRosters.Contains(pl["ID"].ToInt32())).Where(isValidPlayer).ToList();

                foreach (var player in playersBaseNotInRoster)
                {
                    player["IsFA"] = "1";
                    player["TeamID1"] = "-1";
                    player["TeamID2"] = "-1";
                }

                var faPlayersToMerge = playersToMerge.Where(isValidPlayer).Where(isFreeAgentPlayer).ToList();

                foreach (var newPlayer in faPlayersToMerge)
                {
                    var playerToMerge = newPlayer;
                    var matching =
                        playersBase.Where(
                            pl =>
                            pl["Last_Name"] == playerToMerge["Last_Name"] && pl["First_Name"] == playerToMerge["First_Name"] && pl["IsFA"] == "1" &&
                            pl["TeamID1"] == "-1").ToList();

                    ConflictResult = 0;

                    if (matching.Count >= 1)
                    {
                        if (matching.Count > 1)
                        {
                            var cw = new ConflictWindow(presentPlayer(playerToMerge, teamsToMerge),
                                                        matching.Select(pl => presentPlayer(pl, teamsBase)).ToList(),
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
                            MessageBox.Show("Not enough space on the base roster to copy all missing Free Agents.\n" +
                                           "The operation will continue, but some of the Free Agents from the source roster will not" +
                                           "be in the destination roster.");
                            break;
                        }
                    }
                }

                CSV.CSVFromDictionaryList(teamsBase, baseDir + teamsCSVName);
                CSV.CSVFromDictionaryList(staffBase, baseDir + staffCSVName);
            }

            #endregion Do Teams

            CSV.CSVFromDictionaryList(playersBase, baseDir + playersCSVName);
            if (awardsBase.Count > 0)
            {
                CSV.CSVFromDictionaryList(awardsBase, baseDir + awardsCSVName);
            }
            if (headshapesBase.Count > 0)
            {
                CSV.CSVFromDictionaryList(headshapesBase, baseDir + headshapesCSVName);
            }

            if (!withError)
            {
                MessageBox.Show("Done!", App.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Done but with errors. Open the tracelog.txt file located in My Documents\\Roster Workshop to " +
                                "find out more.", App.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool isFreeAgentPlayer(Dictionary<string, string> player)
        {
            return player["IsFA"] == "1" && player["TeamID1"] == "-1" && player["TeamID2"] == "-1";
        }

        private void eraseAward(ref Dictionary<string, string> award)
        {
            award["Team_Name"] = "";
            award["Team_City"] = "";
            var keys = new List<string> {"Year", "AType", "TeamGID", "OpTeamGID", "Pl_ASA_ID", "Value", "Value2"};
            foreach (var key in keys)
            {
                award[key] = "0";
            }
        }

        private static string presentStaff(Dictionary<string, string> staff)
        {
            return String.Format("{0}: {1} {2} (Experience: {3} years)", staff["ID"], staff["Last_Name"], staff["First_Name"],
                                 staff["Experience"]);
        }

        private static string presentTeam(Dictionary<string, string> team)
        {
            string s = String.Format("{0}: {1} {2}", team["ID"], team["City"], team["Name"]);
            if (team["Year"] == "0")
            {
                s += " (Current)";
            }
            else
            {
                s += " '" + team["Year"];
            }
            return s;
        }

        private static string presentPlayer(Dictionary<string, string> player, List<Dictionary<string, string>> teams)
        {
            string s = String.Format("{0}: {1} {2}", player["ID"], player["Last_Name"], player["First_Name"]);
            try
            {
                var teamName = (player["TeamID1"] != "-1") ? teams.Single(team => team["ID"] == player["TeamID1"])["Name"] : "Free Agent";
                s += string.Format(" ({0})", teamName);
            }
            catch (InvalidOperationException)
            {
                s += string.Format(" (TeamID: {0})", player["TeamID1"]);
            }
            return s;
        }

        private bool isValidPlayer(Dictionary<string, string> player)
        {
            if (player["IsRegNBA"] != "1" && player["IsSpecial"] != "1")
                return false;

            if (player["Last_Name"].StartsWith("*"))
                return false;

            if (player["TeamID1"] == "-1" && player["IsFA"] != "1")
                return false;

            return true;
        }

        private bool isCurrentPlayer(Dictionary<string, string> player)
        {
            return player["TeamID1"].ToInt32() < 30;
        }

        private List<string> getAllChecked(Dictionary<string, bool?> dict)
        {
            return dict.Where(pair => pair.Value == true).Select(pair => pair.Key).ToList();
        }
    }

    internal static class RepairTools
    {
        public static void FixSorting(string path)
        {
            var files = Directory.GetFiles(path).ToList();
            //TODO: Fix if Players3 tab is fixed
            files.Remove(files.Single(name => name.Contains("Players3")));
            //
            foreach (var file in files)
            {
                var dictList = CSV.DictionaryListFromCSVFile(file);
                dictList.Sort((dict1, dict2) => dict1["ID"].ToInt32().CompareTo(dict2["ID"].ToInt32()));
                CSV.CSVFromDictionaryList(dictList, file);
            }
        }
    }
}