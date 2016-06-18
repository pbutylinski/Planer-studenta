using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_studenta
{
    public enum EventOccurence
    {
        EvenWeek = 1,
        OddWeek = 2,
        Weekly = 0
    }

    public enum EventType
    {
        Laboratories,
        Lecture,
        Excercises,
        Seminar,
        Other
    }

    public enum EventSide
    {
        Left,
        Right,
        Both
    }

    public enum DayOfWeek
    {
        Monday = 0,
        Tuesday = 1,
        Wednesday = 2,
        Thursday = 3,
        Friday = 4,
        Saturday = 5,
        Sunday = 6
    }

    public class EventTime
    {
        public int Hour { get; set; }
        public int Minute { get; set; }

        public EventTime()
        {
            Hour = 0;
            Minute = 0;
        }

        public EventTime(int hour, int minute)
        {
            Hour = hour;
            Minute = minute;
        }

        public DateTime GetTime()
        {
            return new DateTime(0, 0, 0, Hour, Minute, 0);
        }
    }

    public class EventTypesColors
    {
        public static readonly string Laboratories = "#F0A30A";
        public static readonly string Lectures = "#0050EF";
        public static readonly string Excercises = "#60A917";
        public static readonly string Seminars = "#825A2C";
        public static readonly string Others = "#647687";
    }

    public class SingleEvent
    {
        public Guid ID { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Location { get; set; }

        public EventTime StartTime { get; set; }
        public EventTime EndTime { get; set; }
        public DayOfWeek Day { get; set; }
        public EventOccurence Occurence { get; set; }

        public EventType Type { get; set; }
        public EventSide Side { get; set; }

        public string Lecturer { get; set; }
    }

    public static class AppSettings
    {
        public static readonly string HeigthMultiplier = "HeigthMultiplier";

        public static readonly string DayStart = "DayStart";

        public static readonly string DayVisibilityMonday = "DayVisibilityMo";
        public static readonly string DayVisibilityTuesday = "DayVisibilityTu";
        public static readonly string DayVisibilityWednesday = "DayVisibilityWe";
        public static readonly string DayVisibilityThursday = "DayVisibilityTh";
        public static readonly string DayVisibilityFriday = "DayVisibilityFr";
        public static readonly string DayVisibilitySaturday = "DayVisibilitySa";
        public static readonly string DayVisibilitySunday = "DayVisibilitySu";

        public static readonly string FirstRun = "FirstRun";

        public static readonly string JakiTydzienIntegration = "JakiTydzienIntegration";
        public static readonly string JakiTydzienData = "JakiTydzienData";

        public static readonly string AutomaticDayStart = "AutoDayStart";
    }
}
