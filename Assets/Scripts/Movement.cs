using System;
using System.Collections.Generic;
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

        [Tooltip("additional buffer space used when raycasting to ground")]
        [Range(0f, 1f)]
        public float skinWidth;

        [Tooltip("layer mask to collide with")]
        [Space(5)]
        public LayerMask mask;
    }
    [Tooltip("capsule collider")]
    public new ColliderInfo collider = new ColliderInfo { radius = 0.5f, feet = 1f, head = 1f, skinWidth = 0.1f };
    
    [Serializable]
    public struct PhysicsInfo
    {
        [Tooltip("downward acceleration of gravity")]
        [Range(0f, 100f)]
        public float gravity;
        
        [Tooltip("maximum slope angle that can be climbed")]
        [Range(0f, 89f)]
        public float slopeLimit;

        [Tooltip("minimum distance required to actually move the game object")]
        [Range(0f, 0.5f)]
        public float minMoveDistance;
    }
    [Tooltip("physics settings")]
    public PhysicsInfo physics = new PhysicsInfo { gravity = 9.8f, slopeLimit = 60, minMoveDistance = 0.1f };
    
    // intended direction in world space, along the grounded plane, of movement. magnitude is preserved
    // for speed
    public Vector2 Direction { get; set; }
    
    // current y velocity in world space(read-only)
    public float YVelocity { get; private set; }
    
    // current y drag in world space, which can change at runtime
    public float YDrag { get; set; }

    // is this game-object grounded after the last move?
    public bool Grounded { get; private set; }

    // this game object's transform direction
    private Transform _transform;

    // this game object's capsule collider, added by this component
    private CapsuleCollider _collider;
    
    // cached overlap colliders
    private Collider[] _overlap;

    private void Start()
    {
        _transform = gameObject.GetComponent<Transform>();
        _collider = gameObject.AddComponent<CapsuleCollider>();
        
        _overlap = new Collider[32];
    }

    private void Update()
    {
        // calculate velocity
        YVelocity += (YDrag - physics.gravity) * Time.deltaTime;

        // direction vector in world space
        var dir = new Vector3(Direction.x, 0, Direction.y);
        
        // is grounded?
        Grounded = GroundCast(out var hit);
        if (Grounded)
        {
            // reduce stutter
            YVelocity = Mathf.Max(YVelocity, 0);
        }
        
        // direction along ground
        dir = Vector3.ProjectOnPlane(dir, hit.normal);

        // move
        Move((/* input: */ dir + /* y-velocity: */ Vector3.up * YVelocity) * Time.deltaTime);
        
        Debug.Log(Grounded + "," + dir + ", " + hit.normal);
    }

    // resolve collisions
    private void LateUpdate()
    {
        // update collider for Physics.ComputePenetration
        UpdateCollider();

        // resolve collisions
        foreach (var colB in GetCollisions())
        {
            // ignore self
            if (colB == _collider) { continue; }
            
            // this collider info
            var colA = _collider;
            var posA = _transform.position;
            var rotA = _transform.rotation;
            // other collider info
            var traB = colB.transform;
            var posB = traB.position;
            var rotB = traB.rotation;

            // has penetration --> correct
            if (Physics.ComputePenetration(colA, posA, rotA, colB, posB, rotB, out var dir, out var dist))
            {
                // zero velocity on collision
                YVelocity -= Vector3.Project(YVelocity * Vector3.up, -dir).y;
                
                // remove penetration
                Move(dir * dist);
            }
        }
    }

    // iterate over the colliding colliders
    private IEnumerable<Collider> GetCollisions()
    {
        var pos = _transform.position;
        var rad = collider.radius;

        var top = pos + Vector3.up * (collider.head - rad);
        var btm = pos + Vector3.down * (collider.feet - rad);
        
        var mask = collider.mask;

        var num = Physics.OverlapCapsuleNonAlloc(top, btm, rad, _overlap, mask);

        for (var i = 0; i < num; i++)
        {
            yield return _overlap[i];
        }
    }

    // update this game object's capsule collider
    private void UpdateCollider()
    {
        _collider.radius = collider.radius;
        _collider.height = collider.feet + collider.head;
        _collider.center = (collider.head - collider.feet) * 0.5f * Vector3.up;
    }

    // cast to the ground. returns a hit.normal no matter what
    private bool GroundCast(out RaycastHit hit)
    {
        var dist = collider.feet + collider.skinWidth;
        var pos = _transform.position;
        var dir = Vector3.down;

        if (Physics.Raycast(pos, dir, out hit) && hit.distance <= dist)
        {
            return true;
        }
        hit.normal = Vector3.up;

        return false;
    }

    // jump to height
    public bool Jump(float height)
    {
        YVelocity = Mathf.Sqrt(2 * height * physics.gravity);

        // TODO
        return true;
    }

    // move the transform
    private void Move(Vector3 value)
    {
        if (value.sqrMagnitude >= physics.minMoveDistance * physics.minMoveDistance)
        {
            _transform.position += value;
        }
    }
}
