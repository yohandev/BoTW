using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent
(
    typeof(CharacterController),
    typeof(Gravity),
    typeof(PlayerInput)
)]
public class ThirdPersonController : MonoBehaviour
{
    [Range(1f, 10f)]
    public float maxCameraDistance = 5f;

    [Range(0f, 5f)]
    public float jumpHeight = 2f;
    [Range(0.001f, 10f)]
    public float jumpTime = 0.1f;
    
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
    private Vector3 _camRot;
    /// <summary>
    /// desired camera position
    /// </summary>
    private Vector3 _camPos;

    /// <summary>
    /// character controller
    /// </summary>
    private CharacterController _controller;
    /// <summary>
    /// gravity controller
    /// </summary>
    private Gravity _gravity;

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
        _cam.localPosition = Vector3.back * maxCameraDistance;

        _controller = GetComponent<CharacterController>();
        _gravity = GetComponent<Gravity>();
        _input = GetComponent<PlayerInput>();

        _camPos = _cam.localPosition;

        _input.actions.FindAction("Jump").performed += _ => Jump();
    }

    private void Update()
    {
        UpdateCamera();
        UpdateMovement();
    }

    private void UpdateCamera()
    {
        // rotate
        var input = _input.actions.FindAction("LookAround").ReadValue<Vector2>();

        _camRot.x = ClampRot(_camRot.x, -input.y);
        _camRot.y += input.x;
        
        _axis.rotation = Quaternion.Lerp(_axis.rotation, Quaternion.Euler(_camRot), Time.deltaTime * 10);

        // collide
        var now = _cam.localPosition;
        var mask = ~LayerMask.GetMask("Link");
        
        _cam.localPosition = Vector3.back * maxCameraDistance;
        if (Physics.Linecast(_axis.position, _cam.position, out var hit, mask))
        {
            var dist = maxCameraDistance - (_cam.position - hit.point).magnitude;
            
            _camPos = Vector3.back * dist;
        }
        _cam.localPosition = Vector3.Slerp(now, _camPos, Time.deltaTime * 10);
    }

    private void UpdateMovement()
    {
        // threshold of look rotation dot down at which point controls switch from
        // angle based to simple top down movement
        const float topDownThreshold = 0.9f;
        
        var input = _input.actions.FindAction("Move").ReadValue<Vector2>();
        var down = Vector3.Dot(_axis.forward, Vector3.down);

        var dir = new Vector3(input.x, 0, input.y);
        if (down < topDownThreshold)
        {
            dir = _axis.TransformDirection(dir);
        }
        else
        {
            dir = Quaternion.Euler(0, _axis.eulerAngles.y, 0) * dir;
        }

        dir.y = 0;
        dir.Normalize();

        _controller.Move(10f * Time.deltaTime * dir);
    }

    private void Jump()
    {
        _gravity.Jump(jumpHeight, jumpTime);
    }

    private float ClampRot(float now, float add)
    {
        return now <= 90f
            ? Mathf.Clamp(now + add, -89f, 89f)
            : Mathf.Clamp(now + add, 271f, 361f);
    }
}
