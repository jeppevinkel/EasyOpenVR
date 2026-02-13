using EasyOpenVR.Extensions;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class ChaperoneMethods(EasyOpenVr evr)
    {
        public HmdQuad_t GetPlayAreaRect()
    {
        HmdQuad_t rect = new HmdQuad_t();
        var success = OpenVR.Chaperone.GetPlayAreaRect(ref rect);
        if (!success) evr.DebugLog("Failure getting PlayAreaRect");
        return rect;
    }

    public HmdVector2_t GetPlayAreaSize()
    {
        var size = new HmdVector2_t();
        var success = OpenVR.Chaperone.GetPlayAreaSize(ref size.v0, ref size.v1);
        if (!success) evr.DebugLog("Failure getting PlayAreaSize");
        return size;
    }

    public HmdMatrix34_t GetOriginPose()
    {
        var trackingSpace = OpenVR.Compositor.GetTrackingSpace();
        var originPose = new HmdMatrix34_t();
        switch(trackingSpace) {
            default:
            case ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated:
                break;
            case ETrackingUniverseOrigin.TrackingUniverseStanding: 
                OpenVR.ChaperoneSetup.GetWorkingStandingZeroPoseToRawTrackingPose(ref originPose);
                break;
            case ETrackingUniverseOrigin.TrackingUniverseSeated: OpenVR.ChaperoneSetup.GetWorkingSeatedZeroPoseToRawTrackingPose(ref originPose);
                break;
        }

        return originPose;
    }

    /**
     * Will move (meters) and rotate (degrees) the working copy of the current ChaperoneSetup after having retrieved the current values.
     */
    public void ModifyUniverse(HmdVector3_t offset, float rotate, bool showPreview = true)
    {
        var originPose = GetOriginPose();
        ModifyUniverse(offset, rotate, originPose, originPose, showPreview);
    }

    /**
     * Will move (meters) and rotate (degrees) the working copy of the current ChaperoneSetup based on the provided values.
     */
    public void ModifyUniverse(HmdVector3_t offset, float rotate, HmdMatrix34_t originPose, HmdMatrix34_t correctionPose, bool showPreview = true)
    {
        offset = offset.Rotate(correctionPose);
        var trackingSpace = OpenVR.Compositor.GetTrackingSpace();
        var pose = originPose.Translate(offset).RotateY(rotate);
        switch (trackingSpace)
        {
            case ETrackingUniverseOrigin.TrackingUniverseStanding:
                OpenVR.ChaperoneSetup.SetWorkingStandingZeroPoseToRawTrackingPose(ref pose);
                break;
            case ETrackingUniverseOrigin.TrackingUniverseSeated:
                OpenVR.ChaperoneSetup.SetWorkingSeatedZeroPoseToRawTrackingPose(ref pose);
                break;
        }
        if(showPreview) OpenVR.ChaperoneSetup.ShowWorkingSetPreview();
    }
    
    /**
     * Will hide the preview and reset the working copy of the ChaperoneSetup to the current live settings.
     */
    public void ResetUniverse()
    {
        OpenVR.ChaperoneSetup.HideWorkingSetPreview();
        OpenVR.ChaperoneSetup.RevertWorkingCopy();
    }

    /**
     * Will save the working copy of the ChaperoneSetup to disk.
     */
    public bool SaveUniverse(EChaperoneConfigFile file = EChaperoneConfigFile.Live)
    {
        return OpenVR.ChaperoneSetup.CommitWorkingCopy(file);
    }

    public bool ModifyChaperoneBounds(HmdVector3_t offset)
    {
        var success = OpenVR.ChaperoneSetup.GetWorkingCollisionBoundsInfo(out var physQuad);
        ModifyChaperoneBounds(offset, physQuad);
        
        if (!success) evr.DebugLog("Failure to load Chaperone bounds.");
        return success;
    }

    public void ModifyChaperoneBounds(HmdVector3_t offset, HmdQuad_t[] physQuad) {
        for (var i = 0; i < physQuad.Length; i++)
        {
            MoveCorner(ref physQuad[i].vCorners0);
            MoveCorner(ref physQuad[i].vCorners1);
            MoveCorner(ref physQuad[i].vCorners2);
            MoveCorner(ref physQuad[i].vCorners3);
        }
        OpenVR.ChaperoneSetup.SetWorkingCollisionBoundsInfo(physQuad);
        return;

        void MoveCorner(ref HmdVector3_t corner)
        {
            // Will not change points at vertical 0, that's the bottom of the Chaperone.
            // This as it appears the bottom gets reset to 0 at a regular interval anyway.
            corner.v0 += offset.v0;
            if (corner.v1 != 0) corner.v1 += offset.v1;
            corner.v2 += offset.v2;
        }
    }
    }
}