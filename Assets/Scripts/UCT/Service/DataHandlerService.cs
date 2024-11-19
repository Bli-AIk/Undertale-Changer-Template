using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UCT.Control;
using UCT.Global.Core;
using UCT.Global.UI;
using UnityEngine;

namespace UCT.Service
{
    /// <summary>
    /// 数据处理相关函数
    /// </summary>
    public static class DataHandlerService
    {
        /// <summary>
        /// 分配Item数据
        /// </summary>
        public static void ClassifyItemsData(ItemControl itemControl)
        {
            itemControl.itemFoods.Clear();
            itemControl.itemArms.Clear();
            itemControl.itemArmors.Clear();
            itemControl.itemOthers.Clear();
            foreach (var countText in itemControl.itemMax)
            {
                var text = new string[4];
                var i = 0;
                foreach (var t in countText)
                {
                    if (t == '\\')
                        i++;
                    else if (t != ';')
                        text[i] += t.ToString();

                    if (i != 3 || t != ';') continue;
                    for (var j = 0; j < text.Length; j++)
                    {
                        if (j != 1)
                            AddItemToClassification(itemControl, text[1], text[j]);
                    }
                    i = 0;
                }
            }
        }

        /// <summary>
        /// ItemClassification的一个子void
        /// </summary>
        private static void AddItemToClassification(ItemControl itemControl, string i, string origin)
        {
            if (origin == "null") return;
            switch (i)
            {
                case "Foods":
                    itemControl.itemFoods.Add(origin);
                    break;
                case "Arms":
                    itemControl.itemArms.Add(origin);
                    break;
                case "Armors":
                    itemControl.itemArmors.Add(origin);
                    break;
                case "Others":
                    itemControl.itemOthers.Add(origin);
                    break;
                default:
                    itemControl.itemOthers.Add(origin);
                    break;
            }
        }

        /// <summary>
        /// 通过Id获取Item信息：
        /// type：Foods Arms Armors Others Auto
        /// number：0语言包名称
        ///     1/2：data1/2.
        ///     请勿多输.
        ///     Arm和Armor只有1
        /// </summary>
        public static string ItemIdGetName(ItemControl itemControl, int id, string type, int number)
        {
            int realId;
            List<string> list;
            string idName;//获取编号名称
            switch (type)
            {
                case "Foods":
                    list = itemControl.itemFoods;
                    realId = id * 3 - 3;
                    idName = list[realId];
                    break;

                case "Arms":
                    list = itemControl.itemArms;
                    realId = id * 2 - 2;
                    idName = list[realId];
                    break;

                case "Armors":
                    list = itemControl.itemArmors;
                    realId = id * 2 - 2;
                    idName = list[realId];
                    break;

                case "Others":
                    list = itemControl.itemOthers;
                    realId = id * 3 - 3;
                    idName = list[realId];
                    break;

                case "Auto":
                    switch (id)
                    {
                        case <= 0:
                            list = null;
                            realId = 0;
                            idName = "null";
                            break;
                        case < 10000:
                            list = itemControl.itemFoods;
                            realId = id * 3 - 3;
                            idName = list[realId];
                            break;
                        case < 20000:
                            list = itemControl.itemArms;
                            realId = (id - 10000) * 2 - 2;
                            idName = list[realId];
                            break;
                        case < 30000:
                            list = itemControl.itemArmors;
                            realId = (id - 20000) * 2 - 2;
                            idName = list[realId];
                            break;
                        default:
                            list = itemControl.itemOthers;
                            realId = (id - 30000) * 3 - 3;
                            idName = list[realId];
                            break;
                    }
                    break;

                default:
                    goto case "Others";
            }

            var subText = "";
            if (number == 0)//获取语言包内的名称
            {
                foreach (var t in itemControl.itemTextMaxItem.Where(t => t[..idName.Length] == idName))
                {
                    idName = t[(idName.Length + 1)..];
                    break;
                }

                subText = idName.TakeWhile(t => t != '\\').Aggregate(subText, (current, t) => current + t);
            }
            else
            {
                if (list != null) 
                    subText = list[realId + number];
            }
            return subText;
        }

        /// <summary>
        /// 通过Id获取Item的数据（HP，ATK等）：
        /// type：Foods Arms Armors Others Auto
        /// justId:勾的话会加上 +xxHP/AT/DF等信息
        /// </summary>
        public static string ItemIdGetData(ItemControl itemControl, int id, string type, bool notJustId = false)
        {
            int realId;
            List<string> list;
            string idData;//获取编号名称
            switch (type)
            {
                case "Foods":
                    list = itemControl.itemFoods;
                    realId = id * 3 - 1;

                    if (notJustId)
                    {
                        idData = list[realId] + "HP";
                        if (int.Parse(list[realId]) > 0)
                            idData = $"+{idData}";
                    }
                    else
                        idData = list[realId];
                    break;

                case "Arms":
                    list = itemControl.itemArms;
                    realId = id * 2 - 1;

                    if (notJustId)
                    {
                        idData = list[realId] + "AT";
                        if (int.Parse(list[realId]) > 0)
                            idData = $"+{idData}";
                    }
                    else
                        idData = list[realId];
                    break;

                case "Armors":
                    list = itemControl.itemArmors;
                    realId = id * 2 - 1;

                    if (notJustId)
                    {
                        idData = list[realId] + "DF";
                        if (int.Parse(list[realId]) > 0)
                            idData = $"+{idData}";
                    }
                    else
                        idData = list[realId];
                    break;

                case "Others":
                    list = itemControl.itemOthers;
                    realId = id * 3 - 1;
                    idData = list[realId];
                    break;

                case "Auto":
                    switch (id)
                    {
                        case < 10000:
                        {
                            list = itemControl.itemFoods;
                            realId = id * 3 - 1;

                            if (notJustId)
                            {
                                idData = list[realId] + "HP";
                                if (int.Parse(list[realId]) > 0)
                                    idData = $"+{idData}";
                            }
                            else
                                idData = list[realId];

                            break;
                        }
                        case < 20000:
                        {
                            list = itemControl.itemArms;
                            realId = (id - 10000) * 2 - 1;

                            if (notJustId)
                            {
                                idData = list[realId] + "AT";
                                if (int.Parse(list[realId]) > 0)
                                    idData = $"+{idData}";
                            }
                            else
                                idData = list[realId];

                            break;
                        }
                        case < 30000:
                        {
                            list = itemControl.itemArmors;
                            realId = (id - 20000) * 2 - 1;

                            if (notJustId)
                            {
                                idData = list[realId] + "DF";
                                if (int.Parse(list[realId]) > 0)
                                    idData = $"+{idData}";
                            }
                            else
                                idData = list[realId];

                            break;
                        }
                        default:
                            list = itemControl.itemOthers;
                            realId = (id - 30000) * 3 - 1;
                            idData = list[realId];
                            break;
                    }
                    break;

                default:
                    goto case "Others";
            }
            return idData;
        }

        /// <summary>
        /// 通过物品数据名称获取其ID。
        /// type：Foods Arms Armors Others
        /// </summary>
        public static int ItemNameGetId(ItemControl itemControl, string itemName, string type)
        {
            int id = 0, listInt;
            List<string> list;
            switch (type)
            {
                case "Foods":
                    list = itemControl.itemFoods;
                    listInt = 3;
                    break;

                case "Arms":
                    list = itemControl.itemArms;
                    listInt = 2;
                    id += 10000;
                    break;

                case "Armors":
                    list = itemControl.itemArmors;
                    listInt = 2;
                    id += 20000;
                    break;

                case "Others":
                    list = itemControl.itemOthers;
                    listInt = 3;
                    id += 30000;
                    break;

                default:
                    return 0;
            }

            if (list == null) return id;
            for (var i = 0; i < list.Count / listInt; i++)
            {
                if (list[i * listInt] != itemName) continue;
                id += i + 1;
                break;
            }
            return id;
        }
        
        
        /// <summary>
        /// 从 TextAsset 加载并解析数据，返回新列表
        /// </summary>
        /// <param name="inputText">输入的 TextAsset 数据</param>
        /// <returns>解析后的列表</returns>
        public static List<string> LoadItemData(TextAsset inputText)
        {
            var resultList = new List<string>();
            var text = "";
            for (var i = 0; i < inputText.text.Length; i++)
            {
                if (inputText.text[i] == '/' && inputText.text[i + 1] == '*')
                {
                    i++;
                    while (!(inputText.text[i] == '/' && inputText.text[i - 1] == '*'))
                    {
                        i++;
                    }
                    i += 2;
                }

                if (inputText.text[i] != '\n' && inputText.text[i] != '\r' && inputText.text[i] != ';')
                    text += inputText.text[i];
                if (inputText.text[i] != ';') continue;
                resultList.Add(text + ";");
                text = "";
            }
            return resultList;
        }


        /// <summary>
        /// 从 string 加载并解析数据，返回新列表
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
                    text += inputText[i];
                if (inputText[i] != ';') continue;
                resultList.Add(text + ";");
                text = "";
            }
            return resultList;
        }

        /// <summary>
        /// 转换特殊字符
        /// </summary>
        public static List<string> ChangeItemData(List<string> list, bool isData, List<string> ex)
        {
            var newList = new List<string>();
            var text = "";
            var isXh = false;//检测是否有多个需要循环调用的特殊字符

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
                                break;
                        }
                        j = k;
                        //Debug.Log(list[i] +"/"+ name);
                    }

                    while (t[j] == '<')
                    {
                        var inputText = "";
                        while ((j != 0 && t[j - 1] != '>' && !isXh) || isXh)
                        {
                            inputText += t[j];
                            j++;
                            if (j >= t.Length)
                            {
                                break;
                            }
                            isXh = false;
                        }
                        isXh = true;
                        text = ChangeItemDataSwitch(text, inputText, isData, empty, ex);
                    }
                    isXh = false;

                    if (t[j] == ';')
                    {
                        newList.Add(text + ";");
                        text = "";
                    }
                    else
                    {
                        text += t[j];
                    }
                }
            }
            return newList;
        }

        /// <summary>
        /// ReSharper disable once InvalidXmlDocComment
        /// ChangeItemData中检测'<''>'符号的Switch语句
        /// </summary>
        private static string ChangeItemDataSwitch(string text, string inputText, bool isData, 
            string inputName, List<string> ex)
        {
            switch (inputText)
            {
                case "<playerName>":
                    text += MainControl.Instance.playerControl.playerName;
                    break;

                case "<enter>"://回车
                    text += "\n";
                    break;

                case "<stop...>":
                    text += ".<stop>.<stop>.<stop>";
                    break;

                case "<stop......>":
                    text += ".<stop>.<stop>.<stop>.<stop>.<stop>.<stop>";
                    break;

                case "<autoFoodFull>":
                    text += MainControl.Instance.ItemControl.itemTextMaxData[11][..^1] + "\n";
                    text += "<autoFood>";
                    break;

                case "<autoCheckFood>":
                    inputText = "<data13>";
                    goto default;

                case "<autoArm>":
                    inputText = "<data14>";
                    goto default;
                case "<autoArmor>":
                    inputText = "<data15>";
                    goto default;

                case "<autoCheckArm>":
                    inputText = "<data16>";
                    goto default;

                case "<autoCheckArmor>":
                    inputText = "<data17>";
                    goto default;

                case "<autoLoseFood>":
                    inputText = "<data18>";
                    goto default;
                case "<autoLoseArm>":
                    inputText = "<data19>";
                    goto default;
                case "<autoLoseArmor>":
                    inputText = "<data20>";
                    goto default;
                case "<autoLoseOther>":
                    inputText = "<data21>";
                    goto default;

                case "<itemHp>":
                    if (inputName != "" && !isData)
                    {
                        text += ItemIdGetName(MainControl.Instance.ItemControl, ItemNameGetId(MainControl.Instance.ItemControl, inputName, "Foods"), "Auto", 2);
                        break;
                    }

                    goto default;

                case "<itemAtk>":
                    if (inputName != "" && !isData)
                    {
                        text += ItemIdGetName(MainControl.Instance.ItemControl, ItemNameGetId(MainControl.Instance.ItemControl, inputName, "Arms"), "Auto", 1);
                        break;
                    }

                    goto default;

                case "<itemDef>":
                    if (inputName != "" && !isData)
                    {
                        text += ItemIdGetName(MainControl.Instance.ItemControl, ItemNameGetId(MainControl.Instance.ItemControl, inputName, "Armors"), "Auto", 1);
                        break;
                    }

                    goto default;

                case "<getEnemiesName>":
                    if (inputName != "" && !isData)
                    {
                        text += ex[0];
                        break;
                    }

                    goto default;
                case "<getEnemiesATK>":
                    if (inputName != "" && !isData)
                    {
                        text += ex[1];
                        break;
                    }

                    goto default;
                case "<getEnemiesDEF>":
                    if (inputName != "" && !isData)
                    {
                        text += ex[2];
                        break;
                    }

                    goto default;

                default:
                    if (TextProcessingService.IsSameFrontTexts(inputText, "<data"))
                    {
                        text += MainControl.Instance.ItemControl.itemTextMaxData[int.Parse(inputText.Substring(5, inputText.Length - 6))][..(MainControl.Instance.ItemControl.itemTextMaxData[int.Parse(inputText.Substring(5, inputText.Length - 6))].Length - 1)];
                    }
                    else if (inputText.Length > 9)
                    {
                        switch (inputText[..9])
                        {
                            case "<itemName" when !isData:
                            {
                                if (inputName != "")
                                {
                                    if (ItemNameGetId(MainControl.Instance.ItemControl, inputName, inputText.Substring(9, inputText.Length - 10) + 's') < 10000)
                                        text += ItemIdGetName(MainControl.Instance.ItemControl, ItemNameGetId(MainControl.Instance.ItemControl, inputName, inputText.Substring(9, inputText.Length - 10) + 's'), inputText.Substring(9, inputText.Length - 10) + 's', 0);
                                    else text += ItemIdGetName(MainControl.Instance.ItemControl, ItemNameGetId(MainControl.Instance.ItemControl, inputName, inputText.Substring(9, inputText.Length - 10) + 's'), "Auto", 0);
                                }

                                break;
                            }
                            case "<autoLose":
                                switch (inputText.Substring(9, inputText.Length - 10) + 's')
                                {
                                    case "Food":
                                        text += MainControl.Instance.ItemControl.itemTextMaxData[18];
                                        break;

                                    case "Arm":
                                        text += MainControl.Instance.ItemControl.itemTextMaxData[19];
                                        break;

                                    case "Armor":
                                        text += MainControl.Instance.ItemControl.itemTextMaxData[20];
                                        break;

                                    case "Other":
                                        text += MainControl.Instance.ItemControl.itemTextMaxData[21];
                                        break;
                                }

                                break;
                            default:
                                text += inputText;
                                break;
                        }
                    }
                    else
                    {
                        text += inputText;
                    }
                    break;
            }
            return text;
        }

        /// <summary>
        /// 获取内置语言包ID
        /// </summary>
        public static string GetLanguageInsideId(int id) =>
            id switch
            {
                0 => "CN",
                1 => "TCN",
                _ => "US"
            };

        /// <summary>
        /// 检测当前语言包ID是否在有效范围内。如果不在有效范围内，
        /// 则将语言包ID设置为默认值2。
        /// </summary>
        /// <param name="id">语言包ID</param>
        public static int LanguagePackDetection(int id)
        {
            if (id < 0 || id >= Directory.GetDirectories(Application.dataPath + "\\LanguagePacks").Length + MainControl.LanguagePackInsideNumber)
                return 2;
            return id;
        }

        /// <summary>
        /// 加载对应语言包的数据
        /// </summary>
        public static string LoadLanguageData(string path, int id)
        {
            return id < MainControl.LanguagePackInsideNumber 
                ? Resources.Load<TextAsset>($"TextAssets/LanguagePacks/{DataHandlerService.GetLanguageInsideId(id)}/{path}").text 
                : File.ReadAllText($"{Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[id - MainControl.LanguagePackInsideNumber]}\\{path}.txt");
        }

        /// <summary>
        /// 检测语言包全半角
        /// </summary>
        public static void InitializationLanguagePackFullWidth()
        {
            if (MainControl.Instance.overworldControl.textWidth != bool.Parse(TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.settingSave, "LanguagePackFullWidth")))
            {
                MainControl.Instance.overworldControl.textWidth = bool.Parse(TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.settingSave, "LanguagePackFullWidth"));
                foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(TextChanger)))
                {
                    var textChanger = (TextChanger)obj;
                    textChanger.isUseWidth = MainControl.Instance.overworldControl.textWidth;
                    textChanger.Set();
                    textChanger.Change();
                }
            }

            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture(TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.settingSave, "CultureInfo"));
        }

        /// <summary>
        /// 设置PlayerControl
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
            playerControl.wearAtk = inputPlayerControl.wearAtk;
            playerControl.wearDef = inputPlayerControl.wearDef;
            playerControl.nextExp = inputPlayerControl.nextExp;
            playerControl.missTime = inputPlayerControl.missTime;
            playerControl.missTimeMax = inputPlayerControl.missTimeMax;
            playerControl.atk = inputPlayerControl.atk;
            playerControl.def = inputPlayerControl.def;
            playerControl.playerName = inputPlayerControl.playerName;
            playerControl.myItems = inputPlayerControl.myItems;
            playerControl.wearArm = inputPlayerControl.wearArm;
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