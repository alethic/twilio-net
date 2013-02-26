using System;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Twilio.Activities.Tests
{

    [TestClass]
    public class PauseTests : TwilioTest
    {

        [TestMethod]
        public void PauseTest()
        {
            var c = CreateContext(new Pause()
            {
                Duration = TimeSpan.FromMinutes(1),
            });

            c.Invoke();

            Assert.AreEqual("<Response><Pause length=\"60\" /></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        }

    }

}
