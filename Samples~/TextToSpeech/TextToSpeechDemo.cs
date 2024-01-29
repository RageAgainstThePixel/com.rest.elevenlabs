// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Models;
using ElevenLabs.Voices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Async;

namespace ElevenLabs.Demo
{
    [RequireComponent(typeof(AudioSource))]
    public class TextToSpeechDemo : MonoBehaviour
    {
        [SerializeField]
        private ElevenLabsConfiguration configuration;

        [SerializeField]
        private bool debug = true;

        [SerializeField]
        private Voice voice;

        [TextArea(3, 10)]
        [SerializeField]
        private string message;

        [SerializeField]
        private AudioSource audioSource;

        private readonly Queue<AudioClip> streamClipQueue = new();

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

            try
            {
                var api = new ElevenLabsClient(configuration)
                {
                    EnableDebug = debug
                };

                if (voice == null)
                {
                    voice = (await api.VoicesEndpoint.GetAllVoicesAsync(destroyCancellationToken)).FirstOrDefault();
                }

                streamClipQueue.Clear();
                var streamQueueCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
                PlayStreamQueue(streamQueueCts.Token);
                var voiceClip = await api.TextToSpeechEndpoint.StreamTextToSpeechAsync(message, voice, partialClip =>
                {
                    streamClipQueue.Enqueue(partialClip);
                }, model: Model.EnglishTurboV2, cancellationToken: destroyCancellationToken);
                audioSource.clip = voiceClip.AudioClip;
                await new WaitUntil(() => streamClipQueue.Count == 0 && !audioSource.isPlaying);
                streamQueueCts.Cancel();

                if (debug)
                {
                    Debug.Log($"Full clip: {voiceClip.Id}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async void PlayStreamQueue(CancellationToken cancellationToken)
        {
            try
            {
                await new WaitUntil(() => streamClipQueue.Count > 0);
                var endOfFrame = new WaitForEndOfFrame();

                do
                {
                    if (!audioSource.isPlaying &&
                        streamClipQueue.TryDequeue(out var clip))
                    {
                        Debug.Log($"playing partial clip: {clip.name}");
                        audioSource.PlayOneShot(clip);
                    }

                    await endOfFrame;
                } while (!cancellationToken.IsCancellationRequested);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
