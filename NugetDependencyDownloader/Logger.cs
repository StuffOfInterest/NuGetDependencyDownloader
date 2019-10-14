
using System.Threading.Tasks;
using LINQPad;
using NuGet.Common;

namespace NuGetDependencyDownloader
{
    public class Logger : ILogger
    {
        public void LogDebug(string data) => $"DEBUG: {data}".Dump();
        public void LogVerbose(string data) => $"VERBOSE: {data}".Dump();
        public void LogInformation(string data) => $"INFORMATION: {data}".Dump();
        public void LogMinimal(string data) => $"MINIMAL: {data}".Dump();
        public void LogWarning(string data) => $"WARNING: {data}".Dump();
        public void LogError(string data) => $"ERROR: {data}".Dump();
        public void LogInformationSummary(string data) => $"DEBUG: {data}".Dump();

        public void Log(LogLevel level, string data) => $"{level}: {data}".Dump();

        public async Task LogAsync(LogLevel level, string data) => $"{level.ToString()}: {data}".Dump();

        public void Log(ILogMessage message) => message.Dump();

        public async Task LogAsync(ILogMessage message)=> $"LOG: {message}".Dump();

        public void LogSummary(string data) => $"SUMMARY: {data}".Dump();
    }
}