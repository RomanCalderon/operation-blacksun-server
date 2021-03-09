public class Constants
{
    #region Timestep

    public const int TICKS_PER_SECOND = 60;
    public const int MS_PER_TICK = 1000 / TICKS_PER_SECOND;

    #endregion

    #region Input

    public const int NUM_PLAYER_INPUTS = 8;

    #endregion

    #region Inventory

    public const int INVENTORY_RIG_SIZE = 6;
    public const int INVENTORY_BACKPACK_SIZE = 10;
    public const int SLOT_MAX_STACK_SIZE = 256;

    #endregion

    #region Player

    // Respawn
    public const float PLAYER_RESPAWN_DELAY = 3.0f;

    #endregion
}
