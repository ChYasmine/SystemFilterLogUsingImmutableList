using System.Collections.Concurrent;
using System.Collections.Immutable;

public enum Level
{
    Info, Warning, Error
}
public readonly struct LogEntry
{
    public Level Level { get; }
    public string Message { get; }
    public string Source { get; }
    public DateTime Timestamp { get; }
}

public interface ILogFilter
{
    bool Accept(in LogEntry log);
}


public class LevelFilter : ILogFilter
{
    public bool Accept(in LogEntry log)
    {
        if (log.Level == Level.Error)
            return true;
        return false;
    }

}

public class KeywordFilter : ILogFilter
{
    public bool Accept(in LogEntry log)
    {
        var msg = log.Message ?? String.Empty;

        if (log.Message.Contains("DB") || log.Message.Contains("CRITICAL"))
        {
            return true;
        }
        return false;
    }
}


public class AndFilter
{
    private volatile ImmutableList<ILogFilter>? _filters;
    private readonly ConcurrentQueue<LogEntry>? _logs;



    private void AddFilter(ILogFilter logFilter)
    {
        ImmutableInterlocked.Update(ref _filters, list => list.Add(logFilter));

    }
    private void removeFilter(ILogFilter logFilter)
    {
        ImmutableInterlocked.Update(ref _filters, list => list.Remove(logFilter));

    }

    private bool validateLog(LogEntry log)
    {
        foreach (var filter in _filters)
        {
            if (!filter.Accept(log))
            {
                return false;
            }

        }
        _logs.Enqueue(log);
        return true;
    }




}