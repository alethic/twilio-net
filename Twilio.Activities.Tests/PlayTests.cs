using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Twilio.Activities.Tests
{

    [TestClass]
    public class PlayTests : TwilioTest
    {

        [TestMethod]
        public void PlayTest()
        {
            var c = CreateContext(new Play()
            {
                Url = "Foo.wav",
            });

            c.Invoker.Invoke();

            Assert.AreEqual("<Response><Play>Foo.wav</Play></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        }

    }

}
