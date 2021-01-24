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
    [Range(2f, 20f)]
    public float runSpeed;
    
    [Range(0f, 5f)]
    public float jumpHeight = 1.65f;
    [Range(0.001f, 10f)]
    public float jumpTime = 0.1f;

    [Range(-10f, -0.001f)]
    public float glideVelocity = -4f;
    
    // character controller component
    private CharacterController _controller;
    
    // input component
    private PlayerInput _input;

    // visual character's inverse kinematics
    private CharacterInverseKinematics _ik;

    // camera component
    private ThirdPersonCameraTarget _cam;
    
    // gravity component
    private Gravity _gravity;

    /// <summary>
    /// visual paraglider
    /// </summary>
    private GameObject _paraglider;

    /// <summary>
    /// visual character animator
    /// </summary>
    private Animator _anim;

    /// <summary>
    /// currently gliding?
    /// </summary>
    private bool Gliding
    {
        get => _paraglider.activeSelf;
        set => _paraglider.SetActive(value);
    }

    /// <summary>
    /// current y velocity
    /// </summary>
    private float _yVel;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _cam = GetComponent<ThirdPersonCameraTarget>();
        _gravity = GetComponent<Gravity>();
        _input = GetComponent<PlayerInput>();

        var character = transform.Find("Character");
        
        _paraglider = character.Find("Paraglider").gameObject;
        _anim = character.GetComponent<Animator>();
        _ik = character.GetComponent<CharacterInverseKinematics>();
        
        _input.actions.FindAction("Jump").performed += _ => Jump();
        Gliding = false;
    }

    private void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        // threshold of look rotation dot down at which point controls switch from
        // angle based to simple top down movement
        const float topDownThreshold = 0.9f;

        // xz
        {
            var input = _input.actions.FindAction("Move").ReadValue<Vector2>();
            var down = Vector3.Dot(_cam.Axis.forward, Vector3.down);

            var dir = new Vector3(input.x, 0, input.y);
            if (down < topDownThreshold)
            {
                dir = _cam.Axis.TransformDirection(dir);
            }
            else
            {
                dir = Quaternion.Euler(0, _cam.Axis.eulerAngles.y, 0) * dir;
            }

            dir.y = 0;
            dir.Normalize();

            if (dir.sqrMagnitude > 0)
            {
                _anim.SetBool("Running", !Gliding);
                
                _ik.Forward = dir; //Vector3.Slerp(_character.forward, dir, Time.deltaTime * 10f);
                _controller.Move(_anim.GetFloat("Velocity") * runSpeed * Time.deltaTime * dir);
            }
            else
            {
                _anim.SetBool("Running", false);
            }
        }
    }

    private void Jump()
    {
//        if (Grounded(0.25f))
//        {
//            Gliding = false;
//            _yVel = (jumpHeight - 0.5f * gravity * jumpTime * jumpTime) / jumpTime;
//        }
//        else if (_yVel <= glideVelocity / 2f)
//        {
//            Gliding = !Gliding;
//            if (Gliding)
//            {
//                _yVel = -1.25f * glideVelocity;
//            }
//        }
        if (_gravity.Grounded(out _))
        {
            Gliding = false;
            
            _gravity.Velocity = (jumpHeight - 0.5f * _gravity.force * jumpTime * jumpTime) / jumpTime;
        }
    }

    private float ClampRot(float now, float add)
    {
        return now <= 90f
            ? Mathf.Clamp(now + add, -89f, 89f)
            : Mathf.Clamp(now + add, 271f, 361f);
    }
}
