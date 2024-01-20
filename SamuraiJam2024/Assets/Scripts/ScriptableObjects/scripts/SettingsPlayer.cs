using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettingsObject")]
public class SettingsPlayer : ScriptableObject
{
    public float MouseSensitivity = 2.0f; 
}
