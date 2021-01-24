using System;
using UnityEngine;
using UnityEngine.InputSystem;

// makes the main camera follow whatever game object this component is
// attached to
[RequireComponent(typeof(PlayerInput))]
public class ThirdPersonCameraTarget : MonoBehaviour
{
    [Range(1f, 20f)]
    [Tooltip("maximum camera distance from the target")]
    public float distance = 10f;
    [Range(0f, 100f)]
    [Tooltip("interpolation speed of camera's position/rotation")]
    public float speed = 10f;

    [Tooltip("layer mask the camera will collide with")]
    public LayerMask mask;
    
    // input
    private InputAction _input;
    
    // camera's rotation about the y axis
    private Transform _axis;
    // camera's transform
    private Transform _cam;

    // camera's desired position and rotation
    private Vector3 _pos, _rot;

    // short-hand for camera's rotation on y axis
    public Transform Axis => _axis;

    // short-hand to get the current input
    private Vector2 Input => _input.ReadValue<Vector2>();

    private void Start()
    {
        System.Diagnostics.Debug.Assert(Camera.main != null, "no camera!");

        _input = GetComponent<PlayerInput>().actions.FindAction("LookAround");

        _axis = new GameObject("Third Person Camera Axis").transform;
        _axis.parent = this.transform;
        _axis.localPosition = Vector3.zero;
        _axis.localRotation = Quaternion.identity;

        _cam = Camera.main.transform;
        _cam.parent = _axis;
        _cam.localRotation = Quaternion.identity;
        _cam.localPosition = Vector3.back * distance;

        _pos = _cam.localPosition;
        _rot = _axis.eulerAngles;
    }

    private void Update()
    {
        Orient();
        Collide();
    }

    private void Orient()
    {
        // get current input
        var input = Input;

        // calculate rotation
        _rot.x = _rot.x <= 90f 
            ? Mathf.Clamp(_rot.x - input.y, -89f, 89f)
            : Mathf.Clamp(_rot.x - input.y, 271f, 361f);
        _rot.y += input.x;
        
        // apply rotation
        _axis.rotation = Quaternion.Lerp(_axis.rotation, Quaternion.Euler(_rot), Time.deltaTime * 10);
    }

    private void Collide()
    {
        // store old position for interpolation
        var now = _cam.localPosition;
        
        // reset position
        _cam.localPosition = Vector3.back * distance;
        
        // check for collision
        if (Physics.Linecast(_axis.position, _cam.position, out var hit, mask))
        {
            _pos = Vector3.back * (distance - (_cam.position - hit.point).magnitude);
        }
        
        // apply position
        _cam.localPosition = Vector3.Slerp(now, _pos, Time.deltaTime * speed);
    }
}
