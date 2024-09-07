using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public GameObject notificationGO; 

    private Queue<NotificationData> notificationQueue = new();
    private GameObject currentNotification;
    private bool isNotificationActive = false;

    public TMP_Text notificationMsg;

    private void Start()
    {
        if (notificationGO == null)
        {
            Debug.LogError("Notification Prefab is not assigned.");
        }

        // Ensure notificationGO is inactive initially
        notificationGO.SetActive(false);
    }

    public void EnqueueNotification(NotificationData notificationData)
    {
        notificationQueue.Enqueue(notificationData);
        if (!isNotificationActive)
        {
            ShowNextNotification();
        }
    }

    private void ShowNextNotification()
    {
        if (notificationQueue.Count == 0) return;

        isNotificationActive = true;
        NotificationData data = notificationQueue.Dequeue();

        // Reuse the existing notification GameObject
        if (currentNotification != null)
        {
            currentNotification.SetActive(false);
        }

        currentNotification = notificationGO;
        currentNotification.SetActive(true);

      
         notificationMsg.text = data.message;
        

        RectTransform rectTransform = currentNotification.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, Screen.height / 2); // Start from the center

        // Animation
        rectTransform.DOAnchorPos(new Vector2(0, Screen.height / 2 - 360), 0.5f).SetEase(Ease.OutSine);

        // Auto-dismissing
        if (data.displayTime > 0)
        {
            DOVirtual.DelayedCall(data.displayTime, () => DismissNotification());
        }

        // Dismiss button setup
        var dismissButton = currentNotification.GetComponentInChildren<UnityEngine.UI.Button>();
        if (dismissButton != null)
        {
            dismissButton.onClick.RemoveAllListeners(); // Clear previous listeners
            dismissButton.onClick.AddListener(DismissNotification);
        }
    }

    private void DismissNotification()
    {
        if (currentNotification != null)
        {
            RectTransform rectTransform = currentNotification.GetComponent<RectTransform>();
            rectTransform.DOAnchorPos(new Vector2(0, Screen.height / 2 + 100), 0.5f).SetEase(Ease.InSine).OnComplete(() =>
            {
                currentNotification.SetActive(false);
                isNotificationActive = false;
                ShowNextNotification();
            });
        }
    }
}
