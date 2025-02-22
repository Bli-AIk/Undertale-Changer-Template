using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UCT.Control;
using UCT.Global.Core;
using UCT.Global.Settings;
using UCT.Global.UI;
using UnityEngine;

namespace UCT.Service
{
    /// <summary>
    ///     数据处理相关函数
    /// </summary>
    public static class DataHandlerService
    {
        public static GameItem GetItemFormDataName(string dataName)
        {
            return MainControl.Instance.ItemController.ItemDictionary[dataName];
        }


        /// <summary>
        ///     通过DataName获取Item语言包名称
        /// </summary>
        public static string ItemDataNameGetLanguagePackName(string dataName)
        {
            return GetItemTextValue(dataName, 1);
        }

        /// <summary>
        ///     通过DataName获取Item UseText
        /// </summary>
        public static string ItemDataNameGetLanguagePackUseText(string dataName)
        {
            return GetItemTextValue(dataName, 2);
        }

        /// <summary>
        ///     通过DataName获取Item InfoText
        /// </summary>
        public static string ItemDataNameGetLanguagePackInfoText(string dataName)
        {
            return GetItemTextValue(dataName, 3);
        }

        /// <summary>
        ///     通过DataName获取Item DropText
        /// </summary>
        public static string ItemDataNameGetLanguagePackDropText(string dataName)
        {
            return GetItemTextValue(dataName, 4);
        }

        private static string GetItemTextValue(string dataName, int valueOffset)
        {
            var languagePackControl = MainControl.Instance.LanguagePackControl;
            for (var i = 0; i < languagePackControl.itemTexts.Count; i += 5)
            {
                var sonItem = languagePackControl.itemTexts[i];
                if (sonItem == dataName)
                {
                    return languagePackControl.itemTexts[i + valueOffset];
                }
            }

            return "";
        }
        /// <summary>
        ///     从 string 加载并解析数据，返回新列表
        /// </summary>
        public static List<string> LoadItemData(string inputText)
        {
            var resultList = new List<string>();
            var text = "";
            for (var i = 0; i < inputText.Length; i++)
            {
                if (inputText[i] == '/' && inputText[i + 1] == '*')
                {
                    i++;
                    while (!(inputText[i] == '/' && inputText[i - 1] == '*'))
                    {
                        i++;
                    }

                    i += 2;
                }

                if (inputText[i] != '\n' && inputText[i] != '\r' && inputText[i] != ';')
                {
                    text += inputText[i];
                }

                if (inputText[i] != ';')
                {
                    continue;
                }

                resultList.Add(text + ";");
                text = "";
            }

            return resultList;
        }
        /// <summary>
        ///     转换单个字符串中的特殊字符
        /// </summary>
        public static string ChangeItemData(string input, bool isData, List<string> ex)
        {
            var text = "";
            var empty = "";
            var isLoopSpecialCharsRequired = false;

            for (var j = 0; j < input.Length; j++)
            {
                if (empty == "" && !isData)
                {
                    var k = j;
                    while (input[j] != '\\')
                    {
                        empty += input[j];
                        j++;
                        if (j >= input.Length)
                        {
                            break;
                        }
                    }
                    j = k;
                }

                while (input[j] == '<')
                {
                    var inputText = "";
                    while ((j != 0 && input[j - 1] != '>' && !isLoopSpecialCharsRequired) || isLoopSpecialCharsRequired)
                    {
                        inputText += input[j];
                        j++;
                        if (j >= input.Length)
                        {
                            break;
                        }

                        isLoopSpecialCharsRequired = false;
                    }

                    isLoopSpecialCharsRequired = true;
                    text = TypeWritterTagProcessor.ConvertStaticTagHandlers(text, inputText, isData, empty, ex);
                }

                isLoopSpecialCharsRequired = false;

                if (input[j] == ';')
                {
                    text += ";";
                }
                else
                {
                    text += input[j];
                }
            }

            return text;
        }


        /// <summary>
        ///     转换特殊字符
        /// </summary>
        public static List<string> ChangeItemData(List<string> list, bool isData, List<string> ex)
        {
            var result = new List<string>();
            var text = "";
            var isLoopSpecialCharsRequired = false;

            foreach (var t in list)
            {
                var empty = "";
                for (var j = 0; j < t.Length; j++)
                {
                    if (empty == "" && !isData)
                    {
                        var k = j;
                        while (t[j] != '\\')
                        {
                            empty += t[j];
                            j++;
                            if (j >= t.Length)
                            {
                                break;
                            }
                        }

                        j = k;
                    }

                    while (t[j] == '<')
                    {
                        var inputText = "";
                        while ((j != 0 && t[j - 1] != '>' && !isLoopSpecialCharsRequired) || isLoopSpecialCharsRequired)
                        {
                            inputText += t[j];
                            j++;
                            if (j >= t.Length)
                            {
                                break;
                            }

                            isLoopSpecialCharsRequired = false;
                        }

                        isLoopSpecialCharsRequired = true;
                        text = TypeWritterTagProcessor.ConvertStaticTagHandlers(text, inputText, isData, empty, ex);
                    }

                    isLoopSpecialCharsRequired = false;

                    if (t[j] == ';')
                    {
                        result.Add(text + ";");
                        text = "";
                    }
                    else
                    {
                        text += t[j];
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     获取内置语言包ID
        /// </summary>
        public static string GetLanguageInsideId(int id)
        {
            return id switch
            {
                0 => "CN",
                1 => "TCN",
                _ => "US"
            };
        }

        /// <summary>
        ///     检测当前语言包ID是否在有效范围内。如果不在有效范围内，
        ///     则将语言包ID设置为默认值2。
        /// </summary>
        /// <param name="id">语言包ID</param>
        public static int LanguagePackDetection(int id)
        {
            if (id < 0 || id >= Directory.GetDirectories(Application.dataPath + "\\LanguagePacks").Length +
                MainControl.LanguagePackageInternalNumber)
            {
                return 2;
            }

            return id;
        }

        /// <summary>
        ///     加载对应语言包的数据
        /// </summary>
        public static string LoadLanguageData(string path, int id)
        {
            return id < MainControl.LanguagePackageInternalNumber
                ? Resources.Load<TextAsset>($"TextAssets/LanguagePacks/{GetLanguageInsideId(id)}/{path}").text
                : File.ReadAllText(
                    $"{Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[id - MainControl.LanguagePackageInternalNumber]}\\{path}.txt");
        }

        /// <summary>
        ///     检测语言包全半角
        /// </summary>
        public static void InitializationLanguagePackFullWidth()
        {
            if (SettingsStorage.textWidth != bool.Parse(
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.LanguagePackControl.settingTexts,
                        "LanguagePackFullWidth")))
            {
                SettingsStorage.textWidth = bool.Parse(
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.LanguagePackControl.settingTexts,
                        "LanguagePackFullWidth"));
                foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(TextChanger)))
                {
                    var textChanger = (TextChanger)obj;
                    textChanger.isUseWidth = SettingsStorage.textWidth;
                    textChanger.Set();
                    textChanger.Change();
                }
            }

            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture(
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.settingTexts,
                    "CultureInfo"));
        }

        /// <summary>
        ///     设置PlayerControl
        /// </summary>
        /// <param name="inputPlayerControl"></param>
        public static PlayerControl SetPlayerControl(PlayerControl inputPlayerControl)
        {
            var playerControl = ScriptableObject.CreateInstance<PlayerControl>();
            playerControl.hp = inputPlayerControl.hp;
            playerControl.hpMax = inputPlayerControl.hpMax;
            playerControl.lv = inputPlayerControl.lv;
            playerControl.exp = inputPlayerControl.exp;
            playerControl.gold = inputPlayerControl.gold;
            playerControl.nextExp = inputPlayerControl.nextExp;
            playerControl.missTime = inputPlayerControl.missTime;
            playerControl.missTimeMax = inputPlayerControl.missTimeMax;
            playerControl.atk = inputPlayerControl.atk;
            playerControl.def = inputPlayerControl.def;
            playerControl.playerName = inputPlayerControl.playerName;
            playerControl.items = inputPlayerControl.items;
            playerControl.wearWeapon = inputPlayerControl.wearWeapon;
            playerControl.wearArmor = inputPlayerControl.wearArmor;
            playerControl.canMove = inputPlayerControl.canMove;
            playerControl.gameTime = inputPlayerControl.gameTime;
            playerControl.lastScene = inputPlayerControl.lastScene;
            playerControl.saveScene = inputPlayerControl.saveScene;
            playerControl.isDebug = inputPlayerControl.isDebug;
            playerControl.invincible = inputPlayerControl.invincible;
            return playerControl;
        }
    }
}