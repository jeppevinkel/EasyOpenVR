using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class EventMethods(EasyOpenVr evr)
    {
        internal Dictionary<EVREventType, List<Action<VREvent_t>>> _events =
            new Dictionary<EVREventType, List<Action<VREvent_t>>>();

        ///<summary>Register an event that should trigger an action, run UpdateEvents() to get new events.</summary>
        public void RegisterEvent(EVREventType type, Action<VREvent_t> action)
        {
            RegisterEvents(new EVREventType[1] { type }, action);
        }

        /**
         * Register multiple events that will trigger the same action.
         */
        public void RegisterEvents(EVREventType[] types, Action<VREvent_t> action)
        {
            foreach (var t in types)
            {
                if (!_events.ContainsKey(t)) _events.Add(t, new List<Action<VREvent_t>>());
                _events[t].Add(action);
            }
        }

        public void UnregisterEvent(EVREventType type)
        {
            UnregisterEvents([type]);
        }

        public void UnregisterEvents(EVREventType[] types)
        {
            foreach (var t in types)
            {
                if (_events.ContainsKey(t)) _events.Remove(t);
            }
        }

        /// <summary>Load new events and match them against registered events types, trigger actions.</summary>
        public void UpdateEvents(bool debugUnhandledEvents = false)
        {
            var events = GetNewEvents();
            foreach (var e in events)
            {
                var type = (EVREventType)e.eventType;
                if (_events.ContainsKey(type))
                {
                    foreach (var action in _events[type]) action.Invoke(e);
                }
                else if (debugUnhandledEvents) evr.DebugLog((EVREventType)e.eventType, "Unhandled event");
            }
        }

        ///<summary>Will get all new events in the queue, note that this will cancel out triggering any registered events when running UpdateEvents().</summary>
        public VREvent_t[] GetNewEvents()
        {
            var vrEvents = new List<VREvent_t>();
            var vrEvent = new VREvent_t();
            uint eventSize = (uint)Marshal.SizeOf(vrEvent);
            try
            {
                while (OpenVR.System.PollNextEvent(ref vrEvent, eventSize))
                {
                    vrEvents.Add(vrEvent);
                }
            }
            catch (Exception e)
            {
                evr.DebugLog(e, "Could not get new events");
            }

            return vrEvents.ToArray();
        }
    }
}