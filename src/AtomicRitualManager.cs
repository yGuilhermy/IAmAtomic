using System;
using System.Collections.Generic;
using UnityEngine;
using ThunderRoad;

namespace IAmAtomic
{
    public class AtomicRitualManager : MonoBehaviour
    {
        private enum RitualState
        {
            Idle,
            Charging,
            Detonating
        }

        private RitualState currentState = RitualState.Idle;
        private Side gravitySide;
        private Side bladeSide;
        private Item activeBladeItem;
        private float chargeTimer = 0f;
        private float targetDuration = 8.5f;
        private float cooldownTimer = 0f;
        private readonly AtomicImpactGate impactGate = new AtomicImpactGate();
        private Vector3 impactOrigin;
        private Creature ritualPlayer;
        private bool appPaused, appUnfocused;
        private readonly List<PhysicBody> bodiesToRestore = new List<PhysicBody>();
        private readonly Dictionary<Type, System.Reflection.FieldInfo> areaFields = new Dictionary<Type, System.Reflection.FieldInfo>();

        private AtomicAudioController audioController;
        private AtomicLineGrid lineGrid;
        private string ritualMode;
        private readonly RitualReleaseGate releaseGate = new RitualReleaseGate();
        private readonly HashSet<PhysicBody> touchedBodies = new HashSet<PhysicBody>();
        private readonly Dictionary<PhysicBody, float> floatHeights = new Dictionary<PhysicBody, float>();
        private readonly Dictionary<PhysicBody, bool> originalGravity = new Dictionary<PhysicBody, bool>();

        private void FixedUpdate()
        {
            if (currentState == RitualState.Idle || Time.timeScale <= 0f) return;
            Creature player = GetPlayerCreature();
            try
            {
                if (player != null && !player.isKilled) { ApplyLevitation(GetOrigin(player), player); RestoreUntouchedBodies(); }
            }
            catch (Exception error)
            {
                Debug.LogError("[IAmAtomic] Levitation stopped: " + error);
                CancelRitual();
            }
        }

        private void OnDisable()
        {
            ResetForLevel();
        }

        private void OnApplicationPause(bool paused) { appPaused = paused; if (paused) ResetForLevel(); }
        private void OnApplicationFocus(bool focused) { appUnfocused = !focused; if (!focused) ResetForLevel(); }

        public void ResetForLevel()
        {
            CancelRitual();
            lineGrid?.ClearEffects();
            audioController?.StopImmediately();
        }

        private bool IsFistHeld(Side side)
        {
            var ctrl = PlayerControl.GetHand(side);
            return ctrl != null && (ctrl.gripPressed || ctrl.gripAxis > 0.35f);
        }

        public void Initialize(string modFolder)
        {
            audioController = gameObject.AddComponent<AtomicAudioController>();
            audioController.Initialize(modFolder);

            lineGrid = gameObject.AddComponent<AtomicLineGrid>();
            lineGrid.Initialize();
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (appPaused || appUnfocused) return;
            if (Time.timeScale <= 0f)
            {
                if (currentState == RitualState.Detonating) CancelRitual();
                return;
            }
            if (!AtomicModOptions.modEnabled)
            {
                if (currentState != RitualState.Idle)
                {
                    CancelRitual();
                }
                return;
            }

            Creature playerCreature = GetPlayerCreature();
            if (playerCreature == null || playerCreature.isKilled)
            {
                if (currentState != RitualState.Idle)
                {
                    CancelRitual();
                }
                return;
            }

            if (currentState != RitualState.Idle && playerCreature != ritualPlayer) { CancelRitual(); return; }
            try
            {
                if (currentState == RitualState.Detonating)
                {
                    if (impactGate.ConsumeIfDue(AudioSettings.dspTime)) Detonate(impactOrigin, playerCreature);
                }
                else if (currentState == RitualState.Idle) CheckActivation(playerCreature);
                else if (currentState == RitualState.Charging) ProcessCharging(playerCreature);
            }
            catch (Exception error)
            {
                Debug.LogError("[IAmAtomic] Ritual stopped: " + error);
                CancelRitual();
            }
        }

        private Creature GetPlayerCreature()
        {
            if (Player.local != null && Player.local.creature != null)
            {
                return Player.local.creature;
            }

            for (int i = 0; i < Creature.allActive.Count; i++)
            {
                Creature c = Creature.allActive[i];
                if (c != null && c.isPlayer)
                {
                    return c;
                }
            }

            return null;
        }

        private void CheckActivation(Creature playerCreature)
        {
            if (cooldownTimer > 0f) return;

            RagdollHand leftHand = playerCreature.GetHand(Side.Left);
            RagdollHand rightHand = playerCreature.GetHand(Side.Right);

            Item leftWeapon;
            Item rightWeapon;
            bool leftWeaponImbued = HasGravityImbuedWeapon(leftHand, out leftWeapon);
            bool rightWeaponImbued = HasGravityImbuedWeapon(rightHand, out rightWeapon);

            if (AtomicModes.Normalize(AtomicModOptions.activationMode) == "Direct")
            {
                if (rightWeaponImbued)
                {
                    StartRitual(Side.Left, Side.Right, rightWeapon, playerCreature);
                }
                else if (leftWeaponImbued)
                {
                    StartRitual(Side.Right, Side.Left, leftWeapon, playerCreature);
                }
            }
            else
            {
                bool leftFist = IsGravityFist(leftHand, Side.Left);
                bool rightFist = IsGravityFist(rightHand, Side.Right);

                if (leftFist && rightWeaponImbued)
                {
                    StartRitual(Side.Left, Side.Right, rightWeapon, playerCreature);
                }
                else if (rightFist && leftWeaponImbued)
                {
                    StartRitual(Side.Right, Side.Left, leftWeapon, playerCreature);
                }
            }
        }

        private bool HasGravityImbuedWeapon(RagdollHand hand, out Item weaponItem)
        {
            weaponItem = null;
            if (hand == null || hand.grabbedHandle == null || hand.grabbedHandle.item == null)
            {
                return false;
            }

            Item item = hand.grabbedHandle.item;
            Imbue[] imbues = item.GetComponentsInChildren<Imbue>();
            if (imbues == null || imbues.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < imbues.Length; i++)
            {
                Imbue imbue = imbues[i];
                if (imbue != null && imbue.energy > 0.05f)
                {
                    if (imbue.spellCastBase != null && !string.IsNullOrEmpty(imbue.spellCastBase.id))
                    {
                        if (imbue.spellCastBase.id.IndexOf("Gravity", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            weaponItem = item;
                            return true;
                        }
                    }

                }
            }

            return false;
        }

        private bool IsGravityFist(RagdollHand hand, Side side)
        {
            if (hand == null || hand.grabbedHandle != null)
            {
                return false;
            }

            SpellCaster caster = hand.caster;
            if (caster == null || caster.spellInstance == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(caster.spellInstance.id) ||
                caster.spellInstance.id.IndexOf("Gravity", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            return IsFistHeld(side);
        }

        private void StartRitual(Side gravity, Side blade, Item bladeItem, Creature playerCreature)
        {
            ReleaseLevitation();
            ritualMode = AtomicModes.Normalize(AtomicModOptions.activationMode);
            ritualPlayer = playerCreature;
            releaseGate.Reset(IsFistHeld(gravity));
            gravitySide = gravity;
            bladeSide = blade;
            activeBladeItem = bladeItem;
            currentState = RitualState.Charging;
            chargeTimer = 0f;
            targetDuration = Mathf.Max(0.1f, AtomicModOptions.channelDuration);


            Vector3 origin = bladeItem != null ? bladeItem.transform.position : GetOrigin(playerCreature);
            audioController.PlayCharge(targetDuration, ritualMode == "HoldRelease");
            Debug.Log("[IAmAtomic] Charging mode=" + ritualMode);
            lineGrid.StartExpansion(origin, playerCreature, bladeItem, bladeSide);
        }

        private void ProcessCharging(Creature playerCreature)
        {
            RagdollHand bladeHand = playerCreature.GetHand(bladeSide);
            if (bladeHand == null || bladeHand.grabbedHandle == null || bladeHand.grabbedHandle.item != activeBladeItem)
            {
                CancelRitual();
                return;
            }

            Vector3 origin = GetOrigin(playerCreature);

            if (ritualMode == "HoldRelease")
            {
                var action = releaseGate.Sample(IsFistHeld(gravitySide), chargeTimer, Time.unscaledDeltaTime);
                if (action != ReleaseAction.None)
                {
                    if (action == ReleaseAction.Detonate) BeginDetonation(origin, true);
                    else CancelRitual();
                    return;
                }

                chargeTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(chargeTimer / targetDuration);


                lineGrid.UpdateProgress(origin, progress, playerCreature, bladeHand.grabbedHandle.item, bladeSide);

            }
            else
            {
                if (!HasGravityImbuedWeapon(bladeHand, out _))
                {
                    CancelRitual();
                    return;
                }

                chargeTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(chargeTimer / targetDuration);


                lineGrid.UpdateProgress(origin, progress, playerCreature, bladeHand.grabbedHandle.item, bladeSide);


                if (chargeTimer >= targetDuration)
                {
                    BeginDetonation(origin, false);
                }
            }
        }

        private void BeginDetonation(Vector3 origin, bool immediate)
        {
            if (currentState != RitualState.Charging) return;
            currentState = RitualState.Detonating;
            impactOrigin = origin;
            impactGate.Arm(audioController != null ? audioController.ScheduleBlast(immediate) : AudioSettings.dspTime);
            lineGrid?.PrepareDetonation(origin);
        }

        public void CancelRitual()
        {
            currentState = RitualState.Idle;
            impactGate.Cancel();
            {
                ReleaseLevitation();
                Vector3 origin = GetOrigin(GetPlayerCreature());
                activeBladeItem = null;
                cooldownTimer = 0.5f;
                try { if (lineGrid != null) lineGrid.RetractAndFade(origin, 0.55f); }
                catch (Exception error) { Debug.LogWarning("[IAmAtomic] Visual cleanup: " + error.Message); }
                try { if (audioController != null) audioController.StopWithFadeout(0.35f); }
                catch (Exception error) { Debug.LogWarning("[IAmAtomic] Audio cleanup: " + error.Message); }
            }
        }

        private void Detonate(Vector3 origin, Creature playerCreature)
        {
            currentState = RitualState.Idle;
            cooldownTimer = 0.8f;
            ReleaseLevitation();
            impactGate.Cancel();
            Debug.Log("[IAmAtomic] Detonation mode=" + ritualMode + " charge=" + chargeTimer);
            try
            {
                if (lineGrid != null) lineGrid.TriggerDetonationFlash(origin);

            float maxRange = AtomicModOptions.roomLock ? 28f : 45f;

            var targets = Creature.allActive.ToArray();
            for (int i = 0; i < targets.Length; i++)
            {
                Creature target = targets[i];
                if (target == null || target.isPlayer || target == playerCreature || (Player.local != null && target == Player.local.creature) || target.isKilled) continue;

                if (!InScope(target, target.transform.position, origin, playerCreature, maxRange)) continue;

                try
                {
                if (target.ragdoll != null && target.ragdoll.parts != null && target.ragdoll.parts.Count > 0)
                {
                    int sliceIndex = UnityEngine.Random.Range(0, target.ragdoll.parts.Count);
                    if (target.ragdoll.parts[sliceIndex] != null) target.ragdoll.parts[sliceIndex].TrySlice();
                }

                target.currentHealth -= AtomicModOptions.explosionDamage;
                if (target.currentHealth <= 0f)
                {
                    target.Kill();
                }

                if (target.ragdoll != null)
                {
                    if (!target.isKilled) target.ragdoll.SetState(Ragdoll.State.Destabilized);
                    for (int p = 0; p < target.ragdoll.parts.Count; p++)
                    {
                        RagdollPart part = target.ragdoll.parts[p];
                        if (part != null && part.physicBody != null)
                        {
                            part.physicBody.AddExplosionForce(Mathf.Clamp(AtomicModOptions.knockbackForce / 100f, 1f, 35f), origin, maxRange, 0.5f, ForceMode.VelocityChange);
                        }
                    }
                }
                }
                catch (Exception error) { Debug.LogWarning("[IAmAtomic] Target blast: " + error.Message); }
            }

            for (int i = 0; i < Item.allActive.Count; i++)
            {
                Item item = Item.allActive[i];
                if (item == null || item.physicBody == null || item.physicBody.isKinematic || IsPlayerOrHeldItem(item, playerCreature)) continue;

                if (!InScope(item, item.transform.position, origin, playerCreature, maxRange)) continue;

                item.physicBody.AddExplosionForce(Mathf.Clamp(AtomicModOptions.knockbackForce / 100f, 1f, 35f), origin, maxRange, 0.5f, ForceMode.VelocityChange);
            }

            if (activeBladeItem != null)
            {
                ClearWeaponImbue(activeBladeItem);
            }
            }
            catch (Exception error) { Debug.LogError("[IAmAtomic] Detonation error: " + error); }
            finally
            {
                ReleaseLevitation();
                activeBladeItem = null;
                cooldownTimer = 0.8f;
            }
        }

        private void ClearWeaponImbue(Item item)
        {
            if (item == null) return;
            Imbue[] imbues = item.GetComponentsInChildren<Imbue>();
            if (imbues == null || imbues.Length == 0) return;
            for (int i = 0; i < imbues.Length; i++)
            {
                Imbue imbue = imbues[i];
                if (imbue != null)
                {
                    imbue.SetEnergyInstant(0f);
                    imbue.UnloadCurrentSpell();
                    imbue.Stop();
                    imbue.energy = 0f;
                }
            }
        }

        private bool IsPlayerOrHeldItem(Item item, Creature playerCreature)
        {
            if (item == null) return true;
            if (item == activeBladeItem) return true;
            if (item.isGripped || item.IsHanded()) return true;
            if (item.leftPlayerHand != null || item.rightPlayerHand != null) return true;
            if (item.holder != null && item.holder.creature != null && (item.holder.creature == playerCreature || item.holder.creature.isPlayer)) return true;
            if (item.mainHandler != null && (item.mainHandler.creature == playerCreature || (item.mainHandler.creature != null && item.mainHandler.creature.isPlayer))) return true;

            if (item.handlers != null)
            {
                for (int i = 0; i < item.handlers.Count; i++)
                {
                    RagdollHand h = item.handlers[i];
                    if (h != null && (h.creature == playerCreature || (h.creature != null && h.creature.isPlayer))) return true;
                }
            }

            if (playerCreature != null)
            {
                RagdollHand lh = playerCreature.GetHand(Side.Left);
                if (lh != null && lh.grabbedHandle != null && lh.grabbedHandle.item == item) return true;
                RagdollHand rh = playerCreature.GetHand(Side.Right);
                if (rh != null && rh.grabbedHandle != null && rh.grabbedHandle.item == item) return true;
            }

            return false;
        }

        private void ApplyLevitation(Vector3 origin, Creature playerCreature)
        {
            if (AtomicModOptions.gravityForce <= 0f) { ReleaseLevitation(); return; }

            touchedBodies.Clear();
            float radius = AtomicModOptions.roomLock ? 28f : 45f;
            float forceMag = AtomicModOptions.gravityForce * 4.5f;

            for (int i = 0; i < Creature.allActive.Count; i++)
            {
                Creature target = Creature.allActive[i];
                if (target == null || target.isPlayer || target == playerCreature || (Player.local != null && target == Player.local.creature) || target.isKilled) continue;

                if (InScope(target, target.transform.position, origin, playerCreature, radius))
                {
                    if (target.ragdoll != null && target.ragdoll.parts != null)
                    {
                        if (target.ragdoll.state != Ragdoll.State.Destabilized)
                        {
                            target.ragdoll.SetState(Ragdoll.State.Destabilized);
                        }

                        for (int p = 0; p < target.ragdoll.parts.Count; p++)
                        {
                            RagdollPart part = target.ragdoll.parts[p];
                            if (part != null && part.physicBody != null)
                            {
                                FloatBody(part.physicBody, forceMag);
                            }
                        }
                    }

                }
            }

            for (int i = 0; i < Item.allActive.Count; i++)
            {
                Item item = Item.allActive[i];
                if (item == null || item.physicBody == null || item.physicBody.isKinematic || IsPlayerOrHeldItem(item, playerCreature)) continue;

                if (InScope(item, item.transform.position, origin, playerCreature, radius))
                {
                    FloatBody(item.physicBody, forceMag);

                }
            }
        }

        private void RestoreUntouchedBodies()
        {
            bodiesToRestore.Clear();
            foreach (var entry in originalGravity)
            {
                if (touchedBodies.Contains(entry.Key)) continue;
                try { if (entry.Key != null) entry.Key.useGravity = entry.Value; } catch (Exception error) { Debug.LogWarning("[IAmAtomic] Gravity restore: " + error.Message); }
                bodiesToRestore.Add(entry.Key);
            }
            foreach (var body in bodiesToRestore) { originalGravity.Remove(body); floatHeights.Remove(body); }
        }

        private void FloatBody(PhysicBody body, float force)
        {
            if (body == null || body.isKinematic) return;
            touchedBodies.Add(body);
            if (!originalGravity.ContainsKey(body))
            {
                originalGravity.Add(body, body.useGravity);
                floatHeights.Add(body, body.worldCenterOfMass.y + Mathf.Min(force * 0.4f, 3f));
            }
            body.useGravity = false;
            float speed = Mathf.Clamp((floatHeights[body] - body.worldCenterOfMass.y) * 2f, -1f, Mathf.Min(force * 0.25f, 2.5f));
            body.velocity = Vector3.Lerp(body.velocity, Vector3.up * speed, Time.fixedDeltaTime * 2.5f);
        }

        private void ReleaseLevitation()
        {
            foreach (var entry in originalGravity)
            {
                try { if (entry.Key != null) entry.Key.useGravity = entry.Value; } catch (Exception error) { Debug.LogWarning("[IAmAtomic] Gravity restore: " + error.Message); }
            }
            originalGravity.Clear();
            floatHeights.Clear();
            touchedBodies.Clear();

        }

        private bool InScope(object target, Vector3 position, Vector3 origin, Creature player, float radius)
        {
            if (Vector3.Distance(position, origin) > radius) return false;
            if (!AtomicModOptions.roomLock || player == null) return true;
            var playerArea = ReadArea(player);
            var targetArea = ReadArea(target);
            if (playerArea != null && targetArea != null) return ReferenceEquals(playerArea, targetArea);
            return true;
        }

        private object ReadArea(object target)
        {
            var type = target.GetType();
            if (!areaFields.TryGetValue(type, out var field))
            {
                field = type.GetField("currentArea", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                areaFields[type] = field;
            }
            return field?.GetValue(target);
        }

        private Vector3 GetOrigin(Creature playerCreature)
        {
            if (activeBladeItem != null)
            {
                return activeBladeItem.transform.position;
            }

            if (playerCreature != null)
            {
                RagdollHand bladeHand = playerCreature.GetHand(bladeSide);
                if (bladeHand != null && bladeHand.grabbedHandle != null && bladeHand.grabbedHandle.item != null)
                {
                    return bladeHand.grabbedHandle.item.transform.position;
                }
                if (playerCreature.ragdoll != null)
                {
                    RagdollPart torso = playerCreature.ragdoll.GetPart(RagdollPart.Type.Torso);
                    if (torso != null)
                    {
                        return torso.transform.position;
                    }
                }
                return playerCreature.transform.position;
            }

            return transform.position;
        }

        public void ForceTriggerRitual()
        {
            Creature playerCreature = GetPlayerCreature();
            if (playerCreature == null || currentState != RitualState.Idle) return;
            Item weapon = null;
            RagdollHand rightHand = playerCreature.GetHand(Side.Right);
            if (rightHand != null && rightHand.grabbedHandle != null)
            {
                weapon = rightHand.grabbedHandle.item;
            }
            StartRitual(Side.Left, Side.Right, weapon, playerCreature);
        }

        public void ForceDetonate()
        {
            Creature playerCreature = GetPlayerCreature();
            Vector3 origin = GetOrigin(playerCreature);
            if (currentState == RitualState.Charging) BeginDetonation(origin, ritualMode == "HoldRelease");
        }
    }
}
