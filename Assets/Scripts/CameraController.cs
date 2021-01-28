using UnityEngine;

/// <summary>
/// third person camera controller. this depends on the fact that the main
/// camera has a cinemachine third person camera setup
/// </summary>
public class CameraController : MonoBehaviour
{
    [Tooltip("object the third person camera should target")]
    public Transform target;
    
    [Tooltip("threshold of dot product between camera forward and up vector where top-down movement is applied")]
    public float threshold = 0.9f;
    
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

    /// <summary>
    /// transforms a direction vector in local space, relative to this third
    /// person camera, to world space. magnitude is preserved
    /// </summary>
    public Vector2 TransformDirection(Vector2 value)
    {
        // input space to local space
        var dir = new Vector3(value.x, 0, value.y);
        
        // decide between top down and relative control
        if (Vector3.Dot(Vector3.down, target.forward) < threshold)
        {
            // relative control
            dir = target.TransformDirection(dir);
        }
        else
        {
            // top down control
            dir = Quaternion.Euler(0, target.eulerAngles.y, 0) * dir;
        }

        // eliminate y direction
        return new Vector2(dir.x, dir.z).normalized * value.magnitude;
    }
}
