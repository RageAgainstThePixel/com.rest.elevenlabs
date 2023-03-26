// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Authentication;
using UnityEngine;

namespace ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_00_Authentication
    {
        [SetUp]
        public void Setup()
        {
            var authJson = new AuthInfo("key-test12");
            var authText = JsonUtility.ToJson(authJson, true);
            File.WriteAllText(".elevenlabs", authText);
        }

        [Test]
        public void Test_01_GetAuthFromEnv()
        {
            var auth = ElevenLabsAuthentication.LoadFromEnv();
            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.ApiKey);
            Assert.IsNotEmpty(auth.ApiKey);
        }

        [Test]
        public void Test_02_GetAuthFromFile()
        {
            var auth = ElevenLabsAuthentication.LoadFromDirectory();
            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.ApiKey);
            Assert.AreEqual("key-test12", auth.ApiKey);
        }

        [Test]
        public void Test_03_GetAuthFromNonExistentFile()
        {
            var auth = ElevenLabsAuthentication.LoadFromDirectory(filename: "bad.config");
            Assert.IsNull(auth);
        }

        [Test]
        public void Test_04_Authentication()
        {
            var defaultAuth = ElevenLabsAuthentication.Default;
            var manualAuth = new ElevenLabsAuthentication("key-testAA");
            var api = new ElevenLabsClient();
            var shouldBeDefaultAuth = api.ElevenLabsAuthentication;
            Assert.IsNotNull(shouldBeDefaultAuth);
            Assert.IsNotNull(shouldBeDefaultAuth.ApiKey);
            Assert.AreEqual(defaultAuth.ApiKey, shouldBeDefaultAuth.ApiKey);

            ElevenLabsAuthentication.Default = new ElevenLabsAuthentication("key-testAA");
            api = new ElevenLabsClient();
            var shouldBeManualAuth = api.ElevenLabsAuthentication;
            Assert.IsNotNull(shouldBeManualAuth);
            Assert.IsNotNull(shouldBeManualAuth.ApiKey);
            Assert.AreEqual(manualAuth.ApiKey, shouldBeManualAuth.ApiKey);

            ElevenLabsAuthentication.Default = defaultAuth;
        }

        [Test]
        public void Test_05_GetKey()
        {
            var auth = new ElevenLabsAuthentication("key-testAA");
            Assert.IsNotNull(auth.ApiKey);
            Assert.AreEqual("key-testAA", auth.ApiKey);
        }

        [Test]
        public void Test_06_GetKeyFailed()
        {
            ElevenLabsAuthentication auth = null;

            try
            {
                auth = new ElevenLabsAuthentication("fail-key");
            }
            catch (InvalidCredentialException)
            {
                Assert.IsNull(auth);
            }
            catch (Exception e)
            {
                Assert.IsTrue(false, $"Expected exception {nameof(InvalidCredentialException)} but got {e.GetType().Name}");
            }
        }

        [Test]
        public void Test_07_ParseKey()
        {
            var auth = new ElevenLabsAuthentication("key-testAA");
            Assert.IsNotNull(auth.ApiKey);
            Assert.AreEqual("key-testAA", auth.ApiKey);
            auth = "key-testCC";
            Assert.IsNotNull(auth.ApiKey);
            Assert.AreEqual("key-testCC", auth.ApiKey);

            auth = new ElevenLabsAuthentication("key-testBB");
            Assert.IsNotNull(auth.ApiKey);
            Assert.AreEqual("key-testBB", auth.ApiKey);
        }

        [Test]
        public void Test_08_CustomDomainConfigurationSettings()
        {
            var auth = new ElevenLabsAuthentication("customIssuedToken");
            var settings = new ElevenLabsClientSettings(domain: "api.your-custom-domain.com");
            var api = new ElevenLabsClient(auth, settings);
            Console.WriteLine(api.ElevenLabsClientSettings.BaseRequest);
            Console.WriteLine(api.ElevenLabsClientSettings.BaseRequestUrlFormat);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(".elevenlabs"))
            {
                File.Delete(".elevenlabs");
            }
        }
    }
}
