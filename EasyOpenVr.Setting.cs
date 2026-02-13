using System.Text;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class SettingMethods(EasyOpenVr evr)
    {
        /// <summary>
        ///  Fetches a settings value from the SteamVR settings
        /// </summary>
        /// <param name="section">Example: OpenVR.k_pch_CollisionBounds_Section</param>
        /// <param name="setting">Example: OpenVR.k_pch_SteamVR_SupersampleScale_Float</param>
        /// <returns>float value</returns>
        public float GetFloatSetting(string section, string setting)
        {
            var error = EVRSettingsError.None;
            var value = OpenVR.Settings.GetFloat(
                section,
                setting,
                ref error
            );
            evr.DebugLog(error);
            return value;
        }

        public EasyOpenVrResult SetFloatSetting(string section, string setting, float value)
        {
            var error = EVRSettingsError.None;
            OpenVR.Settings.SetFloat(section, setting, value, ref error);
            return evr.DebugLog(error);
        }

        public bool GetBoolSetting(string section, string setting)
        {
            var error = EVRSettingsError.None;
            var value = OpenVR.Settings.GetBool(
                section,
                setting,
                ref error
            );
            evr.DebugLog(error);
            return value;
        }

        public EasyOpenVrResult SetBoolSetting(string section, string setting, bool value)
        {
            var error = EVRSettingsError.None;
            OpenVR.Settings.SetBool(section, setting, value, ref error);
            return evr.DebugLog(error);
        }

        public int GetIntSetting(string section, string setting)
        {
            var error = EVRSettingsError.None;
            var value = OpenVR.Settings.GetInt32(
                section,
                setting,
                ref error
            );
            evr.DebugLog(error);
            return value;
        }

        public EasyOpenVrResult SetIntSetting(string section, string setting, int value)
        {
            var error = EVRSettingsError.None;
            OpenVR.Settings.SetInt32(section, setting, value, ref error);
            return evr.DebugLog(error);
        }

        public string GetStringSetting(string section, string setting)
        {
            var error = new EVRSettingsError();
            var sb = new StringBuilder((int)OpenVR.k_unMaxSettingsKeyLength);
            OpenVR.Settings.GetString(section, setting, sb, OpenVR.k_unMaxSettingsKeyLength, ref error);
            evr.DebugLog(error);
            return sb.ToString();
        }

        public EasyOpenVrResult SetStringSetting(string section, string setting, string value)
        {
            var error = EVRSettingsError.None;
            OpenVR.Settings.SetString(section, setting, value, ref error);
            return evr.DebugLog(error);
        }
    }
}