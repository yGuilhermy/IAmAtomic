using System;
namespace IAmAtomic
{
    public static class AtomicTiming
    {
        public const double BlastImpactOffset = 1.08;
        public const double ScheduleLead = 0.06;
        public static double ImpactDeadline(double scheduledStart, double audioOffset, double outputLatency, double adjustment)
        {
            return Math.Max(scheduledStart, scheduledStart + Math.Max(0, audioOffset) + Math.Max(0, Math.Min(0.15, outputLatency)) + Math.Max(-0.15, Math.Min(0.25, adjustment)));
        }
    }
    public sealed class AtomicImpactGate
    {
        private double deadline;
        private bool pending;
        public void Arm(double time) { deadline = time; pending = true; }
        public void Cancel() { pending = false; }
        public bool ConsumeIfDue(double dspTime)
        {
            if (!pending || dspTime < deadline) return false;
            pending = false;
            return true;
        }
    }
    public static class AtomicModes
    {
        public static string Normalize(string mode) { return mode == "HoldRelease" ? "HoldRelease" : "Direct"; }
    }
    public static class AtomicVisualBudget
    {
        public static int Lines(int quality) { return quality <= 0 ? 32 : quality == 1 ? 64 : 96; }
        public static int Particles(int quality) { return quality <= 0 ? 96 : quality == 1 ? 192 : 288; }
    }
}
