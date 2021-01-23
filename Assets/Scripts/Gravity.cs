using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Gravity : MonoBehaviour
{
    /// <summary>
    /// downward force of gravity(read-only, positive is downward)
    /// </summary>
    public float force = 50;

    /// <summary>
    /// additive upward force[0..](can change at runtime
    /// for the paraglider, positive is upward)
    /// </summary>
    public float drag = 0;

    /// <summary>
    /// angle, in degrees, at which point the character will start falling
    /// </summary>
    public float slopeLimit = 45;
    /// <summary>
    /// friction to apply when the character begins sliding
    /// </summary>
    public float slideFriction = 0.3f;
    
    private CharacterController _controller;

    /// <summary>
    /// vertical velocity(+ means upward, - means downward)
    /// </summary>
    public float Velocity { get; set; } = 0f;
    
    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _controller.slopeLimit = 80f; // make this arbitrarily large
    }

    private void Update()
    {
        // increment velocity
        Velocity += (drag - force) * Time.deltaTime;
        
        // determine velocity
        var vel = Vector3.up * Velocity;
        
        if (!Grounded(out var normal))
        {
            vel.x += (1f - normal.y) * normal.x * (1f - slideFriction);
            vel.z += (1f - normal.y) * normal.z * (1f - slideFriction);
            Debug.Log("not grounded");
        }
        else
        {
            Debug.Log("grounded");
            Velocity = Mathf.Max(0, Velocity);
        }
        
        // move
        _controller.Move(vel * Time.deltaTime);
    }

    /// <summary>
    /// currently grounded?
    /// </summary>
    public bool Grounded(out Vector3 normal)
    {
        var dist = _controller.height / 2f + _controller.skinWidth + 0.25f;
        var pos = _controller.transform.position + _controller.center;

        if (Physics.Raycast(pos, Vector3.down, out var hit, dist))
        {
            return Vector3.Angle(Vector3.up, normal = hit.normal) <= slopeLimit;
        }
        normal = Vector3.down;

        return false;
    }
}
