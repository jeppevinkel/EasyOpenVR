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

    /// <summary>
    /// Set the frequency at which VR events, input events and transforms are read from the runtime
    /// as well as the rate at which overlays and play space animations are run at.
    /// </summary>
    /// <remarks>
    /// <para>If this is not set the pump is disabled, and these things will have to be triggered in external code.</para>
    /// The pumpValue is used differently depending on the chosen interval.
    /// <list type="bullet">
    /// <item><description>FractionOfHmdHz : Hz = HmdHz / value</description></item>
    /// <item><description>FixedHz : Hz = value</description></item>
    /// <item><description>Millisecond : Hz = 1000 / value</description></item>
    /// </list>
    /// <b>Note</b>: Running this above headset display frequency can bog down the runtime. 
    /// </remarks>
    public EasyOpenVrBuilder SetPumpInterval(EasyOpenVr.EPumpInterval pumpInterval, double pumpValue)
    {
        _initParams.PumpInterval = pumpInterval;
        _initParams.PumpValue = pumpValue;
        return this;
    }
    
    public EasyOpenVrBuilder QuitWithRuntime()
    {
        _initParams.QuitWithRuntime = true;
        return this;
    }
    
    #endregion
}