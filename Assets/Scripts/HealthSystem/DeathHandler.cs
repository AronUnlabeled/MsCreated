using UnityEngine;

/// <summary>
/// 
/// Handles character death events by subscribing to the health component's death action.
/// 
/// </summary>
[RequireComponent(typeof(IHealth))]
public class DeathHandler : MonoBehaviour {

    protected IHealth health;

    protected virtual void Awake() => health = GetComponent<IHealth>(); // grab local compononet
    protected virtual void OnEnable() => health.OnDeath += HandleDeath; //subscribe to action
    protected virtual void OnDisable() => health.OnDeath -= HandleDeath; // unsubscribe to action
    protected virtual void HandleDeath() {

        Debug.Log($"I've died and I'm a dummy! {gameObject.name}");
        Destroy(gameObject); 
    }
}