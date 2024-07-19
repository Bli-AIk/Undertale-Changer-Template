using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all item information.
/// </summary>
[CreateAssetMenu(fileName = "ItemControl", menuName = "ItemControl")]
public class ItemControl : ScriptableObject
{
    public TextAsset itemData;
    // Data packet (built-in data transfer)
    public string itemText;
    //Language packs
    public List<string> itemMax, itemTextMax;
    //Total List storage to be assigned to MainControl
    public List<string> itemTextMaxItem, itemTextMaxItemSon, itemTextMaxData;
    public List<string> itemFoods, itemArms, itemArmors, itemOthers;
    //@ + Name/Attribute1/Attribute2 Sorted in List as ID 3 by 0+ 2 by 10000+ 2 by 20000+ 3 by 30000+

    //-------------------------------------------------------

    public List<int> itemBox1, itemBox2;
    // Box data Storage number
}
