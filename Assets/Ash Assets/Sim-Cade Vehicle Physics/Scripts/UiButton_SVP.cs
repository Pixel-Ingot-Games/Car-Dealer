using UnityEngine;
using UnityEngine.EventSystems;

namespace Ashsvp
{
    public class UiButton_SVP : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool isPressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
        }
    }

}
