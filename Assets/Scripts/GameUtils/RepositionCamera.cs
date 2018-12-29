using UnityEngine;

namespace GameUtils
{
    [RequireComponent(typeof(Camera))]
    public class RepositionCamera : MonoBehaviour
    {
        private void Awake()
        {
            var worldPoint = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(0f, 0f, 5f));
            var newPosition = transform.position;
            newPosition.x += Mathf.Abs(worldPoint.x);
            transform.position = newPosition;
        }
    }

}
