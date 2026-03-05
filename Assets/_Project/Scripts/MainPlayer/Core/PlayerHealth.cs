using System;
using UnityEngine;

/// <summary>
/// RAJON — PlayerHealth
/// Sadece HP hesabı. Ölüm kararını PlayerController'a bildirir.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Ayarlar
    // -------------------------------------------------------------------------
    [Header("HP")]
    [SerializeField] private float _maxHP = 100f;

    // -------------------------------------------------------------------------
    // İç Durum
    // -------------------------------------------------------------------------
    private float _currentHP;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action               OnDeath;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        _currentHP = _maxHP;
    }

    // -------------------------------------------------------------------------
    // Hasar & Heal
    // -------------------------------------------------------------------------
    public void TakeDamage(float amount)
    {
        if (_currentHP <= 0f) return;

        _currentHP = Mathf.Max(_currentHP - amount, 0f);
        OnHealthChanged?.Invoke(_currentHP, _maxHP);

        if (_currentHP <= 0f)
            OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (_currentHP <= 0f) return;

        _currentHP = Mathf.Min(_currentHP + amount, _maxHP);
        OnHealthChanged?.Invoke(_currentHP, _maxHP);
    }

    // -------------------------------------------------------------------------
    // Upgrade — nargile noktasından
    // -------------------------------------------------------------------------
    public void UpgradeMaxHP(float bonus)
    {
        _maxHP     += bonus;
        _currentHP += bonus;
        OnHealthChanged?.Invoke(_currentHP, _maxHP);
    }

    // -------------------------------------------------------------------------
    // Dışarıdan okunabilir
    // -------------------------------------------------------------------------
    public float Current => _currentHP;
    public float Max     => _maxHP;
    public float Ratio   => _currentHP / _maxHP;
    public bool  IsAlive => _currentHP > 0f;
}