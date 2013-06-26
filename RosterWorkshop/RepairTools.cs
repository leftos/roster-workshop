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
    using System.IO;
    using System.Linq;
    using System.Windows;

    using LeftosCommonLibrary;

    #endregion

    internal static class RepairTools
    {
        public static void FixSorting(string path)
        {
            var files = Directory.GetFiles(path).ToList();
            //
            foreach (var file in files)
            {
                var dictList = CSV.DictionaryListFromCSVFile(file);
                dictList.Sort((dict1, dict2) => dict1["ID"].ToInt32().CompareTo(dict2["ID"].ToInt32()));
                CSV.CSVFromDictionaryList(dictList, file);
            }
        }

        public static void FixTeamIDs(string path)
        {
            var teamsFile = path + REDitorInfo.TeamsCSVName;
            var playersFile = path + REDitorInfo.PlayersCSVName;

            if (!File.Exists(teamsFile) || !File.Exists(playersFile))
            {
                MessageBox.Show("You need both the Players and Teams tab exported to CSV in order to fix Team IDs.");
                return;
            }

            var teams = CSV.DictionaryListFromCSVFile(teamsFile);
            var players = CSV.DictionaryListFromCSVFile(playersFile);

            var activePlayersDict =
                teams.Where(team => team["TType"] == "0" || team["TType"] == "21")
                     .SelectMany(
                         team =>
                         team.Keys.Where(key => key.StartsWith("Ros_"))
                             .Select(key => team[key])
                             .ToDictionary(id => id, id => team["ID"]))
                     .ToDictionary(o => o.Key, o => o.Value);
            foreach (var player in players)
            {
                var playerID = player["ID"];
                if (activePlayersDict.ContainsKey(playerID))
                {
                    player["IsFA"] = "0";
                    player["TeamID1"] = activePlayersDict[playerID];
                    player["TeamID2"] = activePlayersDict[playerID];
                }
                else
                {
                    player["TeamID1"] = "-1";
                    player["TeamID2"] = "-1";
                }

                if (player["IsFA"] == "1")
                {
                    player["CClrYears"] = "0";
                    player["COption"] = "0";
                    player["CNoTrade"] = "0";

                    for (var i = 1; i < 6; i++)
                    {
                        player["CYear" + i] = "0";
                    }
                }
            }

            CSV.CSVFromDictionaryList(players, playersFile);
        }

        public static void FixASAIDs(string path)
        {
            var playersFile = path + REDitorInfo.PlayersCSVName;
            var awardsFile = path + REDitorInfo.AwardsCSVName;

            if (!File.Exists(awardsFile) || !File.Exists(playersFile))
            {
                MessageBox.Show("You need both the Players and Awards tab exported to CSV in order to fix ASA_IDs.");
                return;
            }

            var awards = CSV.DictionaryListFromCSVFile(awardsFile);
            var players = CSV.DictionaryListFromCSVFile(playersFile);

            var takenIDs = players.Select(pl => pl["ASA_ID"].ToInt32()).ToList();

            var seenIDs = new List<int>();
            const int maxASAID = 32767;
            foreach (var pl in players)
            {
                var curPlayer = pl;
                if (seenIDs.Contains(curPlayer["ASA_ID"].ToInt32()))
                {
                    var newID = getFreeID(takenIDs, maxASAID);
                    var awardsToEdit = awards.Where(aw => aw["Pl_ASA_ID"] == curPlayer["ASA_ID"]);
                    foreach (var award in awardsToEdit)
                    {
                        award["Pl_ASA_ID"] = newID.ToString();
                    }
                    curPlayer["ASA_ID"] = newID.ToString();
                    takenIDs.Add(newID);
                }
                else
                {
                    seenIDs.Add(curPlayer["ASA_ID"].ToInt32());
                }
            }
        }

        private static int getFreeID(ICollection<int> list, int maxInclusive)
        {
            for (var i = 0; i <= maxInclusive; i++)
            {
                if (!list.Contains(i))
                {
                    return i;
                }
            }
            throw new InvalidOperationException("List has no free IDs smaller or equal to " + maxInclusive);
        }

        public static void FixPlayerNumberAndOrder(string path)
        {
            var teamsFile = path + REDitorInfo.TeamsCSVName;

            if (!File.Exists(teamsFile))
            {
                MessageBox.Show("You need the Teams tab exported to CSV for this feature.");
                return;
            }

            var teams = CSV.DictionaryListFromCSVFile(teamsFile);

            foreach (var team in teams)
            {
                var curTeam = team;
                var rosterKeys = curTeam.Keys.Where(key => key.StartsWith("Ros_")).ToList();
                for (var i = 0; i < rosterKeys.Count; i++)
                {
                    var key = rosterKeys[i];
                    var curID = curTeam[key];
                    if (curID == "-1")
                    {
                        for (var j = rosterKeys.Count - 1; j > i; j--)
                        {
                            var newKey = rosterKeys[j];
                            var newID = curTeam[newKey];
                            if (newID != "-1")
                            {
                                curTeam[key] = newID;
                                curTeam[newKey] = curID;
                            }
                        }
                    }
                }

                curTeam["PlNum"] = rosterKeys.Count(key => curTeam[key] != "-1").ToString();
            }

            CSV.CSVFromDictionaryList(teams, teamsFile);
        }

        public static void FixContracts(string path)
        {
            var playersFile = path + REDitorInfo.PlayersCSVName;

            if (!File.Exists(playersFile))
            {
                MessageBox.Show("You need the Teams tab exported to CSV for this feature.");
                return;
            }

            var players = CSV.DictionaryListFromCSVFile(playersFile);

            foreach (var pl in players)
            {
                var curPlayer = pl;

                if (curPlayer["IsFA"] == "1")
                {
                    curPlayer["CClrYears"] = "0";
                    curPlayer["COption"] = "0";
                    curPlayer["CNoTrade"] = "0";

                    for (var i = 1; i < 6; i++)
                    {
                        curPlayer["CYear" + i] = "0";
                    }
                }
                else
                {
                    for (var i = 1; i <= 6; i++)
                    {
                        if (curPlayer["CYear" + i] == "0")
                        {
                            for (var j = i + 1; j <= 6; j++)
                            {
                                curPlayer["CYear" + j] = "0";
                            }
                            curPlayer["CClrYears"] = (i - 1 - yearsInOption(curPlayer["COption"])).ToString();
                            break;
                        }
                    }
                }
            }

            CSV.CSVFromDictionaryList(players, playersFile);
        }

        private static int yearsInOption(string option)
        {
            switch (option)
            {
                case "0":
                    return 0;
                case "1":
                case "2":
                    return 1;
                case "3":
                    return 2;
                default:
                    throw new ArgumentOutOfRangeException("option");
            }
        }

        //TODO: Calculate Contract Years
    }
}