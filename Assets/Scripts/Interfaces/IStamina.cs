/// <summary>
/// Contract for anything that manages a spendable stamina pool.
/// Consumers (spells, sprint, dodge) depend on this instead of a concrete controller.
/// </summary>
public interface IStamina
{
    float Current { get; }
    float Ratio { get; }

    /// <summary>Attempt to spend stamina. Returns false if there isn't enough.</summary>
    bool TrySpend(float cost);

    /// <summary>Regenerate stamina at a custom rate (amount per second, scaled by deltaTime internally).</summary>
    void Regen(float amount);
}
