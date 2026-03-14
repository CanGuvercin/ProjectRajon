using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// RAJON — WeaponController
/// Silah switching, saldırı delegasyonu, throw/pickup yönetimi.
/// Hasar hesabı yapmaz — her silahın kendi attack class'ı hesaplar (ileride).
/// PlayerController'dan çağrılır.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Revolver")]
    [SerializeField] private int   _revolverMaxAmmo    = 5;
    [SerializeField] private float _revolverReloadTime = 1.8f;

    [Header("Belt")]
    [SerializeField] private float _beltMaxChargeTime  = 2.0f;

    [Header("Silah Geçiş Süreleri")]
    [SerializeField] private float _holsterDuration = 0.4f;
    [SerializeField] private float _drawDuration    = 0.5f;

    [Header("Referanslar")]
    [SerializeField] private EmmiAnimator  _animator;
    [SerializeField] private StaminaSystem _stamina;

    private WeaponType _currentWeapon = WeaponType.Revolver;
    private int        _currentAmmo;
    private bool       _hasKnife;
    private bool       _hasBelt;
    private bool       _isBeltCharging;
    private float      _beltChargeTimer;
    private bool       _revolverOnGround;
    private bool       _isSwitching;

    public event Action<WeaponType> OnWeaponChanged;
    public event Action<int, int>   OnAmmoChanged;

    private void Awake()
    {
        _currentAmmo   = _revolverMaxAmmo;
        _currentWeapon = WeaponType.Revolver;
    }

    private void Start()
    {
        _animator.SetWeaponType(_currentWeapon);
    }

    private void Update()
    {
        HandleBeltChargeRelease();
    }

    // -------------------------------------------------------------------------
    // Silah Değiştirme — Holster → Draw sekansı
    // -------------------------------------------------------------------------
    public void SwitchWeapon(WeaponType type, Action onComplete = null)
    {
        if (type == _currentWeapon)                           { Debug.Log($"SwitchWeapon: zaten {type}"); onComplete?.Invoke(); return; }
        if (type == WeaponType.Knife   && !_hasKnife)         { Debug.Log("SwitchWeapon: bıçak yok");    onComplete?.Invoke(); return; }
        if (type == WeaponType.Belt    && !_hasBelt)          { Debug.Log("SwitchWeapon: kemer yok");    onComplete?.Invoke(); return; }
        if (type == WeaponType.Revolver && _revolverOnGround) { Debug.Log("SwitchWeapon: revolver yerde"); onComplete?.Invoke(); return; }
        if (_isSwitching)                                     { Debug.Log("SwitchWeapon: zaten switching"); onComplete?.Invoke(); return; }

        StartCoroutine(SwitchRoutine(type, onComplete));
    }

    private IEnumerator SwitchRoutine(WeaponType newWeapon, Action onComplete)
    {
        _isSwitching = true;
        Debug.Log($"SwitchRoutine başladı: {_currentWeapon} → {newWeapon}");

        // 1. Holster — mevcut silahı koy
        if (_currentWeapon != WeaponType.Fist)
        {
            _animator.PlayHolster();
            Debug.Log($"Holster trigger atıldı, {_holsterDuration}s bekleniyor");
            yield return new WaitForSeconds(_holsterDuration);
            Debug.Log("Holster bekleme bitti");
        }

        // 2. Silahı değiştir
        _currentWeapon = newWeapon;
        _animator.SetWeaponType(_currentWeapon);
        OnWeaponChanged?.Invoke(_currentWeapon);
        Debug.Log($"WeaponType set edildi: {_currentWeapon}");

        // 3. Draw — yeni silahı çek
        if (_currentWeapon != WeaponType.Fist)
        {
            _animator.PlayDraw();
            Debug.Log($"Draw trigger atıldı, {_drawDuration}s bekleniyor");
            yield return new WaitForSeconds(_drawDuration);
            Debug.Log("Draw bekleme bitti");
        }

        _isSwitching = false;
        Debug.Log("SwitchRoutine bitti, _isSwitching = false");
        onComplete?.Invoke();
    }

    // -------------------------------------------------------------------------
    // Light Attack
    // -------------------------------------------------------------------------
    public void LightAttack(Action onComplete)
    {
        switch (_currentWeapon)
        {
            case WeaponType.Revolver:
                if (_currentAmmo <= 0) { onComplete?.Invoke(); return; }
                _currentAmmo--;
                OnAmmoChanged?.Invoke(_currentAmmo, _revolverMaxAmmo);
                _animator.PlayLightAttack();
                break;

            case WeaponType.Fist:
                _animator.PlayLightAttack();
                break;

            case WeaponType.Knife:
                _animator.PlayLightAttack();
                break;

            case WeaponType.Belt:
                if (!_stamina.TryConsume(StaminaConsumer.BeltLight))
                { onComplete?.Invoke(); return; }
                _stamina.SetConsuming(StaminaConsumer.BeltLight, true);
                _animator.PlayLightAttack();
                break;
        }

        StartCoroutine(WaitForAnimation(onComplete));
    }

    // -------------------------------------------------------------------------
    // Heavy Attack
    // -------------------------------------------------------------------------
    public void HeavyAttack(Action onComplete)
    {
        switch (_currentWeapon)
        {
            case WeaponType.Revolver:
                if (_currentAmmo < 2) { onComplete?.Invoke(); return; }
                _currentAmmo -= 2;
                OnAmmoChanged?.Invoke(_currentAmmo, _revolverMaxAmmo);
                _animator.PlayHeavyAttack();
                break;

            case WeaponType.Fist:
                if (!_stamina.TryConsume(StaminaConsumer.Kick))
                { onComplete?.Invoke(); return; }
                _animator.PlayHeavyAttack();
                break;

            case WeaponType.Knife:
                _animator.PlayHeavyAttack();
                break;

            case WeaponType.Belt:
                _isBeltCharging  = true;
                _beltChargeTimer = 0f;
                _stamina.SetConsuming(StaminaConsumer.BeltLight, false);
                _animator.PlayHeavyAttack();
                break;
        }

        if (_currentWeapon != WeaponType.Belt)
            StartCoroutine(WaitForAnimation(onComplete));
        else
            onComplete?.Invoke();
    }

    // -------------------------------------------------------------------------
    // Belt Şarj
    // -------------------------------------------------------------------------
    private void HandleBeltChargeRelease()
    {
        if (!_isBeltCharging) return;
        _beltChargeTimer += Time.deltaTime;
        if (_beltChargeTimer >= _beltMaxChargeTime) ReleaseBelt();
    }

    public void ReleaseBelt()
    {
        if (!_isBeltCharging) return;
        float chargeRatio = Mathf.Clamp01(_beltChargeTimer / _beltMaxChargeTime);
        _isBeltCharging  = false;
        _beltChargeTimer = 0f;
        _stamina.TryConsume(StaminaConsumer.BeltHeavy);
        // TODO: chargeRatio ile hasar
    }

    // -------------------------------------------------------------------------
    // Reload
    // -------------------------------------------------------------------------
    public void Reload(Action onComplete)
    {
        if (_currentWeapon != WeaponType.Revolver) { onComplete?.Invoke(); return; }
        if (_currentAmmo == _revolverMaxAmmo)       { onComplete?.Invoke(); return; }
        StartCoroutine(ReloadRoutine(onComplete));
    }

    private IEnumerator ReloadRoutine(Action onComplete)
    {
        yield return new WaitForSeconds(_revolverReloadTime);
        _currentAmmo = _revolverMaxAmmo;
        OnAmmoChanged?.Invoke(_currentAmmo, _revolverMaxAmmo);
        onComplete?.Invoke();
    }

    // -------------------------------------------------------------------------
    // Throw / Pickup
    // -------------------------------------------------------------------------
    public void ThrowOrPickup()
    {
        if (_revolverOnGround) { PickupRevolver(); return; }

        switch (_currentWeapon)
        {
            case WeaponType.Revolver: ThrowRevolver(); break;
            case WeaponType.Knife:    ThrowKnife();    break;
        }
    }

    private void ThrowRevolver()
    {
        _revolverOnGround = true;
        _currentWeapon    = WeaponType.Fist;
        _animator.SetWeaponType(_currentWeapon);
        _animator.PlayThrow();
        OnWeaponChanged?.Invoke(_currentWeapon);
    }

    private void PickupRevolver()
    {
        _revolverOnGround = false;
        _currentWeapon    = WeaponType.Revolver;
        _animator.SetWeaponType(_currentWeapon);
        _animator.PlayPickup();
        OnWeaponChanged?.Invoke(_currentWeapon);
    }

    private void ThrowKnife()
    {
        _hasKnife      = false;
        _currentWeapon = WeaponType.Fist;
        _animator.SetWeaponType(_currentWeapon);
        _animator.PlayThrow();
        OnWeaponChanged?.Invoke(_currentWeapon);
    }

    // -------------------------------------------------------------------------
    // Unlock / Upgrade
    // -------------------------------------------------------------------------
    public void PickupKnife()  => _hasKnife = true;
    public void UnlockBelt()   => _hasBelt  = true;

    public void UpgradeMaxAmmo(int bonus)
    {
        _revolverMaxAmmo += bonus;
        if (_currentAmmo > _revolverMaxAmmo) _currentAmmo = _revolverMaxAmmo;
        OnAmmoChanged?.Invoke(_currentAmmo, _revolverMaxAmmo);
    }

    // -------------------------------------------------------------------------
    // Public Properties
    // -------------------------------------------------------------------------
    public WeaponType CurrentWeapon    => _currentWeapon;
    public int        CurrentAmmo      => _currentAmmo;
    public bool       HasKnife         => _hasKnife;
    public bool       HasBelt          => _hasBelt;
    public bool       IsBeltCharging   => _isBeltCharging;
    public bool       IsSwitching      => _isSwitching;
    public float      BeltChargeRatio  => Mathf.Clamp01(_beltChargeTimer / _beltMaxChargeTime);

    private IEnumerator WaitForAnimation(Action onComplete)
    {
        yield return new WaitForSeconds(0.5f);
        onComplete?.Invoke();
    }
}