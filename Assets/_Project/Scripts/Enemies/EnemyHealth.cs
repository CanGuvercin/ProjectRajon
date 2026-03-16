using System;
using UnityEngine;

/// <summary>
/// RAJON — EnemyHealth
/// Düşman HP sistemi.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private float _maxHP = 30f;

    private float _currentHP;

    public event Action<float> OnDamaged;  // damage amount
    public event Action<float, float> OnHealthChanged;  // current, max
    public event Action OnDeath;

    public float Current => _currentHP;
    public float Max => _maxHP;
    public float Ratio => _currentHP / _maxHP;
    public bool IsDead => _currentHP <= 0f;

    private void Awake()
    {
        _currentHP = _maxHP;
    }

   public void TakeDamage(float amount)
{
    if (IsDead) return;

    _currentHP = Mathf.Max(0f, _currentHP - amount);//
    OnDamaged?.Invoke(amount);
    OnHealthChanged?.Invoke(_currentHP, _maxHP);

    if (_currentHP <= 0f)
    {
        OnDeath?.Invoke();
    }
}
}