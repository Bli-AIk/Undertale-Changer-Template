using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UCT.Global.Settings
{
    public enum KeyBindingType
    {
        Primary,
        SecondaryA,
        SecondaryB
    }

    public static class KeyBindings
    {
        private static Dictionary<KeyBindingType, Dictionary<string, KeyCode>> _keyBindings = new()
        {
            {
                KeyBindingType.Primary, new Dictionary<string, KeyCode>
                {
                    { "MoveDown", KeyCode.DownArrow },
                    { "MoveRight", KeyCode.RightArrow },
                    { "MoveUp", KeyCode.UpArrow },
                    { "MoveLeft", KeyCode.LeftArrow },
                    { "Confirm", KeyCode.Z },
                    { "Cancel", KeyCode.X },
                    { "Backpack", KeyCode.C },
                    { "Settings", KeyCode.V },
                    { "FullScreen", KeyCode.F4 },
                    { "Resolution", KeyCode.None },
                    { "Sfx", KeyCode.None },
                    { "ExitGame", KeyCode.Escape }
                }
            },
            {
                KeyBindingType.SecondaryA, new Dictionary<string, KeyCode>
                {
                    { "MoveDown", KeyCode.S },
                    { "MoveRight", KeyCode.D },
                    { "MoveUp", KeyCode.W },
                    { "MoveLeft", KeyCode.A },
                    { "Confirm", KeyCode.Return },
                    { "Cancel", KeyCode.RightShift },
                    { "Backpack", KeyCode.RightControl },
                    { "Settings", KeyCode.None },
                    { "FullScreen", KeyCode.None },
                    { "Resolution", KeyCode.None },
                    { "Sfx", KeyCode.None },
                    { "ExitGame", KeyCode.None }
                }
            },
            {
                KeyBindingType.SecondaryB, new Dictionary<string, KeyCode>
                {
                    { "MoveDown", KeyCode.None },
                    { "MoveRight", KeyCode.None },
                    { "MoveUp", KeyCode.None },
                    { "MoveLeft", KeyCode.None },
                    { "Confirm", KeyCode.None },
                    { "Cancel", KeyCode.LeftShift },
                    { "Backpack", KeyCode.LeftControl },
                    { "Settings", KeyCode.None },
                    { "FullScreen", KeyCode.None },
                    { "Resolution", KeyCode.None },
                    { "Sfx", KeyCode.None },
                    { "ExitGame", KeyCode.None }
                }
            }
        };

        public static void SetKeyCode(KeyBindingType type, string dataName, KeyCode key)
        {
            var subDictionary = GetKeyCodes(type);
            if (subDictionary.ContainsKey(dataName))
                subDictionary[dataName] = key;
            else
                throw new ArgumentException("The key binding does not exist.");
        }

        public static void SetKeyCodeAtIndex(KeyBindingType type, int index, KeyCode key)
        {
            var subDictionary = GetKeyCodes(type);
            if (index < 0 || index >= subDictionary.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            var keyValuePair = subDictionary.ElementAt(index);
            subDictionary[keyValuePair.Key] = key;
        }


        public static KeyCode GetKeyCode(KeyBindingType type, string dataName)
        {
            var subDictionary = GetKeyCodes(type);
            return subDictionary.GetValueOrDefault(dataName, KeyCode.None);
        }

        public static KeyCode GetKeyCodeAtIndex(KeyBindingType type, int index)
        {
            var subDictionary = GetKeyCodes(type);
            if (index < 0 || index >= subDictionary.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            var keyValuePair = subDictionary.ElementAt(index);
            return keyValuePair.Value;
        }

        public static Dictionary<string, KeyCode> GetKeyCodes(KeyBindingType type)
        {
            return _keyBindings[type];
        }

        public static Dictionary<KeyBindingType, Dictionary<string, KeyCode>> GetDictionary()
        {
            return _keyBindings;
        }

        public static void ResetDictionary()
        {
            _keyBindings = new Dictionary<KeyBindingType, Dictionary<string, KeyCode>>
            {
                {
                    KeyBindingType.Primary, new Dictionary<string, KeyCode>
                    {
                        { "MoveDown", KeyCode.DownArrow },
                        { "MoveRight", KeyCode.RightArrow },
                        { "MoveUp", KeyCode.UpArrow },
                        { "MoveLeft", KeyCode.LeftArrow },
                        { "Confirm", KeyCode.Z },
                        { "Cancel", KeyCode.X },
                        { "Backpack", KeyCode.C },
                        { "Settings", KeyCode.V },
                        { "FullScreen", KeyCode.F4 },
                        { "Resolution", KeyCode.None },
                        { "Sfx", KeyCode.None },
                        { "ExitGame", KeyCode.Escape }
                    }
                },
                {
                    KeyBindingType.SecondaryA, new Dictionary<string, KeyCode>
                    {
                        { "MoveDown", KeyCode.S },
                        { "MoveRight", KeyCode.D },
                        { "MoveUp", KeyCode.W },
                        { "MoveLeft", KeyCode.A },
                        { "Confirm", KeyCode.Return },
                        { "Cancel", KeyCode.RightShift },
                        { "Backpack", KeyCode.RightControl },
                        { "Settings", KeyCode.None },
                        { "FullScreen", KeyCode.None },
                        { "Resolution", KeyCode.None },
                        { "Sfx", KeyCode.None },
                        { "ExitGame", KeyCode.None }
                    }
                },
                {
                    KeyBindingType.SecondaryB, new Dictionary<string, KeyCode>
                    {
                        { "MoveDown", KeyCode.None },
                        { "MoveRight", KeyCode.None },
                        { "MoveUp", KeyCode.None },
                        { "MoveLeft", KeyCode.None },
                        { "Confirm", KeyCode.None },
                        { "Cancel", KeyCode.LeftShift },
                        { "Backpack", KeyCode.LeftControl },
                        { "Settings", KeyCode.None },
                        { "FullScreen", KeyCode.None },
                        { "Resolution", KeyCode.None },
                        { "Sfx", KeyCode.None },
                        { "ExitGame", KeyCode.None }
                    }
                }
            };
        }
        
        private static KeyCode GetEveryKeyCodeAtIndex(int index,
            KeyValuePair<KeyBindingType, Dictionary<string, KeyCode>> keyBinding)
        {
            var subDictionary = keyBinding.Value;
            if (index < 0 || index >= subDictionary.Count) return KeyCode.None;
            var keyCode = subDictionary.ElementAt(index).Value;
            return keyCode;
        }

        public static bool GetInputEveryKeyCodeAtIndex(int index,
            KeyValuePair<KeyBindingType, Dictionary<string, KeyCode>> keyBinding, Func<KeyCode, bool> inputMethod)
        {
            return inputMethod(GetEveryKeyCodeAtIndex(index, keyBinding));
        }
    }
}