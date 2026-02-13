using System.Runtime.InteropServices;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class StatisticsMethods(EasyOpenVr evr)
    {
        public Compositor_CumulativeStats GetCumulativeStats()
        {
            var stats = new Compositor_CumulativeStats();
            OpenVR.Compositor.GetCumulativeStats(ref stats, (uint)Marshal.SizeOf(stats));
            return stats;
        }

        public Compositor_FrameTiming GetFrameTiming()
        {
            var timing = new Compositor_FrameTiming();
            timing.m_nSize = (uint)Marshal.SizeOf(timing);
            var success = OpenVR.Compositor.GetFrameTiming(ref timing, 0);
            if (!success) evr.DebugLog("Could not get frame timing.");
            return timing;
        }

        public Compositor_FrameTiming[] GetFrameTimings(uint count)
        {
            var timings = new Compositor_FrameTiming[count];
            var resultCount = OpenVR.Compositor.GetFrameTimings(timings);
            if (resultCount == 0) evr.DebugLog("Could not get frame timings.");
            return timings;
        }
    }
}