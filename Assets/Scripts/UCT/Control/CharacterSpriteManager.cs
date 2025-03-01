using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Control
{
    [CreateAssetMenu(fileName = "CharacterSpriteManager", menuName = "UCT/CharacterSpriteManager")]
    [Serializable]
    public class CharacterSpriteManager : ScriptableObject
    {
        public List<string> fxKeys;
        public List<AudioClip> fxValues;

        public List<string> spriteKeys;
        public List<SpriteExpressionCollection> spriteValues;

        /// <summary>
        ///     将所写的关于角色的富文本转换为展开形式（组，精灵，音频）。
        ///     展开方式为依次检索，如果对应项为空，使用Default。
        /// </summary>
        public static (string result, CharacterSpriteManager manager) StandardizeCharacterKey(string key)
        {
            key = key.ToLower(CultureInfo.InvariantCulture);
            var characterSpriteManagers = MainControl.Instance.CharacterSpriteManagers;
            CharacterSpriteManager characterSpriteManager = null;
            if (string.IsNullOrWhiteSpace(key))
            {
                Other.Debug.LogError("Key is null or empty");
                return (string.Empty, null);
            }

            key = CharacterKeyEscapeUnary(key, ref characterSpriteManager, characterSpriteManagers);

            return (CharacterKeyEscapeBinary(key, ref characterSpriteManager, characterSpriteManagers,
                    out var binaryResult)
                    ? binaryResult
                    : CharacterKeyEscapeBinaryTernary(key, ref characterSpriteManager, characterSpriteManagers),
                characterSpriteManager);
        }

        private static string CharacterKeyEscapeUnary(string key,
            ref CharacterSpriteManager characterSpriteManager,
            CharacterSpriteManager[] characterSpriteManagers)
        {
            if (!Regex.IsMatch(key, "^<[^,]+>$")) // <xxx>
            {
                return key;
            }

            var keyText = key[1..^1];
            var any = false;
            foreach (var manager in characterSpriteManagers)
            {
                if (manager.name.ToLower() != keyText)
                {
                    continue;
                }

                any = true;
                characterSpriteManager = manager;
                break;
            }

            key = any
                ? $"<{keyText}, {keyText}>"
                : $"<default, {keyText}>";

            return key;
        }

        private static bool CharacterKeyEscapeBinary(string key,
            ref CharacterSpriteManager characterSpriteManager,
            CharacterSpriteManager[] characterSpriteManagers,
            out string result)
        {
            var matchBinary = Regex.Match(key, @"^<([^,]+),\s*([^,]+)>$");
            if (!matchBinary.Success)
            {
                Other.Debug.LogError($"Invalid key format: {key}");
                result = string.Empty;
                return false;
            }

            var firstPart = matchBinary.Groups[1].Value.Trim();
            var secondPart = matchBinary.Groups[2].Value.Trim();
            if (!characterSpriteManager)
            {
                var isUnknownGroup = true;
                foreach (var manager in characterSpriteManagers)
                {
                    if (firstPart != manager.name.ToLower())
                    {
                        continue;
                    }

                    isUnknownGroup = false;
                    characterSpriteManager = manager;
                    break;
                }

                if (isUnknownGroup)
                {
                    Other.Debug.LogError($"Invalid key first part format: {key}");
                    result = string.Empty;
                    return true;
                }
            }

            var isMatchingSpriteKey =
                characterSpriteManager.spriteKeys.Any(spriteKey => secondPart == spriteKey.ToLower());
            var isMatchingFxKey = characterSpriteManager.fxKeys.Any(fxKey => secondPart == fxKey.ToLower());

            if (IsMatchBinary(ref key, out result, isMatchingSpriteKey, isMatchingFxKey, firstPart, secondPart))
            {
                return true;
            }

            Other.Debug.LogError($"Invalid key second part format: {key}");
            result = string.Empty;
            return true;
        }

        private static bool IsMatchBinary(ref string key,
            out string result,
            bool isMatchingSpriteKey,
            bool isMatchingFxKey,
            string firstPart,
            string secondPart)
        {
            if (isMatchingSpriteKey && isMatchingFxKey)
            {
                key = $"<{firstPart}, {secondPart}, {secondPart}>";
            }
            else
            {
                if (isMatchingSpriteKey)
                {
                    key = $"<{firstPart}, {secondPart}, default>";
                }

                if (isMatchingFxKey)
                {
                    key = $"<{firstPart}, default, {secondPart}>";
                }
            }

            if (!isMatchingSpriteKey && !isMatchingFxKey)
            {
                result = null;
                return false;
            }

            result = key;
            return true;

        }

        private static string CharacterKeyEscapeBinaryTernary(string key,
            ref CharacterSpriteManager characterSpriteManager,
            CharacterSpriteManager[] characterSpriteManagers)
        {
            var matchTernary = Regex.Match(key, @"^<[^,]+,\s*[^,]+,\s*[^,]+>$");
            var firstPart = matchTernary.Groups[1].Value.Trim();
            var secondPart = matchTernary.Groups[2].Value.Trim();
            var thirdPart = matchTernary.Groups[3].Value.Trim();
            if (!matchTernary.Success)
            {
                Other.Debug.LogError($"Invalid key format: {key}");
                return string.Empty;
            }

            if (!characterSpriteManager)
            {
                var isUnknownGroup = true;
                foreach (var manager in characterSpriteManagers)
                {
                    if (firstPart != manager.name.ToLower())
                    {
                        continue;
                    }

                    isUnknownGroup = false;
                    characterSpriteManager = manager;
                    break;
                }

                if (isUnknownGroup)
                {
                    Other.Debug.LogError($"Invalid key first part format: {key}");
                    return string.Empty;
                }
            }


            var isMatchingSpriteKey =
                characterSpriteManager.spriteKeys.Any(spriteKey => secondPart == spriteKey.ToLower());
            if (!isMatchingSpriteKey)
            {
                Other.Debug.LogError($"Invalid key second part format: {key}");
                return string.Empty;
            }

            var isMatchingFxKey = characterSpriteManager.fxKeys.Any(fxKey => thirdPart == fxKey.ToLower());
            if (isMatchingFxKey)
            {
                return key;
            }

            Other.Debug.LogError($"Invalid key third part format: {key}");
            return string.Empty;
        }

        public static (string Manager, string Sprite, string AudioClip)? ParseTernary(string key)
        {
            var match = Regex.Match(key, @"^<([^,]+),\s*([^,]+),\s*([^,]+)>$");

            if (match.Success)
            {
                return (
                    match.Groups[1].Value.Trim(),
                    match.Groups[2].Value.Trim(),
                    match.Groups[3].Value.Trim()
                );
            }

            return null;
        }
    }
}