using System;
using UnityEngine;

public class Glider : MonoBehaviour
{
    [Tooltip("the target object to make glide")]
    public GameObject target;           // target object to make glide

    [Tooltip("should currently glide?")]
    public bool active;                 // should currently glide?
    [Tooltip("drag coefficient")]
    public float coefficient;           // drag coefficient
    [Tooltip("distance from feet to ground for auto cancel")]
    public float distance;              // distance from feet to ground for auto cancel

    private Rigidbody m_rbody;          // target rigidbody
    private Collider m_collider;        // target collider

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
        // only act if active
        if (!active) { return; }

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
