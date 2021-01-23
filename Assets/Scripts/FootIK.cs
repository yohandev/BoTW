using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour
{
    public bool active = true;

    public Transform left;
    public Transform right;
    
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
    /// offset the foot position after the fact
    /// </summary>
    public Vector3 offset = Vector3.zero;
    /// <summary>
    /// foot's forward direction
    /// </summary>
    public Vector3 forward = Vector3.forward;

    /// <summary>
    /// layer mask to ray cast to
    /// </summary>
    public LayerMask mask;
    
    private Animator _anim;
    
    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        SetFoot(AvatarIKGoal.LeftFoot);
        SetFoot(AvatarIKGoal.RightFoot);
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
            var fwd = bone.TransformDirection(forward);
            var pos = hit.point + offset;
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
