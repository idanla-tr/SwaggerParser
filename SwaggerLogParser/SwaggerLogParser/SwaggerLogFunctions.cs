namespace eToro.Trading.TradingSettingsAPI.DomainLogicServices
{
    public static class Util
    {
        public delegate bool TryParseHandler<T>(string value, out T result);

        public static bool TryParse<T>(string value, TryParseHandler<T> handler, out T result) where T : struct
        {
            result = default(T);
            return !string.IsNullOrEmpty(value) && handler(value, out result);
        }

        private static bool TryExtractSelectedValue<TResult>(IDictionary<string, SettingsItem> dictionary, string resourceName, TryParseHandler<TResult> handler, out TResult selectedValue, out string processLog) where TResult : struct
        {
            selectedValue = default(TResult);
            //1. try to extract the the setting item 
            if (!dictionary.TryGetValue(resourceName, out var setting))
            {
                processLog = $"resource: '{resourceName}' was not found in  resource To Settings Items Dictionary";
                return false;
            }
            //2. try to parse
            if (!TryParse(setting.SelectedValue, handler, out selectedValue))
            {
                string errStr =
                    $"Failed to parse eToroSettings resource '{resourceName}' selected value: '{setting.SelectedValue}' as {typeof(TResult).Name}";
                processLog = errStr;
                Logger.WriteLog(ELogLevel.WARN, errStr);
                return false;
            }

            processLog = $"Resource '{resourceName}' extracted and parsed Selected value: {selectedValue.ToString()}";
            return true;

        }

        public static bool TryExtractSelectedValue(IDictionary<string, SettingsItem> dictionary, string resourceName, out int selectedValue, out string processLog)
        {
            return TryExtractSelectedValue(dictionary, resourceName, int.TryParse, out selectedValue, out processLog);
        }

        public static bool TryExtractSelectedValue(IDictionary<string, SettingsItem> dictionary, string resourceName, out bool selectedValue, out string processLog)
        {
            return TryExtractSelectedValue(dictionary, resourceName, bool.TryParse, out selectedValue, out processLog);

        }

        public static bool TryExtractSelectedValue(IDictionary<string, SettingsItem> dictionary, string resourceName, out decimal selectedValue, out string processLog)
        {
            return TryExtractSelectedValue(dictionary, resourceName, decimal.TryParse, out selectedValue, out processLog);

        }
        public static decimal? ExtractDecimalSetting(
            IDictionary<string, SettingsItem> settingsItemsDictionary,
            string resourceName,
            StringBuilder processLog)
        {
            var settingHasValue = Util.TryExtractSelectedValue(settingsItemsDictionary, resourceName,
                out decimal settingValue, out var extractProcessLog);

            AppendExtractProcessLog(processLog, extractProcessLog);

            return settingHasValue ? settingValue : new decimal?();
        }

        public static bool? ExtractBoolSetting(
            IDictionary<string, SettingsItem> settingsItemsDictionary,
            string resourceName,
            StringBuilder processLog)
        {
            var settingHasValue = Util.TryExtractSelectedValue(settingsItemsDictionary, resourceName,
                out bool settingValue, out var extractProcessLog);

            AppendExtractProcessLog(processLog, extractProcessLog);

            return settingHasValue ? settingValue : new bool?();
        }

        private static void AppendExtractProcessLog(StringBuilder processLog, string extractProcessLog)
        {
            processLog.AppendLine(extractProcessLog);
        }

        public static T ExtractObject<T>(Dictionary<string, SettingsItem> settingsItems, string resourceName, string clientRequestId, out string log)
        {
            var obj = default(T);
            var sb = new StringBuilder();
            if (TryExtractSelectedValue(settingsItems, resourceName, out string jsonValue, out string processLog))
            {
                sb.AppendLine(processLog);
                try
                {
                    obj = JsonConvert.DeserializeObject<T>(jsonValue);

                    if (obj == null)
                    {
                        sb.AppendLine(
                            $"failed to Deserialize JSON Selected value from resource: {resourceName}");
                        Logger.WriteLog(ELogLevel.ERROR,
                            string.Format(
                                "Util.TryExtractObject: JSON object Deserialization FAILURE! {1}, Resource: {2}, ClientRequestId : {0}{1}",
                                clientRequestId, Environment.NewLine, resourceName));
                    }
                }
                catch (Exception e)
                {
                    sb.AppendLine(
                        $"failed to Deserialize JSON Selected value from resource: {resourceName}");
                    Logger.WriteLog(ELogLevel.ERROR,
                        string.Format(
                            "Util.TryExtractObject: JSON object Deserialization FAILURE! {2}, Resource: {3}, ClientRequestId : {0}{2}{1}",
                            clientRequestId, e.Message, Environment.NewLine, resourceName));
                }
            }
            log = sb.ToString();
            return obj;
        }

        internal static bool TryExtractSelectedValue(Dictionary<string, SettingsItem> dictionary, string resourceName, out string selectedValue, out string processLog)
        {
            selectedValue = string.Empty;
            //1. try to extract the the setting item 
            SettingsItem setting;
            if (!dictionary.TryGetValue(resourceName, out setting))
            {
                processLog = string.Format("Resource: '{0}' was not found in  resource To Settings Items Dictionary", resourceName);
                return false;
            }

            //2. try to parse
            if (string.IsNullOrEmpty(setting.SelectedValue))
            {
                string errStr = string.Format("eToroSettings Resource '{0}' selected value returned null or empty", resourceName);
                processLog = errStr;
                Logger.WriteLog(ELogLevel.WARN, errStr);
                return false;
            }

            selectedValue = setting.SelectedValue;
            processLog = string.Format("Resource '{0}' extracted and parsed Selected value: {1}", resourceName, selectedValue.ToString());
            return true;
        }
    }
}




namespace SwaggerLogParser
{
    class SwaggerLogFunctions
    {
        public IDictionary<int, LeverageWhiteListUserEntrie> GetLeveragesRestrictionsWhiteList(long gcid, int? instrumentTypeId, int? instrumentId)
        {

            if (!_tradingSettingsConfiguration.UseLeveragesRestrictionsWhiteList)
            {
                return new Dictionary<int, LeverageWhiteListUserEntrie>();
            }
            else
            {
                if (instrumentId.HasValue)
                {
                    LeverageWhiteListUserEntrie leverageWhiteListUserEntrie;
                    _leveragesRestrictionsWhiteListCache.TryGetUserLeveragesRestrictionsFromWhiteListByInstrumentId(gcid,
                        instrumentId.Value, out leverageWhiteListUserEntrie);
                    return new Dictionary<int, LeverageWhiteListUserEntrie>
                    {{instrumentId.Value, leverageWhiteListUserEntrie}};
                }

                if (instrumentTypeId.HasValue)
                {
                    IDictionary<int, LeverageWhiteListUserEntrie> instrumentTypeIdLeverageWhiteListUserEntriesDictionary;
                    _leveragesRestrictionsWhiteListCache.TryGetUserLeveragesRestrictionsFromWhiteLisByInstrumentTypeId(gcid,
                        instrumentTypeId.Value, out instrumentTypeIdLeverageWhiteListUserEntriesDictionary);
                    return instrumentTypeIdLeverageWhiteListUserEntriesDictionary;
                }

                IDictionary<int, LeverageWhiteListUserEntrie> instrumentIdLeverageWhiteListUserEntriesDictionary;
                _leveragesRestrictionsWhiteListCache.TryGetUserLeveragesRestrictionsFromWhiteLisByGcid(gcid,
                    out instrumentIdLeverageWhiteListUserEntriesDictionary);
                return instrumentIdLeverageWhiteListUserEntriesDictionary;
            }
        }

        public IList<LeverageWhiteListUserEntrieWithUserData> GetLeveragesRestrictionsWhiteListWithUserData()
        {
            return _leveragesRestrictionsWhiteListCache.GetLeveragesRestrictionsWhiteListWithUserData();
        }

        public bool IsUserWhiteListed(long gcid, int instrumnetId, out LeverageWhiteListUserEntrie leverageWhiteListUserEntrie)
        {
            return _leveragesRestrictionsWhiteListCache.TryGetUserLeveragesRestrictionsFromWhiteListByInstrumentId(gcid,
                instrumnetId, out leverageWhiteListUserEntrie);
        }

        public IDictionary<long, HashSet<int>> GetInternalWhiteListUsersLeveragesList()
        {
            return _leveragesRestrictionsWhiteListCache.GetInternalWhiteListUsersLeveragesList();
        }

        public HashSet<int> GetInternalWhiteListUsersLeveragesListByGcid(long gcid)
        {
            _leveragesRestrictionsWhiteListCache.TryGetInternalUserLeveragesFromWhiteListByGcid(gcid, out var leverages);
            return leverages;
        }

        public List<int> GetUsaAllowedInstrumentIDs()
        {
            var domainInstruments = _instrumentUpdatableCache.GetAllInstruments();

            return domainInstruments.Where(b => b.IsUsAllowed).Select(i => i.InstrumentID).ToList();
        }

        public string GetAndDeleteProcessLog()
        {
            var returenedStr = String.Copy(_processLog);
            _processLog = string.Empty;
            return returenedStr;
        }

        public string GetAndDeleteProcessTicks()
        {
            var returenedStr = String.Copy(_ticksLog);
            _ticksLog = string.Empty;
            return returenedStr;
        }

#endregion

        #region Private Methods

        private TradingLeverageSettings ProcessAndLogInstrumentLeverageSettings(long gcid, string clientRequestId,
            Instrument domainInstrumnetData, Dictionary<string, SettingsItem> settingsItems,
            LeverageWhiteListUserEntrie leverageWhiteListUserEntrie, HashSet<int> internalWhiteListLeveragesSet, int tradingRiskStatusID)
        {
            var processedInstrumentLeverageSettings = new TradingLeverageSettings();

            try
            {
                string processLog;
                processedInstrumentLeverageSettings = LeverageService.ProcessLeverageSetting(gcid,
                    leverageWhiteListUserEntrie,
                    internalWhiteListLeveragesSet,
                    clientRequestId,
                    domainInstrumnetData.InstrumentID,
                    domainInstrumnetData.TypeID,
                    domainInstrumnetData.Exchange,
                    domainInstrumnetData.Leverages,
                    domainInstrumnetData.DefaultLeverage,
                    settingsItems,
                    tradingRiskStatusID,
                    out processLog,
                    null,
                    true
                );
                LogProcessInfo(string.Format("Gcid: {0} {1} a white list entrie for InstrumentID: {2}{3}",
                                   gcid.ToString(), (leverageWhiteListUserEntrie != null ? "has" : "does NOT have"),
                                   domainInstrumnetData.InstrumentID.ToString(), Environment.NewLine) + processLog);
            }
            catch (Exception)
            {
                //default based on domain instrument
                processedInstrumentLeverageSettings.GCID = gcid;
                processedInstrumentLeverageSettings.InstrumentID = domainInstrumnetData.InstrumentID;
                processedInstrumentLeverageSettings.InstrumentType = domainInstrumnetData.TypeID;
                processedInstrumentLeverageSettings.PossibleLeverages =
                    new HashSet<int>(domainInstrumnetData.Leverages);
                processedInstrumentLeverageSettings.PotentialDisplayLeverages = new HashSet<int>();
                processedInstrumentLeverageSettings.DefaultValue = domainInstrumnetData.DefaultLeverage;

                LogProcessInfo(string.Format("failed to process internalWhiteListLeveragesSet: domain values will be set{0}",
                    Environment.NewLine));
                Logger.WriteLog(ELogLevel.ERROR,
                    string.Format(
                        "TradingSettingsProvider.ProcessAndLogInstrumentLeverageSettings: error processing Leverage Settings - domain values will be set for Gcid: {0}, InstrumentID: {1}",
                        gcid.ToString(), domainInstrumnetData.InstrumentID.ToString()));
            }

            return processedInstrumentLeverageSettings;
        }

        private AllowManualTradingSettings ProcessAndLogInstrumentAllowManualTradingSettings(long gcid,
            string clientRequestId, Instrument domainInstrumnetData, Dictionary<string, SettingsItem> settingsItems)
        {
            string processLog;
            var proccessedInstrumentAllowManualTradingSettings =
                AllowManualTradingService.ProcessAllowManualTradingSetting(gcid, domainInstrumnetData, clientRequestId,
                    domainInstrumnetData.AllowManualTrading, settingsItems, out processLog);
            LogProcessInfo(processLog);
            return proccessedInstrumentAllowManualTradingSettings;
        }

        private W8BenStatusSettings ProcessAndLogW8BenStatusSettings(long gcid, string clientRequestId,
            Dictionary<string, SettingsItem> settingsItems)
        {
            var processW8BenStatusSettings =
                W8BenService.ProcessW8BenStatusSettings(gcid, clientRequestId, settingsItems, out var processLog);
            LogProcessInfo(processLog);
            return processW8BenStatusSettings;
        }

        private MinPositionAmountSettings ProcessAndLogInstrumentMinPositionAmountSettings(long gcid,
            string clientRequestId, Instrument domainInstrumnetData, Dictionary<string, SettingsItem> settingsItems)
        {
            string processLog;
            var processedInstrumentMinPositionAmountSettings = MinPositionAmountService.ProcessMinPositionAmountSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MinPositionAmount, domainInstrumnetData.MinPositionAmountAbsolute‎, domainInstrumnetData.MinPositionAmountAbsolute‎Discounted, settingsItems, out processLog);
            LogProcessInfo(processLog);
            return processedInstrumentMinPositionAmountSettings;
        }

        private MaxLeverageForTradeTypesSettings ProcessAndLogInstrumentSettledMaxLeverageSettings(int gcid,
            string clientRequestId, Instrument domainInstrumnetData, Dictionary<string, SettingsItem> settingsItems)
        {
            var processedInstrumentMaxLeverageForTradeTypesSettings =
                MaxLeverageForTradeTypesService.ProcessMaxLeverageForTradeTypesSetting(gcid, domainInstrumnetData.InstrumentID,
                    domainInstrumnetData.Exchange, domainInstrumnetData.TypeID, domainInstrumnetData.RealTradeMaxLeverage, clientRequestId, settingsItems, out var processLog);
            LogProcessInfo(processLog);
            return processedInstrumentMaxLeverageForTradeTypesSettings;
        }

        private void ProcessInstrumentSettings(long gcid, string clientRequestId,
            Dictionary<string, SettingsItem> settingsItems, Instrument domainInstrumnetData, HashSet<int> internalWhiteListLeveragesSet,
            LeverageWhiteListUserEntrie leverageWhiteListUserEntrie, bool generalAllowRedeem, int tradingRiskStatusID)
        {
            StringBuilder sb = new StringBuilder();
            string processLog;

            TradingLeverageSettings proccessedInstrumentLeverageSettings = new TradingLeverageSettings();

            try
            {
                proccessedInstrumentLeverageSettings = LeverageService.ProcessLeverageSetting(gcid,
                    leverageWhiteListUserEntrie, internalWhiteListLeveragesSet, clientRequestId, domainInstrumnetData.InstrumentID,
                    domainInstrumnetData.TypeID, domainInstrumnetData.Exchange,
                    domainInstrumnetData.Leverages, domainInstrumnetData.DefaultLeverage, settingsItems,
                    tradingRiskStatusID, out processLog);
                sb.Append(
                    $"GCID: {gcid.ToString()} {(leverageWhiteListUserEntrie != null ? "has" : "does NOT have")} a white list entree for InstrumentID: {domainInstrumnetData.InstrumentID.ToString()}{Environment.NewLine}" +
                    processLog);
            }
            catch (Exception ex)
            {
                sb.Append($"failed to process internalWhiteListLeveragesSet: domain values will be set{Environment.NewLine}");
                Logger.WriteLog(ELogLevel.ERROR, ex,
                    $"TradingSettingsProvider.ProcessInstrumentSettings - Error processing Leverage Settings for Gcid: {gcid.ToString()} {(string.IsNullOrEmpty(clientRequestId) ? string.Empty : $" ,ClientRequestId : {clientRequestId.ToString()}")}, InstrumentID: {domainInstrumnetData.InstrumentID.ToString()}");
            }

            bool forDisplayOnly = true;
            /*leverage1MaintenanceMarginDomainValue is on in order to provide Leverage1 maintenance margin to default SL Non Leveraged*/
            var maxStopLossPercentageSettings = MaxStopLossPercentageService.ProcessMaxStopLossPercentageSetting(gcid,
                clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.MaxStopLossPercentageMatrix, settingsItems, out processLog, tradingRiskStatusID, forDisplayOnly);
            sb.Append(processLog);
            var maxTakeProfitPercentageSettings = MaxTakeProfitPercentageService.ProcessMaxTakeProfitPercentageSetting(
                gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.MaxTakeProfitPercentage, settingsItems, out processLog, forDisplayOnly);
            sb.Append(processLog);
            var proccessedInstrumentMinPositionAmountSettings = MinPositionAmountService.ProcessMinPositionAmountSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.MinPositionAmount, domainInstrumnetData.MinPositionAmountAbsolute‎, domainInstrumnetData.MinPositionAmountAbsolute‎Discounted, settingsItems, out processLog);
            sb.Append(processLog);
            var allowManualTradingSettings = AllowManualTradingService.ProcessAllowManualTradingSetting(gcid, domainInstrumnetData, clientRequestId,
                domainInstrumnetData.AllowManualTrading, settingsItems, out processLog);
            sb.Append(processLog);
            var guaranteedSlTpTradingSettings = GuaranteedSlTpTradingService.ProcessGuaranteedSlTpTradingSetting(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, clientRequestId,
                domainInstrumnetData.IsGuaranteeSlTp, settingsItems, out processLog);
            sb.Append(processLog);
            var restrictedManualOpenSettings = RestrictedManualOpenService.Instance.Process(gcid, clientRequestId,
                domainInstrumnetData, settingsItems, out processLog);
            sb.Append(processLog);
            var settledMaxLeverageSettings = MaxLeverageForTradeTypesService.ProcessMaxLeverageForTradeTypesSetting(gcid,
               domainInstrumnetData.InstrumentID, domainInstrumnetData.Exchange, domainInstrumnetData.TypeID,
               domainInstrumnetData.RealTradeMaxLeverage, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            var requiresW8BenSettings = W8BenService.ProcessRequiresW8BenSettings(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.Exchange, domainInstrumnetData.TypeID,
                domainInstrumnetData.RequiresW8Ben,
                clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            var minStopLossPercentageSettings = MinStopLossPercentageService.ProcessMinStopLossPercentageSettings(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.MinStopLossPercentage,
                clientRequestId, settingsItems, maxStopLossPercentageSettings.MaxStopLossPercentageMatrix, out processLog);
            sb.Append(processLog);
            var minTakeProfitPercentageSettings = MinTakeProfitPercentageService.ProcessMinTakeProfitPercentageSettings(
                gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.MinTakeProfitPercentage,
                clientRequestId, settingsItems, maxTakeProfitPercentageSettings.MaxTakeProfitPercentage,
                out processLog);
            sb.Append(processLog);
            var allowTrailingStopLossSettings = AllowTrailingStopLossService.ProcessAllowTrailingStopLossSettings(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.AllowTrailingStopLoss,
                clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            var allowEditStopLossSettings = AllowEditStopLossService.ProcessAllowEditStopLossSettings(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.AllowEditStopLoss,
                clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            var allowEditTakeProfitSettings = AllowEditTakeProfitService.ProcessAllowEditTakeProfitSettings(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.AllowEditTakeProfit,
                clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            //default stopLossPercentage
            //TODO: remove after roll out 
            var defaultStopLossPercentageSettings = DefaultStopLossPercentageService.ProcessDefaultStopLossPercentageSettings(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.DefaultStopLossPercentage,
                clientRequestId, settingsItems, minStopLossPercentageSettings.MinStopLossPercentage, maxStopLossPercentageSettings.MaxStopLossPercentageMatrix, out processLog);
            sb.Append(processLog);
            //stop loss
            var defaultStopLossPercentageNonLeveragedSettings =
                DefaultStopLossPercentageNonLeveragedService.ProcessStopLossPercentageNonLeveragedSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.DefaultStopLossPercentageNonLeveraged,
                    clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            var defaultStopLossPercentageLeveragedSettings =
                DefaultStopLossPercentageLeveragedService.ProcessDefaultStopLossPercentageLeveragedSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.DefaultStopLossPercentageLeveraged,
                    clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            //take profit
            var defaultTakeProfitPercentageSettings =
                DefaultTakeProfitPercentageService.ProcessDefaultTakeProfitPercentageSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.DefaultTakeProfitPercentage,
                    clientRequestId, settingsItems, minTakeProfitPercentageSettings.MinTakeProfitPercentage,
                    maxTakeProfitPercentageSettings.MaxTakeProfitPercentage, out processLog);
            sb.Append(processLog);
            var defaultTrailingStopLossSettings = DefaultTrailingStopLossService.ProcessDefaultTrailingStopLossSettings(
                gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.DefaultTrailingStopLoss,
                clientRequestId, settingsItems, allowTrailingStopLossSettings.AllowTrailingStopLoss, out processLog);
            sb.Append(processLog);
            var allowBuySellSettings = AllowBuySellService.ProcessAllowBuySellSettings(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.AllowBuy,
                domainInstrumnetData.AllowSell,
                clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            var allowRedeemSettings = AllowRedeemService.ProcessAllowRedeemSetting(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.AllowRedeem, generalAllowRedeem, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            var positionUnitsForRedeemSettings = PositionUnitsForRedeemService.ProcessPositionUnitsForRedeemSettings(
                gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.MinPositionUnitsForRedeem,
                domainInstrumnetData.MaxPositionUnitsForRedeem, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            var allowPartialClosePosition = AllowPartialClosePositionService.ProcessAllowPartialClosePositionSettings(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, clientRequestId, domainInstrumnetData.AllowPartialClosePosition, settingsItems, out processLog);
            sb.Append(processLog);

            var allowEditStopLossLeveragedSettings =
                AllowEditStopLossLeveragedService.ProcessAllowEditStopLossLeveragedSettings(gcid,
                    domainInstrumnetData.InstrumentID,
                    domainInstrumnetData.TypeID, domainInstrumnetData.AllowEditStopLossLeveraged, clientRequestId,
                    settingsItems, out processLog);
            sb.Append(processLog);
            var allowEditTakeProfitLeveragedSettings =
                AllowEditTakeProfitLeveragedService.ProcessAllowEditTakeProfitLeveragedSettings(gcid,
                    domainInstrumnetData.InstrumentID,
                    domainInstrumnetData.TypeID, domainInstrumnetData.AllowEditTakeProfitLeveraged, clientRequestId,
                    settingsItems, out processLog);
            sb.Append(processLog);
            var allowDiscountedRatesSettings = AllowDiscountedRatesService.ProcessAllowDiscountedRatesSetting(gcid, domainInstrumnetData.TypeID, domainInstrumnetData.AllowDiscountedRates, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            domainInstrumnetData.MaxStopLossPercentageMatrix = maxStopLossPercentageSettings.MaxStopLossPercentageMatrix;

            bool allowRevolvingDoors;
            BlockedFromCFDService.ProcessBlockedFromCFDSettings(gcid, allowBuySellSettings, ref proccessedInstrumentLeverageSettings,
                restrictedManualOpenSettings, settledMaxLeverageSettings, clientRequestId, settingsItems, out allowRevolvingDoors, out processLog);
            sb.Append(processLog);

            var processedVisibleInternallyOnlySettings = VisibleInternallyOnlyService.ProcessVisibleInternallyOnlySettings(gcid, domainInstrumnetData.VisibleInternallyOnly,
                clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            var allowClosePosition = AllowClosePositionService.ProcessAllowClosePositionSetting(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.AllowClosePosition, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            var allowPendingOrders = AllowPendingOrdersService.Instance.ResolveSetting(gcid, clientRequestId,
                domainInstrumnetData, settingsItems, out processLog);
            sb.Append(processLog);

            var allowEntryOrders = AllowEntryOrdersService.Instance.ResolveSetting(gcid, clientRequestId,
                domainInstrumnetData, settingsItems, out processLog);
            sb.Append(processLog);

            var allowExitOrder = AllowExitOrderService.Instance.ResolveSetting(gcid, clientRequestId,
                domainInstrumnetData, settingsItems, out processLog);
            sb.Append(processLog);

            LogProcessInfo(sb.ToString());

            domainInstrumnetData.MaxStopLossPercentageMatrix = maxStopLossPercentageSettings.MaxStopLossPercentageMatrix;
            domainInstrumnetData.MaxTakeProfitPercentage = maxTakeProfitPercentageSettings.MaxTakeProfitPercentage;
            domainInstrumnetData.MinPositionAmount = proccessedInstrumentMinPositionAmountSettings.MinPositionAmount;
            domainInstrumnetData.AllowManualTrading = allowManualTradingSettings.AllowManualTrading;
            //TBD remove later
            domainInstrumnetData.Leverage1MaintenanceMargin = maxStopLossPercentageSettings.MaxStopLossPercentageMatrix.NonLeveraged.Buy;
            domainInstrumnetData.MaxStopLossPercentage = maxStopLossPercentageSettings.MaxStopLossPercentageMatrix.Leveraged.Buy;
            domainInstrumnetData.IsGuaranteeSlTp = guaranteedSlTpTradingSettings.IsSlTpGuaranteed;
            domainInstrumnetData.RestrictedManualOpen = restrictedManualOpenSettings.RestrictedManualOpen;
            domainInstrumnetData.RealTradeMaxLeverage = settledMaxLeverageSettings.RealTradeMaxLeverage;
            domainInstrumnetData.MarginTradeMaxLeverage = settledMaxLeverageSettings.MarginTradeMaxLeverage;
            domainInstrumnetData.RequiresW8Ben = requiresW8BenSettings.RequiresW8Ben;
            domainInstrumnetData.MinStopLossPercentage = minStopLossPercentageSettings.MinStopLossPercentage;
            domainInstrumnetData.MinTakeProfitPercentage = minTakeProfitPercentageSettings.MinTakeProfitPercentage;
            domainInstrumnetData.AllowTrailingStopLoss = allowTrailingStopLossSettings.AllowTrailingStopLoss;
            domainInstrumnetData.AllowEditStopLoss = allowEditStopLossSettings.AllowEditStopLossNonLeveraged;
            domainInstrumnetData.AllowEditTakeProfit = allowEditTakeProfitSettings.AllowEditTakeProfitNonLeveraged;
            domainInstrumnetData.DefaultStopLossPercentage = defaultStopLossPercentageSettings.DefaultStopLossPercentage;
            domainInstrumnetData.DefaultTakeProfitPercentage = defaultTakeProfitPercentageSettings.DefaultTakeProfitPercentage;
            domainInstrumnetData.DefaultTrailingStopLoss = defaultTrailingStopLossSettings.DefaultTrailingStopLoss;
            domainInstrumnetData.AllowBuy = allowBuySellSettings.IsBuyAllowed;
            domainInstrumnetData.AllowSell = allowBuySellSettings.IsSellAllowed;
            domainInstrumnetData.AllowRedeem = allowRedeemSettings.AllowRedeem;
            domainInstrumnetData.MinPositionUnitsForRedeem = positionUnitsForRedeemSettings.MinPositionUnitsForRedeem;
            domainInstrumnetData.MaxPositionUnitsForRedeem = positionUnitsForRedeemSettings.MaxPositionUnitsForRedeem;
            domainInstrumnetData.AllowPartialClosePosition = allowPartialClosePosition.AllowPartialClosePosition;
            domainInstrumnetData.AllowEditStopLossLeveraged = allowEditStopLossLeveragedSettings.AllowEditStopLossLeveraged;
            domainInstrumnetData.AllowEditTakeProfitLeveraged = allowEditTakeProfitLeveragedSettings.AllowEditTakeProfitLeveraged;
            domainInstrumnetData.DefaultStopLossPercentageNonLeveraged = defaultStopLossPercentageNonLeveragedSettings.DefaultStopLossPercentageNonLeveraged;
            domainInstrumnetData.DefaultStopLossPercentageLeveraged = defaultStopLossPercentageLeveragedSettings.DefaultStopLossPercentageLeveraged;
            domainInstrumnetData.AllowDiscountedRates = allowDiscountedRatesSettings.AllowDiscountedRates;
            domainInstrumnetData.MinPositionAmountAbsolute‎Discounted = proccessedInstrumentMinPositionAmountSettings.MinPositionAmountAbsolute‎Discounted;
            domainInstrumnetData.Leverages = proccessedInstrumentLeverageSettings.PossibleLeverages.ToList();
            domainInstrumnetData.PotentialDisplayLeverages = proccessedInstrumentLeverageSettings.PotentialDisplayLeverages.ToList();
            domainInstrumnetData.DefaultLeverage = proccessedInstrumentLeverageSettings.DefaultValue;
            domainInstrumnetData.IsNonLeveragedBuyAllowed = proccessedInstrumentLeverageSettings.IsNonLeveragedBuyAllowed;
            domainInstrumnetData.AllowRevolvingDoors = allowRevolvingDoors;
            domainInstrumnetData.VisibleInternallyOnly = processedVisibleInternallyOnlySettings.IsVisibleInternallyOnly;
            domainInstrumnetData.AllowClosePosition = allowClosePosition.AllowClosePosition;
            domainInstrumnetData.AllowPendingOrders = allowPendingOrders;
            domainInstrumnetData.AllowEntryOrders = allowEntryOrders;
            domainInstrumnetData.AllowExitOrder = allowExitOrder;
        }

        private void ProcessInstrumentTypeSettings(long gcid, string clientRequestId,
            Dictionary<string, SettingsItem> settingsItems, InstrumentType instrumentType)
        {
            StringBuilder sb = new StringBuilder();
            string processLog;

            var restrictedManualOpenSettings =
                RestrictedManualOpenService.ProcessRestrictedManualOpenByInstrumentTypeSetting(gcid,
                    instrumentType.InstrumentTypeId, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            instrumentType.RestrictedManualOpen = restrictedManualOpenSettings.RestrictedManualOpen;
        }

        private OpenPositionSettings ProcessAndLogOpenPositionSettings(long gcid, string clientRequestId,
            Dictionary<string, SettingsItem> settingsItems,
            Instrument domainInstrumnetData, AggregatedUnitsTiersByLeverage aggregatedUnitsTiersByLeverage, bool isBuy,
            LeverageWhiteListUserEntrie leverageWhiteListUserEntrie, HashSet<int> internalWhiteListLeveragesSet, int tradingRiskStatusID, int leverage)
        {
            bool forDislpayOnly = false;
            StringBuilder sb =
                new StringBuilder(
                    $"Open Position Settings processing GCID: {gcid.ToString()}, ClientRequestId: {clientRequestId}{Environment.NewLine}");
            OpenPositionSettings openPositionSettings = new OpenPositionSettings(isBuy, leverage) { Gcid = gcid, InstrumentTypeID = domainInstrumnetData.TypeID };
            string processLog;

            openPositionSettings.MaxLeverageForTradeTypesSetting =
                MaxLeverageForTradeTypesService.ProcessMaxLeverageForTradeTypesSettingByDirection(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.Exchange, domainInstrumnetData.TypeID,
                    isBuy, domainInstrumnetData.RealTradeMaxLeverage[isBuy],
                    clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            var leverageSettings = LeverageService.ProcessLeverageSetting(gcid, leverageWhiteListUserEntrie, internalWhiteListLeveragesSet, clientRequestId,
                            domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.Exchange, domainInstrumnetData.Leverages, domainInstrumnetData.DefaultLeverage, settingsItems,
                            tradingRiskStatusID, out processLog, aggregatedUnitsTiersByLeverage, forDislpayOnly, isBuy, openPositionSettings.MaxLeverageForTradeTypesSetting.MarginTradeMaxLeverage);
            sb.Append($"GCID: {gcid.ToString()} {(leverageWhiteListUserEntrie != null ? "has" : "does NOT have")} a white list entrie for InstrumentID: {domainInstrumnetData.InstrumentID.ToString()}{Environment.NewLine}" + processLog);

            openPositionSettings.MaxStopLossPercentageSettings = MaxStopLossPercentageService.ProcessMaxStopLossPercentageSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MaxStopLossPercentageMatrix, settingsItems, out processLog, tradingRiskStatusID, forDislpayOnly, aggregatedUnitsTiersByLeverage, isBuy, leverage);
            sb.Append(processLog);
            openPositionSettings.MaxTakeProfitPercentageSettings = MaxTakeProfitPercentageService.ProcessMaxTakeProfitPercentageSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MaxTakeProfitPercentage, settingsItems, out processLog, forDislpayOnly, aggregatedUnitsTiersByLeverage, isBuy);
            sb.Append(processLog);
            openPositionSettings.MinPositionAmountSettings = MinPositionAmountService.ProcessMinPositionAmountSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MinPositionAmount, domainInstrumnetData.MinPositionAmountAbsolute‎, domainInstrumnetData.MinPositionAmountAbsolute‎Discounted, settingsItems, out processLog);
            sb.Append(processLog);
            openPositionSettings.AllowManualTradingSettings = AllowManualTradingService.ProcessAllowManualTradingSetting(gcid, domainInstrumnetData, clientRequestId, domainInstrumnetData.AllowManualTrading, settingsItems, out processLog);
            sb.Append(processLog);
            openPositionSettings.MaxAllowedOpenedPositionsCountSettings = MaxAllowedOpenedPositionsCountService.ProcessMaxAllowedOpenedPositionsCountSetting(gcid, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            openPositionSettings.RestrictedManualOpenSettings = RestrictedManualOpenService.Instance.Process(gcid,
                clientRequestId, domainInstrumnetData, settingsItems, out processLog);
            sb.Append(processLog);
            openPositionSettings.W8BenExecutionValidationSettings =
                W8BenService.ProcessW8BenExecutionValidationSettings(gcid, domainInstrumnetData.InstrumentID,
                    domainInstrumnetData.Exchange, domainInstrumnetData.TypeID, domainInstrumnetData.RequiresW8Ben,
                    clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            openPositionSettings.MinStopLossPercentageSettings =
                MinStopLossPercentageService.ProcessMinStopLossPercentageSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MinStopLossPercentage, clientRequestId, settingsItems,
                    openPositionSettings.MaxStopLossPercentageSettings.MaxStopLossPercentageMatrix, out processLog);
            sb.Append(processLog);
            openPositionSettings.MinTakeProfitPercentageSettings =
                MinTakeProfitPercentageService.ProcessMinTakeProfitPercentageSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MinTakeProfitPercentage, clientRequestId, settingsItems,
                    openPositionSettings.MaxTakeProfitPercentageSettings.MaxTakeProfitPercentage, out processLog);
            sb.Append(processLog);
            openPositionSettings.AllowTrailingStopLossSettings =
                AllowTrailingStopLossService.ProcessAllowTrailingStopLossSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.AllowTrailingStopLoss, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            openPositionSettings.AllowBuySellSettings = AllowBuySellService.ProcessAllowBuySellSettings(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.AllowBuy,
                domainInstrumnetData.AllowSell, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            openPositionSettings.AllowDiscountedRatesSettings = AllowDiscountedRatesService.ProcessAllowDiscountedRatesSetting(gcid, domainInstrumnetData.TypeID, domainInstrumnetData.AllowDiscountedRates, clientRequestId, settingsItems, out processLog);

            openPositionSettings.BlockedFormCFD = BlockedFromCFDService.ProcessBlockedFromCFDSettings(gcid, openPositionSettings.AllowBuySellSettings, ref leverageSettings,
                openPositionSettings.RestrictedManualOpenSettings, openPositionSettings.MaxLeverageForTradeTypesSetting,
                clientRequestId, settingsItems, out processLog);
            openPositionSettings.LeverageSettings = leverageSettings;
            sb.Append(Environment.NewLine).Append(processLog);

            openPositionSettings.BlockedByCompliance = BlockedByComplianceService.ProcessBlockedByComplianceSettings(gcid, clientRequestId, settingsItems, out processLog);
            sb.Append(Environment.NewLine).Append(processLog);

            openPositionSettings.VisibleInternallyOnlySettings = VisibleInternallyOnlyService.ProcessVisibleInternallyOnlySettings(gcid, domainInstrumnetData.VisibleInternallyOnly, clientRequestId, settingsItems, out processLog);
            sb.Append(Environment.NewLine).Append(processLog);
            openPositionSettings.MaxNopSettings = MaxNopService.ProcessMaxNopSettings(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, isBuy, clientRequestId,
                aggregatedUnitsTiersByLeverage, settingsItems, out processLog);
            sb.Append(Environment.NewLine).Append(processLog);

            LogProcessInfo(sb.Append(Environment.NewLine).Append(Environment.NewLine)
                .Append("Final Open Position Settings:").Append(openPositionSettings.ToString()).ToString());

            return openPositionSettings;
        }

        private EditStopLossSettings ProcessAndLogEditStopLossSettings(long gcid, string clientRequestId,
            Dictionary<string, SettingsItem> settingsItems, Instrument domainInstrumnetData,
            AggregatedUnitsTiersByLeverage aggregatedUnitsTiersByLeverage, bool isBuy, int leverage, int tradingRiskStatusId)
        {
            bool forDislpayOnly = false;
            StringBuilder sb = new StringBuilder(
                $"Position Edit Stop Loss Settings processing GCID: {gcid.ToString()}, ClientRequestId: {clientRequestId}{Environment.NewLine}");
            EditStopLossSettings editStopLossSettings = new EditStopLossSettings(isBuy, leverage);
            editStopLossSettings.Gcid = gcid;
            string processLog;
            editStopLossSettings.MaxStopLossPercentageSettings =
                MaxStopLossPercentageService.ProcessMaxStopLossPercentageSetting(gcid, clientRequestId,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MaxStopLossPercentageMatrix, settingsItems, out processLog,
                    tradingRiskStatusId, forDislpayOnly, aggregatedUnitsTiersByLeverage, isBuy, leverage);
            sb.Append(processLog);
            editStopLossSettings.AllowEditStopLossSettings = AllowEditStopLossService.ProcessAllowEditStopLossSettings(
                gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.AllowEditStopLoss, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            editStopLossSettings.MinStopLossPercentageSettings =
                MinStopLossPercentageService.ProcessMinStopLossPercentageSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MinStopLossPercentage, clientRequestId, settingsItems,
                    editStopLossSettings.MaxStopLossPercentageSettings.MaxStopLossPercentageMatrix, out processLog);
            sb.Append(processLog);
            editStopLossSettings.AllowEditStopLossLeveragedSettings =
                AllowEditStopLossLeveragedService.ProcessAllowEditStopLossLeveragedSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.AllowEditStopLossLeveraged, clientRequestId, settingsItems, out processLog);

            LogProcessInfo(sb.Append(processLog).Append(Environment.NewLine).Append(Environment.NewLine)
                .Append("Final Position Edit StopLoss Settings:").Append(editStopLossSettings).ToString());

            return editStopLossSettings;
        }

        private EditTakeProfitSettings ProcessAndLogEditTakeProfitSettings(long gcid, string clientRequestId,
            Dictionary<string, SettingsItem> settingsItems, Instrument domainInstrumnetData, bool isBuy,
            AggregatedUnitsTiersByLeverage aggregatedUnitsTiersByLeverage = null)
        {
            bool forDislpayOnly = false;
            StringBuilder sb =
                new StringBuilder(
                    $"Position Edit Take Profit Settings processing Gcid: {gcid.ToString()}, ClientRequestId: {clientRequestId}{Environment.NewLine}");
            EditTakeProfitSettings editTakeProfitSettings = new EditTakeProfitSettings();
            editTakeProfitSettings.Gcid = gcid;
            string processLog;
            editTakeProfitSettings.MaxTakeProfitPercentageSettings =
                MaxTakeProfitPercentageService.ProcessMaxTakeProfitPercentageSetting(gcid, clientRequestId,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MaxTakeProfitPercentage, settingsItems, out processLog, forDislpayOnly,
                    aggregatedUnitsTiersByLeverage, isBuy);
            sb.Append(processLog);
            editTakeProfitSettings.AllowEditTakeProfitSettings =
                AllowEditTakeProfitService.ProcessAllowEditTakeProfitSettings(gcid, domainInstrumnetData.InstrumentID,
                    domainInstrumnetData.TypeID, domainInstrumnetData.AllowEditTakeProfit, clientRequestId,
                    settingsItems, out processLog);
            sb.Append(processLog);
            editTakeProfitSettings.MinTakeProfitPercentageSettings =
                MinTakeProfitPercentageService.ProcessMinTakeProfitPercentageSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MinTakeProfitPercentage, clientRequestId, settingsItems,
                    editTakeProfitSettings.MaxTakeProfitPercentageSettings.MaxTakeProfitPercentage, out processLog);
            sb.Append(processLog);
            editTakeProfitSettings.AllowEditTakeProfitLeveragedSettings =
                AllowEditTakeProfitLeveragedService.ProcessAllowEditTakeProfitLeveragedSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.AllowEditTakeProfitLeveraged, clientRequestId, settingsItems, out processLog);

            LogProcessInfo(sb.Append(processLog).Append(Environment.NewLine).Append(Environment.NewLine)
                .Append("Final Position Edit Take Profit Settings:").Append(editTakeProfitSettings.ToString())
                .ToString());

            return editTakeProfitSettings;
        }

        private EditTrailingStopLossSettings ProcessAndLogEditTrailingStopLossSettings(long gcid,
            string clientRequestId, Dictionary<string, SettingsItem> settingsItems, Instrument domainInstrumnetData)
        {
            bool forDislpayOnly = false;
            StringBuilder sb = new StringBuilder(string.Format(
                "Position Edit Trailing Stop Loss Settings processing Gcid: {0}, ClientRequestId: {1}{2}",
                gcid.ToString(), clientRequestId, Environment.NewLine));
            EditTrailingStopLossSettings editTrailingStopLossSettings = new EditTrailingStopLossSettings();
            editTrailingStopLossSettings.Gcid = gcid;
            string processLog;
            editTrailingStopLossSettings.AllowTrailingStopLossSettings =
                AllowTrailingStopLossService.ProcessAllowTrailingStopLossSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.AllowTrailingStopLoss, clientRequestId, settingsItems, out processLog);

            LogProcessInfo(sb.Append(processLog).Append(Environment.NewLine).Append(Environment.NewLine)
                .Append("Final Position Edit Trailing StopLoss Settings:").Append(editTrailingStopLossSettings)
                .ToString());

            return editTrailingStopLossSettings;
        }

        private OpenPendingOrderSettings ProcessAndLogOpenPendingOrderSettings(long gcid, string clientRequestId,
            Dictionary<string, SettingsItem> settingsItems,
            Instrument domainInstrumnetData, LeverageWhiteListUserEntrie leverageWhiteListUserEntry, HashSet<int> internalWhiteListLeveragesSet, bool isBuy, int leverage,
            int tradingRiskStatusId)
        {
            bool forDisplayOnly = false;
            StringBuilder sb =
                new StringBuilder(
                    $"Open Pending Order Settings processing GCID: {gcid.ToString()}, ClientRequestId: {clientRequestId}{Environment.NewLine}");
            OpenPendingOrderSettings openPendingOrderSettings = new OpenPendingOrderSettings(gcid, leverage, isBuy);
            string processLog;

            openPendingOrderSettings.MaxLeverageForTradeTypesSetting = MaxLeverageForTradeTypesService.ProcessMaxLeverageForTradeTypesSettingByDirection(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.Exchange, domainInstrumnetData.TypeID,
                    isBuy, domainInstrumnetData.RealTradeMaxLeverage[isBuy],
                    clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            var leverageSettings =
                LeverageService.ProcessLeverageSetting(gcid, leverageWhiteListUserEntry, internalWhiteListLeveragesSet, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.Exchange, domainInstrumnetData.Leverages,
                    domainInstrumnetData.DefaultLeverage, settingsItems, tradingRiskStatusId, out processLog, null, forDisplayOnly, isBuy, openPendingOrderSettings.MaxLeverageForTradeTypesSetting.MarginTradeMaxLeverage);
            sb.Append($"GCID: {gcid.ToString()} {(leverageWhiteListUserEntry != null ? "has" : "does NOT have")} a white list entrie for InstrumentID: {domainInstrumnetData.InstrumentID.ToString()}{Environment.NewLine}" + processLog);

            openPendingOrderSettings.MaxStopLossPercentageSettings =
                MaxStopLossPercentageService.ProcessMaxStopLossPercentageSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MaxStopLossPercentageMatrix,
                settingsItems, out processLog, tradingRiskStatusId, forDisplayOnly, isBuy: isBuy, leverage: leverage);
            sb.Append(processLog);
            openPendingOrderSettings.MaxTakeProfitPercentageSettings =
                MaxTakeProfitPercentageService.ProcessMaxTakeProfitPercentageSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MaxTakeProfitPercentage, settingsItems, out processLog, forDisplayOnly);
            sb.Append(processLog);
            openPendingOrderSettings.MinPositionAmountSettings =
                MinPositionAmountService.ProcessMinPositionAmountSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MinPositionAmount, domainInstrumnetData.MinPositionAmountAbsolute‎, domainInstrumnetData.MinPositionAmountAbsolute‎Discounted, settingsItems, out processLog);
            sb.Append(processLog);
            openPendingOrderSettings.AllowManualTradingSettings =
                AllowManualTradingService.ProcessAllowManualTradingSetting(gcid, domainInstrumnetData, clientRequestId, domainInstrumnetData.AllowManualTrading, settingsItems, out processLog);
            sb.Append(processLog);
            openPendingOrderSettings.RestrictedManualOpenSettings = RestrictedManualOpenService.Instance.Process(gcid,
                clientRequestId, domainInstrumnetData, settingsItems, out processLog);
            sb.Append(processLog);
            openPendingOrderSettings.MinStopLossPercentageSettings =
                MinStopLossPercentageService.ProcessMinStopLossPercentageSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MinStopLossPercentage, clientRequestId, settingsItems,
                    openPendingOrderSettings.MaxStopLossPercentageSettings.MaxStopLossPercentageMatrix, out processLog);
            sb.Append(processLog);
            openPendingOrderSettings.MinTakeProfitPercentageSettings =
                MinTakeProfitPercentageService.ProcessMinTakeProfitPercentageSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MinTakeProfitPercentage, clientRequestId, settingsItems,
                    openPendingOrderSettings.MaxTakeProfitPercentageSettings.MaxTakeProfitPercentage, out processLog);
            sb.Append(processLog);
            openPendingOrderSettings.AllowTrailingStopLossSettings =
                AllowTrailingStopLossService.ProcessAllowTrailingStopLossSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.AllowTrailingStopLoss, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            openPendingOrderSettings.AllowBuySellSettings =
                AllowBuySellService.ProcessAllowBuySellSettings(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.AllowBuy, domainInstrumnetData.AllowSell, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            openPendingOrderSettings.BlockedFormCFD = BlockedFromCFDService.ProcessBlockedFromCFDSettings(gcid, openPendingOrderSettings.AllowBuySellSettings, ref leverageSettings,
                openPendingOrderSettings.RestrictedManualOpenSettings, openPendingOrderSettings.MaxLeverageForTradeTypesSetting, clientRequestId,
                settingsItems, out processLog);
            openPendingOrderSettings.LeverageSettings = leverageSettings;
            sb.Append(Environment.NewLine).Append(processLog);

            openPendingOrderSettings.BlockedByCompliance = BlockedByComplianceService.ProcessBlockedByComplianceSettings(gcid, clientRequestId, settingsItems, out processLog);
            sb.Append(Environment.NewLine).Append(processLog);

            openPendingOrderSettings.VisibleInternallyOnlySettings = VisibleInternallyOnlyService.ProcessVisibleInternallyOnlySettings(gcid, domainInstrumnetData.VisibleInternallyOnly,
                clientRequestId, settingsItems, out processLog);
            sb.Append(Environment.NewLine).Append(processLog);

            LogProcessInfo(sb.Append(Environment.NewLine).Append(Environment.NewLine).Append("Final Open Pending Order Settings:").Append(openPendingOrderSettings.ToString()).ToString());

            return openPendingOrderSettings;
        }

        private OpenEntryOrderSettings ProcessAndLogOpenEntryOrderSettings(long gcid, string clientRequestId,
            Dictionary<string, SettingsItem> settingsItems,
            Instrument domainInstrumnetData, LeverageWhiteListUserEntrie leverageWhiteListUserEntry, HashSet<int> internalWhiteListLeveragesSet,
            bool isBuy, int leverage,
            int tradingRiskStatusId)
        {
            bool forDislpayOnly = false;
            StringBuilder sb = new StringBuilder(string.Format(
                "Open Entry Order Settings processing GCID: {0}, ClientRequestId: {1}{2}", gcid.ToString(),
                clientRequestId, Environment.NewLine));
            var openEntryOrderSettings = new OpenEntryOrderSettings(gcid, leverage, isBuy);
            string processLog;

            openEntryOrderSettings.MaxLeverageForTradeTypesSetting = MaxLeverageForTradeTypesService.ProcessMaxLeverageForTradeTypesSettingByDirection(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.Exchange, domainInstrumnetData.TypeID,
                    isBuy, domainInstrumnetData.RealTradeMaxLeverage[isBuy],
                    clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            var leverageSettings = LeverageService.ProcessLeverageSetting(gcid, leverageWhiteListUserEntry, internalWhiteListLeveragesSet, clientRequestId, domainInstrumnetData.InstrumentID,
                domainInstrumnetData.TypeID, domainInstrumnetData.Exchange, domainInstrumnetData.Leverages, domainInstrumnetData.DefaultLeverage, settingsItems, tradingRiskStatusId, out processLog, null, forDislpayOnly, isBuy: isBuy, openEntryOrderSettings.MaxLeverageForTradeTypesSetting.MarginTradeMaxLeverage);
            sb.Append($"GCID: {gcid.ToString()} {(leverageWhiteListUserEntry != null ? "has" : "does NOT have")} a white list entrie for InstrumentID: {domainInstrumnetData.InstrumentID.ToString()}{Environment.NewLine}" + processLog);
            openEntryOrderSettings.MaxStopLossPercentageSettings = MaxStopLossPercentageService.ProcessMaxStopLossPercentageSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.MaxStopLossPercentageMatrix, settingsItems, out processLog, tradingRiskStatusId, forDislpayOnly, isBuy: isBuy, leverage: leverage);
            sb.Append(processLog);
            openEntryOrderSettings.MaxTakeProfitPercentageSettings = MaxTakeProfitPercentageService.ProcessMaxTakeProfitPercentageSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MaxTakeProfitPercentage, settingsItems, out processLog, forDislpayOnly);
            sb.Append(processLog);
            openEntryOrderSettings.MinPositionAmountSettings = MinPositionAmountService.ProcessMinPositionAmountSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MinPositionAmount, domainInstrumnetData.MinPositionAmountAbsolute‎, domainInstrumnetData.MinPositionAmountAbsolute‎Discounted, settingsItems, out processLog);
            sb.Append(processLog);
            openEntryOrderSettings.AllowManualTradingSettings = AllowManualTradingService.ProcessAllowManualTradingSetting(gcid, domainInstrumnetData, clientRequestId, domainInstrumnetData.AllowManualTrading, settingsItems, out processLog);
            sb.Append(processLog);
            openEntryOrderSettings.RestrictedManualOpenSettings = RestrictedManualOpenService.Instance.Process(gcid,
                clientRequestId, domainInstrumnetData, settingsItems, out processLog);
            sb.Append(processLog);
            openEntryOrderSettings.MinStopLossPercentageSettings =
                MinStopLossPercentageService.ProcessMinStopLossPercentageSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MinStopLossPercentage, clientRequestId, settingsItems,
                    openEntryOrderSettings.MaxStopLossPercentageSettings.MaxStopLossPercentageMatrix, out processLog);
            sb.Append(processLog);
            openEntryOrderSettings.MinTakeProfitPercentageSettings =
                MinTakeProfitPercentageService.ProcessMinTakeProfitPercentageSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.MinTakeProfitPercentage, clientRequestId, settingsItems,
                    openEntryOrderSettings.MaxTakeProfitPercentageSettings.MaxTakeProfitPercentage, out processLog);
            sb.Append(processLog);
            openEntryOrderSettings.AllowTrailingStopLossSettings =
                AllowTrailingStopLossService.ProcessAllowTrailingStopLossSettings(gcid,
                    domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                    domainInstrumnetData.AllowTrailingStopLoss, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            openEntryOrderSettings.AllowBuySellSettings = AllowBuySellService.ProcessAllowBuySellSettings(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.AllowBuy, domainInstrumnetData.AllowSell, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            openEntryOrderSettings.BlockedFormCFD = BlockedFromCFDService.ProcessBlockedFromCFDSettings(gcid, openEntryOrderSettings.AllowBuySellSettings, ref leverageSettings,
                openEntryOrderSettings.RestrictedManualOpenSettings, openEntryOrderSettings.MaxLeverageForTradeTypesSetting, clientRequestId,
                settingsItems, out processLog);
            openEntryOrderSettings.LeverageSettings = leverageSettings;
            sb.Append(Environment.NewLine).Append(processLog);

            openEntryOrderSettings.BlockedByCompliance = BlockedByComplianceService.ProcessBlockedByComplianceSettings(gcid, clientRequestId, settingsItems, out processLog);
            sb.Append(Environment.NewLine).Append(processLog);

            openEntryOrderSettings.VisibleInternallyOnlySettings = VisibleInternallyOnlyService.ProcessVisibleInternallyOnlySettings(gcid,
                domainInstrumnetData.VisibleInternallyOnly, clientRequestId, settingsItems, out processLog);
            sb.Append(Environment.NewLine).Append(processLog);

            LogProcessInfo(sb.Append(Environment.NewLine).Append(Environment.NewLine).Append("Final Open Entry Order Settings:").Append(openEntryOrderSettings).ToString());
            return openEntryOrderSettings;
        }

        private ClosePositionByLimitSettings ProcessAndLogClosePositionByLimitSettings(long gcid,
            string clientRequestId, Dictionary<string, SettingsItem> settingsItems, Instrument domainInstrumnetData,
            int tradingRiskStatusId)
        {
            StringBuilder sb =
                new StringBuilder(
                    $"close Position Settings processing Gcid: {gcid.ToString()}, ClientRequestId: {clientRequestId}, tradingRiskStatusId: {tradingRiskStatusId} {Environment.NewLine}");
            var guaranteedSlTpTradingSettings = GuaranteedSlTpTradingService.ProcessGuaranteedSlTpTradingSetting(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, clientRequestId,
                domainInstrumnetData.IsGuaranteeSlTp, settingsItems, out var processLog);
            sb.Append(processLog);

            var guaranteedSlByPositionAmountAndRiskStatusSettings =
                GuaranteedSlTpTradingService.ProcessGuaranteedSlByPositionAmountOrRiskStatus(gcid, clientRequestId,
                    settingsItems, domainInstrumnetData, tradingRiskStatusId, out processLog);
            sb.Append(processLog);

            var allowClosePosition = AllowClosePositionService.ProcessAllowClosePositionSetting(gcid, domainInstrumnetData.InstrumentID,
                domainInstrumnetData.AllowClosePosition, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            ClosePositionByLimitSettings closePositionSettings = new ClosePositionByLimitSettings
            {
                Gcid = gcid,
                GuaranteedSlTpTradingSettings = guaranteedSlTpTradingSettings,
                GuaranteedSlByPositionAmountOrRiskStatusSettings = guaranteedSlByPositionAmountAndRiskStatusSettings,
                AllowClosePosition = allowClosePosition.AllowClosePosition
            };

            LogProcessInfo(sb.Append(Environment.NewLine)
                .Append(Environment.NewLine)
                .Append("Final close Position Settings:")
                .Append(closePositionSettings)
                .ToString());

            return closePositionSettings;
        }


        private ClosePositionByRedeemSettings ProcessAndLogClosePositionByRedeemSettings(long gcid,
            string clientRequestId, bool generalAllowRedeem, Dictionary<string, SettingsItem> settingsItems,
            Instrument domainInstrumnetData)
        {
            StringBuilder sb = new StringBuilder(string.Format(
                "Close Position By Redeem Settings processing Gcid: {0}, ClientRequestId: {1}{2}", gcid.ToString(),
                clientRequestId, Environment.NewLine));
            string processLog;

            var allowRedeemSettings = AllowRedeemService.ProcessAllowRedeemSetting(gcid,
                domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.AllowRedeem,
                generalAllowRedeem, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);
            var positionUnitsForRedeemSettings = PositionUnitsForRedeemService.ProcessPositionUnitsForRedeemSettings(
                gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID,
                domainInstrumnetData.MinPositionUnitsForRedeem, domainInstrumnetData.MaxPositionUnitsForRedeem,
                clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            ClosePositionByRedeemSettings closePositionSettings = new ClosePositionByRedeemSettings
            {
                Gcid = gcid,
                InstrumentID = domainInstrumnetData.InstrumentID,
                InstrumentType = domainInstrumnetData.TypeID,
                AllowRedeem = allowRedeemSettings.AllowRedeem,
                MinPositionUnitsForRedeem = positionUnitsForRedeemSettings.MinPositionUnitsForRedeem,
                MaxPositionUnitsForRedeem = positionUnitsForRedeemSettings.MaxPositionUnitsForRedeem
            };

            LogProcessInfo(sb.Append(Environment.NewLine).Append(Environment.NewLine)
                .Append("Final Close Position By Redeem Settings:").Append(closePositionSettings.ToString())
                .ToString());

            return closePositionSettings;
        }
        private PartialClosePositionSettings ProcessAndLogPartialClosePositionSettings(long gcid, string clientRequestId, Dictionary<string, SettingsItem> settingsItems, Instrument domainInstrumnetData)
        {
            StringBuilder sb = new StringBuilder(string.Format("Partial Close Position Settings processing Gcid: {0}, ClientRequestId: {1}{2}", gcid.ToString(), clientRequestId, Environment.NewLine));
            string processLog;

            var allowPartialClosePositionSettings = AllowPartialClosePositionService.ProcessAllowPartialClosePositionSettings(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, clientRequestId, domainInstrumnetData.AllowPartialClosePosition, settingsItems, out processLog);
            sb.Append(processLog);
            var minPositionAmountSettings = MinPositionAmountService.ProcessMinPositionAmountSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MinPositionAmount, domainInstrumnetData.MinPositionAmountAbsolute‎, domainInstrumnetData.MinPositionAmountAbsolute‎Discounted, settingsItems, out processLog);
            sb.Append(processLog);

            PartialClosePositionSettings partialClosePositionSettings = new PartialClosePositionSettings
            {
                Gcid = gcid,
                InstrumentID = domainInstrumnetData.InstrumentID,
                InstrumentType = domainInstrumnetData.TypeID,
                AllowPartialClosePositionSettings = allowPartialClosePositionSettings,
                MinPositionAmountSettings = minPositionAmountSettings
            };

            LogProcessInfo(sb.Append(Environment.NewLine).Append(Environment.NewLine).Append("Final Partial Close Position Settings:").Append(partialClosePositionSettings.ToString()).ToString());

            return partialClosePositionSettings;
        }
        private ClosePositionSettings ProcessAndLogClosePositionSettings(long gcid, string clientRequestId, bool generalAllowRedeem, Dictionary<string, SettingsItem> settingsItems, Instrument domainInstrumnetData, bool isCloseByRedeeem, bool isPartialClose)
        {
            StringBuilder sb = new StringBuilder(string.Format("Close Position Settings processing Gcid: {0}, ClientRequestId: {1}{2}", gcid.ToString(), clientRequestId, Environment.NewLine));
            string processLog;

            ClosePositionSettings closePositionSettings = new ClosePositionSettings
            {
                Gcid = gcid,
                InstrumentID = domainInstrumnetData.InstrumentID,
                InstrumentType = domainInstrumnetData.TypeID
            };

            var allowClosePosition = AllowClosePositionService.ProcessAllowClosePositionSetting(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.AllowClosePosition, clientRequestId, settingsItems, out processLog);
            sb.Append(processLog);

            closePositionSettings.AllowClosePosition = allowClosePosition.AllowClosePosition;

            if (isCloseByRedeeem)
            {
                var allowRedeemSettings = AllowRedeemService.ProcessAllowRedeemSetting(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.AllowRedeem, generalAllowRedeem, clientRequestId, settingsItems, out processLog);
                sb.Append(processLog);
                var positionUnitsForRedeemSettings = PositionUnitsForRedeemService.ProcessPositionUnitsForRedeemSettings(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MinPositionUnitsForRedeem, domainInstrumnetData.MaxPositionUnitsForRedeem, clientRequestId, settingsItems, out processLog);
                sb.Append(processLog);
                var allowRedeemRegadrdlessSettings = AllowRedeemRegardlessOfAllowCloseService.ProcessAllowRedeemRegardlessSetting(gcid, domainInstrumnetData.InstrumentID, clientRequestId, settingsItems, out processLog);
                sb.Append(processLog);

                ClosePositionByRedeemSettings closePositionByRedeemSettings = new ClosePositionByRedeemSettings
                {
                    Gcid = gcid,
                    InstrumentID = domainInstrumnetData.InstrumentID,
                    InstrumentType = domainInstrumnetData.TypeID,
                    AllowRedeem = allowRedeemSettings.AllowRedeem,
                    MinPositionUnitsForRedeem = positionUnitsForRedeemSettings.MinPositionUnitsForRedeem,
                    MaxPositionUnitsForRedeem = positionUnitsForRedeemSettings.MaxPositionUnitsForRedeem,
                    AllowRedeemRegardless = allowRedeemRegadrdlessSettings.AllowRedeemRegardless,
                };

                closePositionSettings.ClosePositionByRedeemSettings = closePositionByRedeemSettings;
            }

            if (isPartialClose)
            {
                var allowPartialClosePositionSettings = AllowPartialClosePositionService.ProcessAllowPartialClosePositionSettings(gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, clientRequestId, domainInstrumnetData.AllowPartialClosePosition, settingsItems, out processLog);
                sb.Append(processLog);
                var minPositionAmountSettings = MinPositionAmountService.ProcessMinPositionAmountSetting(gcid, clientRequestId, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, domainInstrumnetData.MinPositionAmount, domainInstrumnetData.MinPositionAmountAbsolute‎, domainInstrumnetData.MinPositionAmountAbsolute‎Discounted, settingsItems, out processLog);
                sb.Append(processLog);

                PartialClosePositionSettings partialClosePositionSettings = new PartialClosePositionSettings
                {
                    Gcid = gcid,
                    InstrumentID = domainInstrumnetData.InstrumentID,
                    InstrumentType = domainInstrumnetData.TypeID,
                    AllowPartialClosePositionSettings = allowPartialClosePositionSettings,
                    MinPositionAmountSettings = minPositionAmountSettings
                };

                closePositionSettings.PartialClosePositionSettings = partialClosePositionSettings;
            }

            LogProcessInfo(sb.Append(Environment.NewLine).Append(Environment.NewLine).Append("Final Close Position Settings :").Append(closePositionSettings.ToString()).ToString());

            return closePositionSettings;
        }
        private CopyStopLossPercentageSettings ProcessAndLogCopyStopLossPercentageSettings(long gcid, int mirrorTypeId,
            Dictionary<string, SettingsItem> userSettingsData, MirrorValidation domainMirrorValidation,
            string clientRequestId)
        {
            var processedCopyStopLossPercentageSettings = CopyStopLossPercentageService.ProcessCopyStopLossPercentage(
                gcid, mirrorTypeId, domainMirrorValidation, userSettingsData, clientRequestId, out var processLog);
            LogProcessInfo(processLog);

            return processedCopyStopLossPercentageSettings;
        }

        private IList<MirrorValidationSettings> ProcessAndLogMirrorValidationsSettings(long gcid,
            MirrorType[] mirrorTypes, Dictionary<string, SettingsItem> userSettingsData,
            IList<MirrorValidation> domainMirrorValidations, string clientRequestId)
        {
            StringBuilder sb =
                new StringBuilder(
                    $"{Environment.NewLine}Mirror Validations Settings processing Gcid: {gcid.ToString()}, ClientRequestId: {clientRequestId}{Environment.NewLine}");
            IList<MirrorValidationSettings> mirrorValidationsSettings = new List<MirrorValidationSettings>();

            foreach (var mirrorType in mirrorTypes)
            {
                int mirrorTypeId = (int)mirrorType;
                sb.Append(string.Format("{2}Mirror type {0}:{1} Validations", mirrorType.ToString(), mirrorTypeId.ToString(), Environment.NewLine));

                MirrorValidation domainMirrorValidation =
                    domainMirrorValidations.FirstOrDefault(v => v.MirrorType == mirrorTypeId);
                sb.Append(string.Format("{1}Base mirror validations: {0}{1}", domainMirrorValidation,
                    Environment.NewLine));
                string processLog;
                var copyStopLossPercentageSettings = CopyStopLossPercentageService.ProcessCopyStopLossPercentage(gcid,
                    mirrorTypeId, domainMirrorValidation, userSettingsData, clientRequestId, out processLog);
                sb.Append(processLog);
                var allowCopyTradingPreProcessingData = AllowCopyTradingService.ExtractAllowCopyTradingPreProcessingData(gcid, mirrorTypeId, userSettingsData, clientRequestId, out processLog);
                sb.Append(processLog);

                var countryIdsLst = _countryDetailsCache.GetCountryIDsByNames(allowCopyTradingPreProcessingData
                    .CountryToCountriesCopyTradingSelectedValue.ToArray());

                string processingLog;
                decimal AddFundsMinAmountDeltaDollars = CopyAddFundsService.ProcessAddFundsMinAmountDeltaDollars(gcid, mirrorTypeId, domainMirrorValidation, userSettingsData, clientRequestId, out processingLog);
                sb.Append(processingLog);

                MirrorValidationSettings mirrorValidationSettings = new MirrorValidationSettings(gcid, domainMirrorValidation, copyStopLossPercentageSettings,
                    allowCopyTradingPreProcessingData.GeneralAllowCopyTradingSelectedValue, AddFundsMinAmountDeltaDollars, allowCopyTradingPreProcessingData.CountryToCopyFundsCopyTradingSelectedValue, countryIdsLst);

                sb.Append(string.Format("{2}final Mirror type {0}:{1} Validations :{3}{2}", mirrorType.ToString(),
                    mirrorTypeId.ToString(), Environment.NewLine, mirrorValidationSettings.ToString()));

                mirrorValidationsSettings.Add(mirrorValidationSettings);
            }

            LogProcessInfo(sb.Append(Environment.NewLine)
                .Append("Final MirrorValidationsSettings Settings creation completed.").ToString());

            return mirrorValidationsSettings;
        }

        private LeveregedOnlyNopSettings ProcessAndLogLeveregedOnlyNopTradingSettings(long gcid, string clientRequestId,
            Instrument domainInstrumnetData, Dictionary<string, SettingsItem> settingsItems)
        {
            string processLog;
            var proccessedLeveregedOnlyNopTradingSettings = LeveregedOnlyNopService.ProcessLeveregedOnlyNopSettings(
                gcid, domainInstrumnetData.InstrumentID, domainInstrumnetData.TypeID, clientRequestId, settingsItems,
                out processLog);
            LogProcessInfo(processLog);
            return proccessedLeveregedOnlyNopTradingSettings;
        }

        private bool TryGetUserLeveragesRestrictionsFromWhiteList(long gcid, int instrumetnID,
            out LeverageWhiteListUserEntrie leverageWhiteListUserEntrie, out HashSet<int> leverages, string clientRequestId = "")
        {
            if (!_tradingSettingsConfiguration.UseLeveragesRestrictionsWhiteList)
            {
                leverageWhiteListUserEntrie = null;
                leverages = null;
                return false;
            }

            if (_leveragesRestrictionsWhiteListCache.TryGetInternalUserLeveragesFromWhiteListByGcid(gcid, out leverages))
            {
                leverageWhiteListUserEntrie = null;
                Logger.WriteLog(ELogLevel.INFO, $"GCID: {gcid.ToString()} is an internal white listed user - will get the widest leverage range possible for instrumetnID {instrumetnID.ToString()}: {(string.Join(",", leverages))}. {clientRequestId}");
                return true;
            }

            return _leveragesRestrictionsWhiteListCache.TryGetUserLeveragesRestrictionsFromWhiteListByInstrumentId(gcid,
                instrumetnID, out leverageWhiteListUserEntrie);
        }

        private bool TryGetUserLeveragesRestrictionsFromWhiteList(long gcid, out IDictionary<int, LeverageWhiteListUserEntrie> leverageWhiteListUserEntriesDictionary, out HashSet<int> leverages, string clientRequestId = "")
        {
            if (!_tradingSettingsConfiguration.UseLeveragesRestrictionsWhiteList)
            {
                leverageWhiteListUserEntriesDictionary = null;
                leverages = null;
                return false;
            }

            if (_leveragesRestrictionsWhiteListCache.TryGetInternalUserLeveragesFromWhiteListByGcid(gcid, out leverages))
            {
                Logger.WriteLog(ELogLevel.INFO, $"GCID: {gcid.ToString()} is an internal white listed user - will get the widest leverage range possible for all instruments: {(string.Join(",", leverages))}. {clientRequestId}");
                leverageWhiteListUserEntriesDictionary = null;
                return true;
            }

            return _leveragesRestrictionsWhiteListCache.TryGetUserLeveragesRestrictionsFromWhiteLisByGcid(gcid, out leverageWhiteListUserEntriesDictionary);
        }

        private void LogProcessInfo(string processLog)
        {
            var sb = new StringBuilder(processLog).Append(Environment.NewLine)
                .Append("----------------------------------------------------------").Append(Environment.NewLine);
            _tradingSettingsLogicLogger.LogDebug(sb.ToString());

            if (_setProcessLog)
            {
                _processLog = sb.ToString();
            }
        }

        private void LogLatency(TicksTrail ticks)
        {
            string ticksAsJson = ticks.ToStringAsJSON();
            if (_latencyLogger != null)
            {
                _latencyLogger.Dump(ticksAsJson);
            }

            if (_setProcessLog)
            {
                _ticksLog = ticksAsJson;
            }
        }

        private void SetCofig()
        {
            _setProcessLog = Convert.ToBoolean(ConfigurationManager.AppSettings["CacheLastProcessLog"] ?? "false");
        }

        public void SetConfig(bool isCacheLastProcessLog)
        {
            _setProcessLog = isCacheLastProcessLog;
        }

        #endregion

    }
}
