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
    // -------------------------------------------------------------------------
    // Ayarlar
    // -------------------------------------------------------------------------
    [Header("Revolver")]
    [SerializeField] private int   _revolverMaxAmmo    = 5;
    [SerializeField] private float _revolverReloadTime = 1.8f;

    [Header("Belt")]
    [SerializeField] private float _beltMaxChargeTime  = 2.0f;

    [Header("Referanslar")]
    [SerializeField] private EmmiAnimator  _animator;
    [SerializeField] private StaminaSystem _stamina;

    // -------------------------------------------------------------------------
    // İç Durum
    // -------------------------------------------------------------------------
    private WeaponType _currentWeapon = WeaponType.Revolver;
    private int        _currentAmmo;
    private bool       _hasKnife;
    private bool       _hasBelt;          // mid-game unlock
    private bool       _isBeltCharging;
    private float      _beltChargeTimer;

    // Yerde revolver var mı? (throw sonrası)
    private bool _revolverOnGround;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------
    public event Action<WeaponType> OnWeaponChanged;
    public event Action<int, int>   OnAmmoChanged;    // current, max

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
{
    _currentAmmo = _revolverMaxAmmo;
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
    // Silah Değiştirme
    // -------------------------------------------------------------------------
    public void SwitchWeapon(WeaponType type)
    {
        if (type == _currentWeapon)          return;
        if (type == WeaponType.Knife  && !_hasKnife) return;
        if (type == WeaponType.Belt   && !_hasBelt)  return;
        if (type == WeaponType.Revolver && _revolverOnGround) return;

        _currentWeapon = type;
        _animator.SetWeaponType(_currentWeapon);
        OnWeaponChanged?.Invoke(_currentWeapon);
    }

    // -------------------------------------------------------------------------
    // Light Attack — PlayerController callback ile bitişi bildirir
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
                // TODO: Mermi prefab fırlat
                break;

            case WeaponType.Fist:
                _animator.PlayLightAttack();
                break;

            case WeaponType.Knife:
                _animator.PlayLightAttack();
                // Hasar hesabı ileride KnifeAttack class'ında
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
                // Sprey: 2 mermi birden
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
                // Şarj başlar — bırakınca HandleBeltChargeRelease tetikler
                _isBeltCharging    = true;
                _beltChargeTimer   = 0f;
                _stamina.SetConsuming(StaminaConsumer.BeltLight, false);
                _animator.PlayHeavyAttack();
                break;
        }

        if (_currentWeapon != WeaponType.Belt)
            StartCoroutine(WaitForAnimation(onComplete));
        else
            onComplete?.Invoke(); // Belt kendi döngüsünü yönetir
    }

    // -------------------------------------------------------------------------
    // Belt Şarj Döngüsü
    // -------------------------------------------------------------------------
    private void HandleBeltChargeRelease()
    {
        if (!_isBeltCharging) return;

        _beltChargeTimer += Time.deltaTime;

        // Maksimum şarj aşılınca otomatik bırak
        if (_beltChargeTimer >= _beltMaxChargeTime)
            ReleaseBelt();
    }

    // PlayerController R2 bırakıldığında çağırır
    public void ReleaseBelt()
    {
        if (!_isBeltCharging) return;

        float chargeRatio  = Mathf.Clamp01(_beltChargeTimer / _beltMaxChargeTime);
        _isBeltCharging    = false;
        _beltChargeTimer   = 0f;

        _stamina.TryConsume(StaminaConsumer.BeltHeavy);
        // TODO: chargeRatio'ya göre hasar ve alan hesabı BeltAttack class'ında
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
        if (_revolverOnGround)
        {
            PickupRevolver();
            return;
        }

        switch (_currentWeapon)
        {
            case WeaponType.Revolver:
                ThrowRevolver();
                break;
            case WeaponType.Knife:
                ThrowKnife();
                break;
        }
    }

    private void ThrowRevolver()
    {
        _revolverOnGround = true;
        _currentWeapon    = WeaponType.Fist;
        _animator.SetWeaponType(_currentWeapon);
        _animator.PlayThrow();
        OnWeaponChanged?.Invoke(_currentWeapon);
        // TODO: Revolver prefab sahneye spawn
    }

    private void PickupRevolver()
    {
        _revolverOnGround = false;
        _currentWeapon    = WeaponType.Revolver;
        _animator.SetWeaponType(_currentWeapon);
        _animator.PlayPickup();
        OnWeaponChanged?.Invoke(_currentWeapon);
        // TODO: Yerdeki prefab'ı kaldır
    }

    private void ThrowKnife()
    {
        _hasKnife      = false;
        _currentWeapon = WeaponType.Fist;
        _animator.SetWeaponType(_currentWeapon);
        _animator.PlayThrow();
        OnWeaponChanged?.Invoke(_currentWeapon);
        // TODO: Knife prefab sahneye spawn, geri alınamaz
    }

    // -------------------------------------------------------------------------
    // Upgrade & Pickup — dışarıdan çağrılır
    // -------------------------------------------------------------------------
    public void PickupKnife()
    {
        _hasKnife = true;
    }

    public void UnlockBelt()
    {
        _hasBelt = true;
    }

    public void UpgradeMaxAmmo(int bonus)
    {
        _revolverMaxAmmo += bonus;
        if (_currentAmmo > _revolverMaxAmmo)
            _currentAmmo = _revolverMaxAmmo;
        OnAmmoChanged?.Invoke(_currentAmmo, _revolverMaxAmmo);
    }

    // -------------------------------------------------------------------------
    // Dışarıdan okunabilir
    // -------------------------------------------------------------------------
    public WeaponType CurrentWeapon   => _currentWeapon;
    public int        CurrentAmmo     => _currentAmmo;
    public bool       HasKnife        => _hasKnife;
    public bool       HasBelt         => _hasBelt;
    public bool       IsBeltCharging  => _isBeltCharging;
    public float      BeltChargeRatio => Mathf.Clamp01(_beltChargeTimer / _beltMaxChargeTime);

    // -------------------------------------------------------------------------
    // Yardımcı
    // -------------------------------------------------------------------------
    private IEnumerator WaitForAnimation(Action onComplete)
    {
        // Animator'deki animasyon süresi kadar bekler
        // Şimdilik sabit 0.5s — ileride AnimationClip süresiyle replace edilir
        yield return new WaitForSeconds(0.5f);
        onComplete?.Invoke();
    }
}