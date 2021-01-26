using UnityEngine;

/// <summary>
/// a rigidbody based character controller
/// </summary>
public class RigidbodyController : MonoBehaviour
{
    [Tooltip("mass of the rigidbody")]
    public float mass;            // mass of the rigidbody
    
    [Tooltip("movement speed on the xz plane")]
    public float speed;           // movement speed on the xz plane
    [Tooltip("jump height on flat ground")]
    public float jump;            // jump height on flat ground
    
    private Rigidbody m_rbody;    // rigidbody component
    
    private Ground m_ground;      // current ground
    private Input m_input;        // current input

    /// <summary>
    /// move along the current ground plane, or xz plane if not grounded, in the
    /// given directions. inputs are magnitude clamped and adjusted to this component's
    /// max speed
    ///
    /// it doesn't matter how many times this function is called, the force applied will
    /// not exceed `speed` in magnitude, but the inputs will aggregate together
    /// </summary>
    public void Move(float x, float z)
    {
        // manually create vector3 because annoying implicit casts
        // accept vector2's in the place of vector3s where z is
        // discarded
        m_input.Add(new Vector3(x, 0, z));
    }

    /// <summary>
    /// overload for Move(), taking in (horizontal, forward) input vector
    /// </summary>
    public void Move(Vector2 xz)
    {
        Move(xz.x, xz.y);
    }

    /// <summary>
    /// jump straight up on the y axis or, if the current ground is considered "steep",
    /// along its normal.
    ///
    /// it doesn't matter how many times this function is called, it will only execute
    /// one "jump" of height `height` during FixedUpdate
    /// </summary>
    public void Jump()
    {
        // doesn't matter what this is, just has to be higher than 0
        m_input.Add(Vector3.up);
    }
    
    private void Start()
    {
        m_rbody = Rigidbody;
        
        m_ground = new Ground();
        m_input = new Input();
        
        // setups other component's fields
        OnValidate();
    }

    /// <summary>
    /// called when inspector values change
    /// </summary>
    private void OnValidate()
    {
        // rigidbody
        if (m_rbody)
        {
            // character never rotates at all, visual child can rotate freely in local space
            m_rbody.constraints = RigidbodyConstraints.FreezeRotation;
            // rigidbody is configurable through this component
            m_rbody.mass = mass;
            // interpolation
            m_rbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void FixedUpdate()
    {
        AccelerateXZ();
        AccelerateY();
        
        m_ground.Reset();
        m_input.Reset();
    }

    /// <summary>
    /// adjust the velocity in the ground-local x and z directions to handle WASD movement
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private void AccelerateXZ()
    {
        // get the current *actual* velocity
        var velocity = m_rbody.velocity;
        
        // cache the ground normal vector
        var normal = m_ground.Normal;
        // desired velocity in local space
        var input = Time.fixedDeltaTime * speed * m_input.XZ;

        // project on plane with normalized normal shortcut
        // vec - normal * dot(vec, normal)
        var rtAxis = (Vector3.right - normal * normal.x).normalized;
        var fwAxis = (Vector3.forward - normal * normal.z).normalized;

        // how much velocity is currently going on each axis
        var rtNow = Vector3.Dot(velocity, rtAxis);
        var fwNow = Vector3.Dot(velocity, fwAxis);

        // combine acceleration on their axes to one vector
        var acceleration = rtAxis * (input.x - rtNow) + fwAxis * (input.z - fwNow);

        // acceleration is high enough to instantaneously change directions
        m_rbody.velocity += acceleration;
        
        // debug
        Debug.DrawLine(m_ground.Point, m_ground.Point + acceleration, Color.red);
        Debug.DrawLine(m_ground.Point, m_ground.Point + velocity, Color.green);
    }

    /// <summary>
    /// adjusts the velocity in the ground-local y direction to effectively "jump"
    /// </summary>
    private void AccelerateY()
    {
        // only jump if on ground and has input
        if (!m_ground.HasContact || !m_input.Jump) { return; }
        
        // initial velocity using simple newtonian physics eq
        var v0 = Mathf.Sqrt(-2f * Physics.gravity.y * jump);

        // apply velocity
        m_rbody.velocity += v0 * Vector3.up;
        
        // TODO jump along m_ground.Normal if steep
    }
    
    /// <summary>
    /// called the first time there's collision
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionStay(collision);
    }

    /// <summary>
    /// called every time but the first there's collision, after fixed update
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        for (var i = 0; i < collision.contactCount; i++)
        {
            m_ground.Add(collision.GetContact(i));
        }
    }

    /// <summary>
    /// gets or adds a Rigidbody component to this GameObject
    /// </summary>
    private Rigidbody Rigidbody
    {
        get
        {
            // get or add
            if (!gameObject.TryGetComponent(out Rigidbody rb))
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            return rb;
        }
    }

    /// <summary>
    /// current input state
    /// </summary>
    private struct Input
    {
        /// <summary>
        /// input sum where m_vel.x and m_vel.z are along the current ground plane, and
        /// m_vel.y > 0 is desired jump
        /// </summary>
        private Vector3 m_vel;
        
        /// <summary>
        /// get the input on the x and z axis in world space, with y = 0 and magnitude ≤ 1.0
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public Vector3 XZ => Vector3.ClampMagnitude(new Vector3(m_vel.x, 0, m_vel.z), 1.0f);

        /// <summary>
        /// get whether a jump is desired
        /// </summary>
        public bool Jump => m_vel.y > 0;
        
        /// <summary>
        /// aggregate input to self
        /// </summary>
        public void Add(Vector3 value) => m_vel += value;

        /// <summary>
        /// reset input after every physics state
        /// </summary>
        public void Reset()
        {
            m_vel = Vector3.zero;
        }
    }

    /// <summary>
    /// represents the current ground this controller is on, or the absence of one
    /// </summary>
    private struct Ground
    {
        /// <summary>
        /// aggregated normal vector
        /// </summary>
        private Vector3? m_normal;

        /// <summary>
        /// last point of contact
        /// </summary>
        private Vector3 m_point;
        
        /// <summary>
        /// get the normalized, average vector of all the grounds currently in contact
        /// </summary>
        public Vector3 Normal => m_normal?.normalized ?? Vector3.up;

        /// <summary>
        /// body currently has contact with at least one floor? 
        /// </summary>
        public bool HasContact => m_normal.HasValue;

        /// <summary>
        /// get the last point of contact on the ground, if grounded(for debugging)
        /// </summary>
        public Vector3 Point => m_point;
        
        /// <summary>
        /// aggregate collision to self
        /// </summary>
        public void Add(ContactPoint b)
        {
            m_normal = (m_normal ?? default) + b.normal;
            m_point = b.point;
        }

        /// <summary>
        /// reset current ground after every physics state
        /// </summary>
        public void Reset()
        {
            m_normal = null;
            m_point = default;
        }
    }
}
