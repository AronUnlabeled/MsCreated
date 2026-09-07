using System.Collections;
using UnityEngine;

public class LockOnTurret : MonoBehaviour, IDamageable {
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Detection & Aiming")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform turretPivot;
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Vector3 aimOffset = new(0f, 1f, 0f);
    [SerializeField] private LayerMask lineOfSightLayers;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float lockOnDelay = 1.5f;
    [SerializeField] private float timeBetweenBursts = 3f;
    [SerializeField] private float rapidFireInterval = 0.15f;

    [Header("Visual Effects")]
    [SerializeField] private LineRenderer laserVisual;
    [SerializeField] private Color laserLockingColor = new(1f, 0f, 0f, 0.1f);
    [SerializeField] private Color laserReadyColor = new(1f, 0f, 0f, 1f);

    private bool isTargeting = false;
    private bool isFiringBurst = false;
    private bool isLockingOn = false;

    private Vector3 TargetAimPosition => playerTransform.position + aimOffset;

    private void Start() {

        currentHealth = maxHealth;

        if (playerTransform == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
        if (laserVisual != null) laserVisual.enabled = false;
    }
    private void Update() {

        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius && HasLineOfSight()) {
            isTargeting = true;
            RotateTowardsPlayer();

            if (!isFiringBurst) {
                StartCoroutine(FireBurstSequence());
            }
        } else {
            isTargeting = false;
        }
    }
    private void LateUpdate() {

        if (isLockingOn && playerTransform != null) DrawLaserBeam();
    }
    private bool HasLineOfSight() {
        Vector3 directionToPlayer = (playerTransform.position - firePoint.position).normalized;
        float distanceToPlayer = Vector3.Distance(firePoint.position, playerTransform.position);

        if (Physics.Raycast(firePoint.position, directionToPlayer, out RaycastHit hit, distanceToPlayer, lineOfSightLayers)) {
            if (hit.transform == playerTransform) {
                return true;
            }
        }
        return false;
    }
    private void RotateTowardsPlayer() {
        Vector3 targetDirection = playerTransform.position - turretPivot.position;
        targetDirection.y = 0;

        if (targetDirection != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            turretPivot.rotation = Quaternion.Slerp(turretPivot.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    private void DrawLaserBeam() {
        if (laserVisual == null) return;

        laserVisual.enabled = true;
        laserVisual.SetPosition(0, firePoint.position);

        Vector3 directionToPlayer = (playerTransform.position - firePoint.position).normalized;

        if (Physics.Raycast(firePoint.position, directionToPlayer, out RaycastHit hit, detectionRadius, lineOfSightLayers)) {
            laserVisual.SetPosition(1, hit.point);
        } else {
            laserVisual.SetPosition(1, firePoint.position + directionToPlayer * detectionRadius);
        }
    }
    private IEnumerator FireBurstSequence() {
        isFiringBurst = true;
        isLockingOn = true;

        float elapsed = 0f;
        while (elapsed < lockOnDelay) {
            elapsed += Time.deltaTime;

            if (laserVisual != null) {
                Color currentColor = Color.Lerp(laserLockingColor, laserReadyColor, elapsed / lockOnDelay);
                laserVisual.startColor = currentColor;
                laserVisual.endColor = currentColor;
            }
            yield return null;
        }
        isLockingOn = false;
        if (laserVisual != null) laserVisual.enabled = false;

        if (isTargeting && HasLineOfSight()) {
            Vector3 targetPosition = playerTransform.position;

            for (int i = 0; i < 6; i++) {
                FireProjectile(targetPosition);
                yield return new WaitForSeconds(rapidFireInterval);
            }
        }

        yield return new WaitForSeconds(timeBetweenBursts);
        isFiringBurst = false;
    }
    private void FireProjectile(Vector3 targetPos) {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        if (projectile.TryGetComponent<TurretProjectile>(out var instance)) {
            Vector3 direction = (targetPos - firePoint.position).normalized;
            instance.Setup(direction);
        }
    }
    public void OnDamage(float amount) {
        float finalDamage = amount * 0.5f;
        currentHealth -= finalDamage;

        if (currentHealth <= 0) DestroyTurret();
    }
    private void DestroyTurret() {
        if (laserVisual != null) laserVisual.enabled = false;
        Destroy(gameObject);
    }
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}