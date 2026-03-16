using UnityEngine;

/// <summary>
/// RAJON — EnemyBullet
/// Düşman mermisi. Player'a çarparsa damage verir.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private float _damage = 10f;

    private Vector2 _direction = Vector2.left;

    public void SetDirection(Vector2 dir)
    {
        _direction = dir.normalized;
        
        // Merminin rotation'ını yöne çevir
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(_damage);

            Destroy(gameObject);
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}