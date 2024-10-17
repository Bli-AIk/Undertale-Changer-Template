using TMPro;
using UCT.Global.UI;
using UnityEngine;

namespace Debug
{
    public class DebugDraft : MonoBehaviour
    {
        private bool wozhenfule;

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (!wozhenfule)
            {
                GetComponent<TypeWritter>().TypeOpen("<color=red>text123</color>", false, 0, 0, GetComponent<TMP_Text>());
                wozhenfule = true;
            }
        }
    }
}