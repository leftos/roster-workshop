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
    using System.Linq;
    using System.Media;
    using System.Windows;
    using System.Windows.Input;

    #endregion

    /// <summary>Interaction logic for PickGamesWindow.xaml</summary>
    public partial class PickTeamPairsWindow : Window
    {
        private readonly List<Dictionary<string, string>> _teamsBase;
        private readonly List<Dictionary<string, string>> _teamsToMerge;

        private PickTeamPairsWindow()
        {
            InitializeComponent();
        }

        public PickTeamPairsWindow(List<Dictionary<string, string>> teamsBase, List<Dictionary<string, string>> teamsToMerge)
            : this()
        {
            _teamsBase = teamsBase;
            _teamsToMerge = teamsToMerge;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnAddPair_Click(object sender, RoutedEventArgs e)
        {
            if (lstTeamsToMerge.SelectedItems.Count == 1 && lstTeamsBase.SelectedItems.Count == 1)
            {
                var teamToMerge = lstTeamsToMerge.SelectedItem.ToString();
                var teamBase = lstTeamsBase.SelectedItem.ToString();
                lstSelectedPairs.Items.Add(teamToMerge + " to be replaced by " + teamBase);
                MainWindow.TeamPairs.Add(teamBase, teamToMerge);
                lstTeamsToMerge.Items.Remove(teamBase);
                lstTeamsBase.Items.Remove(teamToMerge);
                lstTeamsBase.Items.Remove(teamBase);
                lstTeamsToMerge.Items.Remove(teamToMerge);
                /*
                if (lstAvailableAway.Items.Count == 0 && lstAvailableHome.Items.Count == 0)
                    btnOK.IsEnabled = true;
                */
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void lstSelectedPairs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstSelectedPairs.SelectedItems.Count == 1)
            {
                if (MessageBox.Show("Are you sure you want to remove \"" + lstSelectedPairs.SelectedItem + "\"?")
                    == MessageBoxResult.Yes)
                {
                    var parts = lstSelectedPairs.SelectedItem.ToString()
                                                .Split(new[] { " to be replaced by " }, StringSplitOptions.None);
                    lstSelectedPairs.Items.Remove(lstSelectedPairs.SelectedItem);
                    foreach (var part in parts)
                    {
                        MainWindow.TeamPairs.Remove(parts[0]);
                        lstTeamsToMerge.Items.Add(part);
                        lstTeamsBase.Items.Add(part);
                    }

                    var list = lstTeamsToMerge.Items.Cast<string>().ToList();
                    list.Sort();
                    lstTeamsToMerge.Items.Clear();
                    list.ForEach(item => lstTeamsToMerge.Items.Add(item));

                    list = lstTeamsBase.Items.Cast<string>().ToList();
                    list.Sort();
                    lstTeamsBase.Items.Clear();
                    list.ForEach(item => lstTeamsBase.Items.Add(item));

                    /*
                    if (lstAvailableAway.Items.Count != 0 || lstAvailableHome.Items.Count != 0)
                        btnOK.IsEnabled = false;
                    */
                }
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            _teamsBase.ForEach(item => lstTeamsBase.Items.Add(REDitorInfo.PresentTeamNicer(item)));
            _teamsToMerge.ForEach(item => lstTeamsToMerge.Items.Add(REDitorInfo.PresentTeamNicer(item)));

            //btnOK.IsEnabled = false;
        }
    }
}