using UnityEngine;

/// <summary>
/// glider component
/// </summary>
public class Glider : MonoBehaviour
{
    [Tooltip("the target object to make glide")]
    public GameObject target;           // target object to make glide

    [Tooltip("the graphical paraglider")]
    public GameObject graphics;         // the graphical paraglider
    
    [Tooltip("should currently glide?")]
    public bool active;                 // should currently glide?
    [Tooltip("drag coefficient")]
    public float coefficient;           // drag coefficient
    [Tooltip("distance from feet to ground for auto cancel")]
    public float distance;              // distance from feet to ground for auto cancel

    private Rigidbody m_rbody;          // target rigidbody
    private Collider m_collider;        // target collider

    private int m_sinceActive;          // frames since activated
    
    private void Start()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        m_collider = target.GetComponent<Collider>();
        m_rbody = null;
    }

    private void FixedUpdate()
    {
        // graphics
        graphics.SetActive(m_sinceActive++ > 3);
        
        // only act if active
        if (!active)
        {
            m_sinceActive = 0;
            return;
        }

        // get rigidbody
        var rb = Rigidbody;
        
        // ray cast to auto turn off
        var dist = m_collider.bounds.size.y / 2 + distance;
        if (Physics.Raycast(rb.position, Vector3.down, dist))
        {
            active = false;
            return;
        }

        // drag
        rb.AddForce(coefficient * Mathf.Abs(rb.velocity.y) * Time.deltaTime * Vector3.up);
    }

    /// <summary>
    /// get the target's rigidbody
    /// </summary>
    private Rigidbody Rigidbody => m_rbody ? m_rbody : GetComponent<Rigidbody>();
}
