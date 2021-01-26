using UnityEngine;

/// <summary>
/// third person camera controller. this depends on the fact that the main
/// camera has a cinemachine third person camera setup
/// </summary>
public class CameraController : MonoBehaviour
{
    [Tooltip("object the third person camera should target")]
    public Transform target;

    private Vector3 m_rot;    // target rotation in euler angles
    
    /// <summary>
    /// move the third person camera by (pitch, yaw). change is immediate
    /// </summary>
    public void Move(Vector2 input)
    {
        // calculate new rotation
        m_rot.x = m_rot.x <= 90f 
            ? Mathf.Clamp(m_rot.x - input.y, -89f, 89f)
            : Mathf.Clamp(m_rot.x - input.y, 271f, 361f);
        m_rot.y += input.x;
        
        // apply rotation
        target.rotation = Quaternion.Lerp(target.rotation, Quaternion.Euler(m_rot), Time.deltaTime * 10);
    }
}
