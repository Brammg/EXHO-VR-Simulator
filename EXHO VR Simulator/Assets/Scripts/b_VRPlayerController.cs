
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;


public class b_VRPlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 10.0f;

    [SerializeField]
    private float jumpForce = 250.0f;

    [SerializeField]
    private XRNode controllerNode = XRNode.LeftHand;

    [SerializeField]
    private bool checkForGroundOnJump = true;

    [Header("Capsule Collider Options")]
    [SerializeField]
    private Vector3 capsuleCenter = new Vector3(0, 1, 0);

    [SerializeField]
    private float capsuleRadius = 0.3f;

    [SerializeField]
    private float capsuleHeight = 1.6f;

    [SerializeField]
    private CapsuleDirection capsuleDirection = CapsuleDirection.YAxis;

    private InputDevice controller;

    private bool isGrounded;

    private bool buttonPressed;

    private bool isSeated = false;

    private Rigidbody rigidBodyComponent;

    private CapsuleCollider capsuleCollider;

    private List<InputDevice> devices = new List<InputDevice>();
    private List<XRNodeState> mNodeStates = new List<XRNodeState>();

    private Vector3 mHeadPos;

    private Quaternion mHeadRot;

    public GameObject bHMD, bLHand, bRHand, steerWheelPos, roamFreePos;

    public enum CapsuleDirection
    {
        XAxis,
        YAxis,
        ZAxis
    }

    void OnEnable()
    {
        rigidBodyComponent = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        rigidBodyComponent.constraints = RigidbodyConstraints.FreezeRotation;
        capsuleCollider.direction = (int)capsuleDirection;
        capsuleCollider.radius = capsuleRadius;
        capsuleCollider.center = capsuleCenter;
        capsuleCollider.height = capsuleHeight;
    }

    void Start()
    {
        GetDevice();
    }

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(controllerNode, devices);
        controller = devices.FirstOrDefault();
    }

    void FixedUpdate()
    {
        if (controller == null)
        {
            GetDevice();
        }

        if (!isSeated)
        {
            UpdateMovement();
            UpdateJump(controller);
        }

        EnterOrExitChair();
    }

    private void UpdateMovement()
    {
        Vector2 primary2dValue;

        InputFeatureUsage<Vector2> primary2DVector = CommonUsages.primary2DAxis;

        if (controller.TryGetFeatureValue(primary2DVector, out primary2dValue) && primary2dValue != Vector2.zero)
        {
            var xAxis = primary2dValue.x * speed * Time.deltaTime;
            var zAxis = primary2dValue.y * speed * Time.deltaTime;

            Vector3 right = bHMD.transform.TransformDirection(Vector3.right);
            Vector3 forward = bHMD.transform.TransformDirection(Vector3.forward);

            transform.position += right * xAxis;
            transform.position += forward * zAxis;
        }
    }

    private void UpdateJump(InputDevice controller)
    {
        isGrounded = (Physics.Raycast((new Vector2(transform.position.x, transform.position.y + 2.0f)), Vector3.down, 5.0f));

        Debug.DrawRay((new Vector3(transform.position.x, transform.position.y, transform.position.z)), Vector3.down, Color.red, 1.0f);

        if (!isGrounded && checkForGroundOnJump)
        {
            return;
        }

        bool buttonValue;

        if (controller.TryGetFeatureValue(CommonUsages.primaryButton, out buttonValue) && buttonValue)
        {
            if (!buttonPressed)
            {
                buttonPressed = true;
                rigidBodyComponent.AddForce(Vector3.up * jumpForce);
            }
        }
        else if (buttonPressed)
        {
            buttonPressed = false;
        }
    }

    private void EnterOrExitChair()
    {
        //bool buttonValue;

        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            if (!isSeated)
            {
                isSeated = true;
                gameObject.transform.position = steerWheelPos.transform.position;
                //gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            }
            else if (isSeated)
            {
                isSeated = false;
                gameObject.transform.position = roamFreePos.transform.position;
            }
        }
    }
}