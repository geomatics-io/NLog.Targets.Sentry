#region

using Sentry.Protocol;

#endregion

namespace NLog.Targets.Sentry
{
    internal static class LevelMapping
    {
        public static SentryLevel? ToSentryLevel(this LogEventInfo logEvent)
        {
            switch (logEvent.Level)
            {
                case var l when l == LogLevel.Fatal:
                    return SentryLevel.Fatal;

                case var l when l == LogLevel.Error:
                    return SentryLevel.Error;

                case var l when l == LogLevel.Warn:
                    return SentryLevel.Warning;

                case var l when l == LogLevel.Info
                                || l == LogLevel.Info:
                    return SentryLevel.Info;

                case var l when l == LogLevel.Debug
                                || l == LogLevel.Trace:
                    return SentryLevel.Debug;
            }

            return null;
        }
    }
}