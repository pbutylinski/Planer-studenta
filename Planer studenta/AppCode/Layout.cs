using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Navigation;
using Scheduler;
using System.IO.IsolatedStorage;

namespace Planer_studenta
{
    static class Layout
    {
        private static readonly int Margin = 6;
        private static readonly int Width = 430;

        private static List<SingleEvent> EventsCache { get; set; }

        public static void LoadEventElements(
            MainPage Page,
            ref Grid MonPanel,
            ref Grid TuePanel,
            ref Grid WedPanel,
            ref Grid ThuPanel,
            ref Grid FriPanel,
            ref Grid SatPanel,
            ref Grid SunPanel)
        {
            List<SingleEvent> Events = new List<SingleEvent>();
            DateTime? LastModified;

            if (EventsFile.Open(out Events, out LastModified))
            {
                EventsCache = Events;

                LoadDayEvents(ref MonPanel, DayOfWeek.Monday, Events, Page);
                LoadDayEvents(ref TuePanel, DayOfWeek.Tuesday, Events, Page);
                LoadDayEvents(ref WedPanel, DayOfWeek.Wednesday, Events, Page);
                LoadDayEvents(ref ThuPanel, DayOfWeek.Thursday, Events, Page);
                LoadDayEvents(ref FriPanel, DayOfWeek.Friday, Events, Page);
                LoadDayEvents(ref SatPanel, DayOfWeek.Saturday, Events, Page);
                LoadDayEvents(ref SunPanel, DayOfWeek.Sunday, Events, Page);
            }
        }

        public static void LoadDayEvents(ref Grid Panel, DayOfWeek Day, List<SingleEvent> Events, MainPage Page)
        {
            Panel.Children.Clear();

            if (Events != null)
            {
                var SortedEvents = Events.
                    Where(k => k.Day == Day).
                    OrderBy(k => k.Occurence).
                    OrderBy(k => 100 * k.StartTime.Hour + k.StartTime.Minute).
                    ToArray();

                Panel.HorizontalAlignment = HorizontalAlignment.Left;

                foreach (var item in SortedEvents)
                {
                    Panel.Children.Add(CreateEventElement(item, Page));
                }
            }
        }

        public static Grid CreateEventElement(SingleEvent Event, MainPage Page)
        {
            double HeigthMultiplier = 1;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<double>(AppSettings.HeigthMultiplier, out HeigthMultiplier);

            DateTime DayStart = new DateTime();
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<DateTime>(AppSettings.DayStart, out DayStart);


            double CalculatedHeigth = CalculateHeigth(Event) * HeigthMultiplier;

            Grid EventElementGrid = new Grid();
            EventElementGrid.VerticalAlignment = VerticalAlignment.Top;

            // Auto day start
            double DayStartCorrection = 0;

            bool AutoDayStart = false;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.AutomaticDayStart, out AutoDayStart);

            if (AutoDayStart)
            {
                // Get first event
                EventTime FirstEventTime = EventsCache.
                    Where(k => k.Day == Event.Day).
                    OrderBy(k => k.StartTime.Hour * 100 + k.StartTime.Minute).
                    Select(k => k.StartTime).
                    FirstOrDefault();

                DayStartCorrection = FirstEventTime.Hour * 60 + FirstEventTime.Minute;
            }
            else
            {
                DayStartCorrection = DayStart.Hour * 60 + DayStart.Minute;
            }

            double MarginTop = HeigthMultiplier * ((Event.StartTime.Hour * 60 + Event.StartTime.Minute) - DayStartCorrection);

            EventElementGrid.Margin = new System.Windows.Thickness(0, MarginTop, 0, 0);
            EventElementGrid.Height = CalculatedHeigth;

            // Make opaque if not the week we want
            if ((Week.IsEven && Event.Occurence == EventOccurence.OddWeek) ||
                (!Week.IsEven && Event.Occurence == EventOccurence.EvenWeek))
                EventElementGrid.Opacity = 0.15d;

            // Polygon
            Polygon Poly = new Polygon();
            Poly.Stretch = Stretch.Uniform;

            #region Polygon shape
            PointCollection PolyPoints = new PointCollection();

            switch (Event.Occurence)
            {
                case EventOccurence.OddWeek:
                    PolyPoints.Add(new System.Windows.Point(0, 0));
                    PolyPoints.Add(new System.Windows.Point(Width, 0));
                    PolyPoints.Add(new System.Windows.Point(0, (double)CalculatedHeigth));
                    break;
                case EventOccurence.EvenWeek:
                    PolyPoints.Add(new System.Windows.Point(0, (double)CalculatedHeigth));
                    PolyPoints.Add(new System.Windows.Point(Width, 0));
                    PolyPoints.Add(new System.Windows.Point(Width, (double)CalculatedHeigth));
                    break;
                case EventOccurence.Weekly:
                    PolyPoints.Add(new System.Windows.Point(0, 0));
                    PolyPoints.Add(new System.Windows.Point(0, (double)CalculatedHeigth));
                    PolyPoints.Add(new System.Windows.Point(Width, (double)CalculatedHeigth));
                    PolyPoints.Add(new System.Windows.Point(Width, 0));
                    break;
                default:
                    break;
            }

            Poly.Points = PolyPoints;
            #endregion

            #region Polygon color
            switch (Event.Type)
            {
                case EventType.Laboratories:
                    Poly.Fill = new SolidColorBrush(ConvertStringToColor(EventTypesColors.Laboratories));
                    break;
                case EventType.Lecture:
                    Poly.Fill = new SolidColorBrush(ConvertStringToColor(EventTypesColors.Lectures));
                    break;
                case EventType.Excercises:
                    Poly.Fill = new SolidColorBrush(ConvertStringToColor(EventTypesColors.Excercises));
                    break;
                case EventType.Seminar:
                    Poly.Fill = new SolidColorBrush(ConvertStringToColor(EventTypesColors.Seminars));
                    break;
                case EventType.Other:
                    Poly.Fill = new SolidColorBrush(ConvertStringToColor(EventTypesColors.Others));
                    break;
                default:
                    Poly.Fill = new SolidColorBrush(ConvertStringToColor(EventTypesColors.Others));
                    break;
            }
            #endregion

            // Add text
            StackPanel EventStackPanel = new StackPanel();

            TextBlock EventName = new TextBlock();
            EventName.FontSize = 25;
            EventName.Text = Event.ShortName;
            EventName.Padding = CalculateTextPadding(Event.Occurence, false);

            string Time =
                Event.StartTime.Hour + ":" + Event.StartTime.Minute.ToString("00") + " - " +
                Event.EndTime.Hour + ":" + Event.EndTime.Minute.ToString("00");

            TextBlock EventPlace = new TextBlock();
            EventPlace.Text = HeigthMultiplier == 1 ?
                "🏢 " + Event.Location :
                "⏰ " + Time + Environment.NewLine + "🏢 " + Event.Location;
            EventPlace.Foreground = new SolidColorBrush(ConvertStringToColor("#FFC8C8C8"));
            EventPlace.Padding = CalculateTextPadding(Event.Occurence, true);

            if (Event.Occurence == EventOccurence.EvenWeek)
            {
                EventStackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                EventStackPanel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

                EventName.TextAlignment = System.Windows.TextAlignment.Right;
                EventPlace.TextAlignment = System.Windows.TextAlignment.Right;

                EventPlace.Text = HeigthMultiplier == 1 ?
                    Event.Location + " 🏢" :
                    Time + " ⏰" + Environment.NewLine + Event.Location + " 🏢";
            }

            // Adding actions
            EventName.Tap += (s, e) => ShowEventDetailsDialog(Event.ID, Page);
            EventPlace.Tap += (s, e) => ShowEventDetailsDialog(Event.ID, Page);
            Poly.Tap += (s, e) => ShowEventDetailsDialog(Event.ID, Page);

            // Odd and Even week are overlapping, therefore after clicking only the top one is active
            double MarginLeft = Event.Occurence == EventOccurence.OddWeek ? Width / 2 : 0;

            Rectangle TapRect = new Rectangle();
            TapRect.Width = Width / 2;
            TapRect.Height = CalculatedHeigth;
            TapRect.Margin = new Thickness(MarginLeft, MarginTop, 0, 0);
            TapRect.Tap += (s, e) => ShowEventDetailsDialog(Event.ID, Page);

            // Packing everything up
            if (Event.Occurence == EventOccurence.EvenWeek)
            {
                EventStackPanel.Children.Add(EventPlace);
                EventStackPanel.Children.Add(EventName);
            }
            else
            {
                EventStackPanel.Children.Add(EventName);
                EventStackPanel.Children.Add(EventPlace);
            }

            EventElementGrid.Children.Add(Poly);
            EventElementGrid.Children.Add(EventStackPanel);

            if (Event.Occurence != EventOccurence.Weekly)
            {
                EventElementGrid.Children.Add(TapRect);
            }


            return EventElementGrid;
        }

        static void ShowEventDetailsDialog(Guid EventID, MainPage Page)
        {
            Page.NavigationService.Navigate(new Uri("/Details.xaml?id=" + EventID.ToString(), UriKind.Relative));
        }

        private static double CalculateHeigth(SingleEvent Event)
        {
            int Start = Event.StartTime.Hour * 60 + Event.StartTime.Minute;
            int End = Event.EndTime.Hour * 60 + Event.EndTime.Minute;

            if (Start > End)
                throw new ArgumentException();

            return (End - Start);// +HeigthConstant;
        }

        private static System.Windows.Thickness CalculateTextPadding(EventOccurence Occurence, bool IsLocation)
        {
            int a = Margin;
            if (Occurence == EventOccurence.EvenWeek)
                a = 0;

            int b = 0;

            int c = 0;
            if (Occurence == EventOccurence.EvenWeek)
                c = Margin;

            int d = 0;
            if (IsLocation && Occurence != EventOccurence.EvenWeek ||
                !IsLocation && Occurence == EventOccurence.EvenWeek)
                d = Margin;

            return new System.Windows.Thickness(a, b, c, d);
        }

        public static Color ConvertStringToColor(String hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
