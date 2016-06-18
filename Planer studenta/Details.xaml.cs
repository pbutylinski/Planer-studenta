using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;

namespace Planer_studenta
{
    public partial class Details : PhoneApplicationPage
    {
        private SingleEvent SelectedEvent = new SingleEvent();
        public Details()
        {
            InitializeComponent();
        }

        private void EditButton_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddEditEvent.xaml?id=" + SelectedEvent.ID, UriKind.Relative));
        }

        private void DeleteButton_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("Na pewno chcesz usunąć to wydarzenie?", "Usuwanie", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                List<SingleEvent> Events = new List<SingleEvent>();
                DateTime? LastModified = new DateTime();
                EventsFile.Open(out Events, out LastModified);

                Events.Remove(SelectedEvent);

                EventsFile.Save(Events);

                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private void LayoutRoot_Loaded_1(object sender, RoutedEventArgs e)
        {
            List<SingleEvent> Events = new List<SingleEvent>();
            DateTime? LastModified = new DateTime();
            EventsFile.Open(out Events, out LastModified);

            string g = String.Empty;

            NavigationContext.QueryString.TryGetValue("id", out g);

            Guid EventID = new Guid(g);

            SelectedEvent = Events.
                Where(k => k.ID == EventID).
                FirstOrDefault() ?? new SingleEvent();


            // Loading elements
            NameTextBlock.Text = NameTopTextBlock.Text = SelectedEvent.Name;

            DayTextBlock.Text = "📅 " + (
                SelectedEvent.Day == DayOfWeek.Monday ? "Poniedziałek" :
                SelectedEvent.Day == DayOfWeek.Tuesday ? "Wtorek" :
                SelectedEvent.Day == DayOfWeek.Wednesday ? "Środa" :
                SelectedEvent.Day == DayOfWeek.Thursday ? "Czwartek" :
                SelectedEvent.Day == DayOfWeek.Friday ? "Piątek" :
                SelectedEvent.Day == DayOfWeek.Saturday ? "Sobota" :
                "Niedziela");

            HoursTextBlock.Text = "⏰ " + 
                SelectedEvent.StartTime.Hour + ":" + SelectedEvent.StartTime.Minute.ToString("00") + " - " +
                SelectedEvent.EndTime.Hour + ":" + SelectedEvent.EndTime.Minute.ToString("00");

            LocationTextBlock.Text = "🏢 " + SelectedEvent.Location;

            OccurenceTextBlock.Text =
                SelectedEvent.Occurence == EventOccurence.EvenWeek ? "◗ Tydzień parzysty" :
                SelectedEvent.Occurence == EventOccurence.OddWeek ? "◖ Tydzień nieparzysty" :
                "⚪ Co tydzień";

            TypeTextBlock.Text =
                SelectedEvent.Type == EventType.Excercises ? "📓 Ćwiczenia" :
                SelectedEvent.Type == EventType.Laboratories ? "☢ Laboratoria" :
                SelectedEvent.Type == EventType.Lecture ? "📚 Wykład" :
                SelectedEvent.Type == EventType.Seminar ? "💬 Seminarium" :
                "Inne";

            LecturerTextBlock.Text = "💁 " + SelectedEvent.Lecturer;

            switch (SelectedEvent.Type)
            {
                case EventType.Laboratories:
                    ColorRectangle.Fill = new SolidColorBrush(Layout.ConvertStringToColor(EventTypesColors.Laboratories));
                    break;
                case EventType.Lecture:
                    ColorRectangle.Fill = new SolidColorBrush(Layout.ConvertStringToColor(EventTypesColors.Lectures));
                    break;
                case EventType.Excercises:
                    ColorRectangle.Fill = new SolidColorBrush(Layout.ConvertStringToColor(EventTypesColors.Excercises));
                    break;
                case EventType.Seminar:
                    ColorRectangle.Fill = new SolidColorBrush(Layout.ConvertStringToColor(EventTypesColors.Seminars));
                    break;
                case EventType.Other:
                    ColorRectangle.Fill = new SolidColorBrush(Layout.ConvertStringToColor(EventTypesColors.Others));
                    break;
                default:
                    ColorRectangle.Fill = new SolidColorBrush(Layout.ConvertStringToColor(EventTypesColors.Others));
                    break;
            }
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NameStackPanel.Visibility = NameStackPanel.Visibility == System.Windows.Visibility.Visible ?
                System.Windows.Visibility.Collapsed :
                System.Windows.Visibility.Visible;
        }
    }
}