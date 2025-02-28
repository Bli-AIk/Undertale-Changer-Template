using System;
using System.Collections.Generic;
using System.Linq;
using UCT.Global.Settings;
using UnityEngine;

namespace UCT.Service
{
    /// <summary>
    ///     输入相关函数
    /// </summary>
    public static class InputService
    {
        public static bool GetKeyDown(KeyCode key)
        {
            var keyCodes = KeyBindings.GetDictionary();
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return key switch
            {
                KeyCode.DownArrow => GetKeyDownFrom(keyCodes, 0),
                KeyCode.RightArrow => GetKeyDownFrom(keyCodes, 1),
                KeyCode.UpArrow => GetKeyDownFrom(keyCodes, 2),
                KeyCode.LeftArrow => GetKeyDownFrom(keyCodes, 3),
                KeyCode.Z => GetKeyDownFrom(keyCodes, 4),
                KeyCode.X => GetKeyDownFrom(keyCodes, 5),
                KeyCode.C => GetKeyDownFrom(keyCodes, 6),
                KeyCode.V => GetKeyDownFrom(keyCodes, 7),
                KeyCode.F4 => GetKeyDownFrom(keyCodes, 8),
                KeyCode.Tab => GetKeyDownFrom(keyCodes, 9),
                KeyCode.Semicolon => GetKeyDownFrom(keyCodes, 10),
                KeyCode.Escape => GetKeyDownFrom(keyCodes, 11),
                _ => throw new ArgumentNullException($"Unknown {key}")
            };
        }

        private static bool GetKeyDownFrom(Dictionary<KeyBindingType, Dictionary<string, KeyCode>> keyCodes, int index)
        {
            var result = keyCodes.Any(keyBinding
                => KeyBindings.GetInputEveryKeyCodeAtIndex(index, keyBinding, Input.GetKeyDown));

            return result;
        }

        public static bool GetKey(KeyCode key)
        {
            var keyCodes = KeyBindings.GetDictionary();
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return key switch
            {
                KeyCode.DownArrow => GetKeyFrom(keyCodes, 0),
                KeyCode.RightArrow => GetKeyFrom(keyCodes, 1),
                KeyCode.UpArrow => GetKeyFrom(keyCodes, 2),
                KeyCode.LeftArrow => GetKeyFrom(keyCodes, 3),
                KeyCode.Z => GetKeyFrom(keyCodes, 4),
                KeyCode.X => GetKeyFrom(keyCodes, 5),
                KeyCode.C => GetKeyFrom(keyCodes, 6),
                KeyCode.V => GetKeyFrom(keyCodes, 7),
                KeyCode.F4 => GetKeyFrom(keyCodes, 8),
                KeyCode.Tab => GetKeyFrom(keyCodes, 9),
                KeyCode.Semicolon => GetKeyFrom(keyCodes, 10),
                KeyCode.Escape => GetKeyFrom(keyCodes, 11),
                _ => throw new ArgumentNullException($"Unknown {key}")
            };
        }

        private static bool GetKeyFrom(Dictionary<KeyBindingType, Dictionary<string, KeyCode>> keyCodes, int index)
        {
            var result = keyCodes.Any(keyBinding
                => KeyBindings.GetInputEveryKeyCodeAtIndex(index, keyBinding, Input.GetKey));
            return result;
        }

        public static bool GetKeyUp(KeyCode key)
        {
            var keyCodes = KeyBindings.GetDictionary();
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return key switch
            {
                KeyCode.DownArrow => GetKeyUpFrom(keyCodes, 0),
                KeyCode.RightArrow => GetKeyUpFrom(keyCodes, 1),
                KeyCode.UpArrow => GetKeyUpFrom(keyCodes, 2),
                KeyCode.LeftArrow => GetKeyUpFrom(keyCodes, 3),
                KeyCode.Z => GetKeyUpFrom(keyCodes, 4),
                KeyCode.X => GetKeyUpFrom(keyCodes, 5),
                KeyCode.C => GetKeyUpFrom(keyCodes, 6),
                KeyCode.V => GetKeyUpFrom(keyCodes, 7),
                KeyCode.F4 => GetKeyUpFrom(keyCodes, 8),
                KeyCode.Tab => GetKeyUpFrom(keyCodes, 9),
                KeyCode.Semicolon => GetKeyUpFrom(keyCodes, 10),
                KeyCode.Escape => GetKeyUpFrom(keyCodes, 11),
                _ => throw new ArgumentNullException($"Unknown {key}")
            };
        }

        private static bool GetKeyUpFrom(Dictionary<KeyBindingType, Dictionary<string, KeyCode>> keyCodes, int index)
        {
            var result = keyCodes.Any(keyBinding
                => KeyBindings.GetInputEveryKeyCodeAtIndex(index, keyBinding, Input.GetKeyUp));
            return result;
        }
    }
}