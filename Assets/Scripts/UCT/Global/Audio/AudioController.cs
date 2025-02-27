using System;
using System.Collections.Generic;
using UCT.Control;
using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace UCT.Global.Audio
{
    /// <summary>
    ///     基于对象池 控制音频
    /// </summary>
    public class AudioController : ObjectPool
    {
        public AudioSource audioSource;
        public static AudioController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            poolObject = new GameObject();
            var audioPlayer = poolObject.AddComponent<AudioPlayer>();
            audioPlayer.audioSource = poolObject.AddComponent<AudioSource>();

            poolObject.gameObject.name = "FX Source";
            poolObject.SetActive(false);
            FillPool<AudioPlayer>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            audioSource.outputAudioMixerGroup =
                MainControl.Instance.AudioControl.globalAudioMixer.FindMatchingGroups("BGM")[0];
        }

        public void PlayFx(int index,
            List<AudioClip> list,
            float volume = 0.5f,
            float pitch = 1,
            AudioMixerGroup audioMixerGroup = null)
        {
            if (index < 0 || list == null || index >= list.Count)
            {
                return;
            }

            PlayFxInternal(list[index], volume, pitch, audioMixerGroup);
        }

        public void PlayFx(AudioClip clip, float volume = 0.5f, float pitch = 1, AudioMixerGroup audioMixerGroup = null)
        {
            if (!clip)
            {
                return;
            }

            PlayFxInternal(clip, volume, pitch, audioMixerGroup);
        }

        private void PlayFxInternal(AudioClip clip, float volume, float pitch, AudioMixerGroup audioMixerGroup)
        {
            var fxAudioPlayer = GetFromPool<AudioPlayer>();
            var fxAudioSource = fxAudioPlayer.audioSource;
            fxAudioSource.volume = volume;
            fxAudioSource.pitch = pitch;

            if (!audioMixerGroup)
            {
                var groupName = clip switch
                {
                    _ when MainControl.Instance.AudioControl.fxClipUI.Contains(clip) => "FX/UI",
                    _ when MainControl.Instance.AudioControl.fxClipWalk.Contains(clip) => "FX/Walk",
                    _ when MainControl.Instance.AudioControl.fxClipBattle.Contains(clip) => "FX/Battle",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(groupName))
                {
                    audioMixerGroup =
                        MainControl.Instance.AudioControl.globalAudioMixer.FindMatchingGroups(groupName)[0];
                }
            }

            fxAudioSource.outputAudioMixerGroup = audioMixerGroup;
            fxAudioPlayer.Playing(clip);
        }


        public static (AudioClip result, CharacterSpriteManager manager) GetClipFromCharacterSpriteManager(string key)
        {
            if (key[0] != '<')
            {
                key = $"<{key}";
            }

            if (key[^1] != '>')
            {
                key = $"{key}>";
            }

            var standardizedKey = CharacterSpriteManager.StandardizeCharacterKey(key).result;
            var parsedKey = CharacterSpriteManager.ParseTernary(standardizedKey);
            if (parsedKey == null)
            {
                Other.Debug.LogError($"{key} is null!");
                return (null, null);
            }

            CharacterSpriteManager characterSpriteManager = null;
            CharacterSpriteManager defaultManager = null;

            foreach (var manager in MainControl.Instance.CharacterSpriteManagers)
            {
                if (string.Equals(manager.name, parsedKey.Value.Manager, StringComparison.CurrentCultureIgnoreCase))
                {
                    characterSpriteManager = manager;
                    break;
                }

                if (string.Equals(manager.name, "Default", StringComparison.CurrentCultureIgnoreCase))
                {
                    defaultManager = manager;
                }
            }

            characterSpriteManager ??= defaultManager;

            if (characterSpriteManager)
            {
                for (var index = 0; index < characterSpriteManager.fxKeys.Count; index++)
                {
                    var fxKey = characterSpriteManager.fxKeys[index];
                    if (string.Equals(fxKey, parsedKey.Value.AudioClip, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return (characterSpriteManager.fxValues[index], characterSpriteManager);
                    }
                }
            }

            Other.Debug.LogError($"{characterSpriteManager} does not include {parsedKey.Value.AudioClip}!");
            return (null, null);
        }

        public static AudioClip GetClipFromCharacterSpriteManager(string key, CharacterSpriteManager manager)
        {
            var clip = FindClip(manager, key);
            if (clip)
            {
                return clip;
            }

            if (string.Equals(key, "default", StringComparison.CurrentCultureIgnoreCase))
            {
                clip = FindClip(manager, manager.name);
                if (clip)
                {
                    return clip;
                }
            }

            Other.Debug.LogError($"{manager} does not include {key}!");
            return null;
        }

        private static AudioClip FindClip(CharacterSpriteManager manager, string searchKey)
        {
            for (var i = 0; i < manager.fxKeys.Count; i++)
            {
                if (string.Equals(manager.fxKeys[i], searchKey, StringComparison.CurrentCultureIgnoreCase))
                {
                    return manager.fxValues[i];
                }
            }

            return null;
        }
    }
}