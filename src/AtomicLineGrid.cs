using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderRoad;

namespace IAmAtomic
{
    public class AtomicLineGrid : MonoBehaviour
    {
        private List<LineRenderer> activeLines = new List<LineRenderer>();
        private Vector3[,] pathWaypoints;
        private Vector3[] currentLeadingTips;
        private Vector3[] bladeSpiralOffsets;
        private Vector3[] forearmSpiralOffsets;
        private Vector3[] torsoOffsets;
        private float[] floorAngles;
        private float[] floorTurnAngles;
        private float[] floorDist1;
        private float[] floorDist2;
        private float[] wallHeights;

        private LineRenderer orbitRing1;
        private LineRenderer orbitRing2;
        private Material lineMaterial;
        private readonly RaycastHit[] rayHits = new RaycastHit[64];
        private Coroutine retractRoutine;
        private Coroutine flashRoutine;

        private const int LineCount = 96;
        private int activeLineCount = 64;
        private readonly List<LineRenderer> coreLines = new List<LineRenderer>();
        private AtomicVfx vfx;
        private const int WaypointCount = 20;
        private string activeStyle;
        private Item cachedBlade;
        private Damager[] cachedDamagers;
        private const int OrbitPoints = 32;

        private readonly float[] segmentThresholds = new float[]
        { 0f, .02f, .04f, .06f, .08f, .10f, .12f, .15f, .18f, .22f, .30f, .38f, .44f, .52f, .60f, .68f, .76f, .84f, .92f, 1f };

        public void Initialize()
        {
            Shader shader = Shader.Find("Sprites/Default")
                ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Legacy Shaders/Particles/Additive")
                ?? Shader.Find("Unlit/Color");

            if (shader == null) { Debug.LogError("[IAmAtomic] No compatible line shader found"); enabled = false; return; }
            lineMaterial = new Material(shader);
            if (lineMaterial.HasProperty("_Surface")) lineMaterial.SetFloat("_Surface", 1.0f);
            if (lineMaterial.HasProperty("_Blend")) lineMaterial.SetFloat("_Blend", 2.0f);
            if (lineMaterial.HasProperty("_SrcBlend")) lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (lineMaterial.HasProperty("_DstBlend")) lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            if (lineMaterial.HasProperty("_ZWrite")) lineMaterial.SetInt("_ZWrite", 0);
            if (lineMaterial.HasProperty("_Cull")) lineMaterial.SetFloat("_Cull", 0.0f);
            lineMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            lineMaterial.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            lineMaterial.renderQueue = 3000;

            Color baseColor = Color.white;
            if (lineMaterial.HasProperty("_BaseColor")) lineMaterial.SetColor("_BaseColor", baseColor);
            if (lineMaterial.HasProperty("_Color")) lineMaterial.SetColor("_Color", baseColor);
            lineMaterial.color = baseColor;

            pathWaypoints = new Vector3[LineCount, WaypointCount];
            currentLeadingTips = new Vector3[LineCount];
            bladeSpiralOffsets = new Vector3[LineCount];
            forearmSpiralOffsets = new Vector3[LineCount];
            torsoOffsets = new Vector3[LineCount];
            floorAngles = new float[LineCount];
            floorTurnAngles = new float[LineCount];
            floorDist1 = new float[LineCount];
            floorDist2 = new float[LineCount];
            wallHeights = new float[LineCount];

            for (int i = 0; i < LineCount; i++)
            {
                float angle = (i * (360f / LineCount)) * Mathf.Deg2Rad;
                bladeSpiralOffsets[i] = new Vector3(Mathf.Sin(angle * 2f) * 0.018f, ((float)i / LineCount - 0.5f) * 0.3f, Mathf.Cos(angle * 2f) * 0.018f);
                forearmSpiralOffsets[i] = new Vector3(Mathf.Sin(angle) * 0.032f, ((i % 3) - 1f) * 0.02f, Mathf.Cos(angle) * 0.032f);
                torsoOffsets[i] = new Vector3(Mathf.Cos(angle) * 0.13f, ((i % 4) - 1.5f) * 0.06f, Mathf.Sin(angle) * 0.13f);

                floorAngles[i] = angle;
                floorTurnAngles[i] = (i % 2 == 0 ? 40f : -40f) * Mathf.Deg2Rad;
                floorDist1[i] = Random.Range(2.5f, 4.5f);
                floorDist2[i] = Random.Range(4.0f, 7.5f);
                wallHeights[i] = Random.Range(8f, 16f);

                GameObject lineObj = new GameObject("AtomicLine_" + i);
                lineObj.transform.parent = transform;
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.positionCount = WaypointCount;
                lr.startWidth = 0.0022f;
                lr.endWidth = 0.0014f;
                lr.sharedMaterial = lineMaterial;
                lr.startColor = new Color(0.95f, 0.35f, 1.0f, 0.45f);
                lr.endColor = new Color(0.65f, 0.1f, 0.95f, 0.25f);
                lr.useWorldSpace = true;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.enabled = false;
                activeLines.Add(lr);
                LineRenderer core = null;
                if (i % 4 == 0)
                {
                    var obj = new GameObject("AtomicCore_" + i);
                    obj.transform.parent = transform;
                    core = obj.AddComponent<LineRenderer>();
                    core.positionCount = WaypointCount;
                    core.sharedMaterial = lineMaterial;
                    core.useWorldSpace = true;
                    core.startWidth = .0022f;
                    core.endWidth = .0014f;
                    core.startColor = new Color(1f, .85f, 1f, .8f);
                    core.endColor = new Color(.95f, .65f, 1f, .6f);
                    core.enabled = false;
                }
                coreLines.Add(core);
            }

            GameObject r1Obj = new GameObject("AtomicOrbit_1");
            r1Obj.transform.parent = transform;
            orbitRing1 = r1Obj.AddComponent<LineRenderer>();
            orbitRing1.positionCount = OrbitPoints + 1;
            orbitRing1.startWidth = 0.002f;
            orbitRing1.endWidth = 0.002f;
            orbitRing1.sharedMaterial = lineMaterial;
            orbitRing1.useWorldSpace = true;
            orbitRing1.enabled = false;

            GameObject r2Obj = new GameObject("AtomicOrbit_2");
            r2Obj.transform.parent = transform;
            orbitRing2 = r2Obj.AddComponent<LineRenderer>();
            orbitRing2.positionCount = OrbitPoints + 1;
            orbitRing2.startWidth = 0.002f;
            orbitRing2.endWidth = 0.002f;
            orbitRing2.sharedMaterial = lineMaterial;
            orbitRing2.useWorldSpace = true;
            orbitRing2.enabled = false;
            vfx = gameObject.AddComponent<AtomicVfx>();
            vfx.Initialize(lineMaterial);
        }

        public void StartExpansion(Vector3 origin, Creature playerCreature = null, Item bladeItem = null, Side bladeSide = Side.Right)
        {
            if (lineMaterial == null) return;
            if (retractRoutine != null)
            {
                StopCoroutine(retractRoutine);
                retractRoutine = null;
            }
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            activeStyle = AtomicModOptions.visualStyle;
            activeLineCount = AtomicVisualBudget.Lines(AtomicModOptions.visualQuality);
            for (int i = activeLineCount; i < activeLines.Count; i++) { activeLines[i].enabled = false; if (coreLines[i] != null) coreLines[i].enabled = false; }
            vfx?.StartCharge(AtomicModOptions.visualQuality, AtomicModOptions.particlesEnabled);
            Vector3 bladeTip = GetBladeTip(bladeItem, origin);
            Vector3 handPos = GetHandPos(playerCreature, bladeItem, bladeSide, origin);
            Vector3 bladeBase = bladeItem != null ? bladeItem.transform.position : handPos;
            Vector3 bladeMid = Vector3.Lerp(bladeTip, bladeBase, 0.5f);

            Vector3 forearmPos;
            Vector3 elbowPos;
            Vector3 shoulderPos;
            GetArmChain(playerCreature, bladeSide, handPos, out forearmPos, out elbowPos, out shoulderPos);

            Vector3 chestPos;
            Vector3 waistPos;
            GetTorsoChain(playerCreature, origin, out chestPos, out waistPos);

            Vector3 footPos = GetFootPos(playerCreature, bladeSide, origin);

            for (int i = 0; i < activeLineCount; i++)
            {
                LineRenderer lr = activeLines[i];
                lr.enabled = true;
                lr.positionCount = WaypointCount;
                lr.startWidth = 0.008f;
                lr.endWidth = 0.004f;
                if (coreLines[i] != null)
                {
                    coreLines[i].enabled = true;
                    coreLines[i].startColor = new Color(1f, .85f, 1f, .8f);
                    coreLines[i].endColor = new Color(.95f, .65f, 1f, .6f);
                    coreLines[i].startWidth = .0022f;
                    coreLines[i].endWidth = .0014f;
                }
                lr.startColor = new Color(0.95f, 0.35f, 1.0f, 0.45f);
                lr.endColor = new Color(0.65f, 0.1f, 0.95f, 0.25f);

                Vector3 p0 = bladeTip + bladeSpiralOffsets[i];
                Vector3 p1 = bladeMid + bladeSpiralOffsets[i] * 0.7f;
                Vector3 p2 = bladeBase;
                Vector3 p3 = handPos;
                Vector3 p4 = forearmPos + forearmSpiralOffsets[i];
                Vector3 p5 = elbowPos;
                Vector3 p6 = shoulderPos;
                Vector3 p7 = chestPos + torsoOffsets[i];
                Vector3 p8 = waistPos + torsoOffsets[i] * 0.6f;
                Vector3 p9 = footPos;

                float a = i * (Mathf.PI * 2f / activeLineCount);
                Vector3 outwardDir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)).normalized;
                Vector3 p10 = p9 + outwardDir * floorDist1[i];
                RaycastHit obstacle;
                if (RaycastEnvironment(p9 + Vector3.up * 0.2f, outwardDir, out obstacle, floorDist1[i])) p10 = obstacle.point + obstacle.normal * 0.03f;
                RaycastHit fHit1;
                if (RaycastEnvironment(p10 + Vector3.up * 2f, Vector3.down, out fHit1, 4f))
                {
                    p10 = fHit1.point + Vector3.up * 0.015f;
                }

                float turnAngle = floorTurnAngles[i];
                float cosTurn = Mathf.Cos(turnAngle);
                float sinTurn = Mathf.Sin(turnAngle);
                Vector3 turnDir = new Vector3(outwardDir.x * cosTurn - outwardDir.z * sinTurn, 0f, outwardDir.x * sinTurn + outwardDir.z * cosTurn).normalized;
                Vector3 p11 = p10 + turnDir * floorDist2[i];
                if (RaycastEnvironment(p10 + Vector3.up * 0.2f, turnDir, out obstacle, floorDist2[i])) p11 = obstacle.point + obstacle.normal * 0.03f;
                RaycastHit fHit2;
                if (RaycastEnvironment(p11 + Vector3.up * 2f, Vector3.down, out fHit2, 4f))
                {
                    p11 = fHit2.point + Vector3.up * 0.015f;
                }

                RaycastHit wallHit;
                Vector3 p12;
                Vector3 wallNormal = -turnDir;
                bool wallFound = false;
                if (RaycastEnvironment(p11 + Vector3.up * 0.25f, turnDir, out wallHit, 35f, true))
                {
                    p12 = wallHit.point + wallHit.normal * 0.035f;
                    wallNormal = wallHit.normal;
                    wallFound = true;
                }
                else
                {
                    Vector3 fallbackWall = p11 + turnDir * 20f;
                    p12 = fallbackWall;
                }

                pathWaypoints[i, 0] = p0;
                pathWaypoints[i, 1] = p1;
                pathWaypoints[i, 2] = p2;
                pathWaypoints[i, 3] = p3;
                pathWaypoints[i, 4] = p4;
                pathWaypoints[i, 5] = p5;
                pathWaypoints[i, 6] = p6;
                pathWaypoints[i, 7] = p7;
                pathWaypoints[i, 8] = p8;
                pathWaypoints[i, 9] = p9;
                pathWaypoints[i, 10] = p10;
                pathWaypoints[i, 11] = p11;
                pathWaypoints[i, 12] = p12;
                Vector3 top = p12 + Vector3.up * wallHeights[i];
                RaycastHit ceiling;
                if (RaycastEnvironment(p12 + Vector3.up * 0.05f, Vector3.up, out ceiling, wallHeights[i])) top = ceiling.point - Vector3.up * 0.025f;
                Vector3 wallTangent = Vector3.Cross(Vector3.up, wallNormal).normalized;
                for (int w = 13; w < WaypointCount; w++)
                {
                    float fraction = (w - 12f) / (WaypointCount - 13f);
                    float zig = ((w + i) % 3 - 1) * (0.12f + (i % 4) * 0.07f);
                    Vector3 point = Vector3.Lerp(p12, top, fraction) + wallTangent * zig;
                    RaycastHit surface;
                    if (wallFound && RaycastEnvironment(point + wallNormal * .35f, -wallNormal, out surface, .8f, true))
                        point = surface.point + surface.normal * .035f;
                    pathWaypoints[i, w] = point;
                }
                if (i % 3 != 0)
                    for (int w = 0; w <= 9; w++) pathWaypoints[i, w] = Vector3.Lerp(bladeTip + bladeSpiralOffsets[i], footPos, w / 9f);
                if (activeStyle == "VolumetricWeb")
                {
                    Vector3 direction = new Vector3(outwardDir.x, Mathf.Sin(i * 2.4f) * 0.7f, outwardDir.z).normalized;
                    RaycastHit boundary;
                    Vector3 endpoint = RaycastEnvironment(bladeTip, direction, out boundary, 20f) ? boundary.point + boundary.normal * 0.025f : bladeTip + direction * 20f;
                    for (int w = 0; w < WaypointCount; w++) pathWaypoints[i, w] = Vector3.Lerp(bladeTip, endpoint, w / (float)(WaypointCount - 1));
                }

                currentLeadingTips[i] = p0;

                for (int w = 0; w < WaypointCount; w++)
                {
                    SetTracePoint(i, w, p0);
                }
            }

            orbitRing1.startWidth = orbitRing1.endWidth = 0.002f;
            orbitRing2.startWidth = orbitRing2.endWidth = 0.002f;
            orbitRing1.enabled = true;
            orbitRing2.enabled = true;
            UpdateOrbitRings(bladeMid, 0.3f, 0f);
            vfx?.UpdateCharge(bladeTip, footPos, chestPos, 0f, currentLeadingTips, activeLineCount);
        }

        public void UpdateProgress(Vector3 origin, float progress, Creature playerCreature = null, Item bladeItem = null, Side bladeSide = Side.Right)
        {
            if (lineMaterial == null) return;
            float curvedProgress = Mathf.Clamp01(progress);

            Vector3 bladeTip = GetBladeTip(bladeItem, origin);
            Vector3 handPos = GetHandPos(playerCreature, bladeItem, bladeSide, origin);
            Vector3 bladeBase = bladeItem != null ? bladeItem.transform.position : handPos;
            Vector3 bladeMid = Vector3.Lerp(bladeTip, bladeBase, 0.5f);

            Vector3 forearmPos;
            Vector3 elbowPos;
            Vector3 shoulderPos;
            GetArmChain(playerCreature, bladeSide, handPos, out forearmPos, out elbowPos, out shoulderPos);

            Vector3 chestPos;
            Vector3 waistPos;
            GetTorsoChain(playerCreature, origin, out chestPos, out waistPos);

            Vector3 footPos = GetFootPos(playerCreature, bladeSide, origin);

            float ringRadius = Mathf.Lerp(0.25f, 1.2f, curvedProgress);
            float spin = Time.time * (120f + curvedProgress * 160f);
            UpdateOrbitRings(bladeMid, ringRadius, spin);

            for (int i = 0; i < activeLineCount; i++)
            {
                LineRenderer lr = activeLines[i];
                if (!lr.enabled) lr.enabled = true;

                if (activeStyle != "VolumetricWeb")
                {
                pathWaypoints[i, 0] = bladeTip + bladeSpiralOffsets[i];
                pathWaypoints[i, 1] = bladeMid + bladeSpiralOffsets[i] * 0.7f;
                pathWaypoints[i, 2] = bladeBase;
                pathWaypoints[i, 3] = handPos;
                pathWaypoints[i, 4] = forearmPos + forearmSpiralOffsets[i];
                pathWaypoints[i, 5] = elbowPos;
                pathWaypoints[i, 6] = shoulderPos;
                pathWaypoints[i, 7] = chestPos + torsoOffsets[i];
                pathWaypoints[i, 8] = waistPos + torsoOffsets[i] * 0.6f;
                pathWaypoints[i, 9] = footPos;
                if (i % 3 != 0)
                    for (int w = 0; w <= 9; w++) pathWaypoints[i, w] = Vector3.Lerp(bladeTip + bladeSpiralOffsets[i], footPos, w / 9f);
                }

                Vector3 currentTip = pathWaypoints[i, 0];
                SetTracePoint(i, 0, pathWaypoints[i, 0]);

                for (int w = 1; w < WaypointCount; w++)
                {
                    float startT = segmentThresholds[w - 1];
                    float endT = segmentThresholds[w];

                    if (curvedProgress >= endT)
                    {
                        currentTip = pathWaypoints[i, w];
                        SetTracePoint(i, w, pathWaypoints[i, w]);
                    }
                    else if (curvedProgress <= startT)
                    {
                        SetTracePoint(i, w, currentTip);
                    }
                    else
                    {
                        float segT = Mathf.Clamp01((curvedProgress - startT) / (endT - startT));
                        currentTip = Vector3.Lerp(pathWaypoints[i, w - 1], pathWaypoints[i, w], segT);
                        SetTracePoint(i, w, currentTip);
                    }
                }

                currentLeadingTips[i] = currentTip;

                float pulse = 0.88f + 0.12f * Mathf.Sin(Time.time * 10f + i);
                lr.startColor = new Color(0.95f * pulse, 0.35f * pulse, 1.0f, 0.45f * pulse);
                lr.endColor = new Color(0.65f * pulse, 0.1f * pulse, 0.95f, 0.25f * pulse);
            }
            vfx?.UpdateCharge(bladeTip, footPos, chestPos, curvedProgress, currentLeadingTips, activeLineCount);
        }

        private Vector3 GetBladeTip(Item bladeItem, Vector3 fallbackOrigin)
        {
            if (bladeItem == null)
            {
                return fallbackOrigin + Vector3.up * 0.7f;
            }

            if (cachedBlade != bladeItem) { cachedBlade = bladeItem; cachedDamagers = bladeItem.GetComponentsInChildren<Damager>(); }
            Damager[] damagers = cachedDamagers;
            if (damagers != null && damagers.Length > 0)
            {
                Damager furthest = null;
                float maxD = 0.1f;
                Vector3 basePos = bladeItem.transform.position;
                for (int i = 0; i < damagers.Length; i++)
                {
                    if (damagers[i] != null)
                    {
                        float d = Vector3.Distance(damagers[i].transform.position, basePos);
                        if (d > maxD)
                        {
                            maxD = d;
                            furthest = damagers[i];
                        }
                    }
                }
                if (furthest != null)
                {
                    return furthest.transform.position;
                }
            }

            return bladeItem.transform.position + bladeItem.transform.forward * 0.75f;
        }

        private Vector3 GetHandPos(Creature playerCreature, Item bladeItem, Side bladeSide, Vector3 fallbackOrigin)
        {
            if (bladeItem != null && bladeItem.mainHandler != null)
            {
                return bladeItem.mainHandler.transform.position;
            }

            if (playerCreature != null)
            {
                RagdollHand hand = playerCreature.GetHand(bladeSide);
                if (hand != null)
                {
                    return hand.transform.position;
                }
            }

            return fallbackOrigin;
        }

        private void GetArmChain(Creature playerCreature, Side bladeSide, Vector3 handPos, out Vector3 forearmPos, out Vector3 elbowPos, out Vector3 shoulderPos)
        {
            RagdollPart armPart = null;
            RagdollPart upperPart = null;

            if (playerCreature != null)
            {
                RagdollHand hand = playerCreature.GetHand(bladeSide);
                if (hand != null)
                {
                    armPart = hand.lowerArmPart;
                    upperPart = hand.upperArmPart;
                }

                if (armPart == null && playerCreature.ragdoll != null)
                {
                    RagdollPart.Type t = bladeSide == Side.Right ? RagdollPart.Type.RightArm : RagdollPart.Type.LeftArm;
                    armPart = playerCreature.ragdoll.GetPart(t);
                }
            }

            Vector3 shoulderFallback = handPos + Vector3.up * 0.35f;
            if (playerCreature != null && playerCreature.ragdoll != null)
            {
                RagdollPart torso = playerCreature.ragdoll.GetPart(RagdollPart.Type.Torso);
                if (torso != null)
                {
                    float sign = (bladeSide == Side.Right ? 1f : -1f);
                    shoulderFallback = torso.transform.position + playerCreature.transform.right * (sign * 0.22f) + Vector3.up * 0.18f;
                }
            }

            if (upperPart != null)
            {
                shoulderPos = upperPart.transform.position;
            }
            else
            {
                shoulderPos = shoulderFallback;
            }

            if (armPart != null)
            {
                elbowPos = armPart.transform.position;
                forearmPos = Vector3.Lerp(handPos, elbowPos, 0.5f);
            }
            else
            {
                elbowPos = Vector3.Lerp(handPos, shoulderPos, 0.6f);
                forearmPos = Vector3.Lerp(handPos, elbowPos, 0.5f);
            }
        }

        private void GetTorsoChain(Creature playerCreature, Vector3 fallbackOrigin, out Vector3 chestPos, out Vector3 waistPos)
        {
            if (playerCreature != null && playerCreature.ragdoll != null)
            {
                RagdollPart torso = playerCreature.ragdoll.GetPart(RagdollPart.Type.Torso);
                if (torso != null)
                {
                    chestPos = torso.transform.position + Vector3.up * 0.12f;
                    waistPos = torso.transform.position - Vector3.up * 0.22f;
                    return;
                }
            }

            chestPos = fallbackOrigin - Vector3.up * 0.25f;
            waistPos = fallbackOrigin - Vector3.up * 0.6f;
        }

        private Vector3 GetFootPos(Creature playerCreature, Side bladeSide, Vector3 fallbackOrigin)
        {
            Vector3 rawFoot = fallbackOrigin - Vector3.up * 1.5f;

            if (playerCreature != null)
            {
                if (playerCreature.ragdoll != null)
                {
                    RagdollPart.Type ft = bladeSide == Side.Right ? RagdollPart.Type.RightFoot : RagdollPart.Type.LeftFoot;
                    RagdollPart foot = playerCreature.ragdoll.GetPart(ft);
                    if (foot != null)
                    {
                        rawFoot = foot.transform.position;
                    }
                    else
                    {
                        rawFoot = playerCreature.transform.position + (playerCreature.transform.right * (bladeSide == Side.Right ? 0.14f : -0.14f));
                    }
                }
                else
                {
                    rawFoot = playerCreature.transform.position + (playerCreature.transform.right * (bladeSide == Side.Right ? 0.14f : -0.14f));
                }
            }

            RaycastHit hit;
            if (RaycastEnvironment(rawFoot + Vector3.up * 0.5f, Vector3.down, out hit, 3.0f))
            {
                return hit.point + Vector3.up * 0.015f;
            }

            return rawFoot;
        }

        private void UpdateOrbitRings(Vector3 center, float radius, float spinDeg)
        {
            float step = 360f / OrbitPoints;
            float radOffset = spinDeg * Mathf.Deg2Rad;

            for (int i = 0; i <= OrbitPoints; i++)
            {
                float a = (i * step) * Mathf.Deg2Rad + radOffset;
                float cos = Mathf.Cos(a);
                float sin = Mathf.Sin(a);

                Vector3 p1 = center + new Vector3(cos * radius, sin * 0.35f * radius, sin * radius);
                Vector3 p2 = center + new Vector3(cos * radius, -sin * 0.35f * radius, -sin * radius);

                orbitRing1.SetPosition(i, p1);
                orbitRing2.SetPosition(i, p2);
            }

            float pulse = 0.9f + 0.1f * Mathf.Sin(Time.time * 12f);
            Color c = new Color(0.95f * pulse, 0.35f * pulse, 1.0f, 0.35f);
            orbitRing1.startColor = c;
            orbitRing1.endColor = c;
            orbitRing2.startColor = c;
            orbitRing2.endColor = c;
        }

        public void RetractAndFade(Vector3 origin, float duration)
        {
            if (lineMaterial == null) return;
            if (retractRoutine != null)
            {
                StopCoroutine(retractRoutine);
            }
            vfx?.CancelCharge();
            retractRoutine = StartCoroutine(RetractRoutine(origin, duration));
        }

        private IEnumerator RetractRoutine(Vector3 origin, float duration)
        {
            float timer = 0f;
            Vector3[,] visible = new Vector3[LineCount, WaypointCount];
            for (int i = 0; i < activeLineCount; i++)
                for (int w = 0; w < WaypointCount; w++) visible[i, w] = activeLines[i].GetPosition(w);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = 1.0f - (timer / duration);
                float alpha = Mathf.Clamp01(t);

                for (int i = 0; i < activeLineCount; i++)
                {
                    LineRenderer lr = activeLines[i];

                    for (int w = 0; w < WaypointCount; w++)
                    {
                        SetTracePoint(i, w, Vector3.Lerp(origin, visible[i, w], t));
                    }

                    lr.startWidth = Mathf.Lerp(0.0005f, 0.008f, t);
                    lr.endWidth = Mathf.Lerp(0.0005f, 0.004f, t);
                    lr.startColor = new Color(0.95f, 0.35f, 1.0f, alpha * 0.45f);
                    lr.endColor = new Color(0.65f, 0.1f, 0.95f, alpha * 0.25f);
                    if (coreLines[i] != null)
                    {
                        coreLines[i].startColor = coreLines[i].endColor = new Color(1f, .7f, 1f, alpha * .7f);
                        coreLines[i].startWidth = .0022f * alpha;
                        coreLines[i].endWidth = .0014f * alpha;
                    }
                }

                float r = Mathf.Lerp(0f, 1.0f, t);
                UpdateOrbitRings(origin, r, Time.time * 140f);
                Color rc = new Color(0.95f, 0.35f, 1.0f, alpha * 0.35f);
                orbitRing1.startColor = rc;
                orbitRing1.endColor = rc;
                orbitRing2.startColor = rc;
                orbitRing2.endColor = rc;

                yield return null;
            }

            HideAll();
            retractRoutine = null;
        }

        public void TriggerDetonationFlash(Vector3 origin)
        {
            if (lineMaterial == null) return;
            if (retractRoutine != null)
            {
                StopCoroutine(retractRoutine);
                retractRoutine = null;
            }
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            vfx?.Blast(origin);
            flashRoutine = StartCoroutine(FlashRoutine(origin));
        }

        private IEnumerator FlashRoutine(Vector3 origin)
        {
            for (int i = 0; i < activeLineCount; i++)
            {
                LineRenderer lr = activeLines[i];
                lr.startWidth = 0.010f;
                lr.endWidth = 0.006f;
                lr.startColor = new Color(1.0f, 0.85f, 1.0f, 0.8f);
                lr.endColor = new Color(0.85f, 0.35f, 1.0f, 0.6f);
            }

            if (orbitRing1.enabled)
            {
                orbitRing1.startWidth = 0.012f;
                orbitRing1.endWidth = 0.012f;
                orbitRing2.startWidth = 0.012f;
                orbitRing2.endWidth = 0.012f;
            }

            float elapsed = 0f;
            while (elapsed < .55f)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsed / .55f);
                for (int i = 0; i < activeLineCount; i++)
                {
                    activeLines[i].startColor = new Color(1f, .7f, 1f, alpha);
                    activeLines[i].endColor = new Color(.85f, .25f, 1f, alpha * .7f);
                    if (coreLines[i] != null) coreLines[i].startColor = coreLines[i].endColor = new Color(1f, .9f, 1f, alpha);
                }
                yield return null;
            }
            HideAll();
            flashRoutine = null;
        }

        private void SetTracePoint(int i, int w, Vector3 position)
        {
            activeLines[i].SetPosition(w, position);
            if (coreLines[i] != null) coreLines[i].SetPosition(w, position);
        }

        public void PrepareDetonation(Vector3 origin) { vfx?.Prime(origin); }

        private bool RaycastEnvironment(Vector3 origin, Vector3 direction, out RaycastHit hit, float distance, bool verticalSurface = false)
        {
            hit = default(RaycastHit);
            float closest = distance;
            bool found = false;
            int count = Physics.RaycastNonAlloc(origin, direction, rayHits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var candidate = rayHits[i];
                var collider = candidate.collider;
                if (verticalSurface && Mathf.Abs(candidate.normal.y) > .65f) continue;
                if (collider == null || collider.attachedRigidbody != null || collider.GetComponentInParent<Creature>() != null || collider.GetComponentInParent<Item>() != null) continue;
                if (candidate.distance < closest) { hit = candidate; closest = candidate.distance; found = true; }
            }
            return found;
        }

        public void ClearEffects()
        {
            StopAllCoroutines();
            flashRoutine = retractRoutine = null;
            HideAll();
            vfx?.ClearEffects();
        }

        private void OnDestroy()
        {
            if (lineMaterial != null) Destroy(lineMaterial);
        }

        public void HideAll()
        {
            for (int i = 0; i < activeLines.Count; i++)
            {
                activeLines[i].enabled = false;
                if (coreLines[i] != null) coreLines[i].enabled = false;
            }
            if (orbitRing1 != null) orbitRing1.enabled = false;
            if (orbitRing2 != null) orbitRing2.enabled = false;
        }
    }
}
