using System;
using UnityEngine;

// assigned to the link gameobject, *not* the paraglider itself
[RequireComponent(typeof(Gravity))]
public class ParagliderTarget : MonoBehaviour
{
    [Tooltip("game object visually representing the paraglider")]
    public GameObject paraglider;

    [Tooltip("drag coefficient for when the paraglider is active")]
    [Range(0f, 100f)]
    public float drag = 0.3f;

    [Tooltip("min distance from center to ground required to glide")]
    [Range(0f, 10f)]
    public float minDistance = 2f;
    
    // is the paraglider active?
    public bool Gliding
    {
        get => paraglider.activeSelf;
        set => paraglider.SetActive(value);
    }
    
    // gravity component
    private Gravity _gravity;

    private void Start()
    {
        _gravity = GetComponent<Gravity>();

        Gliding = false;
    }

    private void Update()
    {
        // should be gliding?
        if (Physics.Raycast(transform.position, Vector3.down, minDistance))
        {
            Gliding = false;
        }
        
        // simple F = Bv model
        _gravity.drag = Gliding ? -(drag * _gravity.Velocity) : 0;
    }
}
