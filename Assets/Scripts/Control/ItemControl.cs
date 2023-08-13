using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 存储所有物品信息。
/// </summary>
[CreateAssetMenu(fileName = "ItemControl", menuName = "ItemControl")]
public class ItemControl : ScriptableObject
{
    public TextAsset itemData;//Packet (built-in data call in)
    public string itemText;//Language Pack
    public List<string> itemMax, itemTextMax;//Total List storage, to be allocated to MainControl
    public List<string> itemTextMaxItem, itemTextMaxItemSon, itemTextMaxData;
    public List<string> itemFoods, itemArms, itemArmors, itemOthers;//@+Name/Attribute 1/Attribute 2. Sort as ID in the List. 3 cycles 0+, 2 cycles 10000+, 2 cycles 20000+, 3 cycles 30000+

    //-------------------------------------------------------

    public List<int> itemBox1, itemBox2;//Box data storage number
}
