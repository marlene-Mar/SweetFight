using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Data")]

public class Data : ScriptableObject
{
    public int playerHealth;
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;
    public int playerInventory;
    public int playerGuardians;

}
