using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using EasyOpenVR.Extensions;
using EasyOpenVR.Utils;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public ChaperoneMethods Chaperone { get; }
    public DeviceMethods Device { get; }
    public EventMethods Event { get; }
    public InputMethods Input { get; }
    public NotificationMethods Notification { get; }
    public OverlayMethods Overlay { get; }
    public ScreenshotMethods Screenshot { get; }
    public SettingMethods Setting { get; }
    public StatisticsMethods Statistics { get; }
    public SystemMethods System { get; }
    public VideoMethods Video { get; }

    public record struct EasyOpenVrInitParams(
        EVRApplicationType ApplicationType,
        string VrAppManifestPath,
        bool Debug,
        bool RegisterAutoLaunch,
        bool ForceAutoLaunch
    );

    public record struct EasyOpenVrResult(
        Enum? Error,
        Enum? Value,
        string Message = ""
    )
    {
        public int ErrorOrdinal => Error == null ? -1 : (int) Convert.ChangeType(Error, Error.GetTypeCode());
        public string ErrorType => Error == null ? "" : Error.GetType().Name; 
        public string ErrorName => Error == null ? "" : Enum.GetName(Error.GetType(), Error) ?? "";
        public int ValueOrdinal => Value == null ? -1 : (int)Convert.ChangeType(Value, Value.GetTypeCode());
        public string ValueType => Value == null ? "" : Value.GetType().Name;
        public string ValueName => Value == null ? "" : Enum.GetName(Value.GetType(), Value) ?? "";
        public bool Success => ErrorOrdinal == 0;
    };
    
    public EasyOpenVr(EasyOpenVrInitParams initParams)
    {
        _initParams = initParams;
        Chaperone = new ChaperoneMethods(this);
        Device = new DeviceMethods(this);
        Event = new EventMethods(this);
        Input = new InputMethods(this);
        Notification = new NotificationMethods(this);
        Overlay = new OverlayMethods(this);
        Screenshot = new ScreenshotMethods(this);
        Setting = new SettingMethods(this);
        Statistics = new StatisticsMethods(this);
        System = new SystemMethods(this);
        Video = new VideoMethods(this);
    }

    private readonly EasyOpenVrInitParams _initParams;
    private readonly Random _random = new();
    
    #region Events

    public delegate void DebugMessageHandler(string message);
    public event DebugMessageHandler? DebugMessage;
    private void OnDebugMessage(string message)
    {
        DebugMessage?.Invoke(message);
    }
    #endregion

    #region init

    private uint _initState = 0;

    public bool Init()
    {
        var error = EVRInitError.Unknown;
        try
        {
            _initState = OpenVR.InitInternal(ref error, _initParams.ApplicationType);

            if (_initParams.VrAppManifestPath.Trim().Length > 0)
            {
                System.LoadAppManifest(_initParams.VrAppManifestPath);
            }
        }
        catch (Exception e)
        {
            DebugLog(e, "You might be building for 32bit with a 64bit .dll, error");
        }

        DebugLog(error);
        return error == EVRInitError.None && _initState > 0;
    }

    public bool IsInitialized()
    {
        return _initState > 0;
    }

    #endregion
    
    #region private_utils

    private void DebugLog(string message)
    {
        if (!_initParams.Debug) return;
        
        var st = new StackTrace();
        var sf = st.GetFrame(1);
        var methodName = sf.GetMethod().Name;
        var text = $"{methodName}: {message}";
        OnDebugMessage(text);
    }

    private EasyOpenVrResult DebugLog(Enum errorEnum, string message = "error")
    {
        var result = new EasyOpenVrResult(errorEnum, null);
        if (!_initParams.Debug || result.Success) return result;
        
        var stackTrace = new StackTrace();
        var stackFrame = stackTrace.GetFrame(1);
        var methodName = stackFrame.GetMethod().Name;
        var text = $"{methodName} {message}: {result.ErrorType}.{result.ErrorName} ({result.ErrorOrdinal})";
        OnDebugMessage(text);
        result.Message = text;
        return result;
    }

    private EasyOpenVrResult DebugLog(Enum errorEnum, Enum valueEnum)
    {
        var result = new EasyOpenVrResult(errorEnum, valueEnum);
        if (!_initParams.Debug || result.Success) return result;
        
        var stackTrace = new StackTrace();
        var stackFrame = stackTrace.GetFrame(1);
        var methodName = stackFrame.GetMethod().Name;
        var text =
            $"{methodName} {result.ValueType}.{result.ValueName}: {result.ErrorType}.{result.ErrorName}";
        OnDebugMessage(text);
        result.Message = text;
        return result;
    }

    private EasyOpenVrResult DebugLog(Exception e, string message = "error")
    {
        if (!_initParams.Debug) return new EasyOpenVrResult(null, null);
        
        var st = new StackTrace();
        var sf = st.GetFrame(1);
        var methodName = sf.GetMethod().Name;
        var text = $"{methodName} {message}: {e.Message}";
        var result = new EasyOpenVrResult(null, null, text);
        OnDebugMessage(text);
        return result;
    }

    #endregion
}