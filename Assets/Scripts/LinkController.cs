using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent
(
    typeof(CharacterController),
    typeof(Gravity),
    typeof(PlayerInput)
)]
[RequireComponent
(
    typeof(ThirdPersonCameraTarget)
)]
public class LinkController : MonoBehaviour
{
    [Tooltip("link's running speed")]
    [Range(2f, 20f)]
    public float runSpeed = 4.5f;

    [Tooltip("link's gliding speed")]
    [Range(2f, 20f)]
    public float glideSpeed = 9f;
    
    [Tooltip("maximum height link will reach when jumping in ideal situations")]
    [Range(0f, 5f)]
    public float jumpHeight = 1.65f;
    
    [Tooltip("time it takes link to reach the highest point in his jump, from the ground")]
    [Range(0.001f, 10f)]
    public float jumpTime = 0.1f;
    
    // this behaviour links all other behaviours
    private CharacterController _controller;
    private ThirdPersonCameraTarget _cam;
    private ParagliderTarget _glider;
    private CharacterAnimator _anim;
    private Gravity _gravity;

    // move direction input
    private Vector2 _dir;
    
    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _anim = GetComponentInChildren<CharacterAnimator>();
        _cam = GetComponent<ThirdPersonCameraTarget>();
        _glider = GetComponent<ParagliderTarget>();
        _gravity = GetComponent<Gravity>();
    }

    private void Update()
    {
        // has movement?
        if (_dir.sqrMagnitude > 0)
        {
            // determine direction in world space
            var dir = _cam.TransformInput(_dir);
            
            // determine speed
            var vel = _glider.Gliding ? glideSpeed : _anim.RootVelocity * runSpeed;
            
            // set visual forward direction
            _anim.forward = dir;
            
            // move character
            _controller.Move(Time.deltaTime * vel * dir);
        }
        
        // animation
        _anim.Running = _dir.sqrMagnitude > 0 && _gravity.Grounded(out _);
    }

    // called by the input system
    private void OnMove(InputValue input)
    {
        _dir = input.Get<Vector2>();
    }
    
    // called by input system
    private void OnJump()
    {
        _glider.Gliding = !_gravity.Jump(jumpHeight, jumpTime);
    }

    // called by input system
    private void OnSprint()
    {
        // sprint binding also cancels paraglider
        _glider.Gliding = false;
    }
}
