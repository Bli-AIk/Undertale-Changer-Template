using System;
using TMPro;
using UCT.Core;
using UCT.Service;
using UnityEngine;

namespace UCT.Overworld
{
    public class OverworldChaseUIController : MonoBehaviour
    {
        private static readonly int Crop = Shader.PropertyToID("_Crop");
        private static readonly int Flash = Shader.PropertyToID("_Flash");
        [HideInInspector] public TextMeshPro hpUI;
        [HideInInspector] public SpriteRenderer hp;
        [HideInInspector] public SpriteRenderer hpSpr;
        [HideInInspector] public SpriteRenderer gradientUp;
        [HideInInspector] public SpriteRenderer gradientDown;
        

        private void Start()
        {
            hpUI = transform.Find("HP UI").GetComponent<TextMeshPro>();
            hp = transform.Find("HP").GetComponent<SpriteRenderer>();
            hpSpr = transform.Find("HP Spr").GetComponent<SpriteRenderer>();
            gradientUp = transform.Find("GradientUp").GetComponent<SpriteRenderer>();
            gradientDown = transform.Find("GradientDown").GetComponent<SpriteRenderer>();

            gameObject.SetActive(false);
        }
        
        private void Update()
        {

            if (MainControl.Instance.playerControl.missTime >= 0)
            {
                MainControl.Instance.playerControl.missTime -= Time.deltaTime;
            }

            if (!gameObject.activeSelf)
            {
                return;
            }

            var x = -1.55f + (MainControl.Instance.playerControl.hpMax - 92f) * 0.85f / -72f;

            transform.localPosition = new Vector3(x, transform.localPosition.y, 10);

            hp.transform.localScale = new Vector3(0.525f * MainControl.Instance.playerControl.hpMax, 8.5f);

            hpUI.text = GameUtilityService.FormatWithLeadingZero(MainControl.Instance.playerControl.hp) + " / " +
                        GameUtilityService.FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
            hpUI.transform.localPosition =
                new Vector3(1.25f + 9.85f + 0.0265f * (MainControl.Instance.playerControl.hpMax - 20),
                    hpUI.transform.localPosition.y);

            hp.material.SetFloat(Crop,
                (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
            hp.material.SetFloat(Flash,
                (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
        }
    }
}