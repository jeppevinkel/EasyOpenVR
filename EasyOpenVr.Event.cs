using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class EventMethods(EasyOpenVr evr)
    {
        private readonly uint _vrEventTSize = (uint)Marshal.SizeOf(new VREvent_t());

        public delegate void VrEventHandler(in VREvent_t e);

        internal readonly Dictionary<EVREventType, List<VrEventHandler>> handlers = [];

        public void Register(VrEventHandler handler, params EVREventType[] types)
        {
            foreach (var type in types)
            {
                if (!handlers.TryGetValue(type, out var list))
                {
                    handlers[type] = list = [];
                }
                list.Add(handler);
            }
        }

        public void Unregister(VrEventHandler handler, params EVREventType[] types)
        {
            foreach (var type in types)
            {
                if (handlers.TryGetValue(type, out var list))
                {
                    list.Remove(handler);
                }
            }
        }

        private void OnEvent(ref readonly VREvent_t vrEventT)
        {
            var type = (EVREventType)vrEventT.eventType;
            if (handlers.TryGetValue(type, out var list))
            {
                foreach (var handler in CollectionsMarshal.AsSpan(list))
                {
                    handler(in vrEventT);
                }
            }
            else
            {
                evr.DebugLog("Unhandled event.");
                // TODO: Output unhandled events somehow?
            }
        }

        ///<summary>Will get all new events in the queue, note that this will cancel out triggering any registered events when running UpdateEvents().</summary>
        public void LoadAllNew()
        {
            try
            {
                var vrEvent = new VREvent_t();
                while (OpenVR.System.PollNextEvent(ref vrEvent, _vrEventTSize))
                {
                    OnEvent(ref vrEvent);
                }
            }
            catch (Exception e)
            {
                evr.DebugLog(e, "Could not get new events");
            }
        }
    }
}