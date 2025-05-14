using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Segrom.JobObserver.OzonService.TestUtils;

public class XUnitLogger<T>(ITestOutputHelper output): ILogger<T>
{
    public void Log<TState>(
        LogLevel logLevel, EventId eventId, 
        TState state, Exception? exception, 
        Func<TState, Exception?, string> formatter)
    {
        output.WriteLine(formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }
}