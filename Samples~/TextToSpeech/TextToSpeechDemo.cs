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
using Utilities.Audio;

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

        private readonly Queue<float> sampleQueue = new();

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


                sampleQueue.Clear();
                var request = new TextToSpeechRequest(voice, message, model: Model.EnglishTurboV2, outputFormat: OutputFormat.PCM_24000);
                var voiceClip = await api.TextToSpeechEndpoint.TextToSpeechAsync(request, partialClip =>
                {
                    const int sampleRate = 44100; // default unity audio clip sample rate
                    // we have to resample the partial clip to the unity audio clip sample rate. Ideally use PCM_44100
                    var resampled = PCMEncoder.Resample(partialClip.ClipSamples, partialClip.SampleRate, sampleRate);
                    foreach (var sample in resampled)
                    {
                        sampleQueue.Enqueue(sample);
                    }
                }, cancellationToken: destroyCancellationToken);
                audioSource.clip = voiceClip.AudioClip;
                await new WaitUntil(() => sampleQueue.Count == 0 || destroyCancellationToken.IsCancellationRequested);
                destroyCancellationToken.ThrowIfCancellationRequested();

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

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (sampleQueue.Count <= 0) { return; }

            for (var i = 0; i < data.Length; i += channels)
            {
                var sample = sampleQueue.Dequeue();

                for (var j = 0; j < channels; j++)
                {
                    data[i + j] = sample;
                }
            }
        }

#if !UNITY_2022_3_OR_NEWER
        private void OnDestroy()
        {
            lifetimeCts.Cancel();
        }
#endif
    }
}
