using Godot;

public static class Logger
{
    private static LogLevelEnum _appliedLogLevelThreshold = LogLevelEnum.Info;
    public static LogLevelEnum AppliedLogLevelThreshold
    {
        get => _appliedLogLevelThreshold;
        set
        {
            Log($"Log level threshold set to: {value}", LogTypeEnum.Framework, LogLevelEnum.Info);
            _appliedLogLevelThreshold = value;
        }
    }

    public enum LogTypeEnum
    {
        None,
        EnterTree,
        ExitTree,
        Ready,
        World,
        Script,
        Quest,
        Todo,
        Player,
        Entity,
        Component,
        State,
        Graphics,
        Audio,
        UI,
        Input,
        Framework
    }

    public enum LogLevelEnum
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public static void Log(string message, LogTypeEnum logType = LogTypeEnum.None, LogLevelEnum logLevel = LogLevelEnum.Info)
    {
        Log(message, "", logType, logLevel);
    }

    public static void Log(string message, string userPrefix = "", LogTypeEnum logType = LogTypeEnum.None, LogLevelEnum logLevel = LogLevelEnum.Info)
    {
        if (logLevel < AppliedLogLevelThreshold) return;

        var (typeColor, typeSymbol) = logType switch
        {
            LogTypeEnum.EnterTree => ("magenta", "🔼"),
            LogTypeEnum.ExitTree => ("pink", "🔽"),
            LogTypeEnum.Ready => ("green", "✔️"),
            LogTypeEnum.World => ("blue", "🌍"),
            LogTypeEnum.Script => ("cyan", "🗒️"),
            LogTypeEnum.Quest => ("purple", "📜"),
            LogTypeEnum.Todo => ("orange", "📝TODO: "),
            LogTypeEnum.Player => ("lightseagreen", "👤"),
            LogTypeEnum.Entity => ("fuchsia", "👾"),
            LogTypeEnum.Component => ("teal", "🧩"),
            LogTypeEnum.State => ("lightblue", "🔄"),
            LogTypeEnum.Graphics => ("crimson", "🎨"),
            LogTypeEnum.Audio => ("turquoise", "🔊"),
            LogTypeEnum.UI => ("pink", "🖥️"),
            LogTypeEnum.Input => ("lime", "🕹️"),
            LogTypeEnum.Framework => ("blue", "⚙️"),
            _ => ("gray", "")
        };

        var (levelColor, levelSymbol) = logLevel switch
        {
            LogLevelEnum.Warning => ("yellow", "⚠️"),
            LogLevelEnum.Error => ("orange", "❗"),
            LogLevelEnum.Critical => ("red", "🔥"),
            _ => ("gray", "")
        };

        string prefix = typeSymbol;
        if (logLevel == LogLevelEnum.Warning || logLevel == LogLevelEnum.Error || logLevel == LogLevelEnum.Critical)
            prefix = $"{levelSymbol}{typeSymbol}";

        string userPrefixPart = string.IsNullOrEmpty(userPrefix) ? "" : $"[color={levelColor}]{userPrefix}:[/color] ";
        GD.PrintRich($"{prefix} {userPrefixPart}[color={typeColor}]{message}[/color]");
    }

    public static void LogWarning(string message, string userPrefix = "", LogTypeEnum logType = LogTypeEnum.None)
    {
        Log(message, userPrefix, logType, LogLevelEnum.Warning);
    }

    public static void LogWarning(string message, LogTypeEnum logType = LogTypeEnum.None)
    {
        Log(message, "", logType, LogLevelEnum.Warning);
    }

    public static void LogError(string message, string userPrefix = "", LogTypeEnum logType = LogTypeEnum.None)
    {
        Log(message, userPrefix, logType, LogLevelEnum.Error);
    }

    public static void LogError(string message, LogTypeEnum logType = LogTypeEnum.None)
    {
        Log(message, "", logType, LogLevelEnum.Error);
    }

    public static void LogCritical(string message, string userPrefix = "", LogTypeEnum logType = LogTypeEnum.None)
    {
        Log(message, userPrefix, logType, LogLevelEnum.Critical);
    }

    public static void LogCritical(string message, LogTypeEnum logType = LogTypeEnum.None)
    {
        Log(message, "", logType, LogLevelEnum.Critical);
    }
}
