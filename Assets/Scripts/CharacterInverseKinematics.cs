using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterInverseKinematics : MonoBehaviour
{
    [Tooltip("activate inverse kinematic?")]
    public bool active = true;
    
    [System.Serializable]
    public struct RootIk
    {
        [Range(0f, 1f)]
        [Tooltip("interpolation weight for the root's position")]
        public float positionWeight;
        
        [Range(0f, 1f)]
        [Tooltip("interpolation weight for the root's rotation")]
        public float rotationWeight;
        
        [Tooltip("local position of root when steepness is flat")]
        public Vector3 positionMin;
        [Tooltip("local position of root when steepness is 90º")]
        public Vector3 positionMax;
    }
    
    [System.Serializable]
    public struct FeetIk
    {
        [Tooltip("Feet's transforms")]
        public Transform left, right;
        
        [Range(0f, 1f)]
        [Tooltip("Feet positions' IK blending weights")]
        public float leftPositionWeight, rightPositionWeight;

        [Range(0f, 1f)]
        [Tooltip("Feet rotations' IK blending weights")]
        public float leftRotationWeight, rightRotationWeight;

        [Tooltip("offset for feet's local space positions after IK calculations")]
        public Vector3 offset;
        [Tooltip("feet's forward direction in local space")]
        public Vector3 forward;
    }
    
    
    [Tooltip("Distance to raycast downward")]
    [Range(0f, 5f)]
    public float rayCastDistance = 1.2f;
    
    [Tooltip("Layer mask to raycast to")]
    public LayerMask mask;

    [Tooltip("Root config")]
    public RootIk root;
    [Tooltip("Feet config")]
    public FeetIk feet;
    
    // root's forward direction, to change at runtime
    public Vector3 Forward { get; set; } = Vector3.forward;
    
    private Transform _trans;
    private Animator _anim;

    private void Start()
    {
        _trans = GetComponent<Transform>();
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
        var origin = _trans.position;
        origin.y = 0.5f * (feet.left.position.y + feet.right.position.y);

        if (enabled && Physics.Raycast(origin + Vector3.up, Vector3.down, out var hit, rayCastDistance, mask))
        {
            // calculate upwards and forwards
            var up = Vector3.Lerp(Vector3.up, hit.normal, root.rotationWeight);
            var fwd = Forward;
            
            // set rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(fwd, up), Time.deltaTime * 10);
            
            // calculate position
            var pos = Vector3.Lerp(root.positionMin, root.positionMax, root.positionWeight * (1f - Vector3.Dot(Vector3.up, hit.normal)));

            // set local position
            transform.localPosition = pos;
            
            // debug
            Debug.DrawLine(origin, origin + hit.normal, Color.green);
        }
    }
    
    private void SetFoot(AvatarIKGoal goal)
    {
        var origin = _anim.GetIKPosition(goal);
        var isLeft = goal == AvatarIKGoal.LeftFoot;

        if (enabled && Physics.Raycast(origin + Vector3.up, Vector3.down, out var hit, rayCastDistance, mask))
        {
            // set weight if ray cast found
            _anim.SetIKPositionWeight(goal, isLeft ? feet.leftPositionWeight : feet.rightPositionWeight);
            _anim.SetIKRotationWeight(goal, isLeft ? feet.leftRotationWeight : feet.rightRotationWeight);

            // get bone
            var bone = isLeft ? feet.left : feet.right;
            
            // calculate position and rotation
            var nor = hit.normal;
            var fwd = bone.TransformDirection(feet.forward);
            var pos = hit.point + Vector3.Project(feet.offset, nor);
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
