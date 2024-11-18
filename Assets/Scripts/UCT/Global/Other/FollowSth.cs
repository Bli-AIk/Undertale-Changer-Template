using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Global.Other
{
    public class FollowSth : MonoBehaviour
    {
        public string sthName;
        public GameObject sth;
        public bool followPosition;
        public Vector3 positionAdd;
        public bool followRotation;
        public Vector3 rotationAdd;
        public bool followLocalScale;
        public Vector3 localScaleAdd;

        private void Start()
        {
            if (!sth && !string.IsNullOrEmpty(sthName))
                sth = GameObject.Find(sthName);
        }

        private void Update()
        {
            if (!sth) return;
            if (followPosition) transform.position = sth.transform.position + positionAdd;
            if (followRotation) transform.rotation = sth.transform.rotation * Quaternion.Euler(rotationAdd);
            if (followLocalScale) transform.localScale = sth.transform.localScale + localScaleAdd;

        }
    }
}
