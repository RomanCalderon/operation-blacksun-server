using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// The base class for constructing a PlayerItem ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "New PlayerItem", menuName = "ScriptableObjects/PlayerItem", order = 1)]
public class PlayerItem : ScriptableObject
{
    /// <summary>
    /// A unique identifier for this PlayerItem.
    /// </summary>
    public int Id;
    /// <summary>
    /// The name of this PlayerItem.
    /// </summary>
    public string Name;
}


