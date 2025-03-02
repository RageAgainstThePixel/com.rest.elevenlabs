// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Models;
using ElevenLabs.TextToSpeech;
using ElevenLabs.Voices;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Async;
using Utilities.Audio;

namespace ElevenLabs.Demo
{
    [RequireComponent(typeof(StreamAudioSource))]
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
        private StreamAudioSource streamAudioSource;

#if !UNITY_2022_3_OR_NEWER
        private readonly CancellationTokenSource lifetimeCts = new();
        // ReSharper disable once InconsistentNaming
        private CancellationToken destroyCancellationToken => lifetimeCts.Token;
#endif

        private void OnValidate()
        {
            if (streamAudioSource == null)
            {
                streamAudioSource = GetComponent<StreamAudioSource>();
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

                var request = new TextToSpeechRequest(voice, message, model: Model.FlashV2_5, outputFormat: OutputFormat.PCM_24000);
                var voiceClip = await api.TextToSpeechEndpoint.TextToSpeechAsync(request, async partialClip =>
                {
                    await streamAudioSource.BufferCallbackAsync(partialClip.ClipSamples);
                }, cancellationToken: destroyCancellationToken);
                await new WaitUntil(() => streamAudioSource.IsEmpty || destroyCancellationToken.IsCancellationRequested);
                destroyCancellationToken.ThrowIfCancellationRequested();
                ((AudioSource)streamAudioSource).clip = voiceClip.AudioClip;

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
    }
}
