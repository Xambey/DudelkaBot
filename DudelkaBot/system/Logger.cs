using System;
using System.IO;
using System.Text;
using System.Threading;

namespace DudelkaBot.system
{
    internal static class Logger
    {
        static readonly int timeHours = 24; 
        static string path = $"./logs/log{DateTime.Now.ToString().Replace(':','.')}.txt";
        static FileStream stream = File.Open(path, FileMode.Append);
        static Timer timerLogFile = new Timer(CreateNewLogFile, null, timeHours * 60 * 60000, timeHours * 60 * 60000);

        public static void Write(string message)
        {
            if(stream == null)
                stream = File.Open(path, FileMode.Append);
            var buf = Encoding.UTF8.GetBytes(DateTime.Now.ToString() + ": " + message + "\n");
            stream.Write(buf, 0, buf.Length);
            stream.Flush();
        }
        static void CreateNewLogFile(object obj)
        {
            path = $"./logs/log{DateTime.Now.ToString().Replace(':', '.')}.txt";
            stream = File.Open(path, FileMode.Append);
        }
    }
}
