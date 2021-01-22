using UnityEngine;

/// <summary>
/// gravity for game objects with character controllers
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Gravity : MonoBehaviour
{
    /// <summary>
    /// vertical force of gravity
    /// </summary>
    public float force = -9.81f;
    
    private CharacterController _controller;
    private float _velocity;
    
    // Start is called before the first frame update
    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_velocity <= 0f && Grounded(0.0f))
        {
            _velocity = 0f;
        }
        else
        {
            _velocity += force * Time.deltaTime;
        }
        
        _controller.Move(_velocity * Time.deltaTime * Vector3.up);
    }

    /// <summary>
    /// jump upwards to given height in given time, if grounded
    /// parameters are accurate if in perfect conditions
    /// time is for ground to peak, *not* ground to peak to ground
    /// </summary>
    public void Jump(float height, float time)
    {
        if (Grounded(0.25f))
        {
            _velocity = (height - 0.5f * force * time * time) / time;
        }
    }

    private bool Grounded(float threshold)
    {
        var dist = _controller.height / 2f + _controller.skinWidth + threshold;
        var pos = _controller.transform.position;
        
        return Physics.Raycast(pos, Vector3.down, dist);
    }
}
