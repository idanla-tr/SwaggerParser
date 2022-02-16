using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SwaggerLogParser
{
    class Program
    {
        const string FILENAME = "";
        static void Main(string[] args)
        {
            setFileName();
            Console.WriteLine("Hello World!");
        }

        private static void setFileName()
        {
            throw new NotImplementedException();
        }

        void ReadLog()
        {
            using (StreamReader r = new StreamReader(FILENAME))
            {
                string json = r.ReadToEnd();
                List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);
            }
        }

        public class Item
        {
            public string UserData;
            public string LogicLog;
            public string OpenPositionSettings;
        }
    }
}
