using WATS.Testing;
using Virinco.WATS.Interface;
using Xunit;
using Xunit.Abstractions;
using ChromaConverter = Virinco.WATS.Converter.Chroma.ChromaTextConverter;

namespace ChromaTextConverter.Tests
{
    public class ChromaTextConverterTests : TextConverterTestBase
    {
        public ChromaTextConverterTests(ITestOutputHelper output) : base(output) { }

        protected override IReportConverter_v2 CreateConverter()
        {
            var args = new ChromaConverter().ConverterParameters;
            args["operationTypeCode"] = "10";
            return new ChromaConverter(args);
        }

        [Fact, Trait("TestMode", "ConvertOnly")]
        public void ConvertOnly_AllFiles() => RunAllFiles(TestMode.ConvertOnly);

        [Fact, Trait("TestMode", "ConvertAndValidate")]
        public void ConvertAndValidate_AllFiles() => RunAllFiles(TestMode.ConvertAndValidate);

        [Fact, Trait("TestMode", "ConvertAndSimulate")]
        public void ConvertAndSimulate_AllFiles() => RunAllFiles(TestMode.ConvertAndSimulate);

        [Fact, Trait("TestMode", "ConvertAndUpload"), Trait("RequiresServer", "true")]
        public void ConvertAndUpload_AllFiles() => RunAllFiles(TestMode.ConvertAndUpload);
    }
}
