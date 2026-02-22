using EasyOpenVR.Data;
using Valve.VR;

namespace EasyOpenVR;

/**
 * Used to set up and instantiation an EasyOpenVr object.
 */
public class EasyOpenVrBuilder
{
    private EasyOpenVr.EasyOpenVrInitParams _initParams;
    
    /**
     * Build an instance but do not initialize it.
     */
    private EasyOpenVr Build()
    {
        return new EasyOpenVr(_initParams);
    }
    
    /**
     * Build an instance and immediately initialize it.
     */
    public EasyOpenVr BuildAndInit()
    {
        var vr = Build();
        vr.InitWorkerThread();
        return vr;
    }
    
    #region Setters
    /**
     * This will output debug information in the log output as well as the through the listener.
     */
    public EasyOpenVrBuilder SetDebug(bool debug)
    {
        _initParams.Debug = debug;
        return this;
    }

    /**
     * Various application types provide different features.
     * Most commonly, to enable auto launching Overlay appears important.
     */
    public EasyOpenVrBuilder SetApplicationType(EVRApplicationType appType)
    {
        _initParams.ApplicationType = appType;
        return this;
    }
    
    /**
     * Will register the application to launch with the runtime.
     * Requires a VRAppManifest to have been registered, set the path using this builder.
     * When forced, it will unregister and re-register to ensure auto-launch registration.
     */
    public EasyOpenVrBuilder SetRegisterAutoLaunch(bool force)
    {
        _initParams.RegisterAutoLaunch = true;
        _initParams.ForceAutoLaunch = force;
        return this;
    }
    
    /**
     * The VR app manifest is required to register an application for auto launch.
     */
    public EasyOpenVrBuilder SetVrAppManifestPath(string path)
    {
        _initParams.VrAppManifestPath = path.Trim();
        return this;
    }

    /**
     * The action manifest is required for the application to listen to inputs.
     */
    public EasyOpenVrBuilder SetActionManifestPath(string path)
    {
        _initParams.ActionManifestPath = path.Trim();
        return this;
    }

    /**
     * Set the frequency at which events, input events and transforms are read from the runtime.
     * This runs an internal worker that automatically fetches data at the defined interval.
     * This defaults to the same interval as the headset display frequency.
     */
    public EasyOpenVrBuilder SetPumpMode(EasyOpenVr.EPumpMode pumpMode)
    {
        _initParams.PumpMode = pumpMode;
        return this;
    }
    #endregion
}