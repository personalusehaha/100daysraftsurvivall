namespace RaftSurvival.Combat
{
    /// <summary>
    /// Implemented by anything that can take damage: player, island
    /// creatures, pirates, bosses. Keeps combat code decoupled from
    /// specific entity types.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount, UnityEngine.Vector3 hitPoint, UnityEngine.Vector3 hitDirection);
        bool IsAlive { get; }
    }
}
