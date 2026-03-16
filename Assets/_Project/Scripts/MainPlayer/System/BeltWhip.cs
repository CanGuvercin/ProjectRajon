using UnityEngine;

public class BeltWhip : MonoBehaviour
{
    [SerializeField] private float _damage = 15f;
    [SerializeField] private LayerMask _enemyLayer;
    
    private bool _hasHit = false;

    public void SetDamage(float damage)
    {
        _damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;
        
        if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
        {
            EnemyHealth health = other.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(_damage);
                _hasHit = true;
            }
        }
    }
}