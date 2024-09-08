using UnityEngine;
using UnityEngine.UI;

public class CarInteraction : MonoBehaviour
{
    public GameObject fpsController;
    public GameObject fpsUI;
    public GameObject rccCarUI;
    public GameObject rccCarCamera;
    public GameObject enterCarUI, exitCarUI;
    public Transform exitPosition;
    public float interactionDistance = 3.0f;
    public RCC_CarControllerV3 carController;
    public LayerMask carLayer; // Layer mask to specify which layers are considered "car" layers
    private bool isNearCar = false;
    private bool isInCar = false;

    private void Awake()
    {
        enterCarUI.GetComponent<Button>().onClick.AddListener(EnterCar);
        exitCarUI.GetComponent<Button>().onClick.AddListener(ExitCar);
    }

    private void OnDisable()
    {
        enterCarUI.GetComponent<Button>().onClick.RemoveAllListeners();
        exitCarUI.GetComponent<Button>().onClick.RemoveAllListeners();
    }

    void Update()
    {
        RaycastHit hit;
        Vector3 direction = fpsController.transform.forward; // Raycast direction
        Vector3 origin = fpsController.transform.position; // Raycast origin

        // Perform the raycast
        if (Physics.Raycast(origin, direction, out hit, interactionDistance, carLayer))
        {
            // Draw the ray in the Scene view for debugging
            Debug.DrawRay(origin, direction * hit.distance, Color.green);
            if (hit.collider.GetComponent<RCC_CarControllerV3>() == carController && !isInCar) // Check if the raycast hit this car
            {
                enterCarUI.SetActive(true);
                isNearCar = true;
            }
            else
            {
                isNearCar = false;
            }
        }
        else
        {
            isNearCar = false;

            // Draw the ray in the Scene view for debugging
            Debug.DrawRay(origin, direction * interactionDistance, Color.red);
            enterCarUI.SetActive(false);
        }

        // Optionally, display a prompt when the player is near the car
        if (isNearCar)
        {
            Debug.Log("Press Enter to enter the car");
        }

        // Update exitCarUI visibility based on carController speed
        if (isInCar)
        {
            exitCarUI.SetActive(carController.speed < 5); // Show exitCarUI only if speed > 5
        }
    }

    public void EnterCar()
    {
        fpsController.SetActive(false);
        fpsUI.SetActive(false);
        rccCarUI.SetActive(true);
        rccCarCamera.SetActive(true);
        enterCarUI.SetActive(false);
        isInCar = true;
        // Set player position to the car's exit position
        fpsController.transform.position = exitPosition.position;
    }

    public void ExitCar()
    {
        fpsController.SetActive(true);
        fpsUI.SetActive(true);
        rccCarUI.SetActive(false);
        rccCarCamera.SetActive(false);
        exitCarUI.SetActive(false);
        enterCarUI.SetActive(false);
        isInCar = false;
    }
}