using System.Collections.Generic;
using UnityEngine;

namespace UCT.Control
{
    /// <summary>
    /// 存储所有物品信息。
    /// </summary>
    [CreateAssetMenu(fileName = "ItemControl", menuName = "UCT-Disposable/ItemControl")]
    public class ItemControl : ScriptableObject
    {
        public TextAsset itemData;//数据包(内置数据调入)
        public string itemText;//语言包
        public List<string> itemMax, itemTextMax;//总List存储 将分配到MainControl
        public List<string> itemTextMaxItem, itemTextMaxItemSon, itemTextMaxData;
        public List<string> itemFoods, itemArms, itemArmors, itemOthers;//@ + 名称/属性1/属性2 List中排序为ID    3循0+ 2循10000+ 2循20000+ 3循30000+

        //-------------------------------------------------------

        public List<int> itemBox1, itemBox2;//箱子数据 储存编号
    }
}