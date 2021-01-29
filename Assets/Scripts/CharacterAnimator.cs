using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Tooltip("the target graphics GameObject")]
    public Transform target;       // the target graphics GameObject
    
    private Vector3 m_velocity;    // current move direction

    /// <summary>
    /// set the look direction
    /// </summary>
    public void Move(Vector2 dir)
    {
        if (dir == Vector2.zero) { return; }
        
        m_velocity = new Vector3(dir.x, 0, dir.y);
    }

    private void Start()
    {
        m_velocity = Vector3.forward;
    }

    private void Update()
    {
        // look direction
        target.rotation = Quaternion.Slerp(target.rotation, Quaternion.LookRotation(m_velocity), Time.deltaTime * 10);
    }
}
