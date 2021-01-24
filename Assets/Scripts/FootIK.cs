using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour
{
    public bool active = true;

    public Transform left;
    public Transform right;

    [Range(0f, 1f)]
    public float rootRotWeight = 0.25f;
    [Range(0f, 1f)]
    public float rootPosWeight = 1.0f;
    
    [Range(0f, 1f)]
    public float positionWeight = 1f;
    [Range(0f, 1f)]
    public float rotationWeight = 1f;

    /// <summary>
    /// distance to ray cast downward
    /// </summary>
    [Range(0f, 5f)]
    public float rayCastDistance = 1.2f;

    /// <summary>
    /// local position of root when steepness is flat
    /// </summary>
    public Vector3 localPosMin = Vector3.zero;
    /// <summary>
    /// local position of root when steepness is 90º
    /// </summary>
    public Vector3 localPosMax = Vector3.down;
    
    /// <summary>
    /// offset the foot position after the fact
    /// </summary>
    public Vector3 offset = Vector3.zero;
    /// <summary>
    /// foot's forward direction
    /// </summary>
    public Vector3 footForward = Vector3.forward;

    /// <summary>
    /// layer mask to ray cast to
    /// </summary>
    public LayerMask mask;
    
    /// <summary>
    /// root's forward direction
    /// </summary>
    public Vector3 Forward { get; set; } = Vector3.forward;
    
    private Animator _anim;
    
    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        SetRoot();
        
        SetFoot(AvatarIKGoal.LeftFoot);
        SetFoot(AvatarIKGoal.RightFoot);
    }

    private void SetRoot()
    {
        var origin = 0.5f * (left.position + right.position);

        if (enabled && Physics.Raycast(origin + Vector3.up, Vector3.down, out var hit, rayCastDistance, mask))
        {
            // calculate upwards and forwards
            var up = Vector3.Lerp(Vector3.up, hit.normal, rootRotWeight);
            var fwd = Forward;
            
            // set rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(fwd, up), Time.deltaTime * 10);
            
            // calculate position
            var pos = Vector3.Lerp(localPosMin, localPosMax, rootPosWeight * (1f - Vector3.Dot(Vector3.up, hit.normal)));

            // set local position
            transform.localPosition = pos;
            
            // debug
            Debug.DrawLine(origin, origin + hit.normal, Color.green);
        }
    }
    
    private void SetFoot(AvatarIKGoal goal)
    {
        var origin = _anim.GetIKPosition(goal);

        if (enabled && Physics.Raycast(origin + Vector3.up, Vector3.down, out var hit, rayCastDistance, mask))
        {
            // set weight if ray cast found
            _anim.SetIKPositionWeight(goal, positionWeight);
            _anim.SetIKRotationWeight(goal, rotationWeight);

            // get bone
            var bone = goal == AvatarIKGoal.RightFoot ? right : left;
            
            // calculate position and rotation
            var nor = hit.normal;
            var fwd = bone.TransformDirection(footForward);
            var pos = hit.point + Vector3.Project(offset, nor);
            var rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(fwd, nor), nor);
            
            // set goals
            _anim.SetIKPosition(goal, pos);
            _anim.SetIKRotation(goal, rot);
        }
        else
        {
            // default to animation if ray cat isn't yielded
            _anim.SetIKPositionWeight(goal, 0);
            _anim.SetIKRotationWeight(goal, 0);
        }
    }
}
