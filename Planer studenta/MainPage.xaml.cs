using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using Scheduler;

namespace Planer_studenta
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            ApplicationBarIconButton WeekIcon = (ApplicationBarIconButton)ApplicationBar.Buttons[2];

            if (Week.IsEven)
            {
                WeekIcon.Text = "Parzysty";
                WeekIcon.IconUri = new Uri("/Assets/AppBar/icon-tp.png", UriKind.Relative);
            }
            else
            {
                WeekIcon.Text = "Nieparzysty";
                WeekIcon.IconUri = new Uri("/Assets/AppBar/icon-tn.png", UriKind.Relative);
            }

            LoadEventElements();

            // Live Tile
            InitTile();
        }

        private void LoadEventElements()
        {
            Layout.LoadEventElements(
                this,
                ref MonPanel,
                ref TuePanel,
                ref WedPanel,
                ref ThuPanel,
                ref FriPanel,
                ref SatPanel,
                ref SunPanel);
        }

        private static void InitTile()
        {
            try
            {
                ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault();

                if (Tile != null)
                {
                    // Get current and next event
                    List<SingleEvent> Events = new List<SingleEvent>();
                    DateTime? LastModified = new DateTime();

                    EventsFile.Open(out Events, out LastModified);

                    DateTime Date = DateTime.Now;

                    DayOfWeek CurrentDay = AddEditEvent.DayOfWeekConverter((int)Date.DayOfWeek);
                    EventOccurence CurrentWeek = Week.IsEven ? EventOccurence.EvenWeek : EventOccurence.OddWeek;

                    string EventDesc = null;// "brak zajęć w tej chwili";

                    //if (Events != null)
                    //{
                    //    SingleEvent CurrentEvent = Events.
                    //        Where(k =>
                    //            k.Day == CurrentDay &&
                    //            (k.Occurence == EventOccurence.Weekly || k.Occurence == CurrentWeek) &&
                    //            k.StartTime.Hour <= Date.Hour &&
                    //            k.StartTime.Minute <= Date.Minute &&
                    //            k.EndTime.Hour >= Date.Hour &&
                    //            k.EndTime.Minute >= Date.Minute).
                    //        FirstOrDefault();

                    //    if (CurrentEvent != null)
                    //    {
                    //        EventDesc = CurrentEvent.ShortName;
                    //        EventDesc += Environment.NewLine + Environment.NewLine;
                    //        EventDesc += CurrentEvent.StartTime.Hour + ":" + CurrentEvent.StartTime.Minute.ToString("00") + " - ";
                    //        EventDesc += CurrentEvent.EndTime.Hour + ":" + CurrentEvent.EndTime.Minute.ToString("00");
                    //        EventDesc += Environment.NewLine;
                    //        EventDesc += CurrentEvent.Location;
                    //    }
                    //}

                    // Update tile
                    ShellTileData Data = new FlipTileData()
                    {
                        Title = "",
                        BackTitle = "Bieżące zajęcia",
                        BackContent = EventDesc,
                        SmallBackgroundImage = Week.IsEven ?
                            new Uri("/Assets/Tiles/bg_small_tp.png", UriKind.Relative) :
                            new Uri("/Assets/Tiles/bg_small_tn.png", UriKind.Relative),
                        BackgroundImage = Week.IsEven ?
                            new Uri("/Assets/Tiles/bg_medium_tp.png", UriKind.Relative) :
                            new Uri("/Assets/Tiles/bg_medium_tn.png", UriKind.Relative),
                        BackBackgroundImage = new Uri("/Assets/Tiles/bg_medium.png", UriKind.Relative),
                        WideBackgroundImage = Week.IsEven ?
                            new Uri("/Assets/Tiles/bg_big_tp.png", UriKind.Relative) :
                            new Uri("/Assets/Tiles/bg_big_tn.png", UriKind.Relative),
                        WideBackBackgroundImage = new Uri("/Assets/Tiles/bg_big.png", UriKind.Relative),
                    };

                    Tile.Update(Data);

                    ShellTileSchedule TileSchedule = new ShellTileSchedule(Tile, Data);

                    TileSchedule.Interval = UpdateInterval.EveryHour;
                    TileSchedule.Recurrence = UpdateRecurrence.Interval;
                    TileSchedule.Start();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Kod błędu: 0x04", "Wystąpił błąd", MessageBoxButton.OK);
            }
        }

        protected void HideDays()
        {
            bool Mo = true;
            bool Tu = true;
            bool We = true;
            bool Th = true;
            bool Fr = true;
            bool Sa = true;
            bool Su = true;

            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.DayVisibilityMonday, out Mo);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.DayVisibilityTuesday, out Tu);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.DayVisibilityWednesday, out We);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.DayVisibilityThursday, out Th);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.DayVisibilityFriday, out Fr);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.DayVisibilitySaturday, out Sa);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.DayVisibilitySunday, out Su);

            try
            {
                if (!Mo) MondayItem.Visibility = System.Windows.Visibility.Collapsed;
                if (!Tu) TuesdayItem.Visibility = System.Windows.Visibility.Collapsed;
                if (!We) WednesdayItem.Visibility = System.Windows.Visibility.Collapsed;
                if (!Th) ThursdayItem.Visibility = System.Windows.Visibility.Collapsed;
                if (!Fr) FridayItem.Visibility = System.Windows.Visibility.Collapsed;
                if (!Sa) SaturdayItem.Visibility = System.Windows.Visibility.Collapsed;
                if (!Su) SundayItem.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (Exception)
            {
                MessageBox.Show("Kod błędu: 0x05", "Wystąpił błąd", MessageBoxButton.OK);
            }
        }

        private void FirstRun()
        {
            bool FirstRun = !IsolatedStorageSettings.ApplicationSettings.Any(k => k.Key == AppSettings.FirstRun);

            if (FirstRun)
            {
                try
                {
                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.DayVisibilityMonday, true);
                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.DayVisibilityTuesday, true);
                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.DayVisibilityWednesday, true);
                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.DayVisibilityThursday, true);
                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.DayVisibilityFriday, true);
                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.DayVisibilitySaturday, true);
                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.DayVisibilitySunday, true);

                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.HeigthMultiplier, 1.5);

                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.DayStart, new DateTime(1, 1, 1, 7, 15, 0));

                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.FirstRun, false);

                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.JakiTydzienIntegration, true);
                }
                catch (Exception)
                {
                    MessageBox.Show("Kod błędu: 0x02", "Wystąpił błąd", MessageBoxButton.OK);
                }
            }
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddEditEvent.xaml", UriKind.Relative));
        }

        private void ImportMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Import.xaml", UriKind.Relative));
        }

        private void ExportMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Export.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void LayoutRoot_Loaded_1(object sender, RoutedEventArgs e)
        {
            FirstRun();
            HideDays();

            // Go to current day
            int CurrentDay = (int)DateTime.Now.DayOfWeek;
            SchedulePanorama.DefaultItem = SchedulePanorama.Items[CurrentDay == 0 ? 6 : (CurrentDay - 1)];

            LoadEventElements();
        }

        private void AboutMenuItem_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            bool JakiTydzienIntegration = true;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.JakiTydzienIntegration, out JakiTydzienIntegration);

            if (JakiTydzienIntegration)
            {
                Week.RefreshJakiTydzien(true);
            }
        }
    }
}