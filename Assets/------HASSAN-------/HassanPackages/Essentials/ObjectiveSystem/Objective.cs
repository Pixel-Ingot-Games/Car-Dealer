using UnityEngine;

[CreateAssetMenu(fileName = "New Objective", menuName = "Hassan/Objective")]
public class Objective : ScriptableObject
{
    [Header("Objective Details")] 
    public int id;
    public string title;
    public string description;
    public bool canBeClosed;
    public float typingSpeed = 0.05f;
}