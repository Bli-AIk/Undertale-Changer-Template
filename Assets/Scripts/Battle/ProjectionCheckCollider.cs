using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ͶӰԭ���ĵ��ж���
/// �����ֱ�Ӱѵ�Ļ����ԭ���ĵ�λ�ã���һǧ�������ܻᵼ����ʾ���⡣
/// </summary>
public class ProjectionCheckCollider : ObjectPool
{
    GameObject canvasBoxProjectionSet;
    List<GameObject> sets = new List<GameObject>();
    List<GameObject> checkColliders = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        canvasBoxProjectionSet = GameObject.Find("CanvasBoxProjectionSet");

        obj = (GameObject)Resources.Load("Prefabs/CheckCollider");
        for (int i = 0; i < canvasBoxProjectionSet.transform.childCount; i++)
        {
            sets.Add(canvasBoxProjectionSet.transform.GetChild(i).gameObject);
            checkColliders.Add(GetFromPool());
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 relative = MainControl.instance.battlePlayerController.transform.position - MainControl.instance.battlePlayerController.sceneDrift;
        for (int i = 0; i < sets.Count; i++)
        {
            Vector3 convert = sets[i].transform.position + sets[i].transform.rotation * relative;
            checkColliders[i].transform.position = (convert - canvasBoxProjectionSet.transform.position);
            checkColliders[i].transform.rotation = sets[i].transform.rotation;
        }
    }
}
