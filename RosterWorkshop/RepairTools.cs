using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;

namespace RosterWorkshop
{
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
                         team.Keys.Where(key => key.StartsWith("Ros_")).Select(key => team[key]).ToDictionary(id => id, id => team["ID"]))
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

                    for (int i = 1; i < 6; i++)
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
                Dictionary<string, string> curPlayer = pl;
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
            for (int i = 0; i <= maxInclusive; i++)
            {
                if (!list.Contains(i))
                {
                    return i;
                }
            }
            throw new InvalidOperationException("List has no free IDs smaller or equal to " + maxInclusive);
        }

        //TODO: Calculate Contract Years
        //TODO: Fix PlNum & Player Order
    }
}