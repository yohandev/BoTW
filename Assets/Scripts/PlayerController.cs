using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// player controller for Link. Acts as input agent for Link's rigidbody
/// controller, only inputs are from the player and not AI
/// </summary>
[RequireComponent(typeof(RigidbodyController), typeof(CameraController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private RigidbodyController m_rbody;    // rigidbody controller component
    private CameraController m_cam;         // camera controller component
    
    private Vector2 m_moveAxis;             // desired movement since last update
    private Vector2 m_lookAxis;             // camera movement since last update

    private void Start()
    {
        m_rbody = gameObject.GetComponent<RigidbodyController>();
        m_cam = gameObject.GetComponent<CameraController>();
        
        m_moveAxis = Vector2.zero;
        m_lookAxis = Vector2.zero;
    }

    private void Update()
    {
        m_rbody.Move(m_moveAxis);
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
        m_rbody.Jump();
    }
}
