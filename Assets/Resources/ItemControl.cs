using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ������Ʒ��Ϣ����ǰ������Ϣ��������Ʒ��Ϣ
/// </summary>
[CreateAssetMenu(fileName = "ItemControl", menuName = "ItemControl")]
public class ItemControl : ScriptableObject
{
    public TextAsset itemData;//���ݰ�(����) 
    public string itemText;//���԰�
    public List<string> itemMax, itemTextMax;//��List�洢 �����䵽MainControl
    public List<string> itemTextMaxItem, itemTextMaxItemSon, itemTextMaxData;
    public List<string> itemFoods, itemArms, itemArmors, itemOthers;//@ + ����/����1/����2 List������ΪID    3ѭ0+ 2ѭ10000+ 2ѭ20000+ 3ѭ30000+
    
    //-------------------------------------------------------

    public List<int> itemBox1, itemBox2;//�������� ������
}
