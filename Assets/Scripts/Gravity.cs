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
        _controller.slopeLimit = 80f;
    }

    private void Update()
    {
        // increment velocity
        Velocity += (drag - force) * Time.deltaTime;
        
        // determine velocity
        var vel = Vector3.up * Velocity;
        
        if (!Grounded(out var normal, out var steepness))
        {
            var mult = steepness / slopeLimit;
            
            vel.x += mult * (1f - normal.y) * normal.x * (1f - slideFriction);
            vel.z += mult * (1f - normal.y) * normal.z * (1f - slideFriction);
        }
        else if (Velocity < 0)
        {
            Velocity *= 0.8f;
        }
        
        Debug.Log(vel);
        
        // move
        _controller.Move(vel * Time.deltaTime);
    }

    // currently grounded or otherwise able to move in direction, while considering the slope?
    public bool Grounded(Vector3 dir)
    {
        if (GroundedRaycast(out var hit, 0.25f))
        {
            return Vector3.Angle(Vector3.up, hit.normal) <= slopeLimit || Vector3.ProjectOnPlane(dir, hit.normal).y < 0;
        }
        return false;
    }
    
    // currently grounded, while considering the slope?
    public bool Grounded(out Vector3 normal, out float steepness)
    {
        // initial ray-cast with possibility of being grounded
        if (GroundedRaycast(out var hit, 0.25f))
        {
            return (steepness = Vector3.Angle(Vector3.up, normal = hit.normal)) <= slopeLimit;
        }
        // try with more distance, to get the normal 
        if (GroundedRaycast(out var hit2, 100f))
        {
            steepness = Vector3.Angle(Vector3.up, normal = hit2.normal);
        }
        // in the sky or something...
        else
        {
            normal = Vector3.down;
            steepness = 0;
        }

        return false;
    }

    // currently grounded, with no consideration for slope
    public bool Grounded()
    {
        return GroundedRaycast(out _, 0.25f);
    }

    // perform the grounded raycast hit
    public bool GroundedRaycast(out RaycastHit hit, float threshold)
    {
        var dist = _controller.height / 2f + _controller.skinWidth + threshold;
        var pos = _controller.transform.position + _controller.center;

        return Physics.Raycast(pos, Vector3.down, out hit, dist);
    }

    // attempt to jump, returns if it was successful/grounded
    public bool Jump(float height, float time)
    {
        if (Grounded())
        {
            Velocity = (height - 0.5f * force * time * time) / time;

            return true;
        }
        return false;
    }
}
