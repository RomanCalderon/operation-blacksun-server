using System.Collections.Generic;
using UnityEngine;

public class SimulationObject : MonoBehaviour
{
    private Dictionary<uint, SimulationState> m_stateData = new Dictionary<uint, SimulationState> ();
    private List<uint> m_stateKeys = new List<uint> ();
    private SimulationState m_savedStateData = new SimulationState ();

    // Start is called before the first frame update
    void Start ()
    {
        SimulationHelper.AddSimulationObject ( this );
    }

    private void OnDestroy ()
    {
        SimulationHelper.RemoveSimulationObject ( this );
    }

    public void AddState ( uint serverTick )
    {
        if ( m_stateKeys.Count >= ServerSimulation.STATE_CACHE_SIZE )
        {
            uint key = m_stateKeys [ 0 ];
            m_stateKeys.RemoveAt ( 0 );
            m_stateData.Remove ( key );
        }

        m_stateData.Add ( serverTick, new SimulationState ()
        {
            Position = transform.position,
            Rotation = transform.rotation
        } );
        m_stateKeys.Add ( serverTick );
    }

    public void SetStateTransform ( uint serverTick, uint frameId, float clientSubFrame )
    {
        m_savedStateData.Position = transform.position;
        m_savedStateData.Rotation = transform.rotation;

        if ( frameId == serverTick )
        {
            Debug.Log ( $"frameId == ServerSimulation.Tick ({frameId} = {serverTick})" );
            frameId = m_stateKeys [ m_stateKeys.Count - 1 ];
        }

        if ( m_stateData.ContainsKey ( frameId - 1 ) )
        {
            transform.position = Vector3.Lerp ( m_stateData [ frameId - 1 ].Position, m_stateData [ frameId ].Position, clientSubFrame );
            transform.rotation = m_stateData [ frameId - 1 ].Rotation;
        }
    }

    public void ResetStateTransform ()
    {
        transform.position = m_savedStateData.Position;
        transform.rotation = m_savedStateData.Rotation;
    }
}
