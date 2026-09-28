using System;
using System.Collections.Generic;
using Virinco.WATS.Integration.TextConverter;
using Virinco.WATS.Interface;

namespace Virinco.WATS.Converter.Chroma
{
    public class ChromaTextConverter : TextConverterBase
    {
        string currentStepName = "";
        StepStatusType currentStepStatus;
        double currentStepTime;
        double min = double.NaN;
        double meas = double.NaN;
        double max = double.NaN;
        int currentChannel = -1;
        int currentSlot = -1;

        private void ResetLocals()
        {
            currentStepName = "";
            currentStepStatus = StepStatusType.Passed;
            currentStepTime = double.NaN;
            min = double.NaN;
            meas = double.NaN;
            max = double.NaN;
            currentChannel = -1;
            currentSlot = -1;
        }

        protected override bool ProcessMatchedLine(SearchFields.SearchMatch match, ref ReportReadState readState)
        {
            if (match == null)
            {
                this.SubmitUUT();
                return true;
            }
            switch (match.matchField.fieldName)
            {
                case "PartNo":
                    currentUUT.PartNumber = (string)match.GetSubField("PartNo");
                    currentUUT.ExecutionTime = ((TimeSpan)match.GetSubField("Elapsed")).TotalSeconds;
                    break;
                case "Step":
                    currentStepName = (string)match.GetSubField("StepName");
                    currentStepStatus = (StepStatusType)match.GetSubField("Result");
                    break;
                case "SlotAndChannelNum":
                    currentChannel = (int)match.GetSubField("ChannelNum");
                    currentSlot = (int)match.GetSubField("SlotNum");
                    break;
                case "Measure":
                    min = (double)match.GetSubField("Min");
                    meas = (double)match.GetSubField("Meas");
                    max = (double)match.GetSubField("Max");
                    break;
                case "NewStep":
                    if (currentStepName != "")
                    {
                        if (!double.IsNaN(meas))
                        {
                            NumericLimitStep numericLimitStep = currentSequence.AddNumericLimitStep(currentStepName);
                            numericLimitStep.AddTest(meas, CompOperatorType.GELE, min, max, "", currentStepStatus);
                        }
                        else
                        {
                            if (currentStepName.ToLower().Contains("delay"))
                            {
                                currentSequence.AddGenericStep(GenericStepTypes.Wait, currentStepName);
                            }
                            else
                            {
                                PassFailStep passFailStep = currentSequence.AddPassFailStep(currentStepName);
                                passFailStep.AddTest(currentStepStatus == StepStatusType.Passed);
                            }
                        }
                    }
                    ResetLocals();
                    break;
            }
            return true;
        }

        // Default constructor: returns default ConverterParameters; search fields are NOT set up.
        // Use ChromaTextConverter(args) for actual conversion.
        public ChromaTextConverter() : base()
        {
            ConverterParameters.Add("EnParametervimanger", "Bæ");
        }

        public ChromaTextConverter(Dictionary<string, string> args) : base(args)
        {
            var regExpSearchField = searchFields.AddRegExpField(UUTField.SequenceName, ReportReadState.InHeader, @"Test Program Name\s*:\s*(?<Name>\S+)\s*", null, typeof(string));

            regExpSearchField = searchFields.AddRegExpField(UUTField.UseSubFields, ReportReadState.InHeader, @"Serial No\s*:\s*(?<SerialNum>\S+)\s*System Time\s*:\s*(?<SystemTime>\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}\s(?:AM|PM))", null, typeof(string));
            regExpSearchField.AddSubField("SerialNum", typeof(string), null, UUTField.SerialNumber);
            regExpSearchField.AddSubField("SystemTime", typeof(DateTime), "yyyy/MM/dd hh:mm:ss tt", UUTField.StartDateTime);

            regExpSearchField = searchFields.AddRegExpField("PartNo", ReportReadState.InHeader, @"Model Name\s*:\s*(?<PartNo>\S+)\s*Elapsed Time\s*:\s*(?<Elapsed>\d{2}:\d{2}:\d{2})\s*", null, typeof(string));
            regExpSearchField.AddSubField("PartNo", typeof(string));
            regExpSearchField.AddSubField("Elapsed", typeof(TimeSpan), "HH:mm:ss");

            regExpSearchField = searchFields.AddRegExpField(UUTField.UseSubFields, ReportReadState.InHeader, @"LOT Number\s*:\s*(?<BatchNum>\S*)\s*Environment\s*:\s*(?<Environment>\S*)", null, typeof(string));
            regExpSearchField.AddSubField("BatchNum", typeof(string), null, UUTField.BatchSerialNumber);
            regExpSearchField.AddSubField("Environment", typeof(string), null, UUTField.MiscUUTInfo);

            regExpSearchField = searchFields.AddRegExpField(UUTField.UseSubFields, ReportReadState.InHeader, @"Order Number\s*:\s*(?<OrderNum>\S*)\s+Inspector\s*:\s*(?<Inspector>\S*)", null, typeof(string));
            regExpSearchField.AddSubField("OrderNum", typeof(string), null, UUTField.MiscUUTInfo);
            regExpSearchField.AddSubField("Inspector", typeof(string), null, UUTField.Operator);

            regExpSearchField = searchFields.AddRegExpField(UUTField.UseSubFields, ReportReadState.InHeader, @"Customer\s*:\s*(?<Customer>\S*)\s+Test Result\s*:\s*(?<TestResult>\S*)", null, typeof(string), ReportReadState.InTest);
            regExpSearchField.AddSubField("Customer", typeof(string), null, UUTField.MiscUUTInfo);
            regExpSearchField.AddSubField("TestResult", typeof(UUTStatusType), null, UUTField.Status);

            regExpSearchField = searchFields.AddRegExpField("Step", ReportReadState.InTest, @"STEP[.](?<StepNo>\d+)[(](?<SeqName>[^)]+)[)]\s*:\s*(?<StepName>.+?) ---- [(](?<ExecTime>[^)]+)[)]\s*-*\s*(?<Result>(?:PASS|FAIL))", null, typeof(string));
            regExpSearchField.AddSubField("StepNo", typeof(int));
            regExpSearchField.AddSubField("SeqName", typeof(string));
            regExpSearchField.AddSubField("StepName", typeof(string));
            regExpSearchField.AddSubField("ExecTime", typeof(string));
            regExpSearchField.AddSubField("Result", typeof(StepStatusType));

            regExpSearchField = searchFields.AddRegExpField("SlotAndChannelNum", ReportReadState.InTest, @"Slot No. :\s*(?<SlotNum>\d*)\s+Channel No. :\s*(?<ChannelNum>\d*)", null, typeof(string));
            regExpSearchField.AddSubField("SlotNum", typeof(int));
            regExpSearchField.AddSubField("ChannelNum", typeof(int));

            regExpSearchField = searchFields.AddRegExpField("Measure", ReportReadState.InTest, @"^(:?Result)*\s*(?<Min>[0-9.-]+)+\s+(?<Meas>[0-9.-]+)+\s+(?<Max>[0-9.-]+)+\s*$", null, typeof(string));
            regExpSearchField.AddSubField("Min", typeof(double));
            regExpSearchField.AddSubField("Meas", typeof(double));
            regExpSearchField.AddSubField("Max", typeof(double));

            regExpSearchField = searchFields.AddRegExpField("NewStep", ReportReadState.InTest, @"^={80}", null, typeof(string));
        }
    }
}
