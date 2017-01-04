﻿using NUnit.Framework;
using Xamarin.UITest;

namespace Contoso.Forms.Test.UITests
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void TestEnablingAndDisablingServices()
        {
            ServiceStateHelper.app = app;
            app.Tap(TestStrings.GoToTogglePageButton);

            /* Test setting enabling all services */
            ServiceStateHelper.MobileCenterEnabled = true;
            Assert.IsTrue(ServiceStateHelper.MobileCenterEnabled);
            ServiceStateHelper.AnalyticsEnabled = true;
            Assert.IsTrue(ServiceStateHelper.AnalyticsEnabled);
            ServiceStateHelper.CrashesEnabled = true;
            Assert.IsTrue(ServiceStateHelper.CrashesEnabled);

            /* Test that disabling MobileCenter disables everything */
            ServiceStateHelper.MobileCenterEnabled = false;
            Assert.IsFalse(ServiceStateHelper.MobileCenterEnabled);
            Assert.IsFalse(ServiceStateHelper.AnalyticsEnabled);
            Assert.IsFalse(ServiceStateHelper.CrashesEnabled);

            /* Test disabling individual services */
            ServiceStateHelper.MobileCenterEnabled = true;
            Assert.IsTrue(ServiceStateHelper.MobileCenterEnabled);
            ServiceStateHelper.AnalyticsEnabled = false;
            Assert.IsFalse(ServiceStateHelper.AnalyticsEnabled);
            ServiceStateHelper.CrashesEnabled = false;
            Assert.IsFalse(ServiceStateHelper.CrashesEnabled);

            /* Test that enabling MobileCenter enabling everything, regardless of previous states */
            ServiceStateHelper.MobileCenterEnabled = true;
            Assert.IsTrue(ServiceStateHelper.MobileCenterEnabled);
            Assert.IsTrue(ServiceStateHelper.AnalyticsEnabled);
            Assert.IsTrue(ServiceStateHelper.CrashesEnabled);
        }

        [Test]
        public void SendEvents()
        {
            app.Tap(TestStrings.GoToAnalyticsPageButton);
            app.Tap(TestStrings.SendEventButton);
            app.Tap(TestStrings.AddPropertyButton);
            app.Tap(TestStrings.AddPropertyButton);
            app.Tap(TestStrings.AddPropertyButton);
            app.Tap(TestStrings.AddPropertyButton);
            app.Tap(TestStrings.AddPropertyButton);
            app.Tap(TestStrings.SendEventButton);
            app.Tap(TestStrings.GoToAnalyticsResultsPageButton);

            AnalyticsResultsHelper.app = app;

            Assert.IsTrue(AnalyticsResultsHelper.VerifyEventName());
            Assert.IsTrue(AnalyticsResultsHelper.VerifyNumProperties(5));

            /* TODO also need to add checks for which callbacks were called */
        }

        [Test]
        public void TestCrash()
        {
            /* Crash the application with a divide by zero exception and then restart*/
            app.Tap(TestStrings.GoToCrashesPageButton);
            app.Tap(TestStrings.GenerateTestCrashButton);
            TestSuccessfulCrash();
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionType("TestCrashException"));
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionMessage("Test crash exception generated by SDK"));
        }

        [Test]
        public void InvalidOperation()
        {
            /* Crash the application with a divide by zero exception and then restart*/
            app.Tap(TestStrings.GoToCrashesPageButton);
            app.Tap(TestStrings.GenerateTestCrashButton);
            TestSuccessfulCrash();
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionType("InvalidOperationException"));
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionMessage("Sequence contains no matching element"));
        }

        [Test]
        public void AggregateException()
        {
            /* Crash the application with a divide by zero exception and then restart*/
            app.Tap(TestStrings.GoToCrashesPageButton);
            app.Tap(TestStrings.GenerateTestCrashButton);
            TestSuccessfulCrash();
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionType("AggregateException"));
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionMessage("One or more errors occured."));
        }


        [Test]
        public void DivideByZero()
        {
            /* Crash the application with a divide by zero exception and then restart*/
            app.Tap(TestStrings.GoToCrashesPageButton);
            app.Tap(TestStrings.DivideByZeroCrashButton);
            TestSuccessfulCrash();
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionType("DivideByZeroException"));
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionMessage("Attempted to divide by zero."));
        }

        [Test]
        public void AsyncTaskException()
        {
            /* Crash the application with a divide by zero exception and then restart*/
            app.Tap(TestStrings.GoToCrashesPageButton);
            app.Tap(TestStrings.DivideByZeroCrashButton);
            TestSuccessfulCrash();
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionType("IOException"));
            Assert.IsTrue(LastSessionErrorReportHelper.VerifyExceptionMessage("Server did not respond"));
        }

        public void TestSuccessfulCrash()
        {
            app = AppInitializer.StartApp(platform);
            app.Tap(TestStrings.GoToCrashResultsPageButton);

            /* Ensure that the callbacks were properly called */
            CrashResultsHelper.app = app;
            Assert.IsTrue(CrashResultsHelper.SendingErrorReportWasCalled);
            Assert.IsTrue(CrashResultsHelper.SentErrorReportWasCalled);
            Assert.IsFalse(CrashResultsHelper.FailedToSendErrorReportWasCalled);
            Assert.IsTrue(CrashResultsHelper.ShouldProcessErrorReportWasCalled);
            Assert.IsTrue(CrashResultsHelper.ShouldAwaitUserConfirmationWasCalled);
            Assert.IsTrue(CrashResultsHelper.GetErrorAttachmentWasCalled);

            LastSessionErrorReportHelper.app = app;

            /* Verify the last session error report */
            Assert.IsTrue(LastSessionErrorReportHelper.DeviceReported);
            if (platform == Platform.Android)
            {
                Assert.IsTrue(LastSessionErrorReportHelper.HasAndroidDetails);
                Assert.IsFalse(LastSessionErrorReportHelper.HasIosDetails);
            }
            if (platform == Platform.iOS)
            {
                Assert.IsTrue(LastSessionErrorReportHelper.HasIosDetails);
                Assert.IsFalse(LastSessionErrorReportHelper.HasAndroidDetails);
            }
        }
    }
}
