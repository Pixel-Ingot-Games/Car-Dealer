using UnityEngine;

[CreateAssetMenu(fileName = "New Objective1", menuName = "Hassan/Objective1")]
public class Objective1 : ScriptableObject
{
    [Header("Objective Details")] 
    public int id;
    public string title;
    public string description;
    public bool canBeClosed;
    public float typingSpeed = 0.05f;
}