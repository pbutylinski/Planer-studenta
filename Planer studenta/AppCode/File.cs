using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;
using System.Windows;

namespace Planer_studenta
{
    static class EventsFile
    {
        internal static readonly string Filename = "student_data.xml";
        internal static List<SingleEvent> EventsCache = new List<SingleEvent>();
        internal static DateTime? LastModifiedCache = new DateTime();

        public static bool Save(List<SingleEvent> Events)
        {
            using (var Store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Store.DeleteFile(Filename); // sometimes it does have trash from old entries
                IsolatedStorageFileStream DataFile = Store.OpenFile(Filename, FileMode.OpenOrCreate);

                if (DataFile != null && DataFile.CanWrite)
                {
                    try
                    {
                        XmlSerializer Serializer = new XmlSerializer(typeof(FileStructure));
                        TextWriter Writer = new StreamWriter(DataFile);

                        FileStructure FileStruct = new FileStructure();
                        FileStruct.LastModified = DateTime.Now;
                        FileStruct.Events = Events;

                        EventsCache = Events;
                        LastModifiedCache = FileStruct.LastModified;

                        Serializer.Serialize(Writer, FileStruct);

                        Writer.Flush();
                        Writer.Close();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Kod błędu: 0x10", "Wystąpił błąd", MessageBoxButton.OK);
                    }
                }
                else
                {
                    DataFile.Flush();
                    DataFile.Close();
                    return false;
                }
            }

            return true;
        }

        public static bool Open(out List<SingleEvent> Events, out DateTime? LastModified)
        {
            if (EventsCache == new List<SingleEvent>() ||
                LastModifiedCache == new DateTime() ||
                EventsCache == null ||
                LastModifiedCache == null)
            {
                Events = null;
                LastModified = null;

                string XmlDebug = String.Empty;

                using (var Store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        if (Store.FileExists(Filename))
                        {
                            using (IsolatedStorageFileStream DataFile = new IsolatedStorageFileStream(Filename, FileMode.Open, Store))
                            {
                                using (StreamReader Reader = new StreamReader(DataFile))
                                {
                                    // Debug
                                    XmlDebug = Reader.ReadToEnd();
                                    Reader.BaseStream.Position = 0;
                                    Reader.DiscardBufferedData();

                                    XmlSerializer Serializer = new XmlSerializer(typeof(FileStructure));
                                    FileStructure FileStruct = Serializer.Deserialize(Reader) as FileStructure;

                                    Events = EventsCache = FileStruct.Events;
                                    LastModified = LastModifiedCache = FileStruct.LastModified;
                                }
                            }
                        }
                        else return false;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Kod błędu: 0x11", "Wystąpił błąd", MessageBoxButton.OK);

                        //string XmlDebugNumbered = String.Empty;
                        //int i = 1;

                        //using (StringReader reader = new StringReader(XmlDebug))
                        //{
                        //    string line;
                        //    while ((line = reader.ReadLine()) != null)
                        //    {
                        //        XmlDebugNumbered += i + " " + line + Environment.NewLine;
                        //        i++;
                        //    }
                        //}

                        //MessageBox.Show(XmlDebugNumbered);
                    }
                }
            }
            else
            {
                Events = EventsCache;
                LastModified = LastModifiedCache;
            }

            return true;
        }
    }

    public class FileStructure
    {
        public List<SingleEvent> Events { get; set; }
        public DateTime LastModified { get; set; }
    }
}
