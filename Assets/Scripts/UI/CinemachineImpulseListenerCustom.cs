using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineImpulseListenerCustom : CinemachineImpulseListener
{
    protected override void PostPipelineStageCallback(
    CinemachineVirtualCameraBase vcam,
    CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        /*Debug.Log("MOO");*/
        if (stage != CinemachineCore.Stage.Aim) 
            return;
        
        Vector3 impulsePos = Vector3.zero;
        Quaternion impulseRot = Quaternion.identity;
        if (CinemachineImpulseManager.Instance.GetImpulseAt(
            state.FinalPosition, m_Use2DDistance, m_ChannelMask,
            out impulsePos, out impulseRot))
        {
            state.PositionCorrection += new Vector3(impulsePos.x, impulsePos.y, 0) * -m_Gain;
            //impulseRot = Quaternion.SlerpUnclamped(Quaternion.identity, impulseRot, -m_Gain);
            //state.OrientationCorrection = state.OrientationCorrection * impulseRot;
        }
    }
}
