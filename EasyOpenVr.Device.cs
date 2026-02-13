using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class DeviceMethods(EasyOpenVr evr)
    {
        #region tracking

        public TrackedDevicePose_t[] GetDeviceToAbsoluteTrackingPose(
            ETrackingUniverseOrigin origin = ETrackingUniverseOrigin.TrackingUniverseStanding)
        {
            var trackedDevicePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(origin, 0.0f, trackedDevicePoses);
            return trackedDevicePoses;
        }

        #endregion

        #region controller

        /*
         * Includes things like analogue axes of triggers, pads & sticks
         * OBS: Deprecated
         */
        public VRControllerState_t GetControllerState(uint index)
        {
            VRControllerState_t state = new VRControllerState_t();
            var success = OpenVR.System.GetControllerState(index, ref state, (uint)Marshal.SizeOf(state));
            if (!success) evr.DebugLog("Failure getting ControllerState");
            return state;
        }

        /**
         * Will return the index of the role if found.
         * Useful if you want to know which controller is right or left.
         * Note: Will eventually be removed as it has now been deprecated.
         */
        public uint GetIndexForControllerRole(ETrackedControllerRole role)
        {
            return OpenVR.System.GetTrackedDeviceIndexForControllerRole(role);
        }

        #endregion

        #region tracked_device

        public uint[] GetIndexesForTrackedDeviceClass(ETrackedDeviceClass _class)
        {
            // Not sure how this one works, no ref? Skip for now.
            // var result = new uint[OpenVR.k_unMaxTrackedDeviceCount];
            // var count = OpenVR.System.GetSortedTrackedDeviceIndicesOfClass(_class, result, uint.MaxValue);
            var result = new List<uint>();
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (GetTrackedDeviceClass(i) == _class) result.Add(i);
            }

            return result.ToArray();
        }

        public ETrackedDeviceClass GetTrackedDeviceClass(uint index)
        {
            return OpenVR.System.GetTrackedDeviceClass(index);
        }

        /*
         * Example of property: ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float
         */
        public float GetFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
        {
            var error = new ETrackedPropertyError();
            var result = OpenVR.System.GetFloatTrackedDeviceProperty(index, property, ref error);
            evr.DebugLog(error, property);
            return result;
        }

        /*
         * Example of property: ETrackedDeviceProperty.Prop_SerialNumber_String
         */
        public string GetStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
        {
            var error = new ETrackedPropertyError();
            StringBuilder sb = new StringBuilder((int)OpenVR.k_unMaxPropertyStringSize);
            OpenVR.System.GetStringTrackedDeviceProperty(index, property, sb, OpenVR.k_unMaxPropertyStringSize, ref error);
            evr.DebugLog(error);
            return sb.ToString();
        }


        /*
         * Example of property: ETrackedDeviceProperty.Prop_EdidProductID_Int32
         */
        public int GetIntegerTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
        {
            var error = new ETrackedPropertyError();
            var result = OpenVR.System.GetInt32TrackedDeviceProperty(index, property, ref error);
            evr.DebugLog(error);
            return result;
        }

        /*
         * Example of property: ETrackedDeviceProperty.Prop_CurrentUniverseId_Uint64
         */
        public ulong GetLongTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
        {
            var error = new ETrackedPropertyError();
            var result = OpenVR.System.GetUint64TrackedDeviceProperty(index, property, ref error);
            evr.DebugLog(error);
            return result;
        }

        /*
         * Example of property: ETrackedDeviceProperty.Prop_ContainsProximitySensor_Bool
         */
        public bool GetBooleanTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
        {
            var error = new ETrackedPropertyError();
            var result = OpenVR.System.GetBoolTrackedDeviceProperty(index, property, ref error);
            evr.DebugLog(error);
            return result;
        }

        // TODO: This has apparently been deprecated, figure out how to do it with the new input system.
        public void TriggerHapticPulseInController(ETrackedControllerRole role, ushort durationMicroSec = 10000)
        {
            var index = GetIndexForControllerRole(role);
            OpenVR.System.TriggerHapticPulse(index, 0,
                durationMicroSec); // This works: https://github.com/ValveSoftware/openvr/wiki/IVRSystem::TriggerHapticPulse
        }

        public InputOriginInfo_t GetOriginTrackedDeviceInfo(ulong originHandle)
        {
            var info = new InputOriginInfo_t();
            var error = OpenVR.Input.GetOriginTrackedDeviceInfo(originHandle, ref info, (uint)Marshal.SizeOf(info));
            evr.DebugLog(error);
            return info;
        }

        public EDeviceActivityLevel GetTrackedDeviceActivityLevel(uint index)
        {
            return OpenVR.System.GetTrackedDeviceActivityLevel(index);
        }

        #endregion
    }
}