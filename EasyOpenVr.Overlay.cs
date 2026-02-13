using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using EasyOpenVR.Utils;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class OverlayMethods(EasyOpenVr evr)
    {
        /// <summary>
        /// Creates an overlay that will show up in the headset if you draw to it
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <param name="title"></param>
        /// <param name="transform">Get an empty transform from Utils.GetEmptyTransform</param>
        /// <param name="width">Default is 1, height is derived from the texture aspect ratio and the width</param>
        /// <param name="anchor">Default is none, else index for which tracked device to attach overlay to</param>
        /// <param name="origin">If we have no anchor, we need an origin to set position, defaults to standing</param>
        /// <returns>0 if we failed to create an overlay</returns>
        public ulong CreateOverlay(string uniqueKey, string title, HmdMatrix34_t transform, float width = 1,
            uint anchor = uint.MaxValue, ETrackingUniverseOrigin origin = ETrackingUniverseOrigin.TrackingUniverseStanding)
        {
            ulong handle = 0;
            var error = OpenVR.Overlay.CreateOverlay(uniqueKey, title, ref handle);
            if (error == EVROverlayError.None)
            {
                OpenVR.Overlay.SetOverlayWidthInMeters(handle, width);
                if (anchor != uint.MaxValue)
                    OpenVR.Overlay.SetOverlayTransformTrackedDeviceRelative(handle, anchor, ref transform);
                else OpenVR.Overlay.SetOverlayTransformAbsolute(handle, origin, ref transform);
            }

            evr.DebugLog(error);
            return handle;
        }

        public EasyOpenVrResult SetOverlayTransform(ulong handle, HmdMatrix34_t transform, uint anchor = uint.MaxValue,
            ETrackingUniverseOrigin origin = ETrackingUniverseOrigin.TrackingUniverseStanding)
        {
            EVROverlayError error;
            if (anchor != uint.MaxValue)
                error = OpenVR.Overlay.SetOverlayTransformTrackedDeviceRelative(handle, anchor, ref transform);
            else error = OpenVR.Overlay.SetOverlayTransformAbsolute(handle, origin, ref transform);
            return evr.DebugLog(error);
        }

        public EasyOpenVrResult SetOverlayTextureFromFile(ulong handle, string path)
        {
            var error = OpenVR.Overlay.SetOverlayFromFile(handle, path);
            return evr.DebugLog(error);
        }

        /// <summary>
        /// Preliminiary as I have yet to figure out how to make my own textures at runtime.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        public EasyOpenVrResult SetOverlayTexture(ulong handle, ref Texture_t texture)
        {
            // DXGI_FORMAT_R8G8B8A8_UNORM 
            var error = OpenVR.Overlay.SetOverlayTexture(handle, ref texture);
            return evr.DebugLog(error);
        }

        /// <summary>
        /// Sets raw overlay pixels from Bitmap, appears to crash íf going above 1mpix or near that.
        /// It's also said to be super inefficient by Valve themselves, so never use this for frequently updating overlays.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="bmp"></param>
        public void SetOverlayPixels(ulong handle, Bitmap bmp)
        {
            BitmapUtils.PointerFromBitmap(bmp, true, (pointer) =>
            {
                var bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
                var error = OpenVR.Overlay.SetOverlayRaw(handle, pointer, (uint)bmp.Width, (uint)bmp.Height,
                    (uint)bytesPerPixel);
            });
        }

        public HmdMatrix34_t GetOverlayTransform(ulong handle,
            ETrackingUniverseOrigin origin = ETrackingUniverseOrigin.TrackingUniverseStanding)
        {
            var transform = new HmdMatrix34_t();
            var error = OpenVR.Overlay.GetOverlayTransformAbsolute(handle, ref origin, ref transform);
            evr.DebugLog(error);
            return transform;
        }

        /// <summary>
        /// Sets the alpha of the overlay
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="alpha">Normalized 0.0-1.0</param>
        /// <returns></returns>
        public EasyOpenVrResult SetOverlayAlpha(ulong handle, float alpha)
        {
            var error = OpenVR.Overlay.SetOverlayAlpha(handle, alpha);
            return evr.DebugLog(error);
        }

        public EasyOpenVrResult SetOverlayWidth(ulong handle, float width)
        {
            var error = OpenVR.Overlay.SetOverlayWidthInMeters(handle, width);
            return evr.DebugLog(error);
        }

        public EasyOpenVrResult SetOverlayVisibility(ulong handle, bool visible)
        {
            EVROverlayError error;
            if (visible) error = OpenVR.Overlay.ShowOverlay(handle);
            else error = OpenVR.Overlay.HideOverlay(handle);
            return evr.DebugLog(error);
        }

        /**
         * Will have to explore this at a later date, right now my overlays are non-interactive.
         */
        public VREvent_t[] GetNewOverlayEvents(ulong overlayHandle)
        {
            var vrEvents = new List<VREvent_t>();
            var vrEvent = new VREvent_t();
            var eventSize = (uint)Marshal.SizeOf(vrEvent);
            while (OpenVR.Overlay.PollNextOverlayEvent(overlayHandle, ref vrEvent, eventSize))
            {
                vrEvents.Add(vrEvent);
            }

            return vrEvents.ToArray();
        }

        public ulong FindOverlay(string uniqueKey)
        {
            ulong handle = 0;
            var error = OpenVR.Overlay.FindOverlay(uniqueKey, ref handle);
            evr.DebugLog(error);
            return handle;
        }

        public class OverlayTextureSize
        {
            public uint width;
            public uint height;
            public float aspectRatio;
        }

        public OverlayTextureSize GetOverlayTextureSize(ulong handle)
        {
            uint width = 0;
            uint height = 0;
            var error = OpenVR.Overlay.GetOverlayTextureSize(handle, ref width, ref height);
            evr.DebugLog(error);
            return (width == 0 || height == 0)
                ? new OverlayTextureSize()
                : new OverlayTextureSize { width = width, height = height, aspectRatio = (float)width / (float)height };
        }
    }
}