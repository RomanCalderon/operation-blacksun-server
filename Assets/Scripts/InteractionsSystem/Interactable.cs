using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; set; } = true;
    public bool IsInteracting { get => m_isInteracting; }
    public float MinRange { get => m_minRange; }
    public string AccessKey { get; set; }
    public float InteractTimer { get => m_interactTimer; }

    private const float m_minRange = 2.5f;
    private const float m_interactTimeThreshold = 0.5f;
    private bool m_isInteracting = false;
    private float m_interactTimer = 0f;

    #region Interface

    /// <summary>
    /// Called when Interactable becomes accessible.
    /// </summary>
    public abstract void StartHover ();

    /// <summary>
    /// Called when Interactable begins interaction.
    /// </summary>
    public virtual void StartInteract ( string accessKey = null )
    {
        if ( m_isInteracting )
        {
            return;
        }
        if ( !string.IsNullOrEmpty ( AccessKey ) && !accessKey.Equals ( AccessKey ) )
        {
            return;
        }
        m_isInteracting = true;
        m_interactTimer = m_interactTimeThreshold;
    }

    public virtual void StartInteract ( string [] accessKeys = null )
    {
        if ( m_isInteracting )
        {
            return;
        }
        if ( !string.IsNullOrEmpty ( AccessKey ) )
        {
            foreach ( string accessKey in accessKeys )
            {
                if ( accessKey.Equals ( AccessKey ) )
                {
                    m_isInteracting = true;
                    m_interactTimer = m_interactTimeThreshold;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Called when Interactable interaction timer is complete.
    /// </summary>
    protected abstract void OnInteract ();


    /// <summary>
    /// Called when Interactable interaction has ended or got interrupted.
    /// </summary>
    public virtual void StopInteract ()
    {
        m_isInteracting = false;
        m_interactTimer = 0f;
    }

    /// <summary>
    /// Called when Interactable becomes inaccessible.
    /// </summary>
    public abstract void EndHover ();

    #endregion

    #region Runtime

    private void Update ()
    {
        CheckInteractTime ();
    }

    private void CheckInteractTime ()
    {
        if ( m_isInteracting )
        {
            // Increment interact timer
            if ( m_interactTimer > 0f )
            {
                m_interactTimer -= Time.deltaTime;
                return;
            }
            // Interaction timer is complete
            OnInteract ();
            StopInteract ();
        }
    }

    #endregion
}
