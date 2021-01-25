using System;
using UnityEngine;

// assigned to the link gameobject, *not* the paraglider itself
[RequireComponent(typeof(Movement))]
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
    
    private Movement _move;

    private void Start()
    {
        _move = GetComponent<Movement>();

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
        _move.YDrag = Gliding ? -(drag * _move.YVel) : 0;
    }
}
