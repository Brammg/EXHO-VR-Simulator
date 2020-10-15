using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class b_HandHider : MonoBehaviour
{
    public GameObject handObject = null;

    private b_HandPhysics handPhysics = null;
    private XRDirectInteractor interactor = null;

    private void Awake()
    {
        handPhysics = handObject.GetComponent<b_HandPhysics>();
        interactor = GetComponent<XRDirectInteractor>();
    }

    private void OnEnable()
    {
        interactor.onSelectEnter.AddListener(Hide);
        interactor.onSelectExit.AddListener(Show);
    }

    private void OnDisable()
    {
        interactor.onSelectEnter.RemoveListener(Hide);
        interactor.onSelectExit.RemoveListener(Show);
    }

    private void Show(XRBaseInteractable interactable)
    {
        handPhysics.TeleportToTarget();
        handObject.SetActive(true);
    }

    private void Hide(XRBaseInteractable interactable)
    {
        handObject.SetActive(false);
    }
}
