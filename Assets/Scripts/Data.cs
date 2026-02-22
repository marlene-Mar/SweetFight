using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Data")]

public class Data : ScriptableObject
{
    [SerializeField]
    private int playerHealth;
    private float playerPositionX;
    private float playerPositionY;
    private float playerPositionZ;
    private int playerInventory;
    private int playerGuardians;

}
