using UnityEngine;

namespace IAmAtomic
{
    public sealed class AtomicVfx : MonoBehaviour
    {
        private const int RingPoints = 64;
        private readonly LineRenderer[] rings = new LineRenderer[6];
        private ParticleSystem sparks;
        private Material sparkMaterial;
        private Texture2D sparkTexture;
        private Vector3 sword, floor, chest, blastOrigin;
        private Vector3[] tips;
        private int tipCount, cursor, quality;
        private bool charging, primed, particlesAllowed;
        private float progress, emissionCredit, blastAge = -1f;

        public void Initialize(Material lineMaterial)
        {
            for (int i = 0; i < rings.Length; i++)
            {
                var obj = new GameObject("AtomicHalo_" + i);
                obj.transform.SetParent(transform, false);
                var ring = obj.AddComponent<LineRenderer>();
                ring.sharedMaterial = lineMaterial;
                ring.useWorldSpace = true;
                ring.positionCount = RingPoints + 1;
                ring.enabled = false;
                ring.startWidth = ring.endWidth = i < 3 ? .014f : .025f;
                ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                ring.receiveShadows = false;
                rings[i] = ring;
            }
            sparkTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            sparkTexture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[32 * 32];
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                float distance = new Vector2((x - 15.5f) / 15.5f, (y - 15.5f) / 15.5f).magnitude;
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 2f);
                pixels[y * 32 + x] = new Color(1f, 1f, 1f, alpha);
            }
            sparkTexture.SetPixels(pixels);
            sparkTexture.Apply(false, true);
            sparkMaterial = new Material(lineMaterial);
            if (sparkMaterial.HasProperty("_MainTex")) sparkMaterial.SetTexture("_MainTex", sparkTexture);
            if (sparkMaterial.HasProperty("_BaseMap")) sparkMaterial.SetTexture("_BaseMap", sparkTexture);
            if (sparkMaterial.HasProperty("_Color")) sparkMaterial.SetColor("_Color", Color.white);
            if (sparkMaterial.HasProperty("_BaseColor")) sparkMaterial.SetColor("_BaseColor", Color.white);
            var particleObject = new GameObject("AtomicSparks");
            particleObject.transform.SetParent(transform, false);
            sparks = particleObject.AddComponent<ParticleSystem>();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = sparks.main;
            main.playOnAwake = false;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = .8f;
            main.startSpeed = 0f;
            main.startSize = .035f;
            main.gravityModifier = 0f;
            main.maxParticles = 192;
            var emission = sparks.emission; emission.enabled = false;
            var shape = sparks.shape; shape.enabled = false;
            var color = sparks.colorOverLifetime; color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new [] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(.7f, .15f, 1f), 1) },
                new [] { new GradientAlphaKey(0f, 0), new GradientAlphaKey(1f, .08f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
            var size = sparks.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, .05f));
            var renderer = sparks.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = sparkMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = .12f;
            renderer.lengthScale = 1.5f;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        public void StartCharge(int newQuality, bool allowParticles)
        {
            quality = Mathf.Clamp(newQuality, 0, 2);
            particlesAllowed = allowParticles;
            charging = true; primed = false; blastAge = -1f;
            emissionCredit = 0f; tipCount = 0; cursor = 0;
            for (int i = 0; i < rings.Length; i++) rings[i].enabled = false;
            var main = sparks.main; main.maxParticles = AtomicVisualBudget.Particles(quality);
            sparks.Clear();
            sparks.Play();
        }

        public void UpdateCharge(Vector3 tip, Vector3 feet, Vector3 torso, float amount, Vector3[] leadingTips, int count)
        {
            sword = tip; floor = feet; chest = torso;
            progress = amount; tips = leadingTips; tipCount = count;
        }

        public void Prime(Vector3 origin) { primed = true; progress = 1f; }
        public void CancelCharge()
        {
            charging = false; primed = false;
            for (int i = 0; i < 3; i++) rings[i].enabled = false;
        }
        public void Blast(Vector3 origin)
        {
            charging = false; primed = false;
            blastOrigin = origin; blastAge = 0f;
            for (int i = 0; i < 3; i++) rings[i].enabled = false;
            if (!particlesAllowed) return;
            int burst = quality == 0 ? 40 : quality == 1 ? 80 : 120;
            for (int i = 0; i < burst; i++)
            {
                var velocity = Random.onUnitSphere * Random.Range(3f, 9f);
                Emit(origin + Random.insideUnitSphere * .35f, velocity, Random.Range(.5f, 1.1f), Random.Range(.025f, .07f));
            }
            if (tips != null)
                for (int i = 0; i < tipCount; i += 2) Emit(tips[i], Vector3.up * 2f, .8f, .055f);
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            if (charging)
            {
                float pulse = .8f + .2f * Mathf.Sin(Time.time * (primed ? 25f : 8f));
                Color color = new Color(.95f, .28f, 1f, pulse * (.35f + progress * .35f));
                Ring(rings[0], chest + Vector3.up * 1.25f, .7f + progress * 1.1f, Vector3.right, Vector3.forward, color, Time.time * .3f);
                Ring(rings[1], chest + Vector3.up * 1.32f, .65f + progress * 1.02f, Vector3.right, Vector3.forward, new Color(1f, .5f, 1f, .3f), -Time.time * .4f);
                Ring(rings[2], sword, .15f + progress * .35f, Vector3.right, (Vector3.up + Vector3.forward).normalized, color, Time.time * 1.4f);
                if (particlesAllowed)
                {
                    emissionCredit += Time.deltaTime * (quality == 0 ? 24 : quality == 1 ? 48 : 72);
                    int count = Mathf.Min(6, Mathf.FloorToInt(emissionCredit));
                    emissionCredit = Mathf.Min(6f, emissionCredit - count);
                    for (int i = 0; i < count; i++)
                    {
                        bool onCircuit = tipCount > 0 && (cursor % 3 != 0);
                        Vector3 point = onCircuit ? tips[(cursor * 7) % tipCount] : sword + Random.insideUnitSphere * .12f;
                        Emit(point, Vector3.up * Random.Range(.3f, 1.6f) + Random.insideUnitSphere * .2f, Random.Range(.4f, 1.1f), Random.Range(.018f, .045f));
                        cursor++;
                    }
                }
            }
            if (blastAge >= 0f)
            {
                blastAge += Time.deltaTime;
                float t = Mathf.Clamp01(blastAge / .75f);
                float radius = Mathf.Lerp(.25f, 14f, 1f - (1f - t) * (1f - t));
                Color color = new Color(1f, .65f, 1f, (1f - t) * .85f);
                Ring(rings[3], new Vector3(blastOrigin.x, floor.y + .05f, blastOrigin.z), radius, Vector3.right, Vector3.forward, color, 0f);
                Ring(rings[4], blastOrigin, radius * .8f, Vector3.right, Vector3.up, color, 0f);
                Ring(rings[5], blastOrigin, radius * .8f, Vector3.forward, Vector3.up, color, 0f);
                if (t >= 1f) { blastAge = -1f; for (int i = 3; i < rings.Length; i++) rings[i].enabled = false; }
            }
        }

        private void Emit(Vector3 position, Vector3 velocity, float lifetime, float size)
        {
            var parameters = new ParticleSystem.EmitParams {
                position = position, velocity = velocity, startLifetime = lifetime, startSize = size,
                startColor = new Color(1f, .45f, 1f, .9f)
            };
            sparks.Emit(parameters, 1);
        }
        private void Ring(LineRenderer line, Vector3 center, float radius, Vector3 axisA, Vector3 axisB, Color color, float rotation)
        {
            line.enabled = true; line.startColor = line.endColor = color;
            for (int i = 0; i <= RingPoints; i++)
            {
                float angle = i * (Mathf.PI * 2f / RingPoints) + rotation;
                line.SetPosition(i, center + (axisA * Mathf.Cos(angle) + axisB * Mathf.Sin(angle)) * radius);
            }
        }
        public void ClearEffects()
        {
            charging = primed = false;
            blastAge = -1f;
            for (int i = 0; i < rings.Length; i++) if (rings[i] != null) rings[i].enabled = false;
            if (sparks != null) sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnDestroy()
        {
            if (sparkMaterial != null) Destroy(sparkMaterial);
            if (sparkTexture != null) Destroy(sparkTexture);
        }
    }
}
