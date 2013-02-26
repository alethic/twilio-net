using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Twilio.Activities.Tests
{

    [TestClass]
    public class SayTests : TwilioTest
    {

        [TestMethod]
        public void SayTextTest()
        {
            var c = CreateContext(new Say()
            {
                Text = "Hello",
            });

            c.Invoke();

            Assert.AreEqual("<Response><Say>Hello</Say></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        }

    }

}
