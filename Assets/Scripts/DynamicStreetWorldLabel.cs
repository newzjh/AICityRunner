using UnityEngine;

public class DynamicStreetWorldLabel : MonoBehaviour
{
    public Transform Anchor;
    public Vector3 WorldOffset = new Vector3(0f, 1.35f, 0f);
    public float UniformScale = 0.02f;
    public bool BillboardToCamera = true;

    private Camera _targetCamera;

    private void LateUpdate()
    {
        if (Anchor == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Anchor.position + WorldOffset;
        transform.localScale = Vector3.one * UniformScale;

        if (_targetCamera == null)
        {
            _targetCamera = Camera.main;
        }

        if (BillboardToCamera && _targetCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(_targetCamera.transform.forward, _targetCamera.transform.up);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
    }
}
