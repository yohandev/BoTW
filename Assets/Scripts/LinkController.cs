using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Movement), typeof(PlayerInput))]
public class LinkController : MonoBehaviour
{
    [Serializable]
    public struct Speeds
    {
        [Tooltip("link's running speed on flat ground")]
        [Range(0.1f, 20f)]
        public float running;

        [Tooltip("link's gliding speed in the xz plane")]
        [Range(0.1f, 20f)]
        public float gliding;
    }

    [Tooltip("link's speeds at various states")]
    public Speeds speeds = new Speeds { running = 4.5f, gliding = 9.0f };

    [Serializable]
    public struct Jump
    {
        [Tooltip("maximum height link will reach when jumping on flat ground")]
        [Range(0f, 5f)]
        public float height;

        [Tooltip("time from ground to highest point in link's jump")]
        [Range(0.001f, 1f)]
        public float time;
    }

    [Tooltip("link's jump characteristics")]
    public Jump jump = new Jump { height = 1.65f, time = 0.1f };
    
    private ThirdPersonCameraTarget _cam;
    private ParagliderTarget _glider;
    private CharacterAnimator _anim;
    private Movement _move;

    private Vector2 _dir;
    
    private void Start()
    {
        _cam = GetComponent<ThirdPersonCameraTarget>();
        _glider = GetComponent<ParagliderTarget>();
        _anim = GetComponentInChildren<CharacterAnimator>();
        _move = GetComponent<Movement>();
    }

    private void Update()
    {
        // speed and direction
        var dir = _cam.TransformInput(_dir);
        var vel = _glider.Gliding ? speeds.gliding : speeds.running; // _anim.RootVelocity * 
        
        // move character
        _move.Input = vel * new Vector2(dir.x, dir.z);

        // visuals
        _anim.forward = dir;
        _anim.Running = _dir.sqrMagnitude > 0 && _move.Grounded;
    }

    // called by the input system
    private void OnMove(InputValue input)
    {
        _dir = input.Get<Vector2>();
    }
    
    // called by input system
    private void OnJump()
    {
        _glider.Gliding = !_move.Jump(jump.height);
    }

    // called by input system
    private void OnSprint()
    {
        // sprint binding also cancels paraglider
        _glider.Gliding = false;
    }
}
