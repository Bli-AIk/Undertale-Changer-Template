using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ¿ØÖÆ½²¹ÊÊÂ³¡¾°£¨²¥PPT£©
/// </summary>
public class StorySceneController : MonoBehaviour
{
    public string text;
    TypeWritter typeWritter;
    // Start is called before the first frame update
    void Start()
    {
        typeWritter = GetComponent<TypeWritter>();
        //typeWritter.TypeOpen(MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, text), false, 0, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
