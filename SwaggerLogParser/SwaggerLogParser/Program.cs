using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SwaggerLogParser
{
    class Program
    {
        static string FILENAME = "C:/a.json";
        static void Main(string[] args)
        {
            if (args.Length>0)
            {
                SetFileName(args);
            }
            Console.WriteLine("Loading JSON file...");
            ReadLog();
            Console.WriteLine("Writing results...");
            WriteResult();
        }

        private static void WriteResult()
        {
            string createText = "Hello and Welcome" + Environment.NewLine;
            File.WriteAllText("C:/Result.txt", createText);
        }

        private static void SetFileName(string[] args)
        {
            FILENAME = args[0];
        }

        private static void ReadLog()
        {
            using StreamReader r = new StreamReader(FILENAME);
            string json = r.ReadToEnd();
            List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);

        }

        public class Item
        {
            public string UserData;
            public string LogicLog;
            public string OpenPositionSettings;
        }
    }
}
