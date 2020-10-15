using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;

public class b_RainController : MonoBehaviour
{

    private InputDevice controller;

    [SerializeField]
    private XRNode controllerNode = XRNode.LeftHand;

    private bool buttonPressed;
    private bool stormActive;

    private List<InputDevice> devices = new List<InputDevice>();
    private List<XRNodeState> mNodeStates = new List<XRNodeState>();

    private ParticleSystem rain;

    private int maxParticles;
    private int rainChangeAmount = 50;
    private int rainChangeDelay = 1;

    public GameObject boat, stormPointOne, stormPointTwo;

    // Start is called before the first frame update
    void Awake()
    {
        GetDevice();
        FindRainSystem();
    }

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(controllerNode, devices);
        controller = devices.FirstOrDefault();
    }

    private void FindRainSystem()
    {
        rain = GameObject.Find("Rain").GetComponent<ParticleSystem>();
        maxParticles = 2500;
    }

    private void FixedUpdate()
    {
        if (controller == null)
        {
            GetDevice();
        }
    }

    // Update is called once per frame
    void Update()
    {

        ControllerControls();

        //EnvironmentalControl();

    }

    private void ControllerControls()
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

    private void EnvironmentalControl()
    {
        if (transform.position.x == stormPointOne.transform.position.x)
        {
            SetWeatherState();
        }
        else if (transform.position.x == stormPointTwo.transform.position.x)
        {
            SetWeatherState();
        }
    }

    private void SetWeatherState()
    {
    //    if (stormActive == true)
    //    {
    //        stormActive = false;

    //        for (int i = 0; i < maxParticles; i + rainChangeAmount)
    //        {
    //            rain.main.emissionRate = rain.main.emissionRate + rainChangeAmount;

    //            float t;
    //            t = Time.time + rainChangeDelay;
    //            if (Time.time > t)
    //            {
    //                break;
    //            }
    //        }
    //    }
    //    else if (stormActive == false)
    //    {
    //        stormActive = true;

    //        for (int i = 0; i < maxParticles; i + rainChangeAmount)
    //        {
    //            rain.main.emissionRate = rain.main.emissionRate - rainChangeAmount;

    //            float t;
    //            t = Time.time + rainChangeDelay;
    //            if (Time.time > t)
    //            {
    //                break;
    //            }
    //        }
    //    }
    }
}
