// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using System;
using System.Collections.Generic;
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

        private readonly Queue<AudioClip> streamClipQueue = new Queue<AudioClip>();

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
                    api.VoicesEndpoint.EnableDebug = true;
                    voice = (await api.VoicesEndpoint.GetAllVoicesAsync(lifetimeCancellationTokenSource.Token)).FirstOrDefault();
                }

                streamClipQueue.Clear();
                api.TextToSpeechEndpoint.EnableDebug = true;
                var voiceClip = await api.TextToSpeechEndpoint.StreamTextToSpeechAsync(message, voice, partialClip =>
                {
                    streamClipQueue.Enqueue(partialClip);
                }, cancellationToken: lifetimeCancellationTokenSource.Token);

                audioSource.clip = voiceClip.AudioClip;
                Debug.Log($"Full clip: {voiceClip.Id}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Update()
        {
            if (!audioSource.isPlaying &&
                streamClipQueue.TryDequeue(out var clip))
            {
                Debug.Log($"Playing {clip.name}");
                audioSource.PlayOneShot(clip);
            }
        }

        private void OnDestroy()
        {
            lifetimeCancellationTokenSource?.Cancel();
            lifetimeCancellationTokenSource?.Dispose();
            lifetimeCancellationTokenSource = null;
        }
    }
}
