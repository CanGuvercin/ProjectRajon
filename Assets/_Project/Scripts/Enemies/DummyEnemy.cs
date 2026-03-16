using System.Collections;
using UnityEngine;

/// <summary>
/// RAJON — DummyEnemy
/// Test için basit düşman. Emmi yaklaşınca ateş eder.
/// </summary>
public class DummyEnemy : MonoBehaviour
{
    [Header("Algılama")]
    [SerializeField] private float _detectRadius = 10f;
    [SerializeField] private Transform _target;  // Emmi

    [Header("Ateş")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private int _burstCount = 3;
    [SerializeField] private float _burstInterval = 0.5f;
    [SerializeField] private float _cooldown = 2f;

    [Header("Feedback")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _flashDuration = 0.2f;

    private EnemyHealth _health;
    private bool _isShooting = false;
    private Color _originalColor;

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;

        // Emmi'yi bul
        if (_target == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
                _target = player.transform;
        }
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnDamaged -= OnDamaged;
    }

    private void Update()
    {
        if (_target == null || _isShooting) return;
        if (_health != null && _health.IsDead) return;

        float dist = Vector2.Distance(transform.position, _target.position);
        if (dist <= _detectRadius)
        {
            StartCoroutine(BurstFire());
        }
    }

    private IEnumerator BurstFire()
    {
        _isShooting = true;

        for (int i = 0; i < _burstCount; i++)
        {
            FireBullet();
            yield return new WaitForSeconds(_burstInterval);
        }

        yield return new WaitForSeconds(_cooldown);
        _isShooting = false;
    }

    private void FireBullet()
    {
        if (_bulletPrefab == null || _muzzlePoint == null || _target == null) return;

        GameObject bullet = Instantiate(_bulletPrefab, _muzzlePoint.position, Quaternion.identity);
        EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();

        if (enemyBullet != null)
        {
            // Emmi'nin üst vücuduna (biraz yukarı offset)
            Vector2 targetPos = (Vector2)_target.position + Vector2.up * 0.8f;
            Vector2 direction = (targetPos - (Vector2)_muzzlePoint.position).normalized;
            enemyBullet.SetDirection(direction);
        }
    }

    private void OnDamaged(float damage)
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (_spriteRenderer == null) yield break;

        float elapsed = 0f;
        bool toggle = false;

        while (elapsed < _flashDuration)
        {
            _spriteRenderer.color = toggle ? Color.white : Color.red;
            toggle = !toggle;
            yield return new WaitForSeconds(0.05f);
            elapsed += 0.05f;
        }

        _spriteRenderer.color = _originalColor;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectRadius);
    }
}