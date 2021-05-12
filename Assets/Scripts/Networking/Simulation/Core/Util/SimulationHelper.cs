using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SimulationHelper : MonoBehaviour
{
    private static List<SimulationObject> SimulationObjects = null;

    void Awake ()
    {
        SimulationObjects = new List<SimulationObject> ();
    }

    public static void AddSimulationObject ( SimulationObject simulationObject )
    {
        SimulationObjects.Add ( simulationObject );
    }

    public static void RemoveSimulationObject ( SimulationObject simulationObject )
    {
        SimulationObjects.Remove ( simulationObject );
    }

    public static void AddState ( uint serverTick )
    {
        SimulationObjects.ForEach ( simulationObject => simulationObject.AddState ( serverTick ) );
    }

    public static void Simulate ( uint serverTick, uint frameId, Action action, float clientSubFrame )
    {
        for ( int i = 0; i < SimulationObjects.Count; i++ )
        {
            SimulationObjects [ i ].SetStateTransform ( serverTick, frameId, clientSubFrame );
        }

        action?.Invoke ();

        for ( int i = 0; i < SimulationObjects.Count; i++ )
        {
            SimulationObjects [ i ].ResetStateTransform ();
        }
    }
}
