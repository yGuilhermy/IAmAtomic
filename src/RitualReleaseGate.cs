namespace IAmAtomic
{
    public enum ReleaseAction { None, Cancel, Detonate }
    public sealed class RitualReleaseGate
    {
        private bool armed;
        private float releasedFor;
        public void Reset(bool held) { armed = held; releasedFor = 0f; }
        public ReleaseAction Sample(bool held, float elapsed, float delta)
        {
            if (held) { armed = true; releasedFor = 0f; return ReleaseAction.None; }
            if (!armed) return ReleaseAction.None;
            releasedFor += delta;
            if (releasedFor < 0.02f) return ReleaseAction.None;
            armed = false;
            return ReleaseAction.Detonate;
        }
    }
}
