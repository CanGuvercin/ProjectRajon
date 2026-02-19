using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// RAJON — HealSystem
/// Sigara yakma mekaniği. HP basar, kamera zoom ve müzik tetikler.
/// PlayerController'dan çağrılır.
/// Marlboro UI için event fırlatır.
/// </summary>
public class HealSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Ayarlar
    // -------------------------------------------------------------------------
    [Header("Heal Değerleri")]
    [SerializeField] private int   _maxCigarettes  = 5;
    [SerializeField] private float _healAmount     = 35f;   // her sigaradan gelen HP
    [SerializeField] private float _healDuration   = 2.0f;  // animasyon + zoom süresi
    [SerializeField] private float _healCooldown   = 1.5f;  // art arda heal önleme

    [Header("Referanslar")]
    [SerializeField] private PlayerHealth  _health;
    [SerializeField] private EmmiAnimator  _animator;
    [SerializeField] private HealCinematic _cinematic; // zoom + müzik sistemi

    // -------------------------------------------------------------------------
    // İç Durum
    // -------------------------------------------------------------------------
    private int   _currentCigarettes;
    private float _cooldownTimer;

    // -------------------------------------------------------------------------
    // Events — Marlboro UI dinler
    // -------------------------------------------------------------------------
    public event Action<int, int> OnCigarettesChanged; // current, max
    public event Action           OnHealStarted;
    public event Action           OnHealFinished;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        _currentCigarettes = _maxCigarettes;
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    // -------------------------------------------------------------------------
    // Ana Heal — PlayerController çağırır, bitince callback
    // -------------------------------------------------------------------------
    public void UseHeal(Action onComplete)
    {
        if (!CanHeal())
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(HealRoutine(onComplete));
    }

    private IEnumerator HealRoutine(Action onComplete)
    {
        _currentCigarettes--;
        _cooldownTimer = _healCooldown;
        OnCigarettesChanged?.Invoke(_currentCigarettes, _maxCigarettes);
        OnHealStarted?.Invoke();

        // Animasyon tetikle
        _animator.SetHealing(true);

        // Kamera zoom + müzik — HealCinematic yönetir
        _cinematic?.Play();

        // Dünya durur — Time.timeScale sıfırlamak yerine
        // sadece Emmi'nin kendi sistemleri dondurulur (PlayerController guard'ları zaten engelliyor)
        // HealCinematic kendi coroutine'inde zoom out yapacak

        yield return new WaitForSeconds(_healDuration);

        // HP bas — yavaş yavaş değil, anlık (zoom sırasında zaten hissedilir)
        _health.Heal(_healAmount);

        _animator.SetHealing(false);
        _cinematic?.Stop();

        OnHealFinished?.Invoke();
        onComplete?.Invoke();
    }

    // -------------------------------------------------------------------------
    // Upgrade & Pickup — nargile noktası veya sahadan
    // -------------------------------------------------------------------------
    public void AddCigarettes(int amount)
    {
        _currentCigarettes = Mathf.Min(_currentCigarettes + amount, _maxCigarettes);
        OnCigarettesChanged?.Invoke(_currentCigarettes, _maxCigarettes);
    }

    public void UpgradeCapacity(int bonus)
    {
        _maxCigarettes += bonus;
        OnCigarettesChanged?.Invoke(_currentCigarettes, _maxCigarettes);
    }

    // -------------------------------------------------------------------------
    // Guard
    // -------------------------------------------------------------------------
    public bool CanHeal() => _currentCigarettes > 0 && _cooldownTimer <= 0f;

    // -------------------------------------------------------------------------
    // Dışarıdan okunabilir
    // -------------------------------------------------------------------------
    public int  CurrentCigarettes => _currentCigarettes;
    public int  MaxCigarettes     => _maxCigarettes;
    public bool HasCigarettes     => _currentCigarettes > 0;
}