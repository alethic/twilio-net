using System.Activities;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Twilio.Activities.Tests
{

    [TestClass]
    public class DialTests : TwilioTest
    {

        [TestMethod]
        public void DialNumberTest()
        {
            var c = CreateContext(new Dial()
            {
                Body = new ActivityFunc<DialBody>()
                {
                    Handler = new DialNumber()
                    {
                        Number = "+15555555555",
                    },
                },
            });

            // cannot work without bookmarks
        //    Assert.Fail();

            c.Invoker.Invoke();

            Assert.AreEqual("<Response><Dial>+15555555555</Dial></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        }

        [TestMethod]
        public void DialSipTest()
        {
            var c = CreateContext(new Dial()
            {
                Body = new ActivityFunc<DialBody>()
                {
                    Handler = new DialSip()
                    {
                        Uris = new List<ActivityFunc<DialSipBodyUri>>()
                        {
                            new ActivityFunc<DialSipBodyUri>()
                            {
                                Handler = new DialSipUri()
                                {
                                    Uri = "test1@test.com",
                                    UserName = "test1",
                                },
                            },
                            new ActivityFunc<DialSipBodyUri>()
                            {
                                Handler = new DialSipUri()
                                {
                                    Uri = "test2@test.com",
                                    UserName = "test2",
                                },
                            },
                        },
                    },
                },
            });

            // cannot work without bookmarks
            //    Assert.Fail();

            c.Invoker.Invoke();

            Assert.AreEqual("<Response><Dial>+15555555555</Dial></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        }

    }

}
