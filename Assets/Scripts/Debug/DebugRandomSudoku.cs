using System.Collections.Generic;
using UnityEngine;

namespace Debug
{
    public class DebugRandomSudoku : MonoBehaviour
    {
        //æ≈π¨∏Ò≤‚ ‘
        public List<DebugSudoku> sudokos;

        public int randomNumber;

        private void Start()
        {
            Randomer();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Randomer();
            }
        }

        private void Randomer()
        {
            if (randomNumber > 0)
            {
                for (var i = 0; i < randomNumber; i++)
                {
                    var j = Random.Range(0, sudokos.Count);
                    sudokos[j].change = true;
                    sudokos[j].Changed();
                    ///Debug.Log(j + 1);
                }
            }
        }
    }
}