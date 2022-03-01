using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SwaggerLogParser
{
    class Program
    {
        static string FILENAME = "C:/a.json";

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                SetFileName(args);
            }
            Console.WriteLine("Loading JSON file...");
            ReadLog();
            Console.WriteLine("Finish writing results!");
            Process.Start("notepad.exe", "Result.txt");
            Environment.Exit(0);
        }

        private static void WriteResult(Root myDeserializedClass)
        {
            string createText = String.Join(Environment.NewLine+ Environment.NewLine, myDeserializedClass.LogicLog);
            File.WriteAllText("Result.txt", createText);
        }

        private static void SetFileName(string[] args)
        {
            FILENAME = args[0];
        }

        private static void ReadLog()
        {
            using StreamReader r = new StreamReader(FILENAME);
            string json = r.ReadToEnd();
            Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(json);
            WriteResult(myDeserializedClass);
        }

        public class LeverageSettings
        {
            public int GCID { get; set; }
            public int InstrumentID { get; set; }
            public int InstrumentType { get; set; }
            public bool IsNonLeveragedBuyAllowed { get; set; }
            public List<int> PossibleLeverages { get; set; }
            public List<object> PotentialDisplayLeverages { get; set; }
            public int DefaultValue { get; set; }
            public int MaxLeverage { get; set; }
            public int MinLevergae { get; set; }
        }

        public class Leveraged
        {
            public int Buy { get; set; }
            public int Sell { get; set; }
        }

        public class NonLeveraged
        {
            public int Buy { get; set; }
            public int Sell { get; set; }
        }

        public class MaxStopLossPercentageMatrix
        {
            public Leveraged Leveraged { get; set; }
            public NonLeveraged NonLeveraged { get; set; }
        }

        public class MaxStopLossPercentageSettings
        {
            public int GCID { get; set; }
            public int InstrumentID { get; set; }
            public int InstrumentType { get; set; }
            public MaxStopLossPercentageMatrix MaxStopLossPercentageMatrix { get; set; }
            public bool Leverage1MaintenanceMarginUsageActivated { get; set; }
        }

        public class MaxTakeProfitPercentageSettings
        {
            public int GCID { get; set; }
            public int InstrumentID { get; set; }
            public int InstrumentType { get; set; }
            public int MaxTakeProfitPercentage { get; set; }
        }

        public class MinPositionAmountSettings
        {
            public int GCID { get; set; }
            public int InstrumentID { get; set; }
            public int InstrumentType { get; set; }
            public int MinPositionAmount { get; set; }
            public int MinPositionAmountAbsolute { get; set; }
            public int MinPositionAmountAbsoluteDiscounted { get; set; }
        }

        public class AllowManualTradingSettings
        {
            public int InstrumentID { get; set; }
            public int InstrumentType { get; set; }
            public bool AllowManualTrading { get; set; }
        }

        public class MaxAllowedOpenedPositionsCountSettings
        {
            public int GCID { get; set; }
            public int MaxAllowedOpenedPositionsCount { get; set; }
        }

        public class RestrictedManualOpenSettings
        {
            public int InstrumentID { get; set; }
            public int InstrumentType { get; set; }
            public bool RestrictedManualOpen { get; set; }
        }

        public class MaxLeverageForTradeTypesSetting
        {
            public int InstrumentID { get; set; }
            public int InstrumentType { get; set; }
            public string StockExcange { get; set; }
            public bool IsBuy { get; set; }
            public int RealTradeMaxLeverage { get; set; }
            public int MarginTradeMaxLeverage { get; set; }
            public int SettledMaxLeverage { get; set; }
        }

        public class W8BenStatusSettings
        {
            public int Gcid { get; set; }
            public int W8BenStatus { get; set; }
        }

        public class RequiresW8BenSettings
        {
            public int Gcid { get; set; }
            public int InstrumentID { get; set; }
            public string StockExcange { get; set; }
            public int InstrumentType { get; set; }
            public bool RequiresW8Ben { get; set; }
        }

        public class W8BenExecutionValidationSettings
        {
            public bool IsValidForExecution { get; set; }
            public W8BenStatusSettings W8BenStatusSettings { get; set; }
            public RequiresW8BenSettings RequiresW8BenSettings { get; set; }
        }

        public class MinStopLossPercentageSettings
        {
            public int GCID { get; set; }
            public int InstrumentID { get; set; }
            public int MinStopLossPercentage { get; set; }
        }

        public class MinTakeProfitPercentageSettings
        {
            public int GCID { get; set; }
            public int InstrumentID { get; set; }
            public int MinTakeProfitPercentage { get; set; }
        }

        public class AllowTrailingStopLossSettings
        {
            public int GCID { get; set; }
            public int InstrumentID { get; set; }
            public bool AllowTrailingStopLoss { get; set; }
        }

        public class AllowBuySellSettings
        {
            public int Gcid { get; set; }
            public bool IsBuyAllowed { get; set; }
            public bool IsSellAllowed { get; set; }
        }

        public class AllowDiscountedRatesSettings
        {
            public int InstrumentType { get; set; }
            public bool AllowDiscountedRates { get; set; }
        }

        public class VisibleInternallyOnlySettings
        {
            public int GCID { get; set; }
            public bool IsVisibleInternallyOnly { get; set; }
        }

        public class MaxNopSettings
        {
            public int InstrumentID { get; set; }
            public int InstrumentTypeID { get; set; }
            public bool IsBuy { get; set; }
            public int NopLimit { get; set; }
            public int Nop { get; set; }
            public bool MaxNopReached { get; set; }
        }

        public class OpenPositionSettings
        {
            public int Gcid { get; set; }
            public int InstrumentTypeID { get; set; }
            public bool IsBuy { get; set; }
            public int Leverage { get; set; }
            public int MaxStopLossPercentage { get; set; }
            public LeverageSettings LeverageSettings { get; set; }
            public MaxStopLossPercentageSettings MaxStopLossPercentageSettings { get; set; }
            public MaxTakeProfitPercentageSettings MaxTakeProfitPercentageSettings { get; set; }
            public MinPositionAmountSettings MinPositionAmountSettings { get; set; }
            public AllowManualTradingSettings AllowManualTradingSettings { get; set; }
            public MaxAllowedOpenedPositionsCountSettings MaxAllowedOpenedPositionsCountSettings { get; set; }
            public RestrictedManualOpenSettings RestrictedManualOpenSettings { get; set; }
            public MaxLeverageForTradeTypesSetting MaxLeverageForTradeTypesSetting { get; set; }
            public W8BenExecutionValidationSettings W8BenExecutionValidationSettings { get; set; }
            public MinStopLossPercentageSettings MinStopLossPercentageSettings { get; set; }
            public MinTakeProfitPercentageSettings MinTakeProfitPercentageSettings { get; set; }
            public AllowTrailingStopLossSettings AllowTrailingStopLossSettings { get; set; }
            public AllowBuySellSettings AllowBuySellSettings { get; set; }
            public AllowDiscountedRatesSettings AllowDiscountedRatesSettings { get; set; }
            public bool BlockedFormCFD { get; set; }
            public bool BlockedByCompliance { get; set; }
            public VisibleInternallyOnlySettings VisibleInternallyOnlySettings { get; set; }
            public MaxNopSettings MaxNopSettings { get; set; }
            public bool IsNonRealAsTRS { get; set; }
        }

        public class Root
        {
            public OpenPositionSettings OpenPositionSettings { get; set; }
            public List<string> LogicLog { get; set; }
            public string RequestGuId { get; set; }
            public List<string> LatencyTicks { get; set; }
            public string UserData { get; set; }
        }
    }
}
