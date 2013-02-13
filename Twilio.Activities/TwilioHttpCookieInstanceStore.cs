using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.DurableInstancing;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Twilio.Activities
{

    /// <summary>
    /// Serializes workflow state to cookies.
    /// </summary>
    public class TwilioHttpCookieInstanceStore : TwilioHttpInstanceStore
    {

        /// <summary>
        /// Maximum byte size of cookies.
        /// </summary>
        const int COOKIE_SIZE = 2048;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="httpContext"></param>
        public TwilioHttpCookieInstanceStore(HttpContext httpContext)
            : base(httpContext)
        {

        }

        protected override void SaveToContext(Guid instanceId, XDocument doc)
        {
            // expire all existing related cookies
            for (int i = 0; i < 8; i++)
            {
                var cki = HttpContext.Request.Cookies[string.Format("WF_{0}_p{1}", instanceId, i)];
                if (cki != null)
                {
                    cki.Expires = DateTime.Now.AddYears(-1);
                    HttpContext.Response.SetCookie(cki);
                }
            }

            using (var stm = new MemoryStream())
            {
                // write XML document to compressed stream
                using (var gzp = new GZipStream(stm, CompressionMode.Compress, true))
                using (var wrt = XmlDictionaryWriter.CreateTextWriter(gzp))
                    doc.Root.WriteTo(wrt);

                // dump bytes
                var dat = stm.ToArray();

                // set cookies for data chunks
                for (int i = 0; i * COOKIE_SIZE < dat.Length; i++)
                {
                    var cki = new HttpCookie(string.Format("WF_{0}_p{1}", instanceId, i));
                    cki.Value = Convert.ToBase64String(dat, i * COOKIE_SIZE, Math.Min(COOKIE_SIZE, dat.Length - (i * COOKIE_SIZE)));
                    cki.Expires = DateTime.Now.AddMinutes(10);
                    HttpContext.Response.SetCookie(cki);
                }
            }
        }

        protected override XDocument LoadFromContext(Guid instanceId)
        {
            using (var dat = new MemoryStream())
            {
                // load all available cookie data
                for (int i = 0; i < 8; i++)
                {
                    var cki = HttpContext.Request.Cookies[string.Format("WF_{0}_p{1}", instanceId, i)];
                    if (cki == null || cki.Value == null)
                        break;

                    // append contents of cookie to data
                    var buf = Convert.FromBase64String(cki.Value);
                    dat.Write(buf, 0, buf.Length);
                }

                // check whether we have some data
                dat.Position = 0;
                if (dat.Length < 8)
                    throw new InstancePersistenceException("Not enough data loaded from cookies.");

                // decode and read
                using (var gzp = new GZipStream(dat, CompressionMode.Decompress, true))
                using (var rdr = XmlDictionaryReader.CreateTextReader(gzp, XmlDictionaryReaderQuotas.Max))
                {
                    rdr.MoveToContent();

                    // read in elements and wrap with new document
                    var doc = new XDocument(XElement.ReadFrom(rdr));
                    return doc;
                }
            }
        }

    }

}
