using System.Collections.Generic;
using UnityEngine;

public class DebugRandomSudoku : MonoBehaviour
{
    //æ≈π¨∏Ò≤‚ ‘
    public List<DebugSudoku> sudokos;

    public int randomnumber;

    private void Start()
    {
        Randomer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Randomer();
    }

    private void Randomer()
    {
        if (randomnumber > 0)
            for (int i = 0; i < randomnumber; i++)
            {
                int j = Random.Range(0, sudokos.Count);
                sudokos[j].change = true;
                sudokos[j].Changed();
                ///Debug.Log(j + 1);
            }
    }
}