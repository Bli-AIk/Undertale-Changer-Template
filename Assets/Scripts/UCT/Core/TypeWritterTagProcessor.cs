using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ink.Runtime;
using MEC;
using TMPro;
using UCT.Audio;
using UCT.Control;
using UCT.Scene;
using UCT.Service;
using UCT.Settings;
using UnityEngine;

namespace UCT.Core
{
    public static partial class TypeWritterTagProcessor
    {
        private static string _itemDataName;

        public static void SetItemDataName(string dataName)
        {
            _itemDataName = dataName;
        }

        /// <summary>
        ///     识别打字机的自定义富文本标签并执行相关方法
        /// </summary>
        /// <param name="typeWritter">打字机</param>
        /// <param name="tmpText">打字机应用的TMP文本</param>
        /// <param name="index">打字机执行中的索引</param>
        /// <param name="yieldNum">打字机在这次自定义富文本执行后停顿的时间。如果为O则不会停顿。</param>
        /// <param name="yieldString">打字机停顿过程中会逐步输出的文本内容。根据yieldNum决定一次的输出字数。</param>
        /// <param name="startPassText">是否执行PassText</param>
        /// <returns></returns>
        public static void TypeWritterProcessTag(TypeWritter typeWritter,
            TMP_Text tmpText,
            ref int index,
            out int yieldNum,
            out string yieldString,
            out bool startPassText)
        {
            yieldNum = 0;
            yieldString = null;
            startPassText = false;

            if (typeWritter.originString[index] != '<')
            {
                return;
            }

            var fix = index == 0;

            while (typeWritter.originString[index] == '<')
            {
                var spText = new StringBuilder();
                while (fix || typeWritter.originString[index - 1] != '>')
                {
                    spText.Append(typeWritter.originString[index]);
                    index++;
                    if (fix)
                    {
                        fix = false;
                    }
                }

                typeWritter.passTextString += spText.ToString();


                index = TypeWritterExecuteHalfTag(typeWritter, tmpText, spText.ToString(), index, ref yieldNum,
                    ref yieldString,
                    ref startPassText);

                if (index >= typeWritter.originString.Length)
                {
                    typeWritter.originString += " ";
                    break;
                }

                fix = true;

                if (startPassText)
                {
                    break;
                }
            }
        }

        private static int TypeWritterExecuteHalfTag(TypeWritter typeWritter,
            TMP_Text tmpText,
            string spText,
            int index,
            ref int yieldNum,
            ref string yieldString,
            ref bool startPassText)
        {
            object[] args = { typeWritter, tmpText, spText, index, yieldNum, startPassText };

            if (!TryGetHalfValue(spText, HalfTagHandlers, out var handler))
            {
                return TypeWritterExecuteFullTag(typeWritter, tmpText, spText, index, ref yieldNum, ref yieldString,
                    ref startPassText);
            }

            if (spText.StartsWith("<waitForTime="))
            {
                startPassText = true;
            }
                
            return handler.Invoke(args);

        }

        private static bool TryGetHalfValue<T>(string spText,
            Dictionary<string, IMethodWrapper<T>> dictionary,
            out IMethodWrapper<T> handler)
        {
            string longestMatch = null;
            foreach (var key in dictionary.Keys.Where(key =>
                         spText.StartsWith(key) && (longestMatch == null || key.Length > longestMatch.Length)))
            {
                longestMatch = key;
            }


            if (longestMatch != null)
            {
                return dictionary.TryGetValue(longestMatch, out handler);
            }

            handler = null;
            return false;
        }


        private static void StoryFade(string spText)
        {
            if (!StorySceneController.Instance)
            {
                Debug.LogError("未检测到StorySceneController。");
                return;
            }

            var save = spText[11..^1];
            StorySceneController.Instance.Fade(int.Parse(save));
        }

        private static void PassTextWithDelay(TypeWritter typeWritter, string spText)
        {
            var delay = float.Parse(spText[13..^1]);
            typeWritter.passText = true;
            typeWritter.passTextString = typeWritter.passTextString[..^spText.Length];
            typeWritter.PassTextWithDelay(spText, delay);
        }

        private static int TypeWritterExecuteFullTag(TypeWritter typeWritter,
            TMP_Text tmpText,
            string spText,
            int index,
            ref int yieldNum,
            ref string yieldString,
            ref bool startPassText)
        {
            var data = new FullTagData
            {
                Index = index,
                YieldNum = yieldNum,
                YieldString = yieldString,
                StartPassText = startPassText,
                ProceedToDefault = false,
                OverWriteSpText = null
            };
            object[] args = { typeWritter, tmpText, spText, data, _itemDataName };

            if (FullTagHandlers.TryGetValue(spText, out var handler))
            {
                data = handler.Invoke(args);

                if (data.ProceedToDefault)
                {
                    FullTagDefaultCase(data, typeWritter, tmpText, spText);
                }
            }
            else
            {
                FullTagDefaultCase(data, typeWritter, tmpText, spText);
            }

            index = data.Index;
            yieldNum = data.YieldNum;
            yieldString = data.YieldString;
            startPassText = data.StartPassText;

            return index;
        }

        private static void FullTagDefaultCase(FullTagData data,
            TypeWritter typeWritter,
            TMP_Text tmpText,
            string spText)
        {
            var dataOverWriteSpText = spText;
            if (!string.IsNullOrEmpty(data.OverWriteSpText))
            {
                dataOverWriteSpText = data.OverWriteSpText;
            }

            FullTagDefaultCase(typeWritter, tmpText, dataOverWriteSpText);
        }

        private static void FullTagDefaultCase(TypeWritter typeWritter, TMP_Text tmpText, string spText)
        {
            if (!TextMeshProRichTextChecker.ContainsRichText(spText))
            {
                if (spText.Length >= 2 && spText[0] == '<' && spText[2] == '>')
                {
                    spText = spText[1].ToString();
                }
                else if (spText.Length - 2 > 0 && spText[1] == '-' && spText[^2] == '-')
                {
                    spText = SkipSomeText(typeWritter, spText);
                }
                else
                {
                    ParseCharacterRichText(typeWritter, spText);
                    return;
                }
            }

            typeWritter.endString += spText;
            tmpText.text = typeWritter.endString;
        }

        private static void ParseCharacterRichText(TypeWritter typeWritter, string spText)
        {
            var standardizeCharacterKey = CharacterSpriteManager.StandardizeCharacterKey(spText);
            var parsedValue = CharacterSpriteManager.ParseTernary(standardizeCharacterKey.result);
            if (parsedValue == null)
            {
                return;
            }

            typeWritter.characterSpriteManager = standardizeCharacterKey.manager;
            typeWritter.fxClip = AudioController.GetClipFromCharacterSpriteManager(
                parsedValue.Value.AudioClip,
                typeWritter.characterSpriteManager);

            if (string.Equals(parsedValue.Value.Manager, "default",
                    StringComparison.CurrentCultureIgnoreCase))
            {
                typeWritter.overworldSpriteChanger.spriteExpressionCollection = null;
            }
            else
            {
                for (var index = 0; index < typeWritter.characterSpriteManager.spriteKeys.Count; index++)
                {
                    var spriteKey = typeWritter.characterSpriteManager.spriteKeys[index];
                    var spriteValue = typeWritter.characterSpriteManager.spriteValues[index];
                    if (string.Equals(spriteKey, parsedValue.Value.Sprite,
                            StringComparison.CurrentCultureIgnoreCase))
                    {
                        typeWritter.overworldSpriteChanger.spriteExpressionCollection = spriteValue;
                    }
                }
            }

            typeWritter.overworldSpriteChanger.UpdateSpriteDisplay();
        }

        private static string SkipSomeText(TypeWritter typeWritter, string spText)
        {
            spText = spText.Substring(2, spText.Length - 4);
            if (typeWritter.isSkip || typeWritter.isJumpingText)
            {
                return spText;
            }

            TypeWritterPlayFx(typeWritter);
            typeWritter.isUsedFx = true;

            return spText;
        }

        private static void TypeWritterChangeSkip(TypeWritter typeWritter)
        {
            typeWritter.cantSkip = !typeWritter.cantSkip;
        }

        private static string TypeItemValue(string dataName)
        {
            return DataHandlerService.GetItemFormDataName(dataName).Data.Value.ToString();
        }

        private static int AutoTypeFoodInfo(TypeWritter typeWritter, string spText, int index)
        {
            var foodValue = DataHandlerService.GetItemFormDataName(_itemDataName).Data.Value;
            var plusString = foodValue + typeWritter.hpSave >=
                             MainControl.Instance.playerControl.hpMax
                ? MainControl.Instance.LanguagePackControl.dataTexts[22]
                : MainControl.Instance.LanguagePackControl.dataTexts[12];

            plusString = plusString[..^1];

            typeWritter.originString = TextProcessingService.RemoveSubstring(
                typeWritter.originString,
                index - "<autoFood>".Length, index - 1, plusString);
            index -= spText.Length;
            typeWritter.passTextString = typeWritter.passTextString[..^spText.Length];

            return index;
        }

        public static float GetTypeWritterStopTime(TypeWritter typeWritter)
        {
            return Timing.WaitForSeconds(typeWritter.speedSlow -
                                         typeWritter.speedSlow * 0.25f * Convert.ToInt32(!SettingsStorage.TextWidth));
        }

        public static void TypeWritterPlayFx(TypeWritter typeWritter)
        {
            AudioController.Instance.PlayFx(typeWritter.fxClip,
                typeWritter.volume, typeWritter.pitch, typeWritter.audioMixerGroup);
        }


        private static void TypeWritterFixedSpeed(TypeWritter typeWritter)
        {
            typeWritter.currentSpeed = typeWritter.speed;
        }

        private static void SetTypeWritterFont(TypeWritter typeWritter, TMP_Text tmpText, string spText)
        {
            var save = spText[6..];
            save = save[..^1];
            typeWritter.fontIndex = int.Parse(save);
            tmpText.font = MainControl.Instance.overworldControl.tmpFonts[typeWritter.fontIndex];
        }

        private static void SetTypeWritterFx(TypeWritter typeWritter, string spText)
        {
            var save = spText[4..];
            save = save[..^1];
            var clipFromCharacterSpriteManager = AudioController.GetClipFromCharacterSpriteManager(save);
            if (clipFromCharacterSpriteManager.result)
            {
                typeWritter.fxClip = clipFromCharacterSpriteManager.result;
            }

            if (clipFromCharacterSpriteManager.manager)
            {
                typeWritter.characterSpriteManager = clipFromCharacterSpriteManager.manager;
            }
        }

        public static void SetSpeedMode(TypeWritter typeWritter)
        {
            typeWritter.currentSpeed = SettingsStorage.TypingSpeed switch
            {
                TypingSpeed.Slow => typeWritter.speed + 0.025f,
                TypingSpeed.Medium => typeWritter.speed,
                TypingSpeed.Fast => typeWritter.speed - 0.025f,
                _ => typeWritter.currentSpeed
            };
        }

        /// <summary>
        ///     检测富文本符号并转换
        /// </summary>
        public static string ConvertStaticTagHandlers(string text,
            string inputText,
            bool isData,
            string inputName,
            List<string> ex)
        {
            object[] args = { text, isData, inputName, ex };

            return StaticTagHandlers.TryGetValue(inputText, out var handler)
                ? handler.Invoke(args)
                : StaticTagDefaultCase(text, inputText, isData, inputName);
        }


        private static string HandleAutoFoodFull(string text)
        {
            var itemControl = MainControl.Instance.LanguagePackControl;
            return text + itemControl.dataTexts[11][..^1] + "\n<autoFood>";
        }

        private static string HandleAutoTag(string text, int dataIndex)
        {
            return text + MainControl.Instance.LanguagePackControl.dataTexts[dataIndex][..^1];
        }

        private static string HandleEnemyInfo(string text,
            string inputName,
            bool isData,
            List<string> ex,
            int index)
        {
            return !isData && !string.IsNullOrEmpty(inputName)
                ? text + ex[index]
                : HandleDefault(text);
        }

        private static string StaticTagDefaultCase(string text, string inputText, bool isData, string inputName)
        {
            object[] args = { text, inputText, isData, inputName };

            return TryGetHalfValue(inputText, StaticHalfTagHandlers, out var handler)
                ? handler.Invoke(args)
                : text + inputText;
        }

        private static string RepeatStopTag(string text, string inputText)
        {
            var count = int.Parse(inputText[..^1]["<stop*".Length..]);
            var newStop = string.Concat(Enumerable.Repeat("<stop>", count));
            return text + newStop;
        }


        private static string HandleDataTag(string text, string inputText)
        {
            var index = int.Parse(inputText.AsSpan(5, inputText.Length - 6));
            var data = MainControl.Instance.LanguagePackControl.dataTexts[index];
            return text + data[..^1];
        }

        private static string HandleItemNameTag(string text, string inputText, bool isData, string dataName)
        {
            if (isData || string.IsNullOrEmpty(dataName))
            {
                return text + inputText;
            }

            var result =
                DataHandlerService.ItemDataNameGetLanguagePackName(dataName);
            return text + result;
        }

        private static string HandleAutoLoseTag(string text, string inputText)
        {
            var key = inputText.Substring(9, inputText.Length - 10) + "s";
            var dataIndex = key switch
            {
                "Foods" => 18,
                "Weapons" => 19,
                "Armors" => 20,
                "Others" => 21,
                _ => -1
            };

            return dataIndex != -1
                ? text + MainControl.Instance.LanguagePackControl.dataTexts[dataIndex]
                : text;
        }

        private static string HandleDefault(string text)
        {
            return text;
        }

        private struct FullTagData : IEquatable<FullTagData>
        {
            public int Index;
            public int YieldNum;
            public string YieldString;
            public bool StartPassText;
            public bool ProceedToDefault;
            public string OverWriteSpText;

            public bool Equals(FullTagData other)
            {
                return Index == other.Index && YieldNum == other.YieldNum && YieldString == other.YieldString &&
                       StartPassText == other.StartPassText && ProceedToDefault == other.ProceedToDefault &&
                       OverWriteSpText == other.OverWriteSpText;
            }

            public override bool Equals(object obj)
            {
                return obj is FullTagData other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Index, YieldNum, YieldString, StartPassText, ProceedToDefault, OverWriteSpText);
            }
        }
    }

    /// <summary>
    ///     TypeWritterTagProcessor字典映射部分
    /// </summary>
    public static partial class TypeWritterTagProcessor
    {
        /// <summary>
        ///     静态Tag的字典映射。这类富文本会在进入打字机前进行转义，替换为其他文本。
        /// </summary>
        private static readonly Dictionary<string, IMethodWrapper<string>> StaticTagHandlers =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["<playerName>"] = new MethodWrapper<string>(args =>
                    (string)args[0] + MainControl.Instance.playerControl.playerName),

                ["<enter>"] = new MethodWrapper<string>(args => (string)args[0] + "\n"),
                ["<markPoint>"] = new MethodWrapper<string>(args => (string)args[0] + "*< >"),
                ["<markEnter>"] = new MethodWrapper<string>(args => (string)args[0] + "\n*< >"),
                ["<blankEnter>"] = new MethodWrapper<string>(args => (string)args[0] + "\n< >< >"),

                ["<stop.>"] = new MethodWrapper<string>(args =>
                    (string)args[0] + ".<stop>"),
                ["<stop..>"] = new MethodWrapper<string>(args =>
                    (string)args[0] + ".<stop>.<stop>"),
                ["<stop...>"] = new MethodWrapper<string>(args =>
                    (string)args[0] + ".<stop>.<stop>.<stop>"),
                ["<stop....>"] = new MethodWrapper<string>(args =>
                    (string)args[0] + ".<stop>.<stop>.<stop>.<stop>"),
                ["<stop.....>"] = new MethodWrapper<string>(args =>
                    (string)args[0] + ".<stop>.<stop>.<stop>.<stop>.<stop>"),
                ["<stop......>"] = new MethodWrapper<string>(args =>
                    (string)args[0] + ".<stop>.<stop>.<stop>.<stop>.<stop>.<stop>"),


                ["<autoFoodFull>"] = new MethodWrapper<string>(args =>
                    HandleAutoFoodFull((string)args[0])),

                ["<autoCheckFood>"] = CreateAutoTagHandler(13),
                ["<autoWeapon>"] = CreateAutoTagHandler(14),
                ["<autoArmor>"] = CreateAutoTagHandler(15),
                ["<autoCheckWeapon>"] = CreateAutoTagHandler(16),
                ["<autoCheckArmor>"] = CreateAutoTagHandler(17),
                ["<autoLoseFood>"] = CreateAutoTagHandler(18),
                ["<autoLoseWeapon>"] = CreateAutoTagHandler(19),
                ["<autoLoseArmor>"] = CreateAutoTagHandler(20),
                ["<autoLoseOther>"] = CreateAutoTagHandler(21),

                ["<getEnemiesName>"] = new MethodWrapper<string>(args =>
                    HandleEnemyInfo(
                        (string)args[0],
                        (string)args[2],
                        (bool)args[1],
                        (List<string>)args[3],
                        0)),
                ["<getEnemiesATK>"] = new MethodWrapper<string>(args =>
                    HandleEnemyInfo(
                        (string)args[0],
                        (string)args[2],
                        (bool)args[1],
                        (List<string>)args[3],
                        1)),
                ["<getEnemiesDEF>"] = new MethodWrapper<string>(args =>
                    HandleEnemyInfo(
                        (string)args[0],
                        (string)args[2],
                        (bool)args[1],
                        (List<string>)args[3],
                        2))
            };

        /// <summary>
        ///     动态半Tag的字典映射。这类富文本可定义值，且会在打字机中进行转义，并执行方法。
        /// </summary>
        private static readonly Dictionary<string, IMethodWrapper<int>> HalfTagHandlers =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["<fx="] = new MethodWrapper<int>(args =>
                {
                    SetTypeWritterFx((TypeWritter)args[0], (string)args[2]);
                    return (int)args[3];
                }),
                ["<font="] = new MethodWrapper<int>(args =>
                {
                    SetTypeWritterFont((TypeWritter)args[0], (TMP_Text)args[1], (string)args[2]);
                    return (int)args[3];
                }),
                ["<waitForTime="] = new MethodWrapper<int>(args =>
                {
                    PassTextWithDelay((TypeWritter)args[0], (string)args[2]);
                    return (int)args[3];
                }),
                ["<storyFade"] = new MethodWrapper<int>(args =>
                {
                    StoryFade((string)args[2]);
                    return (int)args[3];
                }),
                ["<loadInk="] = new MethodWrapper<int>(args =>
                {
                    var spText = (string)args[2];
                    var languagePackId = MainControl.Instance.languagePackId;
                    string pathPrefix;
                    Story story;
                    if (languagePackId < MainControl.LanguagePackageInternalNumber)
                    {
                        pathPrefix =
                            $"TextAssets/LanguagePacks/{DataHandlerService.GetLanguageInsideId(languagePackId)}/Ink/";
                        story = InkService.ReadInkJsonFileFromResources(pathPrefix + spText[9..^1]);
                    }
                    else
                    {
                        pathPrefix =
                            $@"{Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[languagePackId - MainControl.LanguagePackageInternalNumber]}\Ink\";
                        story = InkService.ReadInkJsonFileFromLocalPath(pathPrefix + spText[9..^1]);
                    }

                    var typeWritter = (TypeWritter)args[0];
                    typeWritter.SelectController.SetStory(story);

                    var dialogue = typeWritter.SelectController.GetStoryDialogue();
                    dialogue = DataHandlerService.ChangeItemData(dialogue, false, new List<string>());
                    typeWritter.originString += dialogue;

                    return (int)args[3];
                })
            };

        /// <summary>
        ///     静态半Tag的字典映射。这类富文本可定义值，且会在进入打字机前进行转义，替换为其他文本。
        /// </summary>
        private static readonly Dictionary<string, IMethodWrapper<string>> StaticHalfTagHandlers =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["<data"] = new MethodWrapper<string>
                    (args => HandleDataTag((string)args[0], (string)args[1])),
                ["<stop*"] = new MethodWrapper<string>
                    (args => RepeatStopTag((string)args[0], (string)args[1])),
                ["<itemName"] = new MethodWrapper<string>
                    (args => HandleItemNameTag((string)args[0], (string)args[1], (bool)args[2], (string)args[3])),
                ["<autoLose"] = new MethodWrapper<string>
                    (args => HandleAutoLoseTag((string)args[0], (string)args[1]))
            };

        /// <summary>
        ///     动态Tag的字典映射。这类富文本会在打字机中进行转义，并执行方法。
        /// </summary>
        private static readonly Dictionary<string, IMethodWrapper<FullTagData>> FullTagHandlers =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["<storyMask=true>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var data = (FullTagData)args[3];
                    StorySceneController.Instance.mask.SetActive(true);
                    return data;
                }),

                ["<storyMask=false>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var data = (FullTagData)args[3];
                    StorySceneController.Instance.mask.SetActive(false);
                    return data;
                }),
                ["<storyExit>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var data = (FullTagData)args[3];
                    typeWritter.TypeStop();
                    GameUtilityService.FadeOutAndSwitchScene("Start", Color.black, null, true);
                    return data;
                }),
                ["<sprite=0>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var data = (FullTagData)args[3];
                    if (!(typeWritter.isSkip || typeWritter.isJumpingText))
                    {
                        TypeWritterPlayFx(typeWritter);
                    }

                    data.ProceedToDefault = true;
                    return data;
                }),
                ["<stop>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var data = (FullTagData)args[3];
                    if (!(typeWritter.isSkip || typeWritter.isJumpingText))
                    {
                        data.YieldNum++;
                    }

                    return data;
                }),
                ["<autoFood>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var spText = (string)args[2];
                    var data = (FullTagData)args[3];
                    data.Index = AutoTypeFoodInfo(typeWritter, spText, data.Index);
                    return data;
                }),
                ["<itemValue>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var data = (FullTagData)args[3];
                    var dataName = (string)args[4];
                    data.YieldString = TypeItemValue(dataName);
                    data.YieldNum = data.YieldString.Length;
                    return data;
                }),
                ["<changeSkip>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var data = (FullTagData)args[3];
                    TypeWritterChangeSkip(typeWritter);
                    return data;
                }),
                ["<jumpText>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var data = (FullTagData)args[3];
                    typeWritter.isJumpingText = true;
                    return data;
                }),
                ["</jumpText>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var data = (FullTagData)args[3];
                    typeWritter.isJumpingText = false;
                    return data;
                }),
                ["<waitForUpdate>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var data = (FullTagData)args[3];
                    typeWritter.passText = true;
                    data.StartPassText = true;
                    return data;
                }),
                ["<FixedSpeed>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var data = (FullTagData)args[3];
                    TypeWritterFixedSpeed(typeWritter);
                    return data;
                }),
                ["</FixedSpeed>"] = new MethodWrapper<FullTagData>
                (args =>
                {
                    var typeWritter = (TypeWritter)args[0];
                    var data = (FullTagData)args[3];
                    SetSpeedMode(typeWritter);
                    return data;
                })
            };

        private static IMethodWrapper<string> CreateAutoTagHandler(int dataIndex)
        {
            return new MethodWrapper<string>(args =>
                HandleAutoTag((string)args[0], dataIndex));
        }
    }
}