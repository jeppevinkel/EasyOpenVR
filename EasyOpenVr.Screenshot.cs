using System;
using System.IO;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class ScreenshotMethods(EasyOpenVr evr)
    {
        public readonly record struct ScreenshotResult( 
            uint Handle,
            EVRScreenshotType Type,
            string FilePath,
            string FilePathVr
        );

        /*
         * Set screenshot path, if not set they will end up in: %programfiles(x86)%\Steam\steamapps\common\SteamVR\bin\
         * Returns false if the directory does not exist.
         */
        private string _screenshotPath = "";

        public bool SetScreenshotOutputFolder(string path)
        {
            var exists = Directory.Exists(path);
            if (exists) _screenshotPath = path;
            return exists;
        }

        /*
         * Hooks the screenshot function so it overrides the built-in screenshot shortcut in SteamVR!
         * Listen to the VREvent_ScreenshotTriggered event to know when to acquire a screenshot.
         */
        public EasyOpenVrResult HookScreenshots()
        {
            EVRScreenshotType[] arr = { EVRScreenshotType.Stereo };
            var error = OpenVR.Screenshots.HookScreenshot(arr);
            return evr.DebugLog(error);
        }

        private Tuple<string, string> GetScreenshotPaths(string prefix, string postfix,
            string timestampFormat = "yyyyMMdd_HHmmss_fff")
        {
            var screenshotPath = _screenshotPath;
            if (screenshotPath != string.Empty) screenshotPath = $"{screenshotPath}\\";
            if (prefix != string.Empty) prefix = $"{prefix}_";
            if (postfix != string.Empty) postfix = $"_{postfix}";
            var timestamp = DateTime.Now.ToString(timestampFormat);

            var filePath = $"{screenshotPath}{prefix}{timestamp}{postfix}";
            var filePathVR = $"{screenshotPath}{prefix}{timestamp}_vr{postfix}";

            return new Tuple<string, string>(filePath, filePathVR);
        }

        /**
         * Takes a stereo screenshot, works with all applications as it grabs render output directly.
         *
         * OBS: Requires a scene application to be running, else screenshot functionality will stop working.
         */
        public EasyOpenVrResult TakeScreenshot(
            out ScreenshotResult? screenshotResult,
            string prefix = "",
            string postfix = "")
        {
            uint handle = 0;
            var filePaths = GetScreenshotPaths(prefix, postfix);
            var type = EVRScreenshotType.Stereo;
            var error = OpenVR.Screenshots.TakeStereoScreenshot(ref handle, filePaths.Item1, filePaths.Item2);
            screenshotResult =
                error == EVRScreenshotError.None
                    ? new ScreenshotResult(handle, type, filePaths.Item1, filePaths.Item2)
                    : null;
            return evr.DebugLog(error);
        }

        /**
         * Use this to request other types of screenshots.
         *
         * OBS: This will NOT WORK if you have hooked the system screenshot function,
         * it will seemingly leave a screenshot request in limbo preventing future screenshots.
         */
        public EasyOpenVrResult RequestScreenshot(
            out ScreenshotResult? screenshotResult,
            string prefix = "",
            string postfix = "",
            EVRScreenshotType screenshotType = EVRScreenshotType.Stereo)
        {
            var filePaths = GetScreenshotPaths(prefix, postfix);
            uint handle = 0;
            var error = OpenVR.Screenshots.RequestScreenshot(ref handle, screenshotType, filePaths.Item1, filePaths.Item2);
            screenshotResult =
                error == EVRScreenshotError.None
                    ? new ScreenshotResult(handle, screenshotType, filePaths.Item1, filePaths.Item2)
                    : null;
            return evr.DebugLog(error);
        }

        /*
         * This will attempt to submit the screenshot to Steam to be in the screenshot library for the current scene application.
         */
        public EasyOpenVrResult SubmitScreenshotToSteam(ScreenshotResult screenshotResult)
        {
            var error = OpenVR.Screenshots.SubmitScreenshot(
                screenshotResult.Handle,
                screenshotResult.Type,
                $"{screenshotResult.FilePath}.png",
                $"{screenshotResult.FilePathVr}.png"
            );
            return evr.DebugLog(error);
        }
    }
}