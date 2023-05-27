using System;
using System.Linq;
using ElevenLabs.Voices;
using UnityEngine;

namespace ElevenLabs.Demo
{
    [RequireComponent(typeof(AudioSource))]
    public class TextToSpeechDemo : MonoBehaviour
    {
        [SerializeField]
        private Voice voice;

        [SerializeField]
        private string message;

        [SerializeField]
        private AudioSource audioSource;

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
                var api = new ElevenLabsClient();

                if (voice == null)
                {
                    voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
                }

                var (_, clip) = await api.TextToSpeechEndpoint.TextToSpeechAsync(message, voice, deleteCachedFile: true);
                audioSource.PlayOneShot(clip);

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
