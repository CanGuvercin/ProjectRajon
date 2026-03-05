using UnityEngine;

/// <summary>
/// RAJON — EmmiAnimator
/// Sadece Animator parametrelerini set eder.
/// Oyun mantığı bilmez. PlayerController ve sistemler buraya state bildirir.
/// 
/// ÖNEMLİ: Aşağıdaki string isimler Unity Animator'daki
/// parametre ve state isimleriyle birebir eşleşmelidir.
/// </summary>
public class EmmiAnimator : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Animator Parametre İsimleri — Animator'de bunları kullan
    // -------------------------------------------------------------------------
    private static readonly int ParamIsMoving    = Animator.StringToHash("IsMoving");
    private static readonly int ParamIsRunning   = Animator.StringToHash("IsRunning");
    private static readonly int ParamIsDodging   = Animator.StringToHash("IsDodging");
    private static readonly int ParamIsReloading = Animator.StringToHash("IsReloading");
    private static readonly int ParamIsHealing   = Animator.StringToHash("IsHealing");

    private static readonly int TriggerLightAttack  = Animator.StringToHash("LightAttack");
    private static readonly int TriggerHeavyAttack  = Animator.StringToHash("HeavyAttack");
    private static readonly int TriggerThrow        = Animator.StringToHash("Throw");
    private static readonly int TriggerPickup       = Animator.StringToHash("Pickup");
    private static readonly int TriggerHitRanged    = Animator.StringToHash("HitRanged");
    private static readonly int TriggerHitMelee     = Animator.StringToHash("HitMelee");
    private static readonly int TriggerDeath        = Animator.StringToHash("Death");
    private static readonly int TriggerZippo        = Animator.StringToHash("Zippo");

    // Aktif silaha göre Animator'ün doğru attack animasyonunu seçmesi için
    private static readonly int ParamWeaponType = Animator.StringToHash("WeaponType");

    // -------------------------------------------------------------------------
    // Animator State İsimleri (bilgi amaçlı — Animator'de bu isimler olacak)
    // -------------------------------------------------------------------------
    // "Idle"
    // "IdlePlus"       → cep saatine bakma (rastgele tetiklenebilir)
    // "Walk"
    // "Run"
    // "Dodge"          → eğilme + kalkma tek animasyon
    // "Reload"
    // "Heal"           → sigara yakma
    // "Zippo"          → sol el yukarı kaldır + indir
    // "Death"
    //
    // "LightAttack_Fist"
    // "LightAttack_Revolver"
    // "LightAttack_Knife"
    // "LightAttack_Belt"
    //
    // "HeavyAttack_Fist"       → tekme
    // "HeavyAttack_Revolver"   → sprey atış
    // "HeavyAttack_Knife"      → saplama
    // "HeavyAttack_Belt"       → kemer şarj + bırakma
    //
    // "Throw"
    // "Pickup"
    //
    // "HitRanged"              → mermi yeme
    // "HitMelee"               → yakın dövüş yeme

    // -------------------------------------------------------------------------
    // Referans
    // -------------------------------------------------------------------------
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // -------------------------------------------------------------------------
    // Durum Güncellemeleri — PlayerController her Update'te çağırır
    // -------------------------------------------------------------------------
    public void SetMoving(bool isMoving)     => _animator.SetBool(ParamIsMoving, isMoving);
    public void SetRunning(bool isRunning)   => _animator.SetBool(ParamIsRunning, isRunning);
    public void SetDodging(bool isDodging)   => _animator.SetBool(ParamIsDodging, isDodging);
    public void SetReloading(bool isReload)  => _animator.SetBool(ParamIsReloading, isReload);
    public void SetHealing(bool isHealing)   => _animator.SetBool(ParamIsHealing, isHealing);

    /// <summary>
    /// WeaponType enum int değeri Animator'e iletilir.
    /// Fist=0, Revolver=1, Knife=2, Belt=3
    /// Animator bu değere göre doğru attack state'ini seçer.
    /// </summary>
    public void SetWeaponType(WeaponType type) => _animator.SetInteger(ParamWeaponType, (int)type);

    // -------------------------------------------------------------------------
    // Trigger Tetikleyiciler — tek seferlik animasyonlar
    // -------------------------------------------------------------------------
    public void PlayLightAttack()  => _animator.SetTrigger(TriggerLightAttack);
    public void PlayHeavyAttack()  => _animator.SetTrigger(TriggerHeavyAttack);
    public void PlayThrow()        => _animator.SetTrigger(TriggerThrow);
    public void PlayPickup()       => _animator.SetTrigger(TriggerPickup);
    public void PlayZippo()        => _animator.SetTrigger(TriggerZippo);

    public void PlayHit(bool isRanged = true)
    {
        if (isRanged) _animator.SetTrigger(TriggerHitRanged);
        else          _animator.SetTrigger(TriggerHitMelee);
    }

    // PlayerController.OnHit() buraya yönlendirir (varsayılan melee)
    public void PlayHit()  => PlayHit(isRanged: false);
    public void PlayDeath()
{
    _animator.SetInteger(Animator.StringToHash("DeathIndex"), Random.Range(0, 2));
    _animator.SetTrigger(TriggerDeath);
}
}