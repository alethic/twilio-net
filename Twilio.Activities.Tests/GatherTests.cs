using System.Collections.Generic;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Twilio.Activities.Tests
{

    [TestClass]
    public class GatherTests : TwilioTest
    {

        [TestMethod]
        public void GatherTest()
        {
            var c = CreateContext(new Gather()
            {
                NumDigits = 1,
            });

            // cannot work, gather requires bookmarks
            Assert.Fail();

            c.Invoker.Invoke();

            Assert.AreEqual("<Response><Say>Hello</Say></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        }

    }

}
