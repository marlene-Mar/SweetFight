using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Line
{
    public string speakerName;
    [TextArea(2,3)]
    public string dialogueLine;
}
[CreateAssetMenu(fileName = "Dialogos", menuName = "ScriptableObjects/Dialogos")]

public class Dialogos : ScriptableObject
{
    public List<Line> conversationLines;
}
