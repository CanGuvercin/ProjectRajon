using UnityEngine;

/// <summary>
/// RAJON — MeleeHitbox
/// Animation Event ile tetiklenen melee damage sistemi.
/// </summary>
public class MeleeHitbox : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private Transform _hitPoint;
    [SerializeField] private float _hitRadius = 0.8f;
    [SerializeField] private LayerMask _enemyLayer;
    
    [Header("Damage")]
    [SerializeField] private float _lightDamage = 10f;
    [SerializeField] private float _heavyDamage = 25f;
    [SerializeField] private float _walkingLightDamage = 5f;

    private readonly Collider2D[] _hitBuffer = new Collider2D[8];

    // Animation Event'ten çağrılır
    public void DealLightDamage()
    {
        DealDamage(_lightDamage);
    }

    public void DealHeavyDamage()
    {
        DealDamage(_heavyDamage);
    }

    public void DealWalkingLightDamage()
    {
        DealDamage(_walkingLightDamage);
    }

    private void DealDamage(float damage)
    {
        // Emmi'nin baktığı yöne göre hitpoint offset
        Vector2 hitPos = GetHitPosition();
        
        int count = Physics2D.OverlapCircleNonAlloc(hitPos, _hitRadius, _hitBuffer, _enemyLayer);

        for (int i = 0; i < count; i++)
        {
            EnemyHealth health = _hitBuffer[i].GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    private Vector2 GetHitPosition()
    {
        if (_hitPoint != null)
            return _hitPoint.position;

        // Hitpoint yoksa Emmi'nin önüne bak
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        return (Vector2)transform.position + Vector2.right * direction * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 hitPos = _hitPoint != null ? _hitPoint.position : (Vector2)transform.position;
        Gizmos.DrawWireSphere(hitPos, _hitRadius);
    }
}