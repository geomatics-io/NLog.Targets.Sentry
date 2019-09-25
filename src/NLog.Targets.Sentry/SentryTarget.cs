#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog.Config;
using Sentry;
using Sentry.Extensibility;
using Sentry.Protocol;
using Sentry.Reflection;

#endregion

namespace NLog.Targets.Sentry
{
    [Target("Sentry")]
    public sealed class SentryTarget : TargetWithLayout
    {
        private readonly Func<string, IDisposable> _initAction;
        private volatile IDisposable _sdkHandle;
        private readonly object _initSync = new object();
        internal IHub Hub { get; set; }
        public bool SendIdentity { get; set; }

        internal static readonly (string Name, string Version) NameAndVersion
            = typeof(SentryTarget).Assembly.GetNameAndVersion();

        private static readonly string ProtocolPackageName = "nuget:" + NameAndVersion.Name;

        private Dsn dsn;

        internal SentryTarget(
            Func<string, IDisposable> initAction,
            IHub hubGetter)
        {
            Debug.Assert(initAction != null);
            Debug.Assert(hubGetter != null);

            _initAction = initAction;
            Hub = hubGetter;
        }

        public SentryTarget() : this(SentrySdk.Init, HubAdapter.Instance)
        {
        }

        /// <summary>
        /// The DSN for the Sentry host
        /// </summary>
        [RequiredParameter]
        public string Dsn
        {
            get { return dsn == null ? null : dsn.ToString(); }
            set { dsn = new Dsn(value); }
        }

        public string User { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent == null)
            {
                return;
            }

            if (!Hub.IsEnabled && _sdkHandle == null)
            {
                if (Dsn == null)
                {
                    return;
                }

                lock (_initSync)
                {
                    if (_sdkHandle == null)
                    {
                        _sdkHandle = _initAction(Dsn);
                        Debug.Assert(_sdkHandle != null);
                    }
                }
            }

            var exception = logEvent.Exception;
            var evt = new SentryEvent(exception)
            {
                Sdk =
                {
                    Name = Constants.SdkName,
                    Version = NameAndVersion.Version
                },
                Logger = logEvent.LoggerName,
                Level = logEvent.ToSentryLevel()
            };

            evt.Sdk.AddPackage(ProtocolPackageName, NameAndVersion.Version);

            if (!string.IsNullOrWhiteSpace(logEvent.FormattedMessage))
            {
                evt.Message = logEvent.FormattedMessage;
            }

            evt.SetExtras(GetLoggingEventProperties(logEvent));

            if (SendIdentity)
            {
                evt.User = new User
                {
                    Username = User
                };
            }

            Hub.CaptureEvent(evt);
        }

        private static IEnumerable<KeyValuePair<string, object>> GetLoggingEventProperties(LogEventInfo loggingEvent)
        {
            var properties = loggingEvent.Properties;
            if (properties == null)
            {
                yield break;
            }

            foreach (var key in properties.Keys)
            {
                if (!string.IsNullOrWhiteSpace(key as string))
                {
                    var value = properties[key];
                    if (value != null
                        && (!(value is string stringValue) || !string.IsNullOrWhiteSpace(stringValue)))
                    {
                        yield return new KeyValuePair<string, object>((string) key, value);
                    }
                }
            }

            if (!string.IsNullOrEmpty(loggingEvent.CallerClassName))
            {
                yield return new KeyValuePair<string, object>(nameof(loggingEvent.CallerClassName),
                    loggingEvent.CallerClassName);
            }

            if (!string.IsNullOrEmpty(loggingEvent.CallerFilePath))
            {
                yield return new KeyValuePair<string, object>(nameof(loggingEvent.CallerFilePath),
                    loggingEvent.CallerFilePath);
            }

            if (loggingEvent.CallerLineNumber > 0)
            {
                yield return new KeyValuePair<string, object>(nameof(loggingEvent.CallerLineNumber),
                    loggingEvent.CallerLineNumber);
            }

            if (!string.IsNullOrEmpty(loggingEvent.CallerMemberName))
            {
                yield return new KeyValuePair<string, object>(nameof(loggingEvent.CallerMemberName),
                    loggingEvent.CallerMemberName);
            }

            if (loggingEvent.Level != null)
            {
                yield return new KeyValuePair<string, object>("nlog-level", loggingEvent.Level.Name);
            }
        }

        protected override void CloseTarget()
        {
            base.CloseTarget();

            _sdkHandle?.Dispose();
        }
    }
}