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
    using System.Linq;

    using LeftosCommonLibrary;

    #endregion

    public static class REDitorInfo
    {
        public const string RecordsCSVName = @"\Records.csv";
        public const string PlayerStatsCSVName = @"\Player_Stats.csv";
        public const string TeamStatsCSVName = @"\Team_Stats.csv";
        public const string TeamsCSVName = @"\Teams.csv";
        public const string StaffCSVName = @"\Staff.csv";
        public const string PlayersCSVName = @"\Players.csv";
        public const string HeadshapesCSVName = @"\Headshapes.csv";
        public const string AwardsCSVName = @"\Awards.csv";
        public const string JerseysCSVName = @"\Jerseys.csv";

        public static bool IsFreeAgentPlayer(Dictionary<string, string> player)
        {
            return player["IsFA"] == "1" && player["TeamID1"] == "-1" && player["TeamID2"] == "-1";
        }

        public static bool IsValidPlayer(Dictionary<string, string> player)
        {
            if (player["IsRegNBA"] != "1" && player["IsSpecial"] != "1")
            {
                return false;
            }

            if (player["Last_Name"].StartsWith("*"))
            {
                return false;
            }

            if (player["IsFA"] == "1" && player["TeamID1"] != "-1")
            {
                return false;
            }

            return true;
        }

        public static bool IsCurrentPlayer(Dictionary<string, string> player)
        {
            return player["TeamID1"].ToInt32() < 30;
        }

        public static string PresentStaff(Dictionary<string, string> staff)
        {
            return String.Format(
                "{0}: {2}{1} (SType: {3} - Experience: {4} years)",
                staff["ID"],
                staff["Last_Name"],
                !String.IsNullOrWhiteSpace(staff["First_Name"]) ? staff["First_Name"] + " " : "",
                staff["SType"],
                staff["Experience"]);
        }

        public static string PresentTeam(Dictionary<string, string> team)
        {
            var s = String.Format(
                "{0}: {1}{2}", team["ID"], !String.IsNullOrWhiteSpace(team["City"]) ? team["City"] + " " : "", team["Name"]);
            if (team["Year"] == "0")
            {
                s += " (Current)";
            }
            else
            {
                s += " '" + team["Year"].PadLeft(2, '0');
            }
            return s;
        }

        public static string PresentTeamNicer(Dictionary<string, string> team)
        {
            var s = String.Format("{0}{1}", !String.IsNullOrWhiteSpace(team["City"]) ? team["City"] + " " : "", team["Name"]);
            if (team["Year"] == "0")
            {
                s += " (Current)";
            }
            else
            {
                s += " '" + team["Year"].PadLeft(2, '0');
            }
            s += String.Format(" (ID: {0})", team["ID"]);
            return s;
        }

        public static string PresentPlayer(Dictionary<string, string> player, IEnumerable<Dictionary<string, string>> teams)
        {
            var s = String.Format(
                "{0}: {2}{1}",
                player["ID"],
                player["Last_Name"],
                !String.IsNullOrWhiteSpace(player["First_Name"]) ? player["First_Name"] + " " : "");
            try
            {
                var teamName = (player["TeamID1"] != "-1")
                                   ? teams.Single(team => team["ID"] == player["TeamID1"])["Name"]
                                   : "Free Agent";
                var isHidden = (player["IsFA"] == "0" && player["TeamID1"] == "-1") ? "Hidden" : "Unhidden";
                s += String.Format(" ({0}", teamName);
                if (teamName == "Free Agent")
                {
                    s += String.Format(" - {0}", isHidden);
                }
                s += ")";
            }
            catch (InvalidOperationException)
            {
                s += String.Format(" (TeamID: {0})", player["TeamID1"]);
            }
            return s;
        }

        public static bool IsValidTeam(Dictionary<string, string> team)
        {
            if (team["Name"].StartsWith("*"))
            {
                return false;
            }

            if (team["TType"] != "0" && team["TType"] != "21")
            {
                return false;
            }

            return true;
        }
    }
}