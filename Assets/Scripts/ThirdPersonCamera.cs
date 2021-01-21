using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Range(1f, 10f)]
    public float distance;
    
    private Transform _transform;
    private Transform _parent;

    void Start()
    { 
        _transform = GetComponent<Transform>();

        // camera axis
        _parent = new GameObject("Third Person Camera Axis").GetComponent<Transform>();
        _parent.parent = _transform.parent;
        _transform.parent = _parent;
        _parent.localPosition = Vector3.zero;
        _parent.localRotation = Quaternion.identity;

        // camera on axis
        _transform.rotation = Quaternion.identity;
        _transform.localPosition = Vector3.back * distance;
    }

    void Update()
    {
        Debug.Log(_parent.eulerAngles.x);

        var x = _parent.eulerAngles.x;
        var lessThan90 = x <= 90f;

        x += Input.GetAxis("Vertical");

        if (lessThan90)
        {
            x = Mathf.Clamp(x, -89f, 89f);
        }
        else
        {
            x = Mathf.Clamp(x, 271f, 361f);
        }
        
        _parent.rotation = Quaternion.Euler
            (
            x,
            _parent.eulerAngles.y - Input.GetAxis("Horizontal"),
            0
            );
    }
}
