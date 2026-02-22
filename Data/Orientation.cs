using Valve.VR;

namespace EasyOpenVR.Data;

public class Orientation
{
    public double Yaw;
    public double Pitch;
    public double Roll;

    public Orientation()
    {
    }

    public Orientation(double yaw, double pitch, double roll)
    {
        this.Yaw = yaw;
        this.Pitch = pitch;
        this.Roll = roll;
    }

    public Orientation(HmdVector3_t vec)
    {
        Pitch = vec.v0;
        Yaw = vec.v1;
        Roll = vec.v2;
    }
}