using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesHpLineController : MonoBehaviour
{
    SpriteRenderer greenSprite;
    [Header("0¿ª")]
    public int num;
    void Start()
    {
        transform.localScale = Vector2.zero;
        greenSprite = GetComponent<SpriteRenderer>();
        greenSprite.material = Instantiate(greenSprite.material);
    }

    // Update is called once per frame
    void Update()
    {
        if (MainControl.instance.selectUIController.selectUI == 1 && MainControl.instance.selectUIController.selectLayer == 1)
        {
            if (MainControl.instance.BattleControl.enemiesHp.Count - 1 < num * 2)
                transform.localScale = Vector2.zero;
            else
            {
                transform.localScale = new Vector3(42, 7.25f, 1);

                greenSprite.material.SetFloat("_Crop", (float)MainControl.instance.BattleControl.enemiesHp[num * 2] / MainControl.instance.BattleControl.enemiesHp[num * 2 + 1]);
                greenSprite.material.SetFloat("_Flash", (float)MainControl.instance.BattleControl.enemiesHp[num * 2] / MainControl.instance.BattleControl.enemiesHp[num * 2 + 1]);

                Debug.Log((float)MainControl.instance.BattleControl.enemiesHp[num * 2] / MainControl.instance.BattleControl.enemiesHp[num * 2 + 1]);
            }
        }
    }
}
