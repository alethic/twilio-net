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
                Activities =
                {
                    new DialNumber()
                    {
                        Number = "+15555555555",
                    },
                },
            });

            c.Invoke();

            Assert.AreEqual("<Response><Dial>+15555555555</Dial></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        }

        [TestMethod]
        public void DialSipTest()
        {
            var c = new DelegateInArgument<CallContext>();

            var a = new CallScope()
            {
                Body = new ActivityAction<CallContext>()
                {
                    Argument = c,
                    Handler = new Dial()
                    {
                        Activities =
                        {
                            new DialSip()
                            {
                                Uris =
                                {
                                    new DialSipUri()
                                    {
                                        Uri = "test1@test.com",
                                        UserName = "test1",
                                    },
                                    new DialSipUri()
                                    {
                                        Uri = "test2@test.com",
                                        UserName = "test2",
                                    },
                                },
                            },
                        },
                    },
                },
            };

            var ctx = CreateContext(a);
            ctx.Invoke();

            Assert.AreEqual("<Response><Dial>+15555555555</Dial></Response>", ctx.Response.ToString(SaveOptions.DisableFormatting));
        }

        //[TestMethod]
        //public void DialSipTest2()
        //{
        //    var c = CreateContext(new SampleActivity1());

        //    // cannot work without bookmarks
        //    //    Assert.Fail();

        //    c.Invoke();

        //    Assert.AreEqual("<Response><Dial>+15555555555</Dial></Response>", c.Response.ToString(SaveOptions.DisableFormatting));
        //}

    }

}
