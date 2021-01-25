using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(ThirdPersonCameraTarget), typeof(Movement))]
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
    private ThirdPersonCameraTarget _cam;
    private ParagliderTarget _glider;
    private CharacterAnimator _anim;
    private Movement _move;

    // move direction input
    private Vector2 _dir;
    
    private void Start()
    {
        _anim = GetComponentInChildren<CharacterAnimator>();
        _cam = GetComponent<ThirdPersonCameraTarget>();
        _glider = GetComponent<ParagliderTarget>();
        _move = GetComponent<Movement>();
    }

    private void Update()
    {
        // has movement?
        if (_dir.sqrMagnitude > 0)
        {
            // determine direction in plane space
            var dir = _cam.TransformInput(_dir);
            
            // determine speed
            var vel = _glider.Gliding ? glideSpeed : _anim.RootVelocity * runSpeed;
            
            // set visual forward direction
            _anim.forward = new Vector3(dir.x, 0, dir.y);
            
            // move character
            _move.Direction = vel * dir;
            
            // animation
            _anim.Running = _move.Grounded;
        }
        else
        {
            // update direction
            _move.Direction = Vector2.zero;
            
            // no movement -> idle animation
            _anim.Running = false;
        }
    }

    // called by the input system
    private void OnMove(InputValue input)
    {
        _dir = input.Get<Vector2>();
    }
    
    // called by input system
    private void OnJump()
    {
        _glider.Gliding = !_move.Jump(jumpHeight);
    }

    // called by input system
    private void OnSprint()
    {
        // sprint binding also cancels paraglider
        _glider.Gliding = false;
    }
}
