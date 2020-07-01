using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New PlayerItem", menuName = "ScriptableObjects/PlayerItem", order = 1)]
public class PlayerItem : ScriptableObject
{
    public int Id;
    public string Name;
}


