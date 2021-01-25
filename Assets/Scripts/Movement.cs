using System;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Movement : MonoBehaviour
{
    [Serializable]
    public struct ColliderInfo
    {
        [Tooltip("radius on the xz plane, from transform center, of the capsule collider")]
        [Range(0f, 10f)]
        public float radius;
        
        [Tooltip("distance from transform center to the capsule collider's highest point")]
        [Space(5)]
        [Range(0f, 10f)]
        public float head;
        [Tooltip("distance from transform center to the capsule collider's lowest point")]
        [Range(0f, 10f)]
        public float feet;

        [Tooltip("layer mask to collide with")]
        [Space(5)]
        public LayerMask mask;
    }
    
    [Tooltip("capsule collider")]
    public new ColliderInfo collider = new ColliderInfo { radius = 0.5f, feet = 1f, head = 1f };
    
    [Tooltip("downward acceleration of gravity")]
    [Range(0f, 100f)]
    public float gravity;
        
    [Tooltip("maximum slope angle that can be climbed")]
    [Range(0f, 89f)]
    public float slopeLimit;

    [Tooltip("input direction on the xz plane in world space. magnitude is used as speed")]
    [NonSerialized]
    public Vector2 Input;

    [Tooltip("current y velocity")]
    [NonSerialized]
    public float YVel;

    [Tooltip("current drag on the y axis")]
    [NonSerialized]
    public float YDrag;
    
    private CharacterController _controller;
    private Transform _transform;

    private void Start()
    {
        _controller = gameObject.AddComponent<CharacterController>();
        _transform = gameObject.GetComponent<Transform>();
    }

    private void Update()
    {
        UpdateInner();

        // gravity
        YVel += (YDrag - gravity) * Time.deltaTime;

        // convert input space to world space
        var dir = new Vector3(Input.x, 0, Input.y);
        
        // if on ground, move along ground(y may not be 0)
        if (_controller.isGrounded && RaycastDown(out var hit))
        {
            // transform direction on ground
            dir = Vector3.ProjectOnPlane(dir, hit.normal).normalized * dir.magnitude;
            
            // reset velocity
            YVel = Mathf.Max(YVel, -gravity * Time.deltaTime);
        }
        
        // construct final velocity for this frame
        var vel = dir + YVel * Vector3.up;
        
        // move
        _controller.Move(vel * Time.deltaTime);
    }

    // update inner CharacterController with this' fields
    private void UpdateInner()
    {
        _controller.radius = collider.radius;
        _controller.height = collider.feet + collider.head;
        _controller.center = (collider.head - collider.feet) * 0.5f * Vector3.up;
        
        _controller.slopeLimit = slopeLimit;
    }

    // raycast directly down from the current position
    private bool RaycastDown(out RaycastHit hit)
    {
        // TODO maybe change to spherecast?
        return Physics.Raycast(_transform.position, Vector3.down, out hit);
    }

    // jump upwards to given height if grounded
    public bool Jump(float height)
    {
        if (_controller.isGrounded)
        {
            YVel = Mathf.Sqrt(2 * gravity * height);

            return true;
        }
        return false;
    }

    // grounded during last move?
    public bool Grounded => _controller.isGrounded;
}
