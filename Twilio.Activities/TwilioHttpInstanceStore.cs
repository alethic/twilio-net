using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Twilio.Activities
{

    class TwilioHttpInstanceStore : InstanceStore
    {

        class CompletedAsyncResult<T> : IAsyncResult
        {

            public static T End(IAsyncResult result)
            {
                return ((CompletedAsyncResult<T>)result).value;
            }

            WaitHandle wh = new ManualResetEvent(true);
            T value;
            AsyncCallback callback;
            object state;

            public CompletedAsyncResult(T value, AsyncCallback callback, object state)
            {
                this.value = value;
                this.callback = callback;
                this.state = state;
            }

            public object AsyncState
            {
                get { return state; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return wh; }
            }

            public bool CompletedSynchronously
            {
                get { return true; }
            }

            public bool IsCompleted
            {
                get { return true; }
            }

            public AsyncCallback AsyncCallback
            {
                get { return callback; }
            }

        }

        HttpContext httpContext;
        PersistenceStorageMode storageMode;
        Guid ownerInstanceId;

        public TwilioHttpInstanceStore(HttpContext httpContext, PersistenceStorageMode storageMode)
            : this(httpContext, storageMode, Guid.NewGuid())
        {

        }

        public TwilioHttpInstanceStore(HttpContext httpContext, PersistenceStorageMode storageMode, Guid ownerInstanceId)
        {
            this.httpContext = httpContext;
            this.storageMode = storageMode;
            this.ownerInstanceId = ownerInstanceId;
        }

        protected override bool TryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
        {
            return EndTryCommand(BeginTryCommand(context, command, timeout, null, null));
        }

        protected override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (command is CreateWorkflowOwnerCommand)
                context.BindInstanceOwner(ownerInstanceId, Guid.NewGuid());
            else if (command is SaveWorkflowCommand)
            {
                var saveCommand = (SaveWorkflowCommand)command;
                Save(context.InstanceView.InstanceId, saveCommand.InstanceData);
            }
            else if (command is LoadWorkflowCommand)
                context.LoadedInstance(InstanceState.Initialized, Load(context.InstanceView.InstanceId), null, null, null);
            else if (command is DeleteWorkflowOwnerCommand)
            {

            }

            return new CompletedAsyncResult<bool>(true, callback, state);
        }

        protected override bool EndTryCommand(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        /// <summary>
        /// Saves the serialized state document to a cookie.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="doc"></param>
        void SaveToCookie(Guid instanceId, XDocument doc)
        {
            // encode and write
            using (var stm = new MemoryStream())
            using (var gzp = new GZipStream(stm, CompressionMode.Compress, true))
            {
                var wrt = XmlDictionaryWriter.CreateBinaryWriter(gzp);
                doc.WriteTo(wrt);
                wrt.Flush();
                wrt.Close();

                // create and set cookie
                var cki = new HttpCookie(string.Format("WF_{0}", instanceId), Convert.ToBase64String(stm.ToArray()));
                httpContext.Response.SetCookie(cki);
            }
        }

        /// <summary>
        /// Loads the serialized state document from a cookie.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        XDocument LoadFromCookie(Guid instanceId)
        {
            // load serialized value from either response (we've already submitted it) or request
            var key = string.Format("WF_{0}", instanceId);
            var cki = httpContext.Request.Cookies[key];
            if (cki == null || cki.Value == null)
                throw new InstancePersistenceException("Could not load workflow instance from cookie.");

            // decode and read
            using (var stm = new MemoryStream( Convert.FromBase64String(cki.Value)))
            using (var gzp = new GZipStream(stm, CompressionMode.Decompress, true))
            {
                // read binary xml from compressed stream
                var rdr = XmlDictionaryReader.CreateBinaryReader(gzp, XmlDictionaryReaderQuotas.Max);
                rdr.MoveToContent();

                // read in elements and wrap with new document
                var doc = new XDocument(XElement.ReadFrom(rdr));
                return doc;
            }
        }

        /// <summary>
        /// Saves the serialized state document to the session.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="doc"></param>
        void SaveToSession(Guid instanceId, XDocument doc)
        {
            // store state in session
            httpContext.Session[string.Format("WF_{0}", instanceId)] = doc;
        }

        /// <summary>
        /// Loads the serialized state document from the session.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        XDocument LoadFromSession(Guid instanceId)
        {
            // resolve cookie data for workflow instance
            var doc = (XDocument)httpContext.Session[string.Format("WF_{0}", instanceId)];
            if (doc == null)
                throw new InstancePersistenceException("Could not load workflow instance from session.");

            return doc;
        }

        /// <summary>
        /// Loads the serialized state document using the selected storage mode.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        XDocument LoadFromContext(Guid instanceId)
        {
            switch (storageMode)
            {
                case PersistenceStorageMode.Session:
                    return LoadFromSession(instanceId);
                case PersistenceStorageMode.Cookies:
                    return LoadFromCookie(instanceId);
                default:
                    throw new InstancePersistenceException("Unknown storage mode.");
            }
        }

        /// <summary>
        /// Saves the serialized state document using the selected storage mode.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="doc"></param>
        void SaveToContext(Guid instanceId, XDocument doc)
        {
            switch (storageMode)
            {
                case PersistenceStorageMode.Session:
                    SaveToSession(instanceId, doc);
                    break;
                case PersistenceStorageMode.Cookies:
                    SaveToCookie(instanceId, doc);
                    break;
                default:
                    throw new InstancePersistenceException("Unknown storage mode.");
            }
        }

        IDictionary<XName, InstanceValue> Load(Guid instanceId)
        {
            // load from session
            var doc = LoadFromContext(instanceId);

            // to deserialize the cookie data
            var ser = new NetDataContractSerializer();

            // parse the cookie contents and deserialize keys and values
            return doc
                .Element("InstanceValues")
                .Elements("InstanceValue")
                .Select(i => new
                {
                    Key = (XName)DeserializeObject(ser, i.Element("Key").Elements().First()),
                    Value = DeserializeObject(ser, i.Element("Value").Elements().First()),
                })
                .ToDictionary(i => i.Key, i => new InstanceValue(i.Value));
        }

        object DeserializeObject(NetDataContractSerializer serializer, XElement e)
        {
            var stm = new MemoryStream();
            e.Save(stm);
            stm.Position = 0;

            return serializer.Deserialize(stm);
        }

        void Save(Guid instanceId, IDictionary<XName, InstanceValue> instanceData)
        {
            var doc = new XDocument(
                new XElement("InstanceValues",
                    instanceData.Select(i =>
                        new XElement("InstanceValue",
                            new XElement("Key", SerializeObject(i.Key)),
                            new XElement("Value", SerializeObject(i.Value.Value))))));

            // store state in session
            SaveToContext(instanceId, doc);
        }

        XElement SerializeObject(object o)
        {
            var s = new NetDataContractSerializer();
            var m = new MemoryStream();

            // serialize to stream
            s.Serialize(m, o);
            m.Position = 0;

            // parse serialized contents into new element
            return XElement.Load(m);
        }

    }

}