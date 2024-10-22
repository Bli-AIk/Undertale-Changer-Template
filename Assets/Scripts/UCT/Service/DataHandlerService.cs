using System.Collections.Generic;
using System.Linq;
using UCT.Control;

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
    }
}