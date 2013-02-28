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

using System.Collections.Generic;
using LeftosCommonLibrary;

#endregion

namespace RosterWorkshop
{
    public static class REDitorInfo
    {
        public const string TeamsCSVName = @"\Teams.csv";
        public const string StaffCSVName = @"\Staff.csv";
        public const string PlayersCSVName = @"\Players.csv";
        public const string HeadshapesCSVName = @"\Headshapes.csv";
        public const string AwardsCSVName = @"\Awards.csv";

        public static bool isFreeAgentPlayer(Dictionary<string, string> player)
        {
            return player["IsFA"] == "1" && player["TeamID1"] == "-1" && player["TeamID2"] == "-1";
        }

        public static bool isValidPlayer(Dictionary<string, string> player)
        {
            if (player["IsRegNBA"] != "1" && player["IsSpecial"] != "1")
                return false;

            if (player["Last_Name"].StartsWith("*"))
                return false;

            if (player["IsFA"] == "1" && player["TeamID1"] != "-1")
                return false;

            return true;
        }

        public static bool isCurrentPlayer(Dictionary<string, string> player)
        {
            return player["TeamID1"].ToInt32() < 30;
        }
    }
}