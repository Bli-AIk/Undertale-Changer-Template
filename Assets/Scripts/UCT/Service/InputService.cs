using System.Collections.Generic;
using System.Linq;
using UCT.Global.Core;
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
            var keyCodes = MainControl.Instance.overworldControl.KeyCodes;
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
                _ => false
            };
        }

        private static bool GetKeyDownFrom(List<KeyCode>[] keyCodes, int index)
        {
            return keyCodes.Any(keyCode => Input.GetKeyDown(keyCode[index]));
        }

        public static bool GetKey(KeyCode key)
        {
            var keyCodes = MainControl.Instance.overworldControl.KeyCodes;
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
                _ => false
            };
        }

        private static bool GetKeyFrom(List<KeyCode>[] keyCodes, int index)
        {
            return keyCodes.Any(keyCode => Input.GetKey(keyCode[index]));
        }

        public static bool GetKeyUp(KeyCode key)
        {
            var keyCodes = MainControl.Instance.overworldControl.KeyCodes;
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
                _ => false
            };
        }

        private static bool GetKeyUpFrom(List<KeyCode>[] keyCodes, int index)
        {
            return keyCodes.Any(keyCode => Input.GetKeyUp(keyCode[index]));
        }

        /// <summary>
        ///     应用默认键位
        /// </summary>
        public static List<KeyCode>[] ApplyDefaultControl()
        {
            var keyCodes = new List<KeyCode>[3];
            keyCodes[0] = new List<KeyCode>
            {
                KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.LeftArrow,
                KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V,
                KeyCode.F4, KeyCode.None, KeyCode.None, KeyCode.Escape
            };
            keyCodes[1] = new List<KeyCode>
            {
                KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.A,
                KeyCode.Return, KeyCode.RightShift, KeyCode.RightControl, KeyCode.None,
                KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None
            };
            keyCodes[2] = new List<KeyCode>
            {
                KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None,
                KeyCode.None, KeyCode.LeftShift, KeyCode.LeftControl, KeyCode.None,
                KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None
            };

            return keyCodes;
        }
    }
}