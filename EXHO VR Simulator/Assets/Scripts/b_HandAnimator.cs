using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class b_HandAnimator : MonoBehaviour
{

    public float speed = 5.0f;
    public XRController controller = null;

    private Animator animator = null;

    private readonly List<b_Finger> gripFingers = new List<b_Finger>()
    {
        new b_Finger(FingerType.Middle),
        new b_Finger(FingerType.Ring),
        new b_Finger(FingerType.Pinky)
    };

    private readonly List<b_Finger> pointFingers = new List<b_Finger>
    {
        new b_Finger(FingerType.Index),
        new b_Finger(FingerType.Thumb)
    };

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckGrip();
        CheckPointer();

        SmoothFinger(pointFingers);
        SmoothFinger(gripFingers);

        AnimateFinger(pointFingers);
        AnimateFinger(gripFingers);
    }

    private void CheckGrip()
    {
        if (controller.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            SetFingerTargets(gripFingers, gripValue);
        }
    }

    private void CheckPointer()
    {
        if (controller.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float pointerValue))
        {
            SetFingerTargets(pointFingers, pointerValue);
        }
    }

    private void SetFingerTargets(List<b_Finger> fingers, float value)
    {
        foreach (b_Finger finger in fingers)
        {
            finger.target = value;
        }
    }

    private void SmoothFinger(List<b_Finger> fingers)
    {
        foreach (b_Finger finger in fingers)
        {
            float time = speed * Time.unscaledDeltaTime;
            finger.current = Mathf.MoveTowards(finger.current, finger.target, time);
        }
    }

    private void AnimateFinger(List<b_Finger> fingers)
    {
        foreach (b_Finger finger in fingers)
        {
            AnimateFinger(finger.type.ToString(), finger.current);
        }
    }

    private void AnimateFinger(string finger, float blend)
    {
        animator.SetFloat(finger, blend);
    }
}