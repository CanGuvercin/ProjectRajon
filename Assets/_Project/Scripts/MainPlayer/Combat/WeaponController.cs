using System;
using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Revolver")]
    [SerializeField] private int   _revolverMaxAmmo    = 5;
    [SerializeField] private float _revolverReloadTime = 1.8f;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform  _muzzlePoint;

    [Header("Belt")]
    [SerializeField] private float _beltMaxChargeTime  = 2.0f;

    [Header("Referanslar")]
    [SerializeField] private EmmiAnimator  _animator;
    [SerializeField] private StaminaSystem _stamina;
    [SerializeField] private Transform     _playerTransform;
    [SerializeField] private AmmoUI        _ammoUI;

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
        
        // TEST İÇİN - sonra kaldır
        _hasKnife = true;
        _hasBelt = true;
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
    // Bullet Spawn
    // -------------------------------------------------------------------------
    private void SpawnBullet()
    {
        if (_bulletPrefab == null || _muzzlePoint == null) return;
        
        GameObject bullet = Instantiate(_bulletPrefab, _muzzlePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        
        Vector2 direction = _playerTransform.localScale.x > 0 ? Vector2.right : Vector2.left;
        bulletScript.SetDirection(direction);
    }

    // -------------------------------------------------------------------------
    // Silah Animasyon Süreleri
    // -------------------------------------------------------------------------
    private float GetDrawDuration(WeaponType type)
    {
        return type switch
        {
            WeaponType.Fist     => 0.833f,
            WeaponType.Revolver => 0.917f,
            WeaponType.Knife    => 0.833f,
            WeaponType.Belt     => 1.167f,
            _ => 0.5f
        };
    }

    private float GetHolsterDuration(WeaponType type)
    {
        return type switch
        {
            WeaponType.Fist     => 0.833f,
            WeaponType.Revolver => 0.917f,
            WeaponType.Knife    => 0.833f,
            WeaponType.Belt     => 1.167f,
            _ => 0.5f
        };
    }

    // -------------------------------------------------------------------------
    // Silah Değiştirme
    // -------------------------------------------------------------------------
    public void SwitchWeapon(WeaponType type, Action onComplete = null)
    {
        if (type == _currentWeapon)                           { onComplete?.Invoke(); return; }
        if (type == WeaponType.Knife   && !_hasKnife)         { onComplete?.Invoke(); return; }
        if (type == WeaponType.Belt    && !_hasBelt)          { onComplete?.Invoke(); return; }
        if (type == WeaponType.Revolver && _revolverOnGround) { onComplete?.Invoke(); return; }
        if (_isSwitching)                                     { onComplete?.Invoke(); return; }

        StartCoroutine(SwitchRoutine(type, onComplete));
    }

    private IEnumerator SwitchRoutine(WeaponType newWeapon, Action onComplete)
    {
        _isSwitching = true;

        if (_currentWeapon != WeaponType.Fist)
        {
            float holsterTime = GetHolsterDuration(_currentWeapon);
            _animator.PlayHolster();
            yield return new WaitForSeconds(holsterTime);
        }

        _currentWeapon = newWeapon;
        _animator.SetWeaponType(_currentWeapon);
        OnWeaponChanged?.Invoke(_currentWeapon);

        if (_currentWeapon != WeaponType.Fist)
        {
            float drawTime = GetDrawDuration(_currentWeapon);
            _animator.PlayDraw();
            yield return new WaitForSeconds(drawTime);
        }

        _isSwitching = false;
        onComplete?.Invoke();
    }

    // -------------------------------------------------------------------------
    // Light Attack
    // -------------------------------------------------------------------------
    public void LightAttack(bool isMoving, Action onComplete)
    {
        switch (_currentWeapon)
        {
            case WeaponType.Revolver:
                if (_currentAmmo <= 0) { onComplete?.Invoke(); return; }
                _currentAmmo--;
                OnAmmoChanged?.Invoke(_currentAmmo, _revolverMaxAmmo);
                SpawnBullet();
                break;

            case WeaponType.Fist:
                break;

            case WeaponType.Knife:
                break;

            case WeaponType.Belt:
                if (!_stamina.TryConsume(StaminaConsumer.BeltLight))
                { onComplete?.Invoke(); return; }
                _stamina.SetConsuming(StaminaConsumer.BeltLight, true);
                break;
        }

        if (isMoving && _currentWeapon != WeaponType.Belt)
            _animator.PlayWalkingLightAttack();
        else
            _animator.PlayLightAttack();
            
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
                StartCoroutine(SprayBullets());
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

    private IEnumerator SprayBullets()
    {
        SpawnBullet();
        yield return new WaitForSeconds(0.15f);
        SpawnBullet();
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
    }

    // -------------------------------------------------------------------------
    // Reload
    // -------------------------------------------------------------------------
    public void Reload(Action onComplete)
    {
        if (_currentWeapon != WeaponType.Revolver) { onComplete?.Invoke(); return; }
        if (_currentAmmo == _revolverMaxAmmo)       { onComplete?.Invoke(); return; }
        
        // Şarjör kontrolü
        if (_ammoUI != null && !_ammoUI.HasMagazine()) 
        { 
            onComplete?.Invoke(); 
            return; 
        }
        
        StartCoroutine(ReloadRoutine(onComplete));
    }

    private IEnumerator ReloadRoutine(Action onComplete)
    {
        // Şarjör harca
        if (_ammoUI != null)
            _ammoUI.UseMagazine();
        
        // Emmi reload animasyonu
        _animator.PlayReload();
        
        // Animasyonun %40'ında UI reload başlasın
        yield return new WaitForSeconds(_revolverReloadTime * 0.4f);
        
        // UI reload animasyonu
        if (_ammoUI != null)
            _ammoUI.PlayReloadAnimation(_revolverMaxAmmo);
        
        // Kalan süreyi bekle
        yield return new WaitForSeconds(_revolverReloadTime * 0.6f);
        
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