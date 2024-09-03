// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace ElevenLabs
{
    /// <summary>
    /// Represents authentication for ElevenLabs
    /// </summary>
    public sealed class ElevenLabsAuthentication : AbstractAuthentication<ElevenLabsAuthentication, ElevenLabsAuthInfo, ElevenLabsConfiguration>
    {
        internal const string CONFIG_FILE = ".elevenlabs";
        private const string ELEVENLABS_API_KEY = nameof(ELEVENLABS_API_KEY);
        private const string ELEVEN_LABS_API_KEY = nameof(ELEVEN_LABS_API_KEY);

        /// <summary>
        /// Allows implicit casting from a string, so that a simple string API key can be provided in place of an instance of Authentication.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        public static implicit operator ElevenLabsAuthentication(string apiKey) => new(apiKey);

        /// <summary>
        /// Instantiates an empty Authentication object.
        /// </summary>
        public ElevenLabsAuthentication() { }

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="apiKey"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="apiKey">The API key, required to access the API endpoint.</param>
        public ElevenLabsAuthentication(string apiKey)
        {
            Info = new ElevenLabsAuthInfo(apiKey);
            cachedDefault = this;
        }

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="authInfo"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="authInfo"></param>
        public ElevenLabsAuthentication(ElevenLabsAuthInfo authInfo)
        {
            Info = authInfo;
            cachedDefault = this;
        }

        /// <summary>
        /// Instantiates a new Authentication object with the given <see cref="configuration"/>.
        /// </summary>
        /// <param name="configuration"><see cref="ElevenLabsConfiguration"/>.</param>
        public ElevenLabsAuthentication(ElevenLabsConfiguration configuration) : this(configuration.ApiKey) { }

        /// <inheritdoc />
        public override ElevenLabsAuthInfo Info { get; }

        private static ElevenLabsAuthentication cachedDefault;

        /// <summary>
        /// The default authentication to use when no other auth is specified.
        /// This can be set manually, or automatically loaded via environment variables or a config file.
        /// <seealso cref="LoadFromEnvironment"/><seealso cref="LoadFromDirectory"/>
        /// </summary>
        public static ElevenLabsAuthentication Default
        {
            get => cachedDefault ?? new ElevenLabsAuthentication().LoadDefault();
            internal set => cachedDefault = value;
        }

        /// <inheritdoc />
        public override ElevenLabsAuthentication LoadFromAsset(ElevenLabsConfiguration configuration = null)
        {
            if (configuration == null)
            {
                Debug.LogWarning($"This can be speed this up by passing a {nameof(ElevenLabsConfiguration)} to the {nameof(ElevenLabsAuthentication)}.ctr");
                configuration = Resources.LoadAll<ElevenLabsConfiguration>(string.Empty).FirstOrDefault(o => o != null);
            }

            return configuration != null ? new ElevenLabsAuthentication(configuration) : null;
        }

        /// <inheritdoc />
        public override ElevenLabsAuthentication LoadFromEnvironment()
        {
            var apiKey = Environment.GetEnvironmentVariable(ELEVEN_LABS_API_KEY);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable(ELEVENLABS_API_KEY);
            }

            return string.IsNullOrEmpty(apiKey) ? null : new ElevenLabsAuthentication(apiKey);
        }

        /// <inheritdoc />
        /// ReSharper disable once OptionalParameterHierarchyMismatch
        public override ElevenLabsAuthentication LoadFromDirectory(string directory = null, string filename = CONFIG_FILE, bool searchUp = true)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Environment.CurrentDirectory;
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = CONFIG_FILE;
            }

            ElevenLabsAuthInfo tempAuthInfo = null;

            var currentDirectory = new DirectoryInfo(directory);

            while (tempAuthInfo == null && currentDirectory?.Parent != null)
            {
                var filePath = Path.Combine(currentDirectory.FullName, filename);

                if (File.Exists(filePath))
                {
                    try
                    {
                        tempAuthInfo = JsonUtility.FromJson<ElevenLabsAuthInfo>(File.ReadAllText(filePath));
                        break;
                    }
                    catch (Exception)
                    {
                        // try to parse the old way for backwards support.
                    }

                    var lines = File.ReadAllLines(filePath);
                    string apiKey = null;

                    foreach (var line in lines)
                    {
                        var parts = line.Split('=', ':');

                        for (var i = 0; i < parts.Length - 1; i++)
                        {
                            var part = parts[i];
                            var nextPart = parts[i + 1];

                            apiKey = part switch
                            {
                                ELEVENLABS_API_KEY => nextPart.Trim(),
                                ELEVEN_LABS_API_KEY => nextPart.Trim(),
                                _ => apiKey
                            };
                        }
                    }

                    tempAuthInfo = new ElevenLabsAuthInfo(apiKey);
                }

                if (searchUp)
                {
                    currentDirectory = currentDirectory.Parent;
                }
                else
                {
                    break;
                }
            }

            return string.IsNullOrEmpty(tempAuthInfo?.ApiKey) ? null : new ElevenLabsAuthentication(tempAuthInfo);
        }
    }
}
