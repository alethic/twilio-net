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
                Noun = new ActivityFunc<DialNoun>()
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
                Noun = new ActivityFunc<DialNoun>()
                {
                    Handler = new DialSip()
                    {
                        Uris = new List<ActivityFunc<DialSipUriNoun>>()
                        {
                            new ActivityFunc<DialSipUriNoun>()
                            {
                                Handler = new DialSipUri()
                                {
                                    Uri = "test1@test.com",
                                    UserName = "test1",
                                },
                            },
                            new ActivityFunc<DialSipUriNoun>()
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

        [TestMethod]
        public void DialSipTest2()
        {
            var c = CreateContext(new SampleActivity1());

            // cannot work without bookmarks
            //    Assert.Fail();

            c.Invoker.Invoke();

            Assert.AreEqual("<Response><Dial>+15555555555</Dial></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        }

    }

}
