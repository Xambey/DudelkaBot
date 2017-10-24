using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DudelkaBot.Logging
{
    internal static class Logger
    {
        #region Constants
            static readonly int timeHours = 24;
            static readonly int countElementsForWriteChannels = 100;
            static readonly int countElementsForWriteCommon = 100;
        #endregion

        #region Fields
        static string commonPath = $"./logs/log{DateTime.Now.ToString().Replace(':', '.').Replace(' ', '_') }.log";
        static string channelPath = "./logs/channels";
        static string CommonPath { get => commonPath; set => commonPath = value; }
        static bool ActiveLog = true; 
        #endregion

        #region References
        static Timer timerLogFile = new Timer(UpdateLogFileName, null, timeHours * 60 * 60000, timeHours * 60 * 60000);
        static ConcurrentQueue<string> CommonContainer = new ConcurrentQueue<string>();
        static ConcurrentDictionary<string, ConcurrentQueue<string>> channelslog = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
        static Dictionary<string, string> channelPaths = new Dictionary<string, string>(); 
        #endregion

        public static void UpdateChannelPaths(string channelname)
        {
            if (channelPaths == null)
                channelPaths = new Dictionary<string, string>();
            if (!channelPaths.ContainsKey(channelname))
                channelPaths.Add(channelname, channelPath + $"/{channelname}/log{ DateTime.Now.ToString().Replace(':', '.').Replace(' ', '_') }.log");
            else
                channelPaths[channelname] = channelPath + $"/{channelname}/log{ DateTime.Now.ToString().Replace(':', '.').Replace(' ', '_') }.log";
            Thread.Sleep(2000);
        }

        public static void SaveChannelLog(string channelname)
        {
            try
            {
                if (!channelslog.ContainsKey(channelname) || channelslog[channelname].IsEmpty)
                    return;
                if (!File.Exists(channelPaths[channelname]))
                    File.Create(channelPaths[channelname]);
                using (var stream = new StreamWriter(File.Open(channelPaths[channelname], FileMode.Append), Encoding.Unicode))
                {
                    while (!channelslog[channelname].IsEmpty)
                    {
                        string mes;
                        if (!channelslog[channelname].TryDequeue(out mes))
                        {
                            throw new InvalidOperationException("Ошибка! Не удалось удалить элемент из очереди");
                        }
                        stream.Write(mes);
                    }

                    stream.Flush();
                }
            }
            catch(IOException)
            {
                return;
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("74 " + ex.Message + ex.Data + ex.StackTrace);
                if (ex.InnerException != null)
                    ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.Data + ex.InnerException.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static void SaveCommonLog()
        {
            if (CommonContainer.IsEmpty)
                return;
            using (var stream = new StreamWriter(File.Open(CommonPath, FileMode.Append), Encoding.Unicode))
            {
                while (!CommonContainer.IsEmpty)
                {
                    string mes;
                    if (!CommonContainer.TryDequeue(out mes))
                    {
                        throw new InvalidOperationException("Ошибка! Не удалось удалить элемент из очереди");
                    }
                    stream.Write(mes);
                }

                stream.Flush();
            }
        }

        public static void ShowLineCommonMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
            WriteLineMessage(message);
        }

        public static void ShowCommonMessage(string message)
        {
            Console.Write(" " + message);
            WriteMessage(message);
        }

        public static void ShowLineChannelMessage(string username, string message, string channelname)
        {
            Console.WriteLine($"{DateTime.Now} {username}: {message}");
            WriteLineMessage(username, message, channelname);
        }

        public static void ShowChannelMessage(string username, string message, string channelname)
        {
            Console.Write(" " + message);
            WriteMessage(message, channelname);
        }

        static void UpdateCommonLog(string message)
        {
            WriteLineMessage(message);
        }

        static bool TryCreateSUBDirectory(string path, string foldername)
        {
            try
            {
                if (!Directory.Exists(path + $"/{foldername}"))
                    Directory.CreateDirectory(channelPath + $"/{foldername}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void WriteLineMessage(string username,string message, string channelname)
        {
            try
            { 
                if (!ActiveLog || string.IsNullOrEmpty(message))
                    return;
                if (channelslog == null)
                    channelslog = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
                if (!channelslog.ContainsKey(channelname))
                    channelslog.GetOrAdd(channelname, new ConcurrentQueue<string>());
                if (!channelPaths.ContainsKey(channelname))
                    UpdateChannelPaths(channelname);
                lock (channelslog[channelname])
                {
                    if (channelslog[channelname].Count >= countElementsForWriteChannels)
                    {
                        channelslog[channelname].Enqueue(DateTime.Now.ToString() + $" {username}: " + message + "\n");
                        if (!TryCreateSUBDirectory(channelPath, channelname))
                            throw new InvalidOperationException("Директория не создана!");

                        using (var stream = new StreamWriter(File.Open(channelPaths[channelname], FileMode.Append),Encoding.Unicode))
                        {
                            while (!channelslog[channelname].IsEmpty)
                            {
                                string mes;
                                if (!channelslog[channelname].TryDequeue(out mes))
                                {
                                    throw new InvalidOperationException("Ошибка! Не удалось удалить элемент из очереди");
                                }
                                stream.Write(mes);
                            }
                            stream.Flush();
                        }
                        GC.Collect();
                    }
                    else
                    {
                        channelslog[channelname].Enqueue(DateTime.Now.ToString() + $" {username}: " + message + "\n");
                    }
                }

            }
            catch (IOException)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage(ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (CommonContainer == null)
                    CommonContainer = new ConcurrentQueue<string>();
                if (channelslog == null)
                    channelslog = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
                return;
            }
        }
        /// <summary>
        /// write in common log
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLineMessage(string message)
        {
            try
            {
                if (!ActiveLog || string.IsNullOrEmpty(message))
                    return;
                lock (CommonContainer)
                {
                    if (CommonContainer == null)
                        CommonContainer = new ConcurrentQueue<string>();
                    if (CommonContainer.Count >= countElementsForWriteCommon)
                    {
                        CommonContainer.Enqueue(DateTime.Now.ToString() + ": " + message + "\n");
                        using (var stream = new StreamWriter(File.Open(CommonPath, FileMode.Append),Encoding.Unicode))
                        {
                            while (!CommonContainer.IsEmpty)
                            {
                                string mes;
                                if (!CommonContainer.TryDequeue(out mes))
                                {
                                    throw new InvalidOperationException("Ошибка! Не удалось удалить элемент из очереди");
                                }
                                stream.Write(mes);
                            }

                            stream.Flush();
                        }
                        GC.Collect();
                    }
                    else
                    {
                        CommonContainer.Enqueue(DateTime.Now.ToString() + ": " + message + "\n");
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("244 " + ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (CommonContainer == null)
                    CommonContainer = new ConcurrentQueue<string>();
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("252 " + ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
                WriteLineMessage(ex.Message);
                Thread.Sleep(10000);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("260 " + ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static void WriteMessage(string message, string channelname)
        {
            try
            {
                if (!ActiveLog || string.IsNullOrEmpty(message))
                    return;
                if (channelslog == null)
                    channelslog = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
                if (!channelslog.ContainsKey(channelname))
                    channelslog.GetOrAdd(channelname, new ConcurrentQueue<string>());
                if (!channelPaths.ContainsKey(channelname))
                    UpdateChannelPaths(channelname);

                lock (channelslog[channelname])
                {
                    if (channelslog[channelname].Count >= countElementsForWriteChannels)
                    {
                        channelslog[channelname].Enqueue(" " + message);
                        if (!TryCreateSUBDirectory(channelPath, channelname))
                            throw new InvalidOperationException("Директория не создана!");

                        using (var stream = new StreamWriter(File.Open(channelPaths[channelname], FileMode.Append), Encoding.Unicode))
                        {
                            while (!channelslog[channelname].IsEmpty)
                            {
                                string mes;
                                if (!channelslog[channelname].TryDequeue(out mes))
                                {
                                    throw new InvalidOperationException("Ошибка! Не удалось удалить элемент из очереди");
                                }
                                stream.Write(mes);
                            }

                            stream.Flush();
                        }
                        GC.Collect();
                    }
                    else
                    {
                        channelslog[channelname].Enqueue(" " + message);
                    }
                }

            }
            catch (NullReferenceException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("312 " + ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (CommonContainer == null)
                    CommonContainer = new ConcurrentQueue<string>();
                if (channelslog == null)
                    channelslog = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("322 " + ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("328 " + ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /// <summary>
        /// write in common log
        /// </summary>
        /// <param name="message"></param>
        public static void WriteMessage(string message)
        {
            try
            {
                if (!ActiveLog || string.IsNullOrEmpty(message))
                    return;
                lock (CommonContainer)
                {
                    if (CommonContainer == null)
                        CommonContainer = new ConcurrentQueue<string>();
                    if (CommonContainer.Count >= countElementsForWriteCommon)
                    {
                        CommonContainer.Enqueue(" " + message);
                        using (var stream = new StreamWriter(File.Open(CommonPath, FileMode.Append),Encoding.Unicode))
                        {
                            while (!CommonContainer.IsEmpty)
                            {
                                string mes;
                                if (!CommonContainer.TryDequeue(out mes))
                                {
                                    throw new InvalidOperationException("Ошибка! Не удалось удалить элемент из очереди");
                                }
                                stream.Write(mes);
                            }

                            stream.Flush();
                        }
                        GC.Collect();
                    }
                    else
                    {
                        CommonContainer.Enqueue(" " + message);
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("374 " + ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (CommonContainer == null)
                    CommonContainer = new ConcurrentQueue<string>();
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("382 " + ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
                WriteLineMessage(ex.Message);
                Thread.Sleep(10000);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ShowLineCommonMessage("390 " + ex.Source + " " + ex.Message + " " + ex.Data);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        static void UpdateLogFileName(object obj)
        {
            lock(CommonPath)
                CommonPath = $"./logs/log{DateTime.Now.ToString().Replace(':', '.').Replace(' ', '_') }.log";
        }

        public static void StopWrite()
        {
            ActiveLog = false;
        }

        public static void StartWrite()
        {
            ActiveLog = true;
        }
    }
}
