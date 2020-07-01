using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "ScriptableObjects/PlayerItems/Equipment", order = 4)]
public class Equipment : PlayerItem
{
    public enum EquipmentTypes
    {
        Grenade,    // frag, smoke, etc
        Optic       // binoculars, rangefinder, etc
    }

    public EquipmentTypes EquipmentType;
}