using System;
using System.IO;
using UnityEngine;
using ThunderRoad;

namespace IAmAtomic
{
    public class AtomicThunderScript : ThunderScript
    {
        public static AtomicThunderScript Instance { get; private set; }
        public static AtomicRitualManager RitualManager { get; private set; }
        public string ModFolder { get; private set; }

        private GameObject managerObject;

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            Instance = this;
            Debug.Log("[IAmAtomic] v1.2.0 loaded");

            ModFolder = modData != null ? modData.fullPath : null;
            if (string.IsNullOrEmpty(ModFolder))
            {
                ModFolder = Path.GetDirectoryName(typeof(AtomicThunderScript).Assembly.Location);
            }

            if (managerObject == null)
            {
                managerObject = new GameObject("IAmAtomic_Manager");
                GameObject.DontDestroyOnLoad(managerObject);
                RitualManager = managerObject.AddComponent<AtomicRitualManager>();
                RitualManager.Initialize(ModFolder);
            }
        }

        public override void ScriptEnable()
        {
            base.ScriptEnable();
            if (RitualManager != null) RitualManager.enabled = true;
            EventManager.onLevelLoad -= OnLevelLoad;
            EventManager.onLevelLoad += OnLevelLoad;
        }
        public override void ScriptDisable()
        {
            EventManager.onLevelLoad -= OnLevelLoad;
            if (RitualManager != null) RitualManager.enabled = false;
            base.ScriptDisable();
        }
        private void OnLevelLoad(LevelData data, LevelData.Mode mode, EventTime time)
        {
            if (time == EventTime.OnStart && RitualManager != null) RitualManager.ResetForLevel();
        }
    }
}
