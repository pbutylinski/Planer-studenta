using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;

namespace Planer_studenta
{
    public partial class Import : PhoneApplicationPage
    {
        public Import()
        {
            InitializeComponent();
        }


        private void IdTextBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (IdTextBox.Text == "abcdef123456")
                IdTextBox.Text = "";
        }

        public void StartImport(string Path)
        {
            if (MessageBox.Show("Importowanie usunie wszystkie istniejące wpisy w planie! Kontynuować?", "Uwaga!", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ImportData(Path);
            }
        }

        public void ImportData(string Path)
        {
            ProgressIndicator ImportProgressIndicator = new ProgressIndicator();

            ImportProgressIndicator.IsVisible = true;
            ImportProgressIndicator.IsIndeterminate = true;
            ImportProgressIndicator.Text = "Importowanie planu...";

            SystemTray.ProgressIndicator = ImportProgressIndicator;

            Uri SourcePath = new Uri(Path);
            string Source = String.Empty;

            WebClient Client = new WebClient();
            Client.DownloadStringCompleted += Client_DownloadStringCompleted;
            Client.DownloadStringAsync(SourcePath);
        }

        void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            bool IsOk = true;

            try
            {
                string Source = String.Empty;

                if (e != null)
                    Source = e.Result;

                // removing uneccessary encapsulation from Infeka
                if (Source.Contains("{\"schedule\":"))
                {
                    Source = Source.Replace("{\"schedule\":", "").Replace("}]}}", "}]}");
                }

                if (!String.IsNullOrWhiteSpace(Source))
                {
                    PlanPwrWrapper PlanPwr = JsonConvert.DeserializeObject<PlanPwrWrapper>(Source);

                    List<SingleEvent> Events = new List<SingleEvent>();

                    if (PlanPwr != null)
                    {
                        foreach (var item in PlanPwr.entries)
                        {
                            SingleEvent ev = new SingleEvent();
                            ev.Day = (DayOfWeek)item.week_day;
                            ev.EndTime = new EventTime(item.end_hour, item.end_min);
                            ev.ID = Guid.NewGuid();
                            ev.Lecturer = item.lecturer;
                            ev.Location = item.room + " " + item.building;
                            ev.Name = item.course_name;
                            ev.Occurence =
                                item.week == 0 ? EventOccurence.Weekly :
                                item.week == 1 ? EventOccurence.OddWeek :
                                EventOccurence.EvenWeek;
                            ev.ShortName = TrimName(item.course_name);
                            ev.StartTime = new EventTime(item.start_hour, item.start_min);
                            ev.Type =
                                item.course_type == "W" ? EventType.Lecture :
                                item.course_type == "C" ? EventType.Excercises :
                                item.course_type == "L" ? EventType.Laboratories :
                                item.course_type == "S" ? EventType.Seminar :
                                EventType.Other;

                            Events.Add(ev);
                        }
                    }

                    EventsFile.Save(Events);

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Zaimportowano " + Events.Count + " zajęć!");
                    });
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Czy na pewno kod i serwis jest poprawny?" + Environment.NewLine + "Kod błędu: 0x03", "Wystąpił błąd", MessageBoxButton.OK);
                IsOk = false;
            }

            SystemTray.ProgressIndicator = null;

            if (IsOk)
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private string TrimName(string Name)
        {
            if (Name.Length <= 22)
            {
                return Name;
            }
            else
            {
                string ShortName = Name.Substring(0, 20);
                if (ShortName.EndsWith(" "))
                {
                    return ShortName.TrimEnd(new char[] { ' ' });
                }
                else
                {
                    return ShortName + ".";
                }
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(IdTextBox.Text) || IdTextBox.Text == "abcdef123456")
            {
                MessageBox.Show("Wprowadź identyfikator planu!");
            }
            else
            {
                string Site = String.Empty;

                switch (ServiceListPicker.SelectedIndex)
                {
                    case 0: Site = "http://plan-pwr.pl/"; break;
                    case 1: Site = "http://infeka.co.vu/"; break;
                    case 2: Site = "http://infeka.cf/"; break;
                    default: break;
                }

                StartImport(Site + IdTextBox.Text + ".js");
            }
        }
    }
}