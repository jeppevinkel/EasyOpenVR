using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class VideoMethods(EasyOpenVr evr)
    {
        public float GetRenderTargetForCurrentApp()
        {
            return evr.Setting.GetFloatSetting(OpenVR.k_pch_SteamVR_Section, OpenVR.k_pch_SteamVR_SupersampleScale_Float);
        }

        public bool GetSuperSamplingEnabledForCurrentApp()
        {
            return evr.Setting.GetBoolSetting(OpenVR.k_pch_SteamVR_Section, OpenVR.k_pch_SteamVR_SupersampleManualOverride_Bool);
        }

        public EasyOpenVrResult SetSuperSamplingEnabledForCurrentApp(bool enabled)
        {
            return evr.Setting.SetBoolSetting(OpenVR.k_pch_SteamVR_Section, OpenVR.k_pch_SteamVR_SupersampleManualOverride_Bool,
                enabled);
        }

        public float GetSuperSamplingForCurrentApp()
        {
            return evr.Setting.GetFloatSetting(OpenVR.k_pch_SteamVR_Section, OpenVR.k_pch_SteamVR_SupersampleScale_Float);
        }

        /**
         * Will set the render scale for the current app
         * scale 1 = 100%
         * OBS: Will enable super sampling override if it is not enabled
         */
        public EasyOpenVrResult SetSuperSamplingForCurrentApp(float scale)
        {
            return evr.Setting.SetFloatSetting(OpenVR.k_pch_SteamVR_Section, OpenVR.k_pch_SteamVR_SupersampleScale_Float, scale);
        }
    }
}