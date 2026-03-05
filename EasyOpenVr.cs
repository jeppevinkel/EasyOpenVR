using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        string? VrAppManifestPath,
        string? ActionManifestPath,
        bool Debug,
        bool RegisterAutoLaunch,
        bool ForceAutoLaunch,
        EPumpMode PumpMode
        // TODO: Add stuff for input actions? Are they one-time only set-at-start stuff? Figure this out.
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
    }

    public enum EPumpMode
    {
        HmdHz,
        HalfHmdHz,
        ThirdHmdHz,
        QuarterHmdHz,
        OncePerSecond,
        OncePerTenSeconds
    }
    
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
    /**
     * Used for mostly all debug handling in the library, to allow monitoring of internal events. 
     */
    private void OnDebugMessage(string message)
    {
        DebugMessage?.Invoke(message);
    }

    public delegate void StateHandler(bool connected);

    public event StateHandler? State;
    /**
     * Will trigger when the running state is changed.
     */
    private void OnState(bool connected)
    {
        State?.Invoke(connected);
    }
    #endregion

    #region init

    private uint _initState;

    private bool Init()
    {
        var error = EVRInitError.Unknown;
        var oldState = _initState;
        try
        {
            _initState = OpenVR.InitInternal(ref error, _initParams.ApplicationType);
        }
        catch (Exception e)
        {
            DebugLog(e, "You might be building for 32bit with a 64bit .dll, error");
        }

        var connected = error == EVRInitError.None && _initState > 0; 
        if(_initState != oldState) OnState(connected);
        DebugLog(error);
        return connected;
    }

    public bool IsInitialized()
    {
        return _initState > 0;
    }

    #endregion
    
    #region Worker
    private Thread? _workerThread;

    internal void InitWorkerThread()
    {
        Task.Delay(1000).Wait(); // Allow the API connection to complete 
        _workerThread = new Thread(Worker);
        if (!_workerThread.IsAlive) _workerThread.Start();
    }

    private void Worker()
    {
        /* TODO
         Alright, the concept here. Instead of manually keeping track of indices, new devices, events, let us keep that
         inside the library. Make the event pump MANDATORY even if at a low Hz, then have live lists of events and
         transforms and indices that are continuously updated, compared to OpenVR2WS where we have a bunch of lists. 
         */
        
        Thread.CurrentThread.IsBackground = true;
        var initComplete = false;
        while (true)
        {
            if (_initState > 0)
            {
                if (!initComplete)
                {
                    initComplete = true;
                    if (_initParams.VrAppManifestPath is { Length: > 0 })
                    {
                        System.LoadAppManifest(_initParams.VrAppManifestPath);
                        // TODO: Look over the auto-launch stuff in the call to System... it's a mess.
                    }
                    if (_initParams.ActionManifestPath is { Length: > 0 })
                    {
                        Input.LoadActionManifest(_initParams.ActionManifestPath);
                    }
                    Console.WriteLine("OK");
                }
                else
                {
                }
            }
            else
            {
                // Idle with attempted init
                Thread.Sleep(1000);
                Init();
            }
        }
    }
    #endregion
    
    #region Debug

    private void DebugLog(string message)
    {
        if (!_initParams.Debug) return;
        
        var stackTrace = new StackTrace();
        var stackFrame = stackTrace.GetFrame(1);
        var methodName = stackFrame?.GetMethod()?.Name;
        var text = $"{methodName}: {message}";
        OnDebugMessage(text);
    }

    private EasyOpenVrResult DebugLog(Enum errorEnum, string message = "error")
    {
        var result = new EasyOpenVrResult(errorEnum, null);
        if (!_initParams.Debug || result.Success) return result;
        
        var stackTrace = new StackTrace();
        var stackFrame = stackTrace.GetFrame(1);
        var methodName = stackFrame?.GetMethod()?.Name;
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
        var methodName = stackFrame?.GetMethod()?.Name;
        var text =
            $"{methodName} {result.ValueType}.{result.ValueName}: {result.ErrorType}.{result.ErrorName}";
        OnDebugMessage(text);
        result.Message = text;
        return result;
    }

    private EasyOpenVrResult DebugLog(Exception e, string message = "error")
    {
        if (!_initParams.Debug) return new EasyOpenVrResult(null, null);
        
        var stackTrace = new StackTrace();
        var stackFrame = stackTrace.GetFrame(1);
        var methodName = stackFrame?.GetMethod()?.Name;
        var text = $"{methodName} {message}: {e.Message}";
        var result = new EasyOpenVrResult(null, null, text);
        OnDebugMessage(text);
        return result;
    }

    #endregion
}