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
                while (lines[++i].StartsWith("\t"))
                {
                    var line = lines[i];
                    category.Children.Add(new FooViewModel(line.Replace("\t", "")));
                    if (i == lines.Count - 1)
                        break;
                }
                columnsTree.Add(category);
                if (i == lines.Count - 1)
                    break;
                i--;
            }
            var rootItem = new FooViewModel("All");
            rootItem.Children.AddRange(columnsTree);
            root = new List<FooViewModel> {rootItem};

            trvColumns.ItemsSource = root;

            foreach (FooViewModel item in trvColumns.Items)
            {
                item.IsExpanded = item.Name == "All";
            }

            rbPlayersCurrent.IsChecked = true;
            rbTeamsCurrent.IsChecked = true;

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

                if (!_mergeSettings.ContainsKey(oldItem))
                {
                    _mergeSettings.Add(oldItem, new Dictionary<string, bool?>());
                }

                var dict = _mergeSettings[oldItem];
                populateDict(ref dict, root);
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

        private void resetIsChecked(IEnumerable<FooViewModel> items)
        {
            foreach (FooViewModel item in items)
            {
                item.IsChecked = false;
                resetIsChecked(item.Children);
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
            bool withError = false;
            foreach (var item in lstRostersToMerge.Items)
            {
                RepairTools.FixSorting(item.ToString());
            }

            var doTeams = rbTeamsNone.IsChecked != true;

            List<Dictionary<string, string>> teamsToMerge = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> teamsBase = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> staffToMerge = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> staffBase = new List<Dictionary<string, string>>();

            var baseDir = txtRosterBase.Text;
            var teamsCSVName = @"\Teams.csv";
            var teamsBaseFile = baseDir + teamsCSVName;

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

                const string staffCSVName = @"\Staff.csv";
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
            var usedHeadshapeIDs = new List<int>();
            var freeHeadshapeIDs = new List<int>();
            var availableHSCount = 0;
            var headshapesCSVName = @"\Headshapes.csv";
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
                usedHeadshapeIDs = playersBase.Select(dict => dict["HS_ID"].ToInt32()).ToList();
                freeHeadshapeIDs = headshapesBase.Select(hs => hs["ID"].ToInt32()).Where(id => !usedHeadshapeIDs.Contains(id)).ToList();
                availableHSCount = headshapesBase.Count;
            }

            var awardsBase = new List<Dictionary<string, string>>();
            var freeAwardIDs = new List<int>();
            var availableAwardsCount = 0;
            var awardsCSVName = @"\Awards.csv";
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
                availableAwardsCount = awardsBase.Count;
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

                foreach (var player in validPlayersBase)
                {
                    var basePlayer = player;

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

                    var pickedPlayer = matching[ConflictResult];

                    if (headshapesCur.Count > 0 && allChecked.Contains("HS_ID"))
                    {
                        var baseHS = headshapesBase.Single(hs => hs["ID"] == basePlayer["HS_ID"]);
                        var pickedHS = headshapesCur.Single(hs => hs["ID"] == pickedPlayer["HS_ID"]);

                        foreach (var property in pickedHS.Keys.Where(key => key != "ID").ToList())
                        {
                            baseHS[property] = pickedHS[property];
                        }
                    }

                    if (awardsCur.Count > 0 && awardsCur.Any(award => award["Pl_ASA_ID"] == pickedPlayer["ASA_ID"]))
                    {
                        var pickedAwards = awardsCur.Where(award => award["Pl_ASA_ID"] == pickedPlayer["ASA_ID"]).ToList();
                        var baseAwards = awardsBase.Where(award => award["Pl_ASA_ID"] == basePlayer["ASA_ID"]).ToList();
                        var baseAwardsIDs = baseAwards.Select(award => award["ID"].ToInt32()).ToList();

                        foreach (var pickedAward in pickedAwards)
                        {
                            int baseIDToReplace = -1;
                            if (baseAwardsIDs.Count > 0)
                            {
                                baseIDToReplace = baseAwardsIDs[0];
                                baseAwardsIDs.RemoveAt(0);
                            }
                            else if (freeHeadshapeIDs.Count > 0)
                            {
                                baseIDToReplace = freeHeadshapeIDs[0];
                                freeHeadshapeIDs.RemoveAt(0);
                            }
                            if (baseIDToReplace == -1)
                            {
                                withError = true;
                                Tools.WriteToTrace("Ran out of space while writing awards.");
                                break;
                            }
                            foreach (var property in baseAwards[0].Keys.Where(key => key != "ID").ToList())
                            {
                                baseAwards[baseIDToReplace][property] = pickedAward[property];
                            }
                        }
                    }

                    foreach (var property in allChecked)
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
                }
            }

            if (doTeams)
            {
                var playersToMerge = CSV.DictionaryListFromCSVFile(teamsToMergeDir + playersCSVName);
                foreach (var team in teamsToMerge)
                {
                    var newTeam = team;
                    var plCount = newTeam["PlNum"].ToInt32();

                    var matchingTeams =
                        teamsBase.Where(possibleTeam => possibleTeam["Name"] == newTeam["Name"] && possibleTeam["City"] == newTeam["City"])
                                 .ToList();
                    matchingTeams = matchingTeams.Where(possibleTeam => possibleTeam["Year"] == newTeam["Year"]).ToList();

                    ConflictResult = 0;
                    if (matchingTeams.Count > 1)
                    {
                        var cw = new ConflictWindow(presentTeam(newTeam), matchingTeams.Select(presentTeam).ToList(), ConflictWindow.Mode.Teams);
                        cw.ShowDialog();
                    }
                    if (ConflictResult == -2)
                    {
                        continue;
                    }

                    var baseTeam = matchingTeams[ConflictResult];
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

                            var freeID = freePlayerIDs[0];
                            freePlayerIDs.RemoveAt(0);

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
                    }

                    // TODO: Implement old lines from 546 onwards
                }
            }

            CSV.CSVFromDictionaryList(playersBase, baseDir + playersCSVName);
            if (awardsBase.Count > 0)
            {
                CSV.CSVFromDictionaryList(awardsBase, baseDir + awardsCSVName);
            }
            if (headshapesBase.Count > 0)
            {
                CSV.CSVFromDictionaryList(headshapesBase, baseDir + headshapesCSVName);
            }

            MessageBox.Show("Done!", App.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
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
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var dictList = CSV.DictionaryListFromCSVFile(file);
                dictList.Sort((dict1, dict2) => dict1["ID"].ToInt32().CompareTo(dict2["ID"].ToInt32()));
                CSV.CSVFromDictionaryList(dictList, file);
            }
        }
    }
}