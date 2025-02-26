using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Overworld
{
    /// <summary>
    ///     Overworld摄像机跟随
    /// </summary>
    public class CameraFollowPlayer : MonoBehaviour
    {
        public static CameraFollowPlayer Instance { get; private set; }

        [FormerlySerializedAs("limit")] public bool isLimit = true;
        public bool isFollow;


        public float minX;
        public float minY;
        public float maxX;
        public float maxY;

        public GameObject player;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (!isFollow)
            {
                return;
            }

            if (!player)
            {
                player = GameObject.Find("Player");
            }

            if (isLimit)
            {
                if (player.transform.position.x >= minX || player.transform.position.x <= maxX)
                {
                    transform.position = new Vector3(player.transform.position.x, transform.position.y,
                        transform.position.z);
                }

                if (player.transform.position.y >= minY || player.transform.position.y <= maxY)
                {
                    transform.position = new Vector3(transform.position.x, player.transform.position.y,
                        transform.position.z);
                }

                transform.position = GetLimitedPosition(transform.position);
            }
            else
            {
                transform.position = new Vector3(player.transform.position.x, player.transform.position.y,
                    transform.position.z);
            }
        }

        public Vector3 GetLimitedPosition(Vector3 pos)
        {
            if (pos.x <= minX)
            {
                pos = new Vector3(minX, pos.y, pos.z);
            }
            else if (pos.x >= maxX)
            {
                pos = new Vector3(maxX, pos.y, pos.z);
            }

            if (pos.y <= minY)
            {
                pos = new Vector3(pos.x, minY, pos.z);
            }
            else if (pos.y >= maxY)
            {
                pos = new Vector3(pos.x, maxY, pos.z);
            }

            return pos;
        }
    }
}