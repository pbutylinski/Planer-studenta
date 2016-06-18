using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using Scheduler;

namespace Planer_studenta
{
    public partial class Settings : PhoneApplicationPage
    {
        private static bool NewlyLoaded = true;
        public Settings()
        {
            InitializeComponent();
        }

        private void LoadData()
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

            ShowMoCheckBox.IsChecked = Mo;
            ShowTuCheckBox.IsChecked = Tu;
            ShowWeCheckBox.IsChecked = We;
            ShowThCheckBox.IsChecked = Th;
            ShowFrCheckBox.IsChecked = Fr;
            ShowSaCheckBox.IsChecked = Sa;
            ShowSuCheckBox.IsChecked = Su;




            double HeigthMultiplier = 1;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<double>(AppSettings.HeigthMultiplier, out HeigthMultiplier);
            HeigthMultiplierListPicker.SelectedIndex = SizeToIndex(HeigthMultiplier);

            DateTime DayStart = new DateTime();
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<DateTime>(AppSettings.DayStart, out DayStart);
            DayStartTimePicker.Value = DayStart;


            bool JakiTydzienIntegration = true;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.JakiTydzienIntegration, out JakiTydzienIntegration);
            JakiTydzienIntegrationCheckBox.IsChecked = JakiTydzienIntegration;

            bool AutoDayStart = false;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.AutomaticDayStart, out AutoDayStart);
            AutoDayStartCheckBox.IsChecked = AutoDayStart;

            DayStartTimePicker.IsEnabled = !AutoDayStart;

            // Stats
            UpdateStats();
        }

        private void UpdateStats()
        {
            DateTime? LastModified = new DateTime();
            List<SingleEvent> Events = new List<SingleEvent>();

            EventsFile.Open(out Events, out LastModified);

            LastModifiedTextBlock.Text =
                LastModified == null || LastModified == new DateTime() ? "brak" :
                LastModified.ToString();

            FileCountTextBlock.Text =
                Events == null || !Events.Any() ? "brak" :
                Events.Count().ToString();
        }

        private double IndexToSize(int index)
        {
            switch (index)
            {
                case 0: return 1.0;
                case 1: return 1.5;
                case 2: return 2;
                default: return 1.0;
            }
        }

        private int SizeToIndex(double Size)
        {
            switch ((int)(Size * 10))
            {
                case 10:
                default: return 0;
                case 15: return 1;
                case 20: return 2;
            }
        }

        

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveOrCreate<DateTime>(AppSettings.DayStart, DayStartTimePicker.Value ?? new DateTime());
            SaveOrCreate<double>(AppSettings.HeigthMultiplier, IndexToSize(HeigthMultiplierListPicker.SelectedIndex));

            SaveOrCreate<bool>(AppSettings.DayVisibilityMonday, ShowMoCheckBox.IsChecked ?? false);
            SaveOrCreate<bool>(AppSettings.DayVisibilityTuesday, ShowTuCheckBox.IsChecked ?? false);
            SaveOrCreate<bool>(AppSettings.DayVisibilityWednesday, ShowWeCheckBox.IsChecked ?? false);
            SaveOrCreate<bool>(AppSettings.DayVisibilityThursday, ShowThCheckBox.IsChecked ?? false);
            SaveOrCreate<bool>(AppSettings.DayVisibilityFriday, ShowFrCheckBox.IsChecked ?? false);
            SaveOrCreate<bool>(AppSettings.DayVisibilitySaturday, ShowSaCheckBox.IsChecked ?? false);
            SaveOrCreate<bool>(AppSettings.DayVisibilitySunday, ShowSuCheckBox.IsChecked ?? false);

            SaveOrCreate<bool>(AppSettings.JakiTydzienIntegration, JakiTydzienIntegrationCheckBox.IsChecked ?? true);
            SaveOrCreate<bool>(AppSettings.AutomaticDayStart, AutoDayStartCheckBox.IsChecked ?? false);

            NewlyLoaded = true;
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        public static void SaveOrCreate<T>(string Key, T Value)
        {
            System.IO.IsolatedStorage.IsolatedStorageSettings appSettings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;

            if (appSettings.Any(k => k.Key == Key))
                appSettings[Key] = Value;
            else appSettings.Add(Key, Value);

            appSettings.Save();
        }

        private void LayoutRoot_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (NewlyLoaded)
            {
                LoadData();
                NewlyLoaded = false;
            }
        }

        private void ClearDataButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Czy na pewno chcesz usunąć wszystkie zajęcia z planu?", "Potwierdzenie", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if (EventsFile.Save(new List<SingleEvent>()))
                {
                    MessageBox.Show("Wszystkie zajęcia zostały usunięte");
                    UpdateStats();
                }
            }
        }

        private void AutoDayStartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DayStartTimePicker.IsEnabled = false;
        }

        private void AutoDayStartCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DayStartTimePicker.IsEnabled = true;
        }
    }
}