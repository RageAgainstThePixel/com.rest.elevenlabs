// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Models;
using ElevenLabs.TextToSpeech;
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

#if !UNITY_2022_3_OR_NEWER
        private readonly CancellationTokenSource lifetimeCts = new();
        // ReSharper disable once InconsistentNaming
        private CancellationToken destroyCancellationToken => lifetimeCts.Token;
#endif

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
                var streamTask = PlayStreamQueueAsync(streamQueueCts.Token);
                var request = new TextToSpeechRequest(voice, message, model: Model.EnglishTurboV2, outputFormat: OutputFormat.PCM_24000);
                var voiceClip = await api.TextToSpeechEndpoint.TextToSpeechAsync(request, partialClip =>
                {
                    streamClipQueue.Enqueue(partialClip);
                }, cancellationToken: destroyCancellationToken);
                audioSource.clip = voiceClip.AudioClip;
                await streamTask.ConfigureAwait(true);
                destroyCancellationToken.ThrowIfCancellationRequested();
                streamQueueCts.Cancel();

                if (debug)
                {
                    Debug.Log($"Full clip: {voiceClip.Id}");
                }
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TaskCanceledException:
                    case OperationCanceledException:
                        break;
                    default:
                        Debug.LogException(e);
                        break;
                }
            }
        }

#if !UNITY_2022_3_OR_NEWER
        private void OnDestroy()
        {
            lifetimeCts.Cancel();
        }
#endif

        private async Task PlayStreamQueueAsync(CancellationToken cancellationToken)
        {
            try
            {
                await new WaitUntil(() => streamClipQueue.Count > 0 || cancellationToken.IsCancellationRequested);
                cancellationToken.ThrowIfCancellationRequested();

                do
                {
                    if (!audioSource.isPlaying &&
                        streamClipQueue.TryDequeue(out var clip))
                    {
                        if (debug)
                        {
                            Debug.Log($"playing partial clip: {clip.name}");
                        }

                        audioSource.clip = clip;
                        audioSource.Play();
                        await Task.Delay(TimeSpan.FromSeconds(clip.length), cancellationToken).ConfigureAwait(true);
                    }

                    await Task.Yield();
                } while (!cancellationToken.IsCancellationRequested);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TaskCanceledException:
                    case OperationCanceledException:
                        break;
                    default:
                        Debug.LogException(e);
                        break;
                }
            }
        }
    }
}
