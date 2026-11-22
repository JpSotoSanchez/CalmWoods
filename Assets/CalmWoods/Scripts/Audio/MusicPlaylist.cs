using System.Collections.Generic;
using CalmWoods.Environment;
using UnityEngine;
using UnityEngine.UI;

namespace CalmWoods.Audio
{
    // Background music: plays every track once in a random order, one after another,
    // then reshuffles and starts again. Tracks cross-fade into each other.
    // Volume follows the Music slider (SessionSettings.MusicVolume).
    // Put it on an empty GameObject; it creates its own AudioSources.
    public class MusicPlaylist : MonoBehaviour
    {
        [SerializeField] AudioClip[] tracks;

        [Header("Volume")]
        [Tooltip("Optional. The Music slider of this scene's settings panel.")]
        [SerializeField] Slider musicSlider;
        [SerializeField, Range(0f, 1f)] float musicVolume = 0.6f;

        [Header("Playback")]
        [SerializeField] bool playOnStart = true;
        [Tooltip("Seconds the end of one track overlaps the start of the next. 0 = no overlap.")]
        [SerializeField, Min(0f)] float crossfadeSeconds = 4f;

        readonly AudioSource[] sources = new AudioSource[2];
        readonly List<int> order = new List<int>();
        int orderIndex;
        int lastTrack = -1;
        int active;
        float fade = 1f;          // 0..1 progress of the current cross-fade
        float trackStartedAt;
        bool playing;

        public AudioClip CurrentTrack => playing ? sources[active].clip : null;

        void Awake()
        {
            for (int i = 0; i < sources.Length; i++)
            {
                var go = new GameObject($"Music_{i}");
                go.transform.SetParent(transform, false);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                source.volume = 0f;
                sources[i] = source;
            }
        }

        void Start()
        {
            if (musicSlider != null)
            {
                musicSlider.minValue = 0f;
                musicSlider.maxValue = 1f;
                musicSlider.SetValueWithoutNotify(SessionSettings.MusicVolume);
                musicSlider.onValueChanged.AddListener(v => SessionSettings.MusicVolume = v);
            }

            if (playOnStart) Play();
        }

        public void Play()
        {
            if (playing) return;
            playing = true;
            PlayNextTrack(instant: true);
        }

        public void Stop()
        {
            playing = false;
            foreach (AudioSource source in sources) source.Stop();
        }

        // Can be wired to a "next song" button.
        public void Skip()
        {
            if (playing) PlayNextTrack(instant: false);
        }

        void Update()
        {
            if (!playing) return;

            float master = SessionSettings.MusicVolume;
            master *= master * musicVolume;   // Squared: closer to perceived loudness

            fade = crossfadeSeconds > 0f ? Mathf.MoveTowards(fade, 1f, Time.deltaTime / crossfadeSeconds) : 1f;

            // Equal-power cross-fade between the outgoing and the incoming track.
            AudioSource current = sources[active];
            AudioSource previous = sources[1 - active];
            current.volume = master * Mathf.Sin(fade * Mathf.PI * 0.5f);
            previous.volume = master * Mathf.Cos(fade * Mathf.PI * 0.5f);
            if (fade >= 1f && previous.isPlaying) previous.Stop();

            if (current.clip == null) return;

            float remaining = current.clip.length - current.time;
            float overlap = Mathf.Min(crossfadeSeconds, current.clip.length * 0.25f);
            bool nearEnd = current.isPlaying && remaining <= overlap;
            // Safety net in case the end was missed (e.g. very short clip).
            bool ended = !current.isPlaying && Time.unscaledTime - trackStartedAt > 1f;

            if (nearEnd || ended) PlayNextTrack(instant: false);
        }

        void PlayNextTrack(bool instant)
        {
            int track = NextTrackIndex();
            if (track < 0)
            {
                Debug.LogWarning("[Music] No tracks assigned.", this);
                playing = false;
                return;
            }

            active = 1 - active;
            AudioSource source = sources[active];
            source.clip = tracks[track];
            source.time = 0f;
            source.Play();

            lastTrack = track;
            trackStartedAt = Time.unscaledTime;
            fade = instant ? 1f : 0f;
            if (instant) sources[1 - active].Stop();
        }

        // Walks through a shuffled list; reshuffles when every track has played once.
        int NextTrackIndex()
        {
            if (orderIndex >= order.Count) Reshuffle();
            if (order.Count == 0) return -1;
            return order[orderIndex++];
        }

        void Reshuffle()
        {
            order.Clear();
            orderIndex = 0;
            if (tracks == null) return;

            for (int i = 0; i < tracks.Length; i++)
                if (tracks[i] != null) order.Add(i);

            // Fisher-Yates shuffle.
            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }

            // Never play the same song twice in a row across two rounds.
            if (order.Count > 1 && order[0] == lastTrack)
            {
                int j = Random.Range(1, order.Count);
                (order[0], order[j]) = (order[j], order[0]);
            }
        }
    }
}
