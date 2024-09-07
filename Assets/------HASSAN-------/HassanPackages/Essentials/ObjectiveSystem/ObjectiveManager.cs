using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject objectivePanel; 
    [SerializeField] private TMP_Text objectiveTitle;
    [SerializeField] private TMP_Text objectiveText; 
    [SerializeField] private GameObject closeBtn; 
    [SerializeField] private CanvasGroup canvasGroup;
    [Header("Display Settings")]
    [SerializeField] private float displayTime = 5f;

    private readonly Queue<Objective> _objectivesQueue = new(); 
    private Coroutine _displayCoroutine; 
    private Coroutine _typingCoroutine; 

    [SerializeField] private float fadeDuration = 1f;

    public static Action<int> OnObjectiveClosed;

    private Objective _currentObjective; // Store the current objective
    private int _currentObjectiveId; // Store the ID of the current objective

    private void Start()
    {
        if (objectivePanel == null || objectiveTitle == null || objectiveText == null)
        {
            Debug.LogError("ObjectiveManager: UI elements are not assigned.");
        }

        objectivePanel.SetActive(false); // Hide the panel initially
    }

    public void ShowObjectiveImmediate(Objective objective)
    {
        if (_displayCoroutine != null)
        {
            StopCoroutine(_displayCoroutine);
            ResetObjectivePanel(); // Reset the panel if currently active
        }
        _currentObjective = objective;
        _currentObjectiveId = objective.id; // Update the current objective ID
        DisplayObjectiveImmediately(objective);
    }
    
    public void ShowObjective(Objective objective)
    {
        
        _objectivesQueue.Enqueue(objective);
        if (_displayCoroutine == null)
        {
            DisplayNextObjective();
        }
    }

    private void DisplayNextObjective()
    {
        if (_objectivesQueue.Count <= 0) return;
        var nextObjective = _objectivesQueue.Dequeue();
        _currentObjective = nextObjective;
        _currentObjectiveId = nextObjective.id; // Update the current objective ID
        DisplayObjectiveImmediately(nextObjective);
    }

    private void DisplayObjectiveImmediately(Objective objective)
    {
        objectivePanel.SetActive(true);
        closeBtn.SetActive(false);
        StartCoroutine(FadeIn(() =>
        {
            objectiveTitle.text = objective.title; 
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine); // Stop any ongoing typing effect
                _typingCoroutine = null;
            }
            _typingCoroutine = StartCoroutine(TypeText(objective.description, objective.typingSpeed));
        }));

        if (objective.canBeClosed)
        {
            FunctionTimer.CreateCountdownWithSlider(3f).OnComplete(() => { closeBtn.SetActive(true); });
        }
    }
    private IEnumerator TypeText(string text, float typingSpeed)
    {
        objectiveText.text = string.Empty;
        AudioManager.Instance.Play(SoundName.Typing);
        foreach (var letter in text.ToCharArray())
        {
            objectiveText.text += letter; 
            yield return new WaitForSeconds(typingSpeed); 
        }
        AudioManager.Instance.Stop(SoundName.Typing);
        _displayCoroutine = StartCoroutine(HideObjectiveAfterDelay());
    }

    private IEnumerator HideObjectiveAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        CloseCurrentObjective();
    }

    public void CloseCurrentObjective()
    {
        if (_displayCoroutine != null)
        {
            StopCoroutine(_displayCoroutine); // Stop the current display coroutine
            _displayCoroutine = null; // Clear the reference to the coroutine
        }
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine); // Stop the typing effect
            _typingCoroutine = null;
        }
        StartCoroutine(FadeOut(() =>
        {
            ResetObjectivePanel(); // Reset the panel after fading out
            DisplayNextObjective();
        }));
        OnObjectiveClosed?.Invoke(_currentObjectiveId); // Pass the ID of the current objective
    }

    private void ResetObjectivePanel()
    {
        objectiveText.text = string.Empty;
        canvasGroup.alpha = 0f; // Ensure it's fully transparent before showing again
        objectivePanel.SetActive(false); // Hide the panel
    }

    private IEnumerator FadeIn(Action onComplete)
    {
        var elapsedTime = 0f;
        canvasGroup.alpha = 0f; // Start with full transparency

        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f; // Ensure fully opaque
        onComplete?.Invoke();
    }

    private IEnumerator FadeOut(Action onComplete)
    {
        var elapsedTime = 0f;
        canvasGroup.alpha = 1f; // Start with full opacity

        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        onComplete?.Invoke();
        objectivePanel.SetActive(false); 
    }
}
