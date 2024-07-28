using UnityEngine;

namespace Ashsvp
{
    public class CarSwitcher : MonoBehaviour
    {
        public GameObject[] cars; // Array to hold references to your different car GameObjects.
        private int currentCarIndex = 0; // Index of the currently active car.

        private void Start()
        {
            // Disable all cars except the first one.
            for (int i = 1; i < cars.Length; i++)
            {
                cars[i].SetActive(false);
            }
        }


        public void SwitchToCar(int newIndex)
        {
            // Disable the current car.
            cars[currentCarIndex].SetActive(false);

            // Enable the new car.
            cars[newIndex].SetActive(true);

            // Update the current car index.
            currentCarIndex = newIndex;
        }
    }
}
