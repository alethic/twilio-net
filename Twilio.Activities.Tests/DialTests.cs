using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Twilio.Activities.Tests
{

    [TestClass]
    public class DialTests : TwilioTest
    {

        [TestMethod]
        public void DialTest()
        {
            var c = CreateContext(new Dial()
            {
                 Number = "+15555555555",
            });

            // cannot work without bookmarks
            Assert.Fail();

            c.Invoker.Invoke();

            Assert.AreEqual("<Response><Dial>+15555555555</Dial></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        }

    }

}
