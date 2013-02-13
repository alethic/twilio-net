using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;
using System.Xml.Linq;

namespace Twilio.Activities
{

    public abstract class TwilioHttpInstanceStore : InstanceStore
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

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="httpContext"></param>
        internal TwilioHttpInstanceStore(HttpContext httpContext)
            : this(httpContext, Guid.NewGuid())
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="ownerInstanceId"></param>
        internal TwilioHttpInstanceStore(HttpContext httpContext, Guid ownerInstanceId)
        {
            HttpContext = httpContext;
            OwnerInstanceId = ownerInstanceId;
        }

        protected HttpContext HttpContext { get; private set; }

        protected Guid OwnerInstanceId { get; private set; }

        protected override bool TryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
        {
            return EndTryCommand(BeginTryCommand(context, command, timeout, null, null));
        }

        protected override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (command is CreateWorkflowOwnerCommand)
                context.BindInstanceOwner(OwnerInstanceId, Guid.NewGuid());
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
        /// Loads the serialized state document using the selected storage mode.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        protected abstract XDocument LoadFromContext(Guid instanceId);

        /// <summary>
        /// Saves the serialized state document using the selected storage mode.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="doc"></param>
        protected abstract void SaveToContext(Guid instanceId, XDocument doc);

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