using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesHpLineController : MonoBehaviour
{
    SpriteRenderer greenSprite;
    [Header("0¿ª")]
    public int num;
    SelectUIController selectController;
    void Start()
    {
        transform.localScale = Vector2.zero;
        selectController = transform.parent.parent.GetComponent<SelectUIController>();
        greenSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectController.selectUI == 1 && selectController.selectLayer == 1)
        {
            if (MainControl.instance.BattleControl.enemiesHp.Count - 1 < num * 2)
                transform.localScale = Vector2.zero;
            else
            {
                transform.localScale = new Vector3(42, 7.25f, 1);
                greenSprite.transform.localScale = new Vector3((float)MainControl.instance.BattleControl.enemiesHp[num * 2] / MainControl.instance.BattleControl.enemiesHp[num * 2 + 1], greenSprite.transform.localScale.y);
            }
        }
    }
}
