using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class ThirdPersonController : MonoBehaviour
{
    [Range(1f, 10f)]
    public float cameraDistance;
    
    /// <summary>
    /// camera rotation axis
    /// </summary>
    private Transform _axis;

    /// <summary>
    /// Camera transform
    /// </summary>
    private Transform _cam;

    /// <summary>
    /// desired camera rotation
    /// </summary>
    private Vector3 _rot;

    /// <summary>
    /// character controller
    /// </summary>
    private CharacterController _controller;

    /// <summary>
    /// player input
    /// </summary>
    private PlayerInput _input;
    
    private void Start()
    {
        _axis = new GameObject("Third Person Camera Axis").transform;
        _axis.parent = transform;
        _axis.localPosition = Vector3.zero;
        _axis.localRotation = Quaternion.identity;

        _cam = Camera.main.transform;
        _cam.parent = _axis;
        _cam.localRotation = Quaternion.identity;
        _cam.localPosition = Vector3.back * cameraDistance;

        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        UpdateCamera();
        UpdateMovement();
    }

    private void UpdateCamera()
    {
        var input = _input.actions.FindAction("LookAround").ReadValue<Vector2>();

        _rot.x = ClampRot(_rot.x, input.y);
        _rot.y += input.x;
        
        _axis.rotation = Quaternion.Lerp(_axis.rotation, Quaternion.Euler(_rot), Time.deltaTime * 10);
    }

    private void UpdateMovement()
    {
        var input = _input.actions.FindAction("Move").ReadValue<Vector2>();
        var dir = _axis.TransformDirection(new Vector3(input.x, 0, input.y));

        _controller.SimpleMove(dir * 10f);
    }

    private float ClampRot(float now, float add)
    {
        return now <= 90f
            ? Mathf.Clamp(now + add, -89f, 89f)
            : Mathf.Clamp(now + add, 271f, 361f);
    }
}
