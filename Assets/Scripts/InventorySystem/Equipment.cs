using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A PlayerItem which defines the properties of a piece of equipment.
/// </summary>
[CreateAssetMenu(fileName = "New Equipment", menuName = "ScriptableObjects/PlayerItems/Equipment", order = 4)]
public class Equipment : PlayerItem
{
    public enum EquipmentTypes
    {
        /// <summary>
        /// An object which is thrown. Examples include frag grenades, gas grenades, etc.
        /// </summary>
        Throwable,
        /// <summary>
        /// A hand-tossed object. Examples include C4, smoke canisters, etc.
        /// </summary>
        Tossable,
        /// <summary>
        /// An object used for viewing. Examples include binoculars, rangefinders, etc.
        /// </summary>
        Viewing
    }

    /// <summary>
    /// The type of equipment.
    /// </summary>
    public EquipmentTypes EquipmentType;
}