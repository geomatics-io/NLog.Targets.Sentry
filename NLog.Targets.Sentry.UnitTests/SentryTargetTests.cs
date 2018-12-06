using System;
using System.Collections.Generic;
using Moq;
using NLog.Config;
using NUnit.Framework;
using Sentry;
using Sentry.Protocol;

namespace NLog.Targets.Sentry.UnitTests
{
    [TestFixture]
    class SentryTargetTests
    {
        [SetUp]
        public void Setup()
        {
            LogManager.ThrowExceptions = true;
        }

        [TearDown]
        public void Teardown()
        {
            LogManager.ThrowExceptions = false;
        }

        [Test]
        public void TestPublicConstructor()
        {
            Assert.DoesNotThrow(() => new SentryTarget());
            Assert.Throws<NLogConfigurationException>(() =>
            {
                var sentryTarget = new SentryTarget();
                var configuration = new LoggingConfiguration();
                configuration.AddTarget("NLogSentry", sentryTarget);
                configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, sentryTarget));
                LogManager.Configuration = configuration;
            });
        }

        [Test]
        public void TestBadDsn()
        {
            Assert.Throws<ArgumentException>(() => SentrySdk.Init("http://localhost"));
        }

        [Test]
        public void TestLoggingToSentry()
        {
            SentryLevel lErrorLevel = SentryLevel.Debug;
            IDictionary<string, string> lTags = null;
            Exception lException = null;

            var sentryClient = new Mock<ISentryClient>();

            sentryClient
                .Setup(x => x.CaptureException(It.IsAny<Exception>()))
                .Callback((Exception exception, SentryEvent msg, SentryLevel lvl, IDictionary<string, string> d, object extra) =>
                {
                    lException = exception;
                    lErrorLevel = lvl;
                    lTags = d;
                })
                .Returns(new SentryId());
            
            // Setup NLog
            var sentryTarget = new SentryTarget()
            {
                Dsn = "https://e80e676a68784007bff7f645d4660188@sentry.io/1337801",
            };

            var configuration = new LoggingConfiguration();
            configuration.AddTarget("NLogSentry", sentryTarget);
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, sentryTarget));
            LogManager.Configuration = configuration;

            try
            {
                throw new Exception("Oh No!");
            }
            catch (Exception e)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.ErrorException("Error Message", e);
            }

            Assert.IsTrue(lException.Message == "Oh No!");
            Assert.IsTrue(lTags == null);
            Assert.IsTrue(lErrorLevel == SentryLevel.Error);
        }




        //[Test]
        //public void TestLoggingToSentry_SendLogEventInfoPropertiesAsTags()
        //{
        //    var sentryClient = new Mock<IRavenClient>();
        //    ErrorLevel lErrorLevel = ErrorLevel.Debug;
        //    IDictionary<string, string> lTags = null;
        //    Exception lException = null;

        //    sentryClient
        //        .Setup(x => x.CaptureException(It.IsAny<Exception>(), It.IsAny<SentryMessage>(), It.IsAny<ErrorLevel>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<object>()))
        //        .Callback((Exception exception, SentryMessage msg, ErrorLevel lvl, IDictionary<string, string> d, object extra) =>
        //        {
        //            lException = exception;
        //            lErrorLevel = lvl;
        //            lTags = d;
        //        })
        //        .Returns("Done");

        //    // Setup NLog
        //    var sentryTarget = new SentryTarget(sentryClient.Object)
        //    {
        //        Dsn = "http://25e27038b1df4930b93c96c170d95527:d87ac60bb07b4be8908845b23e914dae@test/4",
        //        SendLogEventInfoPropertiesAsTags = true,
        //    };
        //    var configuration = new LoggingConfiguration();
        //    configuration.AddTarget("NLogSentry", sentryTarget);
        //    configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, sentryTarget));
        //    LogManager.Configuration = configuration;

        //    var tag1Value = "abcde";

        //    try
        //    {
        //        throw new Exception("Oh No!");
        //    }
        //    catch (Exception e)
        //    {
        //        var logger = LogManager.GetCurrentClassLogger();

        //        var logEventInfo = LogEventInfo.Create(LogLevel.Error, "default", "Error Message", e);
        //        logEventInfo.Properties.Add("tag1", tag1Value);
        //        logger.Log(logEventInfo);
        //    }

        //    Assert.IsTrue(lException.Message == "Oh No!");
        //    Assert.IsTrue(lTags != null);
        //    Assert.IsTrue(lErrorLevel == ErrorLevel.Error);
        //}
    }
}
