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
        // simple F = Bv model
        _gravity.drag = Gliding ? -(drag * _gravity.Velocity) : 0;
    }
}
