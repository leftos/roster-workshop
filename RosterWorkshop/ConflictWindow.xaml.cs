using System;
using System.Collections.Generic;
using System.Windows;

namespace RosterWorkshop
{
    /// <summary>
    /// Interaction logic for ConflictWindow.xaml
    /// </summary>
    public partial class ConflictWindow : Window
    {
        public ConflictWindow()
        {
            InitializeComponent();
        }

        public ConflictWindow(string current, List<string> matching, Mode mode) : this()
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
                return;

            MainWindow.ConflictResult = lstMatches.SelectedIndex;
            Close();
        }

        private void btnSkip_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ConflictResult = -2;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.NoConflictForMatchingTeamID = chkSelectiveConflict.IsChecked.GetValueOrDefault();
            MainWindow.OnlyShowCurrentMatchesForCurrent = chkCurrentOnly.IsChecked.GetValueOrDefault();
            MainWindow.PreferUnhidden = chkPreferUnhidden.IsChecked.GetValueOrDefault();
        }

        public enum Mode
        {
            Players,
            Teams,
            PlayersInDoTeams,
            Staff
        }
    }
}