using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace TJAPlayer3.Tests
{
    [TestFixture]
    public sealed class CDTXStyleExtractorTests
    {
        private static readonly Encoding ShiftJisEncoding = Encoding.GetEncoding("Shift_JIS");

        [Test, Combinatorial]
        public void Test_tセッション譜面がある(
            [Values(
                "205 example",
                "205 example but with duplicate sheets",
                "expected case 1 and 2",
                "expected case 1 only",
                "expected case 2 only",
                "expected case couple only",
                "expected case double only",
                "expected case double only but with duplicate sheets",
                "expected case single and couple",
                "expected case single and double",
                "expected case single and double but with duplicate sheets",
                "expected case single only",
                "expected case single only whole file due to no course",
                "expected case single only whole file due to no course but with duplicate sheets",
                "expected case single only whole file due to no course but with no start",
                "kitchen sink couple only",
                "kitchen sink double only",
                "kitchen sink single and couple",
                "kitchen sink single and double",
                "kitchen sink single only",
                "mixed case double only",
                "mixed case single and double",
                "mixed case single only",
                "no course",
                "no style",
                "no style but with duplicate sheets",
                "trailing characters double only",
                "trailing characters single and double",
                "trailing characters single only")]
            string scenarioName,
            [Values(0, 1, 2)] int seqNo)
        {
            var assemblyPath = new Uri(GetType().Assembly.EscapedCodeBase).LocalPath;
            var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            var testDataDirectory = Path.Combine(Path.Combine(assemblyDirectory, "コード"), "スコア、曲");

            var scenarioFileNamePart = scenarioName.Replace(' ', '_');

            var inputDirectory = Path.Combine(testDataDirectory, "input");
            var inputFileName = $"{scenarioFileNamePart}.tja";
            var inputPath = Path.Combine(inputDirectory, inputFileName);
            var input = File.ReadAllText(inputPath, ShiftJisEncoding)
                .Replace("\r\n", "\n")
                .Replace('\t', ' ');

            var result = CDTXStyleExtractor.tセッション譜面がある(input, seqNo, inputPath);

            // I would use Approt進行LoopalTests.Net for this,
            // but cannot until we upgrade past .net 3.5.
            // Until then, this test will approximate it.

            var inputReferenceDirectory = Path.Combine(testDataDirectory, "inputReference");
            Directory.CreateDirectory(inputReferenceDirectory);
            var inputReferenceFileName = $"{scenarioFileNamePart}.{seqNo}.tja";
            var inputReferencePath = Path.Combine(inputReferenceDirectory, inputReferenceFileName);
            File.Delete(inputReferencePath);
            File.Copy(inputPath, inputReferencePath);
            
            var receit進行LoopedDirectory = Path.Combine(testDataDirectory, "receit進行Looped");
            Directory.CreateDirectory(receit進行LoopedDirectory);
            var receit進行LoopedFileName = $"{scenarioFileNamePart}.{seqNo}.tja";
            var receit進行LoopedPath = Path.Combine(receit進行LoopedDirectory, receit進行LoopedFileName);
            File.Delete(receit進行LoopedPath);
            File.WriteAllText(receit進行LoopedPath, result, ShiftJisEncoding);

            var approt進行LoopedDirectory = Path.Combine(testDataDirectory, "approt進行Looped");
            Directory.CreateDirectory(approt進行LoopedDirectory);
            var approt進行LoopedFileName = $"{scenarioFileNamePart}.{seqNo}.tja";
            var approt進行LoopedPath = Path.Combine(approt進行LoopedDirectory, approt進行LoopedFileName);

            var approt進行Looped = File.ReadAllText(approt進行LoopedPath, ShiftJisEncoding).Replace("\r\n", "\n");
            var receit進行Looped = File.ReadAllText(receit進行LoopedPath, ShiftJisEncoding);

            Assert.AreEqual(approt進行Looped, receit進行Looped);
        }
    }
}