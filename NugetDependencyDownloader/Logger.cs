using System;
using System.Threading.Tasks;
using NuGet.Common;

namespace NuGetDependencyDownloader
{
    public class Logger : ILogger
    {
        public void LogDebug(string data) => Console.WriteLine($@"DEBUG: {data}");
        public void LogVerbose(string data) => Console.WriteLine($@"VERBOSE: {data}");
        public void LogInformation(string data) => Console.WriteLine($@"INFORMATION: {data}");
        public void LogMinimal(string data) => Console.WriteLine($@"MINIMAL: {data}");
        public void LogWarning(string data) => Console.WriteLine($@"WARNING: {data}");
        public void LogError(string data) => Console.WriteLine($@"ERROR: {data}");
        public void LogInformationSummary(string data) => Console.WriteLine($@"DEBUG: {data}");

        public void Log(LogLevel level, string data) => Console.WriteLine($@"{level}: {data}");

        public async Task LogAsync(LogLevel level, string data) => Console.WriteLine($@"{level.ToString()}: {data}");

        public void Log(ILogMessage message) =>  Console.WriteLine(message);

        public async Task LogAsync(ILogMessage message) =>  Console.WriteLine(message);

        public void LogSummary(string data) =>  Console.WriteLine($@"SUMMARY: {data}");
    }
}