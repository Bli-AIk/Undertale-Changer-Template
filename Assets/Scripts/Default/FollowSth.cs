using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSth : MonoBehaviour
{
    public GameObject sth;
    public Vector3 posAdd;
    void Update()
    {
        if(sth != null)
        {
            transform.position = sth.transform.position + posAdd;
        }
    }
}
