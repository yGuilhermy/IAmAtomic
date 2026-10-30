using System;
using IAmAtomic;

int count = 0;
void Check(bool condition, string name)
{
    count++;
    if (!condition) throw new Exception(name);
}

var release = new RitualReleaseGate();
release.Reset(false);
Check(release.Sample(false, 1, 0.1f) == ReleaseAction.None, "Unarmed hand");
release.Reset(true);
for (int i = 0; i < 3600; i++)
    Check(release.Sample(true, i / 90f, 1f / 90f) == ReleaseAction.None, "Held hand must not detonate");
Check(release.Sample(false, 40, 0.01f) == ReleaseAction.None, "Release debounce");
Check(release.Sample(false, 40.01f, 0.011f) == ReleaseAction.Detonate, "Confirmed release");
Check(release.Sample(false, 41, 1) == ReleaseAction.None, "Single detonation");
release.Reset(true);
release.Sample(false, 0, 0.01f);
release.Sample(true, 0.01f, 0.01f);
Check(release.Sample(false, 0.02f, 0.01f) == ReleaseAction.None, "Grip bounce resets debounce");
release.Reset(false);
Check(release.Sample(false, 1, 1) == ReleaseAction.None, "Reset cancels release");
var impact = new AtomicImpactGate();
Check(!impact.ConsumeIfDue(100), "Unarmed impact");
impact.Arm(10);
Check(!impact.ConsumeIfDue(9.99), "Impact cannot fire early");
Check(impact.ConsumeIfDue(10), "Impact at deadline");
Check(!impact.ConsumeIfDue(11), "Impact fires once");
impact.Arm(12);
impact.Cancel();
Check(!impact.ConsumeIfDue(20), "Cancellation suppresses impact");
Check(Math.Abs(AtomicTiming.ImpactDeadline(10, 1.08, 1, 1) - 11.48) < 0.000001, "Latency correction limits");
Check(AtomicTiming.ImpactDeadline(10, -2, -1, -1) == 10, "Deadline never precedes scheduling");
Check(AtomicModes.Normalize(null) == "Direct", "Default mode");
Check(AtomicModes.Normalize("HoldRelease") == "HoldRelease", "Release mode");
Check(AtomicVisualBudget.Lines(0) == 32 && AtomicVisualBudget.Lines(1) == 64 && AtomicVisualBudget.Lines(2) == 96, "Line budget");
Check(AtomicVisualBudget.Particles(0) == 96 && AtomicVisualBudget.Particles(1) == 192 && AtomicVisualBudget.Particles(2) == 288, "Particle budget");
Console.WriteLine($"{count} release, timing and visual budget checks passed.");
