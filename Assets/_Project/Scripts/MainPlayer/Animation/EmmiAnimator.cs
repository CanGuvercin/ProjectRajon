using UnityEngine;

/// <summary>
/// RAJON — EmmiAnimator
/// Sadece Animator parametrelerini set eder.
/// Oyun mantığı bilmez.
/// </summary>
public class EmmiAnimator : MonoBehaviour
{
    private static readonly int ParamIsMoving    = Animator.StringToHash("IsMoving");
    private static readonly int ParamIsRunning   = Animator.StringToHash("IsRunning");
    private static readonly int ParamIsDodging   = Animator.StringToHash("IsDodging");
    private static readonly int ParamIsReloading = Animator.StringToHash("IsReloading");
    private static readonly int ParamIsHealing   = Animator.StringToHash("IsHealing");
    private static readonly int ParamWeaponType  = Animator.StringToHash("WeaponType");

    private static readonly int TriggerLightAttack = Animator.StringToHash("LightAttack");
    private static readonly int TriggerHeavyAttack = Animator.StringToHash("HeavyAttack");
    private static readonly int TriggerThrow       = Animator.StringToHash("Throw");
    private static readonly int TriggerPickup      = Animator.StringToHash("Pickup");
    private static readonly int TriggerHitRanged   = Animator.StringToHash("HitRanged");
    private static readonly int TriggerHitMelee    = Animator.StringToHash("HitMelee");
    private static readonly int TriggerDeath       = Animator.StringToHash("Death");
    private static readonly int TriggerZippo       = Animator.StringToHash("Zippo");
    private static readonly int TriggerHolster     = Animator.StringToHash("Holster"); // silahı sor
    private static readonly int TriggerDraw        = Animator.StringToHash("Draw");    // silahı çek

    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    // -------------------------------------------------------------------------
    // Bool Parametreler
    // -------------------------------------------------------------------------
    public void SetMoving(bool v)
{
    Debug.Log($"SetMoving: {v}");
    _animator.SetBool(ParamIsMoving, v);
}
    public void SetRunning(bool v)   => _animator.SetBool(ParamIsRunning, v);
    public void SetDodging(bool v)   => _animator.SetBool(ParamIsDodging, v);
    public void SetReloading(bool v) => _animator.SetBool(ParamIsReloading, v);
    public void SetHealing(bool v)   => _animator.SetBool(ParamIsHealing, v);

    /// <summary>WeaponType int → Animator blend tree için. Fist=0, Revolver=1, Knife=2, Belt=3</summary>
    // EmmiAnimator.cs
public void SetWeaponType(WeaponType type) => 
    _animator.SetFloat(ParamWeaponType, (float)type);

    // -------------------------------------------------------------------------
    // Trigger Tetikleyiciler
    // -------------------------------------------------------------------------
    public void PlayLightAttack() => _animator.SetTrigger(TriggerLightAttack);
    public void PlayHeavyAttack() => _animator.SetTrigger(TriggerHeavyAttack);
    public void PlayThrow()       => _animator.SetTrigger(TriggerThrow);
    public void PlayPickup()      => _animator.SetTrigger(TriggerPickup);
    public void PlayZippo()       => _animator.SetTrigger(TriggerZippo);

    /// <summary>
    /// Mevcut silahı yerine koy animasyonu.
    /// Animator'de WeaponType int zaten set edilmiş olduğundan
    /// doğru Holster klibini seçer.
    /// </summary>
    public void PlayHolster() => _animator.SetTrigger(TriggerHolster);

    /// <summary>
    /// Yeni silahı çek animasyonu.
    /// SwitchRoutine içinde SetWeaponType çağrıldıktan SONRA tetiklenir.
    /// </summary>
    public void PlayDraw() => _animator.SetTrigger(TriggerDraw);

    public void PlayHit(bool isRanged = true)
    {
        if (isRanged) _animator.SetTrigger(TriggerHitRanged);
        else          _animator.SetTrigger(TriggerHitMelee);
    }

    public void PlayHit()   => PlayHit(isRanged: false);

    public void PlayDeath()
    {
        _animator.SetInteger(Animator.StringToHash("DeathIndex"), Random.Range(0, 2));
        _animator.SetTrigger(TriggerDeath);
    }
}