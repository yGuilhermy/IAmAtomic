using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace IAmAtomic
{
    public class AtomicAudioController : MonoBehaviour
    {
        private AudioSource audioSource;
        private AudioClip clipCharge;
        private AudioClip clipBlast;
        private AudioClip clipImpact;
        private bool blastScheduled;
        private Coroutine fadeRoutine;
        private string audioFolderPath;

        public void Initialize(string modFolder)
        {
            audioFolderPath = Path.Combine(modFolder, "audio");
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.dopplerLevel = 0f;
            audioSource.minDistance = 8f;
            audioSource.maxDistance = 120f;
            audioSource.playOnAwake = false;
            StartCoroutine(LoadAudioClips());
        }

        private IEnumerator LoadAudioClips()
        {
            yield return LoadClip("atomic_impact.ogg", clip => clipImpact = clip);
            yield return LoadClip("atomic_blast.ogg", clip => clipBlast = clip);
            yield return LoadClip("atomic_charge.ogg", clip => clipCharge = clip);
        }

        private IEnumerator LoadClip(string file, Action<AudioClip> assign)
        {
            var path = Path.Combine(audioFolderPath, file);
            if (!File.Exists(path)) { Debug.LogWarning("[IAmAtomic] Missing audio: " + file); yield break; }
            using (var request = UnityWebRequestMultimedia.GetAudioClip(new Uri(path).AbsoluteUri, AudioType.OGGVORBIS))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success) assign(DownloadHandlerAudioClip.GetContent(request));
                else Debug.LogWarning("[IAmAtomic] Audio load failed: " + file + " " + request.error);
            }
        }

        private void StopFade()
        {
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
        }

        public void PlayCharge(float duration, bool waitForRelease = false)
        {
            if (audioSource == null) return;
            StopFade();
            audioSource.Stop();
            blastScheduled = false;
            if (waitForRelease) return;
            audioSource.volume = 1f;
            audioSource.pitch = waitForRelease ? 1f : Mathf.Clamp(8.2f / Mathf.Max(0.1f, duration), 0.8f, 1.8f);
            audioSource.loop = waitForRelease || duration > 8.5f;
            audioSource.clip = clipCharge;
            if (clipCharge != null && duration > 1.5f) audioSource.Play();
        }

        public double ScheduleBlast(bool immediate)
        {
            double now = AudioSettings.dspTime;
            if (audioSource == null) return now;
            StopFade();
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.pitch = 1f;
            audioSource.volume = 1f;
            AudioClip clip = immediate ? clipImpact : clipBlast;
            double marker = immediate ? 0 : AtomicTiming.BlastImpactOffset;
            if (clip == null)
            {
                clip = clipImpact;
                marker = 0;
            }
            if (clip == null)
            {
                Debug.LogWarning("[IAmAtomic] Blast audio not loaded; using immediate visual/physical impact");
                return now;
            }
            audioSource.clip = clip;
            double start = now + AtomicTiming.ScheduleLead;
            audioSource.PlayScheduled(start);
            blastScheduled = true;
            AudioSettings.GetDSPBufferSize(out int frames, out int buffers);
            double latency = AudioSettings.outputSampleRate > 0 ? (double)frames * buffers / AudioSettings.outputSampleRate : 0;
            return AtomicTiming.ImpactDeadline(start, marker, latency, AtomicModOptions.impactAdjustment);
        }

        public void StopWithFadeout(float fadeTime)
        {
            if (audioSource == null) return;
            StopFade();
            if (blastScheduled) { audioSource.Stop(); blastScheduled = false; return; }
            fadeRoutine = StartCoroutine(FadeOutRoutine(Mathf.Max(0.01f, fadeTime)));
        }

        private IEnumerator FadeOutRoutine(float duration)
        {
            float volume = audioSource.volume;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(volume, 0f, timer / duration);
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = volume;
            audioSource.loop = false;
            fadeRoutine = null;
        }

        public void StopImmediately()
        {
            StopFade();
            if (audioSource != null) audioSource.Stop();
            blastScheduled = false;
        }

        private void OnDestroy()
        {
            if (audioSource != null) audioSource.Stop();
            if (clipCharge != null) Destroy(clipCharge);
            if (clipBlast != null) Destroy(clipBlast);
            if (clipImpact != null) Destroy(clipImpact);
        }
    }
}
