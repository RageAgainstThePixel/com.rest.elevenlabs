using ElevenLabs.Voices;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace ElevenLabs.Demo
{
    [RequireComponent(typeof(AudioSource))]
    public class TextToSpeechDemo : MonoBehaviour
    {
        [SerializeField]
        private Voice voice;

        [TextArea(3, 10)]
        [SerializeField]
        private string message;

        [SerializeField]
        private AudioSource audioSource;

        private CancellationTokenSource lifetimeCancellationTokenSource;

        private void OnValidate()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private async void Start()
        {
            OnValidate();
            lifetimeCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var api = new ElevenLabsClient();

                if (voice == null)
                {
                    voice = (await api.VoicesEndpoint.GetAllVoicesAsync(lifetimeCancellationTokenSource.Token)).FirstOrDefault();
                }

                var clipOffset = 0;
                var (_, clip) = await api.TextToSpeechEndpoint.StreamTextToSpeechAsync(message, voice, audioClip =>
                    {
                        audioSource.PlayOneShot(audioClip);
                        clipOffset = audioClip.samples;
                        Debug.Log($"Stream Playback {clipOffset}");
                    },
                    deleteCachedFile: true,
                    cancellationToken: lifetimeCancellationTokenSource.Token);

                audioSource.clip = clip;
                Debug.Log($"Stream complete {clip.samples}");

                if (clipOffset != clip.samples)
                {
                    Debug.LogWarning($"offset by {clip.samples - clipOffset}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnDestroy()
        {
            lifetimeCancellationTokenSource?.Cancel();
        }
    }
}
