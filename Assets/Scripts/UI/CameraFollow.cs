using UnityEngine;

/// <summary>カメラ追従</summary>
public class CameraFollow : MonoBehaviour
{
    Transform target;

    void LateUpdate()
    {
        if (target == null)
        {
            var p = Object.FindObjectOfType<Player>();
            if (p == null) return;
            target = p.transform;
        }
        var pos = target.position;
        pos.z = -10f;
        transform.position = pos;
    }
}
