using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRandomSudoku : MonoBehaviour
{

    //?Ź???????
    public List<DebugSudoku> sudokos;
    public int randomNum;
    // Start is called before the first frame update
    void Start()
    {
        Randomer();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Randomer();
    }
    void Randomer()
    {
        if (randomNum > 0)
            for (int i = 0; i < randomNum; i++)
            {
                int j = Random.Range(0, sudokos.Count);
                sudokos[j].change = true;
                sudokos[j].Changed();
                ///Debug.Log(j + 1);
            }
    }
}
