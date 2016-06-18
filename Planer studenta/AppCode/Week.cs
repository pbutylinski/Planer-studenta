using Microsoft.Phone.Shell;
using System;
using System.Globalization;
using System.Net;
using System.Windows;
using System.IO.IsolatedStorage;
using Planer_studenta;

namespace Scheduler
{
    static class Week
    {
        public static bool IsEven
        {
            get
            {
                // Check if JakiTydzien integration
                bool JakiTydzienIntegration = true;
                IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>(AppSettings.JakiTydzienIntegration, out JakiTydzienIntegration);

                if (JakiTydzienIntegration)
                {
                    try
                    {
                        JakiTydzien JakiTydzienData = new JakiTydzien();

                        // Not yet checked
                        if (JakiTydzienData == new JakiTydzien())
                        {
                            RefreshJakiTydzien(false);
                            return IsEvenByMath;
                        }
                        else
                        {
                            DateTime JakiTydzienExpires = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(JakiTydzienData.expires / 1000d)).ToLocalTime();

                            if (JakiTydzienExpires < DateTime.Now)
                            {
                                // JakiTydzien expired
                                RefreshJakiTydzien(false);
                                return IsEvenByMath;
                            }
                            else
                            {
                                // All fine
                                return JakiTydzienData.tydzien == "parzysty";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return IsEvenByMath;
                    }
                }
                else
                {
                    // No JakiTydzien integration
                    return IsEvenByMath;
                }
            }
        }

        public static bool IsEvenByMath
        {
            get
            {
                DateTime Date = DateTime.Now;
                int day = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(Date);
                int WeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(Date.AddDays(4 - (day == 0 ? 7 : day)), CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday);

                return WeekNumber % 2 == 0;
            }
        }

        public static void RefreshJakiTydzien(bool ShowMessageBox)
        {
            Uri SourcePath = new Uri("http://jakitydzien.pl/api/?type=json&api_key=" + Scheduler.Settings.JakiTydzienAPIKey);

            ProgressIndicator JakiTydzienProgressIndicator = new ProgressIndicator();

            JakiTydzienProgressIndicator.IsVisible = true;
            JakiTydzienProgressIndicator.IsIndeterminate = true;
            JakiTydzienProgressIndicator.Text = "Pobieranie danych z JakiTydzien.pl...";

            try
            {
                SystemTray.ProgressIndicator = JakiTydzienProgressIndicator;
            }
            catch (Exception) { }


            WebClient Client = new WebClient();
            Client.DownloadStringCompleted += (v, d) =>
            {
                JakiTydzien JakiTydzienData = new JakiTydzien();
                bool IsDownloaded = true;

                try
                {
                    JakiTydzienData = Newtonsoft.Json.JsonConvert.DeserializeObject<JakiTydzien>(d.Result);
                }
                catch (Exception)
                {
                    if (ShowMessageBox)
                    {
                        MessageBox.Show("Dane otrzymane z serwisu JakiTydzien.pl wydają się być niepoprawne" + Environment.NewLine + "Kod błędu: 0xA0", "Wystąpił błąd", MessageBoxButton.OK);
                    }

                    IsDownloaded = false;
                }

                if (IsDownloaded && !String.IsNullOrEmpty(JakiTydzienData.tydzien))
                {
                    // Show message
                    if (ShowMessageBox)
                    {
                        // Has to be wrapped, otherwise ProgressIndicator is still visible
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show(
                                String.IsNullOrEmpty(JakiTydzienData.details) ?
                                    "Po prostu tydzień " + JakiTydzienData.tydzien + ", bez udziwnień" :
                                    JakiTydzienData.details,
                                "Tydzień " + JakiTydzienData.tydzien,
                                MessageBoxButton.OK);
                        });
                    }

                    // Save data
                    System.IO.IsolatedStorage.IsolatedStorageSettings appSettings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;

                    if (appSettings.Contains(AppSettings.JakiTydzienData))
                    {
                        appSettings[AppSettings.JakiTydzienData] = JakiTydzienData;
                    }
                    else
                    {
                        appSettings.Add(AppSettings.JakiTydzienData, JakiTydzienData);
                    }
                }

                try
                {
                    JakiTydzienProgressIndicator = null;
                    SystemTray.ProgressIndicator = null;
                }
                catch (Exception) { }
            };

            try
            {
                Client.DownloadStringAsync(SourcePath);
            }
            catch (Exception)
            {
                if (ShowMessageBox)
                {
                    MessageBox.Show("Błąd podczas pobieraniu danych z serwisu JakiTydzien.pl" + Environment.NewLine + "Kod błędu: 0xA1", "Wystąpił błąd", MessageBoxButton.OK);
                }
            }

        }

        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is millisecods past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(javaTimeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }
    }
}
