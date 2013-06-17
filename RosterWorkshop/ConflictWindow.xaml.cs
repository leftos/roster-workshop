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



#endregion

namespace RosterWorkshop
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;

    #endregion

    /// <summary>Interaction logic for ConflictWindow.xaml</summary>
    public partial class ConflictWindow : Window
    {
        #region Mode enum

        public enum Mode
        {
            Players,
            Teams,
            PlayersInDoTeams,
            Staff
        }

        #endregion

        public ConflictWindow()
        {
            InitializeComponent();
        }

        public ConflictWindow(string current, List<string> matching, Mode mode)
            : this()
        {
            switch (mode)
            {
                case Mode.Players:
                    txbMessage.Text = "There's more than one matching players in the roster to be merged for the following player:";
                    break;
                case Mode.Teams:
                    txbMessage.Text = "There's more than one matching teams in the base roster for the following team:";
                    chkCurrentOnly.Visibility = Visibility.Collapsed;
                    chkSelectiveConflict.Visibility = Visibility.Collapsed;
                    chkPreferUnhidden.Visibility = Visibility.Collapsed;
                    break;
                case Mode.PlayersInDoTeams:
                    txbMessage.Text = "There's more than one matching players in the base roster for the following player:";
                    break;
                case Mode.Staff:
                    txbMessage.Text = "There's more than one matching staff members in the base roster for the following staff member:";
                    chkCurrentOnly.Visibility = Visibility.Collapsed;
                    chkSelectiveConflict.Visibility = Visibility.Collapsed;
                    chkPreferUnhidden.Visibility = Visibility.Collapsed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
            txbMessage.Text += "\n" + current;
            lstMatches.ItemsSource = matching;

            chkCurrentOnly.IsChecked = MainWindow.OnlyShowCurrentMatchesForCurrent;
            chkSelectiveConflict.IsChecked = MainWindow.NoConflictForMatchingTeamID;
            chkPreferUnhidden.IsChecked = MainWindow.PreferUnhidden;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (lstMatches.SelectedIndex == -1)
            {
                return;
            }

            MainWindow.ConflictResult = lstMatches.SelectedIndex;
            Close();
        }

        private void btnSkip_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ConflictResult = -2;
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MainWindow.NoConflictForMatchingTeamID = chkSelectiveConflict.IsChecked.GetValueOrDefault();
            MainWindow.OnlyShowCurrentMatchesForCurrent = chkCurrentOnly.IsChecked.GetValueOrDefault();
            MainWindow.PreferUnhidden = chkPreferUnhidden.IsChecked.GetValueOrDefault();
        }
    }
}