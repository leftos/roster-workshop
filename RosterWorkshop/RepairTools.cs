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
            }

            CSV.CSVFromDictionaryList(players, playersFile);
        }
    }
}