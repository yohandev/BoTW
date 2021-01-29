using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// player controller for Link. Acts as input agent for Link's rigidbody
/// controller, only inputs are from the player and not AI
/// </summary>
[RequireComponent(typeof(CharacterBody), typeof(CameraController), typeof(CharacterGlider))]
[RequireComponent(typeof(PlayerInput), typeof(CharacterAnimator))]
public class CharacterBrain : MonoBehaviour
{
    private CharacterBody m_rbody;          // rigidbody controller component
    private CharacterAnimator m_anim;       // character animator component
    private CameraController m_cam;         // camera controller component
    private CharacterGlider m_glider;       // glider component
    
    private Vector2 m_moveAxis;             // desired movement since last update
    private Vector2 m_lookAxis;             // camera movement since last update

    private void Start()
    {
        m_rbody = gameObject.GetComponent<CharacterBody>();
        m_anim = gameObject.GetComponent<CharacterAnimator>();
        m_cam = gameObject.GetComponent<CameraController>();
        m_glider = gameObject.GetComponent<CharacterGlider>();
        
        m_moveAxis = Vector2.zero;
        m_lookAxis = Vector2.zero;
    }

    private void Update()
    {
        var dir = m_cam.TransformDirection(m_moveAxis);
        
        m_rbody.Move(dir);
        m_anim.Move(dir);
        m_cam.Move(m_lookAxis);
    }

    // called by the input system
    private void OnMove(InputValue input)
    {
        m_moveAxis = input.Get<Vector2>();
    }

    // called by the input system
    private void OnLook(InputValue input)
    {
        m_lookAxis = input.Get<Vector2>();
    }

    // called by the input system
    private void OnJump()
    {
        if (m_rbody.OnGround)
        {
            m_rbody.Jump();
        }
        else
        {
            m_glider.active = true;
        }
    }

    // called by the input system
    private void OnSprint()
    {
        m_glider.active = false;
    }
}
