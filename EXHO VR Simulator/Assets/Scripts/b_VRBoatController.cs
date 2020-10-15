using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;
using Crest;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class b_VRBoatController : MonoBehaviour
{
    [SerializeField]
    private float speed = 10.0f;
    [SerializeField]
    private XRNode controllerNodeL = XRNode.LeftHand;
    [SerializeField]
    private XRNode controllerNodeR = XRNode.RightHand;

    private InputDevice controller;

    private List<InputDevice> devicesL = new List<InputDevice>();
    private List<InputDevice> devicesR = new List<InputDevice>();

    private bool buttonPressed;

    public GameObject currentBoat;

    private BoatProbes currentProbes;

    public GameObject rainObject;
    private ParticleSystem rain;

    private InputDevice controllerL, controllerR;

    // Start is called before the first frame update
    void Start()
    {
        GetBoatProbesComponent(currentBoat);
        GetDevice();

        rain = rainObject.GetComponent<ParticleSystem>();
    }

    private void GetBoatProbesComponent(GameObject boat)
    {
        currentProbes = boat.GetComponent<BoatProbes>();
    }

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(controllerNodeL, devicesL);
        controllerL = devicesL.FirstOrDefault();

        InputDevices.GetDevicesAtXRNode(controllerNodeR, devicesR);
        controllerL = devicesR.FirstOrDefault();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        Vector2 primary2dValue;
        Vector2 secondary2dValue;

        InputFeatureUsage<Vector2> primary2DVector = CommonUsages.primary2DAxis;
        InputFeatureUsage<Vector2> secondary2DVector = CommonUsages.secondary2DAxis;

        if (controllerL.TryGetFeatureValue(primary2DVector, out primary2dValue) && primary2dValue != Vector2.zero)
        {
            currentProbes._turnBias = primary2dValue.x;
        }

        if (controllerR.TryGetFeatureValue(secondary2DVector, out secondary2dValue) && secondary2dValue != Vector2.zero)
        {
            currentProbes._enginePower = secondary2dValue.y;
        }
    }

    private void EnableOrDisableRain()
    {
        bool buttonValue;

        if (controller.TryGetFeatureValue(CommonUsages.primaryButton, out buttonValue) && buttonValue)
        {
            if (!buttonPressed)
            {
                buttonPressed = true;
                if (rain.isEmitting == true)
                {
                    rain.Stop();
                }
                else
                {
                    rain.Play();
                }

            }
        }
        else if (buttonPressed)
        {
            buttonPressed = false;
        }
    }
}
