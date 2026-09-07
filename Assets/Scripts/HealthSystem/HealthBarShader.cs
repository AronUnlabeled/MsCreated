using UnityEngine;

/// <summary>
/// 
/// Manages the health bar shader effect based on the health of a target GameObject.
/// 
/// </summary>
/// 
/// <remarks>
/// 
/// Updates the shader property to reflect the current health percentage, smoothing the
/// transition.
/// 
/// </remarks>
public class HealthBarShader : MonoBehaviour {

    [Header("Reference")]
    [SerializeField] private GameObject target;

    [Header("Material Settings")]
    [SerializeField] private new Renderer renderer;
    [SerializeField] private string shaderPropertyName = "_BarPercent";
    [SerializeField] private float smoothSpeed = 5f;

    private IHealth healthSystem;
    private MaterialPropertyBlock _propertyBlock;
    private int _propertyID;
    [SerializeField] private float targetPercent = 1f;
    [SerializeField] private float currentPercent = 1f;

    private void Awake() {
        
        _propertyID = Shader.PropertyToID(shaderPropertyName);
        _propertyBlock = new MaterialPropertyBlock();

        if (renderer == null) renderer = GetComponent<Renderer>();
        if (target == null) {
            Debug.LogError($" Target is missing HealthSystem {gameObject.name}");
            return;
        }

        if (target.TryGetComponent<IHealth>(out var health)) healthSystem = health;
    }
    private void OnEnable() {
        healthSystem.OnHealthChanged += UpdateHealthBar;

        targetPercent = healthSystem.CurrentHealth / healthSystem.MaxHealth;
        currentPercent = targetPercent;
        ApplyShaderChange(currentPercent);
    }


    private void OnDisable() {
        if (healthSystem != null) healthSystem.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth) {

        if (maxHealth <= 0) return;
        
        targetPercent = currentHealth / maxHealth;

        Debug.Log($"[Health bar] Recieved Event! HP : {currentHealth}/{maxHealth}. Target Percent: {targetPercent}");
    }
    private void Update() {

        currentPercent = Mathf.MoveTowards(currentPercent, targetPercent, smoothSpeed * Time.deltaTime);
        
        ApplyShaderChange(currentPercent);
    }
    private void ApplyShaderChange(float value) {

        renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(_propertyID, value);
        renderer.SetPropertyBlock(_propertyBlock);
    }
}