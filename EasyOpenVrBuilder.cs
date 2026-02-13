using Valve.VR;

namespace EasyOpenVR;

/**
 * Used to set up and instantiation an EasyOpenVr object.
 */
public class EasyOpenVrBuilder
{
    private EasyOpenVr.EasyOpenVrInitParams _initParams;

    public EasyOpenVrBuilder()
    {
    }

    /**
     * Build an instance but do not initialize it yet.
     */
    public EasyOpenVr Build()
    {
        return new EasyOpenVr(_initParams);
    }
    
    /**
     * Build an instance and immediately initialize it.
     */
    public EasyOpenVr BuildAndInit()
    {
        var vr = Build();
        vr.Init();
        return vr;
    }
    
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
     * Will register the application to launch with OpenVR.
     * Requires a VRAppManifest to have been registered.
     * When forced, it will unregister and re-register just to force auto-launch registration.
     */
    public EasyOpenVrBuilder SetRegisterAutoLaunch(bool force = false)
    {
        _initParams.RegisterAutoLaunch = true;
        _initParams.ForceAutoLaunch = force;
        return this;
    }
}