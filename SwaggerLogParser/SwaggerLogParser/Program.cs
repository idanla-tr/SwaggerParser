using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SwaggerLogParser
{
    class Program
    {
        const string FILENAME = "C:/a.json";
        static void Main(string[] args)
        {
            SetFileName(args);
            ReadLog();
            Console.WriteLine("Loading JSON file");
        }

        private static void SetFileName(string[] args)
        {
            throw new NotImplementedException();
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
