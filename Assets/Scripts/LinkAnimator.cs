using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LinkAnimator : MonoBehaviour
{
    /// <summary>
    /// foot distance to raycast downward
    /// </summary>
    public float footDistance = 0.5f;
    /// <summary>
    /// distance from base of foot to its transform center
    /// </summary>
    public float footThickness = 0.1f;
    /// <summary>
    /// distance from base of foot to its geometrical center
    /// </summary>
    public float footLength = 0.2f;
    
    private Animator _animator;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Idle State"))
        {
            // set weights
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            //var forward = transform.forward;

            // sample from slightly above foot
            var posL = _animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up;
            var posR = _animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up;

            // sample from slightly in front of ankle
            var fwdL = _animator
                .GetBoneTransform(HumanBodyBones.LeftFoot)
                .GetChild(0)
                .up;
            var fwdR = _animator
                .GetBoneTransform(HumanBodyBones.RightFoot)
                .GetChild(0)
                .up;
            
            // obviously don't hit link
            var mask = ~LayerMask.GetMask("Link");
            
            // raycast
            if (Physics.Raycast(posL + fwdL * footLength, Vector3.down, out var hitL, footDistance + 1f, mask))
            {
                _animator.SetIKPosition(AvatarIKGoal.LeftFoot, hitL.point + Vector3.up * footThickness);
                _animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(fwdL, hitL.normal));
            }
            if (Physics.Raycast(posR + fwdR * footLength, Vector3.down, out var hitR, footDistance + 1f, mask))
            {
                _animator.SetIKPosition(AvatarIKGoal.RightFoot, hitR.point + Vector3.up * footThickness);
                _animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(fwdR, hitR.normal));
            }
        }
        else
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
        }
    }
}
