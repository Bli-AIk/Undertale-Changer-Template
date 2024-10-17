using UnityEngine;

namespace UCT.Global.Other
{
    public class FollowSth : MonoBehaviour
    {
        public GameObject sth;
        public Vector3 posAdd;

        private void Update()
        {
            if(sth != null)
            {
                transform.position = sth.transform.position + posAdd;
            }
        }
    }
}
