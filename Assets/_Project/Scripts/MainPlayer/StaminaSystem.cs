using System;
using UnityEngine;

/// <summary>
/// RAJON — StaminaSystem
/// Sadece stamina hesabı ve tüketim yönetimi.
/// PlayerController hangi sistemin stamina harcadığını bildirir.
/// Emmi 60 yaşında — regen kasıtlı olarak yavaş.
/// </summary>
public class StaminaSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Ayarlar
    // -------------------------------------------------------------------------
    [Header("Stamina Değerleri")]
    [SerializeField] private float _maxStamina    = 100f;
    [SerializeField] private float _regenRate     = 8f;   // saniyede dolacak miktar (kasıtlı yavaş)
    [SerializeField] private float _regenDelay    = 1.2f; // son harcamadan sonra regenin başlaması için bekleme

    [Header("Tüketim Miktarları")]
    [SerializeField] private float _runCostPerSecond  = 18f;
    [SerializeField] private float _kickCost          = 22f;
    [SerializeField] private float _beltLightCost     = 14f;
    [SerializeField] private float _beltHeavyRelease  = 40f; // şarj bırakılınca

    // -------------------------------------------------------------------------
    // İç Durum
    // -------------------------------------------------------------------------
    private float _currentStamina;
    private float _regenDelayTimer;
    private bool  _isRunConsuming;
    private bool  _isBeltLightConsuming;

    // -------------------------------------------------------------------------
    // Event — dışarıya bildirim (UI vs. dinleyebilir)
    // -------------------------------------------------------------------------
    public event Action<float, float> OnStaminaChanged; // current, max
    public event Action               OnStaminaEmpty;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        _currentStamina = _maxStamina;
    }

    private void Update()
    {
        ConsumePerSecond();
        HandleRegen();
    }

    // -------------------------------------------------------------------------
    // Sürekli Tüketim (Run, Belt Light) — her frame çalışır
    // -------------------------------------------------------------------------
    private void ConsumePerSecond()
    {
        if (_isRunConsuming)
            Consume(_runCostPerSecond * Time.deltaTime);

        if (_isBeltLightConsuming)
            Consume(_beltLightCost * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Regen
    // -------------------------------------------------------------------------
    private void HandleRegen()
    {
        if (_isRunConsuming || _isBeltLightConsuming)
        {
            _regenDelayTimer = _regenDelay;
            return;
        }

        if (_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= Time.deltaTime;
            return;
        }

        if (_currentStamina < _maxStamina)
        {
            _currentStamina = Mathf.Min(_currentStamina + _regenRate * Time.deltaTime, _maxStamina);
            OnStaminaChanged?.Invoke(_currentStamina, _maxStamina);
        }
    }

    // -------------------------------------------------------------------------
    // PlayerController buradan hangi sistemin tükettiğini bildirir
    // -------------------------------------------------------------------------
    public void SetConsuming(StaminaConsumer consumer, bool active)
    {
        switch (consumer)
        {
            case StaminaConsumer.Run:
                _isRunConsuming = active;
                break;
            case StaminaConsumer.BeltLight:
                _isBeltLightConsuming = active;
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Tek Seferlik Tüketim (Kick, Belt Heavy Release)
    // -------------------------------------------------------------------------
    public bool TryConsume(StaminaConsumer consumer)
    {
        float cost = GetCost(consumer);
        if (_currentStamina < cost) return false;
        Consume(cost);
        return true;
    }

    // -------------------------------------------------------------------------
    // Upgrade Sistemi — nargile noktasından çağrılır
    // -------------------------------------------------------------------------
    public void UpgradeRegenRate(float bonusMultiplier)
    {
        _regenRate *= (1f + bonusMultiplier);
    }

    // -------------------------------------------------------------------------
    // Dışarıdan okunabilir
    // -------------------------------------------------------------------------
    public bool  HasStamina()                  => _currentStamina > 0.01f;
    public bool  HasEnoughFor(StaminaConsumer c) => _currentStamina >= GetCost(c);
    public float Current                       => _currentStamina;
    public float Max                           => _maxStamina;
    public float Ratio                         => _currentStamina / _maxStamina;

    // -------------------------------------------------------------------------
    // Yardımcılar
    // -------------------------------------------------------------------------
    private void Consume(float amount)
    {
        _currentStamina = Mathf.Max(_currentStamina - amount, 0f);
        OnStaminaChanged?.Invoke(_currentStamina, _maxStamina);

        if (_currentStamina <= 0.01f)
            OnStaminaEmpty?.Invoke();
    }

    private float GetCost(StaminaConsumer consumer)
    {
        return consumer switch
        {
            StaminaConsumer.Run            => _runCostPerSecond,
            StaminaConsumer.Kick           => _kickCost,
            StaminaConsumer.BeltLight      => _beltLightCost,
            StaminaConsumer.BeltHeavy      => _beltHeavyRelease,
            _                              => 0f
        };
    }
}

// -------------------------------------------------------------------------
// Enum — hangi sistem stamina harcıyor
// -------------------------------------------------------------------------
public enum StaminaConsumer
{
    Run,
    Kick,
    BeltLight,
    BeltHeavy
}