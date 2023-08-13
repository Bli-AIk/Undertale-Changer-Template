using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesHpLineController : MonoBehaviour
{
    SpriteRenderer greenSprite;
    [Header("Starting from 0")]
    public int num;
    void Start()
    {
        transform.localScale = Vector2.zero;
        greenSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    
    void Update()
    {
        if (MainControl.instance.selectUIController.selectUI == 1 && MainControl.instance.selectUIController.selectLayer == 1)
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
