using System;
using UnityEngine;

public class Movement2 : MonoBehaviour
{
    /*
     * Movement Controller 2
     *
     * When on the ground(determined by a variable), the character raycasts down to the
     * floor in the intended direction and "sticks" there. The normal of the detected floor
     * is used for slope slipping and slowing down.
     *
     * When off the ground, whether through a jump or too steep of a slope, the character is
     * at the mercy of physics until flat-enough ground is detected
     */
    
    /// <summary>
    /// capsule collider settings
    /// </summary>
    [Serializable]
    public struct ColliderInfo
    {
        /// <summary>
        /// radius on the xz plane, from transform center, of the capsule collider
        /// </summary>
        [Range(0f, 10f)]
        public float radius;
        
        /// <summary>
        /// distance from transform center to the capsule collider's highest point
        /// </summary>
        [Space(5)]
        [Range(0f, 10f)]
        public float head;
        /// <summary>
        /// distance from transform center to the capsule collider's lowest point
        /// </summary>
        [Range(0f, 10f)]
        public float feet;

        /// <summary>
        /// layer mask to collide with
        /// </summary>
        [Space(5)]
        public LayerMask mask;
    }

    /// <summary>
    /// ground the character is currently sitting on
    /// </summary>
    private struct GroundHit
    {
        /// <summary>
        /// vector normal to the ground
        /// </summary>
        public Vector3 normal;
        
        /// <summary>
        /// position of the collider's lowermost point on the ground
        /// </summary>
        public Vector3 feet;
    }

    /// <summary>
    /// capsule collider settings
    /// </summary>
    [SerializeField]
    private new ColliderInfo collider = new ColliderInfo
    {
        radius = 0.5f,
        head = 1f,
        feet = 1f
    };
    
    /// <summary>
    /// graph in the speed multiplier[0-1] vs angle(0-90) of slope domain
    /// </summary>
    [SerializeField]
    private AnimationCurve slope;
    
    /// <summary>
    /// last ground hit, or none if not grounded
    /// </summary>
    private GroundHit? m_ground;

    /// <summary>
    /// this game object's transform
    /// </summary>
    private Transform m_transform;

    /// <summary>
    /// was the collider on the ground after the last move?
    /// </summary>
    public bool OnGround => m_ground != null;

    /// <summary>
    /// position of the topmost point on this object's capsule collider, in world space
    /// </summary>
    public Vector3 Head => m_transform.position + collider.head * Vector3.up;
    
    /// <summary>
    /// position of the lowermost point on this object's capsule collider, in world space
    /// </summary>
    public Vector3 Feet => m_transform.position - collider.feet * Vector3.up;

    /// <summary>
    /// distance from this object capsule collider's feet to head
    /// </summary>
    public float Height => collider.head + collider.feet;
    
    private void Start()
    {
        m_transform = gameObject.GetComponent<Transform>();
    }

    /// <summary>
    /// move in the given velocity on the xz plane, or on the ground plane if grounded
    ///
    /// the magnitude of the displacement vector(as returned) is velocity.magnitude times
    /// the slope multiplier if on the ground
    /// </summary>
    public Vector3 Move(Vector2 velocity)
    {
        // velocity on the xz plane
        var vel = new Vector3(velocity.x, 0, velocity.y);
        
        // update ground
        FindGround(vel, out m_ground);
    }

    /// <summary>
    /// raycast downwards to find ground
    /// </summary>
    private void FindGround(Vector3 vel, out GroundHit? ground)
    {
        
        
        if (Physics.Raycast(Head + vel, Vector3.down, out var rhit))
        {
            
        }

        hit = null;
    }
}
