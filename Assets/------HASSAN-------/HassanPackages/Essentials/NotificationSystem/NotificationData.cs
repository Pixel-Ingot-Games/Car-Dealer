using UnityEngine;

[CreateAssetMenu(fileName = "NewNotification", menuName = "Hassan/Notification")]
public class NotificationData : ScriptableObject
{
    public string message;
    public float displayTime = 5f; // Time to display the notification
    public bool canDismiss = true; // Can the notification be dismissed manually?
}
