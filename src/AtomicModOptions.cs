using System;
using ThunderRoad;

namespace IAmAtomic
{
    public static class AtomicModOptions
    {
        public static bool modEnabled = true;
        public static float channelDuration = 8.5f;
        public static float gravityForce = 1.0f;
        public static float explosionDamage = 1000f;
        public static float knockbackForce = 1500f;
        public static bool roomLock = true;
        public static string visualStyle = "AnimeCircuits";
        public static string activationMode = "Direct";

        [ModOptionOrder(1)]
        public static ModOptionBool[] EnableOptions()
        {
            return new ModOptionBool[]
            {
                new ModOptionBool("Enabled", true),
                new ModOptionBool("Disabled", false)
            };
        }

        [ModOptionButton]
        [ModOptionCategory("I Am Atomic", 1)]
        [ModOption("Mod Status", "Enable or disable I Am Atomic mod", "EnableOptions", defaultValueIndex = 0)]
        public static void SetModEnabled(bool enabled)
        {
            modEnabled = enabled;
        }

        [ModOptionOrder(2)]
        public static ModOptionFloat[] DurationOptions()
        {
            return new ModOptionFloat[]
            {
                new ModOptionFloat("Anime Faithful (8.5s)", 8.5f),
                new ModOptionFloat("Instant (0.1s)", 0.1f),
                new ModOptionFloat("1 Second (1.0s)", 1.0f),
                new ModOptionFloat("Fast (2.5s)", 2.5f),
                new ModOptionFloat("Medium (5.0s)", 5.0f),
                new ModOptionFloat("Long (15.0s)", 15.0f),
                new ModOptionFloat("Extreme (30.0s)", 30.0f),
                new ModOptionFloat("1 Minute (60.0s)", 60.0f)
            };
        }

        [ModOptionButton]
        [ModOptionCategory("I Am Atomic", 1)]
        [ModOption("Channeling Time", "Charge time for timed mode; expansion speed only in release mode. Timed mode adds the spoken prelude before impact.", "DurationOptions", defaultValueIndex = 0)]
        public static void SetChannelDuration(float duration)
        {
            channelDuration = duration;
        }

        [ModOptionOrder(3)]
        public static ModOptionFloat[] GravityOptions()
        {
            return new ModOptionFloat[]
            {
                new ModOptionFloat("Normal Float", 1.0f),
                new ModOptionFloat("Light Float", 0.5f),
                new ModOptionFloat("Heavy Float", 2.0f),
                new ModOptionFloat("Zero Gravity (Off)", 0.0f)
            };
        }

        [ModOptionButton]
        [ModOptionCategory("I Am Atomic", 1)]
        [ModOption("Gravity Levitation", "Intensity of enemy levitation during charging", "GravityOptions", defaultValueIndex = 0)]
        public static void SetGravityForce(float force)
        {
            gravityForce = force;
        }

        [ModOptionOrder(4)]
        public static ModOptionFloat[] DamageOptions()
        {
            return new ModOptionFloat[]
            {
                new ModOptionFloat("Lethal Balanced (1000)", 1000f),
                new ModOptionFloat("Arcade One-Shot (99999)", 99999f),
                new ModOptionFloat("Moderate (300)", 300f)
            };
        }

        [ModOptionButton]
        [ModOptionCategory("I Am Atomic", 1)]
        [ModOption("Explosion Damage", "Damage dealt by the atomic detonation", "DamageOptions", defaultValueIndex = 0)]
        public static void SetExplosionDamage(float damage)
        {
            explosionDamage = damage;
        }

        [ModOptionOrder(5)]
        public static ModOptionFloat[] KnockbackOptions()
        {
            return new ModOptionFloat[]
            {
                new ModOptionFloat("Heavy Blast (1500)", 1500f),
                new ModOptionFloat("Catastrophic (3500)", 3500f),
                new ModOptionFloat("Moderate (800)", 800f)
            };
        }

        [ModOptionButton]
        [ModOptionCategory("I Am Atomic", 1)]
        [ModOption("Knockback Force", "Physical force exerted on ragdolls and objects", "KnockbackOptions", defaultValueIndex = 0)]
        public static void SetKnockbackForce(float force)
        {
            knockbackForce = force;
        }

        [ModOptionOrder(6)]
        public static ModOptionBool[] RoomLockOptions()
        {
            return new ModOptionBool[]
            {
                new ModOptionBool("Current Room Only", true),
                new ModOptionBool("Open Radius (40m)", false)
            };
        }

        [ModOptionButton]
        [ModOptionCategory("I Am Atomic", 1)]
        [ModOption("Area Scope", "Limit explosion to the current dungeon room or sphere radius", "RoomLockOptions", defaultValueIndex = 0)]
        public static void SetRoomLock(bool lockRoom)
        {
            roomLock = lockRoom;
        }

        [ModOptionOrder(7)]
        public static ModOptionString[] VisualStyleOptions()
        {
            return new ModOptionString[]
            {
                new ModOptionString("Anime Accurate (Circuits & Orbit)", "AnimeCircuits"),
                new ModOptionString("Volumetric Web (3D Space Rays)", "VolumetricWeb")
            };
        }

        [ModOptionButton]
        [ModOptionCategory("I Am Atomic", 1)]
        [ModOption("Visual Style", "Choose between anime-accurate wall circuits or volumetric 3D energy web", "VisualStyleOptions", defaultValueIndex = 0)]
        public static void SetVisualStyle(string style)
        {
            visualStyle = style;
        }

        [ModOptionOrder(8)]
        public static ModOptionString[] ActivationModeOptions()
        {
            return new ModOptionString[]
            {
                new ModOptionString("Principal: por tempo (com audio)", "Direct"),
                new ModOptionString("Secundario: soltar mao (carga silenciosa)", "HoldRelease")
            };
        }

        [ModOptionButton]
        [ModOptionCategory("I Am Atomic", 1)]
        [ModOption("Trigger Mode", "Timed: charge, spoken prelude and synchronized blast. Release: silent charge, instant blast on grip release.", "ActivationModeOptions", defaultValueIndex = 0)]
        public static void SetActivationMode(string mode)
        {
            activationMode = mode;
        }

        public static int visualQuality = 1;
        public static bool particlesEnabled = true;
        public static float impactAdjustment = 0f;

        public static ModOptionInt[] QualityOptions() => new[] {
            new ModOptionInt("Leve (32 circuitos)", 0),
            new ModOptionInt("Equilibrado (64 circuitos)", 1),
            new ModOptionInt("Cinematico (96 circuitos)", 2)
        };
        [ModOptionOrder(9), ModOptionCategory("I Am Atomic", 1), ModOptionButton]
        [ModOption("Qualidade Visual", "Applied at next ritual. More circuits and particles increase rendering cost.", "QualityOptions", defaultValueIndex = 1)]
        public static void SetQuality(int quality) { visualQuality = Math.Max(0, Math.Min(2, quality)); }

        [ModOptionOrder(10), ModOptionCategory("I Am Atomic", 1), ModOptionButton]
        [ModOption("Particulas", "Sword embers, wall sparks and impact burst", "EnableOptions", defaultValueIndex = 0)]
        public static void SetParticles(bool enabled) { particlesEnabled = enabled; }

        public static ModOptionFloat[] SyncOptions() => new[] {
            new ModOptionFloat("Automatico (buffer de audio)", 0f),
            new ModOptionFloat("Impacto 50 ms depois", 0.05f),
            new ModOptionFloat("Impacto 100 ms depois", 0.10f),
            new ModOptionFloat("Impacto 50 ms antes", -0.05f)
        };
        [ModOptionOrder(11), ModOptionCategory("I Am Atomic", 1), ModOptionButton]
        [ModOption("Ajuste fino de sincronizacao", "Optional correction for perceived headset audio latency", "SyncOptions", defaultValueIndex = 0)]
        public static void SetSync(float offset) { impactAdjustment = offset; }
    }
}
