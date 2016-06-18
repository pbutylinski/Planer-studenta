using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Planer_studenta
{
    public partial class AddEditEvent : PhoneApplicationPage
    {
        SingleEvent SelectedEvent = new SingleEvent();

        bool NewlyLoaded = true;

        public AddEditEvent()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            bool IsOkay = true;
            var br = Environment.NewLine;

            string Message = String.Empty;

            if (String.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                IsOkay = false;
                Message += "Brak nazwy zajęć" + br;
            }

            if (String.IsNullOrWhiteSpace(ShortNameTextBox.Text))
            {
                IsOkay = false;
                Message += "Brak skróconej nazwy zajęć" + br;
            }

            if (StartTimePicker.Value > EndTimePicker.Value)
            {
                IsOkay = false;
                Message += "Data rozpoczęcia nie może być późniejsza od daty zakończenia" + br;
            }


            if (IsOkay)
            {
                List<SingleEvent> ExistingEvents;
                DateTime? LastModified;

                EventsFile.Open(out ExistingEvents, out LastModified);

                SingleEvent Event = SelectedEvent;

                Event.ID = SelectedEvent == new SingleEvent() ? Guid.NewGuid() : Event.ID;
                Event.Name = NameTextBox.Text;
                Event.ShortName = ShortNameTextBox.Text;
                Event.Location = LocationTextBox.Text;
                Event.Day = DayOfWeekConverter(DayOfWeekListPicker.SelectedIndex);
                Event.Occurence = OccurenceConverter(OccurenceListPicker.SelectedIndex);
                Event.StartTime = new EventTime(
                    (StartTimePicker.Value ?? new DateTime()).Hour,
                    (StartTimePicker.Value ?? new DateTime()).Minute);
                Event.EndTime = new EventTime(
                    (EndTimePicker.Value ?? new DateTime()).Hour,
                    (EndTimePicker.Value ?? new DateTime()).Minute);
                Event.Type = TypeConverter(TypeListPicker.SelectedIndex);
                Event.Lecturer = LecturerTextBox.Text;

                if (ExistingEvents == null)
                    ExistingEvents = new List<SingleEvent>();


                if (SelectedEvent != new SingleEvent())
                {
                    SingleEvent s = ExistingEvents.
                        Where(k => k.ID == SelectedEvent.ID).
                        FirstOrDefault();
                    ExistingEvents.Remove(s);
                }

                ExistingEvents.Add(Event);

                if (EventsFile.Save(ExistingEvents))
                {
                    MessageBox.Show("Zajęcia o nazwie \"" + Event.Name + "\" zostały zapisane", "Sukces!", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas zapisywania!");
                }

                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show(Message, "Nie można zapisać wydarzenia", MessageBoxButton.OK);
            }

        }

        EventType TypeConverter(int Input)
        {
            switch (Input)
            {
                case 0: return EventType.Lecture;
                case 2: return EventType.Laboratories;
                case 1: return EventType.Excercises;
                case 3: return EventType.Seminar;
                case 4:
                default: return EventType.Other;
            }
        }

        int TypeConverter (EventType Input) {
            switch (Input)
            {
                case EventType.Laboratories:
                    return 2;
                case EventType.Lecture:
                    return 0;
                case EventType.Excercises:
                    return 1;
                case EventType.Seminar:
                    return 3;
                case EventType.Other:
                default:
                    return 4;
            }
        }

        EventOccurence OccurenceConverter(int Input)
        {
            switch (Input)
            {
                case 0: return EventOccurence.Weekly;
                case 1: return EventOccurence.EvenWeek;
                case 2: return EventOccurence.OddWeek;
                default: return EventOccurence.Weekly;
            }
        }

        int OccurenceConverter(EventOccurence Input)
        {
            switch (Input)
            {
                case EventOccurence.EvenWeek:
                    return 1;
                case EventOccurence.OddWeek:
                    return 2;
                case EventOccurence.Weekly:
                default:
                    return 0;
            }
        }

        public static DayOfWeek DayOfWeekConverter(int Input)
        {
            switch (Input)
            {
                case 0: return DayOfWeek.Monday;
                case 1: return DayOfWeek.Tuesday;
                case 2: return DayOfWeek.Wednesday;
                case 3: return DayOfWeek.Thursday;
                case 4: return DayOfWeek.Friday;
                case 5: return DayOfWeek.Saturday;
                case 6: return DayOfWeek.Sunday;
                default: return DayOfWeek.Monday;
            }
        }

        int DayOfWeekConverter(DayOfWeek Input)
        {
            switch (Input)
            {
                case DayOfWeek.Monday:
                    return 0;
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                case DayOfWeek.Sunday:
                    return 6;
                default:
                    return 0;
            }
        }

        private void NameTextBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            string Text = NameTextBox.Text;
            if (Text.Length > 26)
            {
                Text = Text.Substring(0, 24);

                if (!Text.EndsWith(" "))
                    Text = Text + '.';
            }

            if (String.IsNullOrWhiteSpace(ShortNameTextBox.Text))
                ShortNameTextBox.Text = Text;
        }

        private void ContentPanel_Loaded(object sender, RoutedEventArgs e)
        {
            List<SingleEvent> Events = new List<SingleEvent>();
            DateTime? LastModified = new DateTime();
            EventsFile.Open(out Events, out LastModified);

            string g = String.Empty;
            NavigationContext.QueryString.TryGetValue("id", out g);

            if (!String.IsNullOrEmpty(g) && NewlyLoaded)
            {
                NewlyLoaded = false;

                Guid EventID = new Guid(g);

                SelectedEvent = Events.
                    Where(k => k.ID == EventID).
                    FirstOrDefault() ?? new SingleEvent();


                // load data
                TitleTextBlock.Text = "edycja";

                NameTextBox.Text = SelectedEvent.Name;
                ShortNameTextBox.Text = SelectedEvent.ShortName;
                LocationTextBox.Text = SelectedEvent.Location;
                DayOfWeekListPicker.SelectedIndex = DayOfWeekConverter(SelectedEvent.Day);
                OccurenceListPicker.SelectedIndex = OccurenceConverter(SelectedEvent.Occurence);
                StartTimePicker.Value = new DateTime(1, 1, 1, SelectedEvent.StartTime.Hour, SelectedEvent.StartTime.Minute, 0);
                EndTimePicker.Value = new DateTime(1, 1, 1, SelectedEvent.EndTime.Hour, SelectedEvent.EndTime.Minute, 0);
                TypeListPicker.SelectedIndex = TypeConverter(SelectedEvent.Type);
                LecturerTextBox.Text = SelectedEvent.Lecturer;
            }
        }
    }
}