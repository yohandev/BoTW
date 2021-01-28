using UnityEngine;

/// <summary>
/// a rigidbody based character controller
/// </summary>
public class CharacterBody : MonoBehaviour
{
    [Tooltip("mass of the rigidbody")]
    public float mass;            // mass of the rigidbody
    
    [Tooltip("movement speed on the xz plane")]
    public float speed;           // movement speed on the xz plane
    [Tooltip("acceleration speed on the xz plane")]
    public float acceleration;    // acceleration speed on the xz plane
    [Tooltip("jump height on flat ground")]
    public float jump;            // jump height on flat ground

    [Tooltip("maximum speed to be snapped to the ground")]
    public float snapThreshold;   // maximum speed to be snapped to the ground

    [Tooltip("extra down ward gravity force to jump faster")]
    public float extraGravity;    // extra downward gravity force to jump faster
    
    private Rigidbody m_rbody;    // character controller component
    private Collider m_collider;  // collider component, whatever shape
    
    private Ground m_ground;      // current ground
    private Input m_input;        // current input

    private Vector3 m_velocity;   // current velocity

    /// <summary>
    /// is the character touching the ground?
    /// </summary>
    public bool OnGround => m_ground.HasContact;
    
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
        m_collider = Collider;

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
            // works only with dynamic bodies
            m_rbody.isKinematic = false;
        }
    }

    private void FixedUpdate()
    {
        // get current, actual velocity
        m_velocity = m_rbody.velocity;
        
        AccelerateFriction();
        AccelerateGround();
        AccelerateXZ();
        AccelerateY();
        
        m_ground.Reset();
        m_input.Reset();
        
        // set modified velocity
        m_rbody.velocity = m_velocity;
    }

    /// <summary>
    /// adjust the velocity in the ground-local x and z directions to handle WASD movement
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private void AccelerateXZ()
    {
        // cache the ground normal vector
        var normal = m_ground.Normal;
        // desired velocity in local space
        var input = speed * m_input.XZ;
        
        // project on plane with normalized normal shortcut
        // vec - normal * dot(vec, normal)
        var rtAxis = (Vector3.right - normal * normal.x).normalized;
        var fwAxis = (Vector3.forward - normal * normal.z).normalized;
        
        // how much velocity is currently going on each axis
        var rtNow = Vector3.Dot(m_velocity, rtAxis);
        var fwNow = Vector3.Dot(m_velocity, fwAxis);

        // combine acceleration on their axes to one vector
        var dVdt = rtAxis * (input.x - rtNow) + fwAxis * (input.z - fwNow);

        // accelerate
        m_velocity += dVdt * Mathf.Clamp01(Time.deltaTime * acceleration);
    }

    /// <summary>
    /// adjusts the velocity in the ground-local y direction to effectively "jump"
    /// </summary>
    private void AccelerateY()
    {
        // currently falling
        if (m_ground.StepSinceContact > 3)
        {
            m_velocity += extraGravity * Time.deltaTime * Vector3.down;
        }
        // jump
        else if (m_ground.StepSinceContact <= 1 && m_input.Jump)
        {
            // initial velocity using simple newtonian physics eq
            var v0 = Mathf.Sqrt(-2f * (Physics.gravity.y - extraGravity) * jump);

            // apply velocity
            m_velocity += v0 * Vector3.up;

            // TODO jump along m_ground.Normal if steep
        }
    }

    /// <summary>
    /// snap to ground
    /// </summary>
    private void AccelerateGround()
    {
        // current speed
        var vel = m_velocity.magnitude;
        // raycast distance; 0.5 is an arbitrary buffer
        var probeDist = m_collider.bounds.size.y / 2 + 0.5f;
        
        // series of check before snapping
        if (false
            // don't snap too early
            || m_ground.StepSinceContact > 1
            || m_input.StepSinceJump <= 2
            // preserve momentum if sufficiently fast
            || vel > snapThreshold
            // if no ground detected, can't possibly snap
            || !Physics.Raycast(m_rbody.position, Vector3.down, out var hit, probeDist))
        {
            return;
        }
//        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
//        {
//            return;
//        }

        // register collision
        m_ground.Add(hit.normal);
        
        // how much velocity is currently snapping
        var dot = Vector3.Dot(m_velocity, hit.normal);
        // "snap" to ground
        if (dot > 0f)
        {
            m_velocity = (m_velocity - hit.normal * dot).normalized * vel;
        }
    }

    private void AccelerateFriction()
    {
        // only apply friction if there's no input and is grounded
        if (!m_input.None || !m_ground.HasContact) { return; }

        // ground normal
        var normal = m_ground.Normal;
        
        //m_velocity.x += (1f - normal.y) * normal.x * (1f - 0.3f);
        //m_velocity.z += (1f - normal.y) * normal.z * (1f - 0.3f);
        
    }

    private void OnCollisionEnter(Collision other)
    {
        OnCollisionStay(other);
    }

    private void OnCollisionStay(Collision other)
    {
        for (var i = 0; i < other.contactCount; i++)
        {
            m_ground.Add(other.GetContact(i).normal);
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
            if (!gameObject.TryGetComponent(out Rigidbody cmp))
            {
                cmp = gameObject.AddComponent<Rigidbody>();
            }
            return cmp;
        }
    }

    /// <summary>
    /// gets the Collider component, or adds a capsule to this game object
    /// </summary>
    private Collider Collider
    {
        get
        {
            // get or add
            if (!gameObject.TryGetComponent(out Collider cmp))
            {
                cmp = gameObject.AddComponent<CapsuleCollider>();
            }
            return cmp;   
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
        /// update states since jumping
        /// </summary>
        private int m_stepSinceJump;
        
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
        public void Add(Vector3 value)
        {
            m_vel += value;
            if (value.y > 0) { m_stepSinceJump = 0; }
        }

        /// <summary>
        /// has no input at all?
        /// </summary>
        public bool None => m_vel == Vector3.zero;

        /// <summary>
        /// number of updates since last jump input
        /// </summary>
        public int StepSinceJump => m_stepSinceJump;
        
        /// <summary>
        /// reset input after every physics state
        /// </summary>
        public void Reset()
        {
            m_vel = Vector3.zero;
            m_stepSinceJump++;
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
        /// update states since last ground contact
        /// </summary>
        private int m_stepSinceContact;

        /// <summary>
        /// get the normalized, average vector of all the grounds currently in contact
        /// </summary>
        public Vector3 Normal => m_normal?.normalized ?? Vector3.up;

        /// <summary>
        /// body currently has contact with at least one floor? 
        /// </summary>
        public bool HasContact => m_normal.HasValue;

        /// <summary>
        /// number of fixed updates since last grounded
        /// </summary>
        public int StepSinceContact => m_stepSinceContact;

        /// <summary>
        /// aggregate collision's normal vector to self
        /// </summary>
        public void Add(Vector3 normal)
        {
            m_normal = (m_normal ?? default) + normal;
            m_stepSinceContact = 0;
        }

        /// <summary>
        /// reset current ground after every physics state
        /// </summary>
        public void Reset()
        {
            m_normal = null;
            m_stepSinceContact++;
        }
    }
}
