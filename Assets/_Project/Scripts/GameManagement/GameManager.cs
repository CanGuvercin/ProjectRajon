using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// RAJON — GameManager
/// Oyun boyunca yaşayan singleton.
/// PlayerData'yı tutar, sistemlerle sync eder.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool _forceNewGame = false;
    
    // -------------------------------------------------------------------------
    // Aktif Oyuncu Verisi
    // -------------------------------------------------------------------------
    public PlayerData Data { get; private set; }
    
    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Oyun başında veri yükle veya yeni oluştur
        InitializeData();
    }
    
    private void InitializeData()
    {
        if (_forceNewGame)
        {
            Data = PlayerData.NewGame();
            Debug.Log("[GameManager] Force new game aktif.");
            return;
        }
        
        // Save var mı?
        PlayerData loaded = SaveSystem.Load();
        
        if (loaded != null)
        {
            Data = loaded;
            Debug.Log($"[GameManager] Save yüklendi. Bölüm: {Data.currentChapter}");
        }
        else
        {
            Data = PlayerData.NewGame();
            Debug.Log("[GameManager] Yeni oyun başlatıldı.");
        }
    }
    
    // -------------------------------------------------------------------------
    // Checkpoint Kaydet (Nargile noktası)
    // -------------------------------------------------------------------------
    public void SaveCheckpoint(string checkpointID, Vector2 position)
    {
        Data.lastCheckpointID = checkpointID;
        Data.checkpointPosX = position.x;
        Data.checkpointPosY = position.y;
        
        // Runtime sistemlerden güncel verileri çek
        SyncFromSystems();
        
        // Diske yaz
        SaveSystem.Save(Data);
        
        Debug.Log($"[GameManager] Checkpoint kaydedildi: {checkpointID}");
    }
    
    // -------------------------------------------------------------------------
    // Bölüm Geçişi
    // -------------------------------------------------------------------------
    public void AdvanceChapter()
    {
        Data.currentChapter++;
        SaveSystem.Save(Data);
        
        Debug.Log($"[GameManager] Bölüm {Data.currentChapter}'e geçildi.");
    }
    
    // -------------------------------------------------------------------------
    // Yeni Oyun Başlat
    // -------------------------------------------------------------------------
    public void StartNewGame()
    {
        SaveSystem.DeleteSave();
        Data = PlayerData.NewGame();
        
        Debug.Log("[GameManager] Yeni oyun başlatıldı.");
    }
    
    // -------------------------------------------------------------------------
    // Runtime Sistemlerle Sync
    // -------------------------------------------------------------------------
    
    /// <summary>
    /// Kaydetmeden önce çağır - runtime sistemlerden verileri PlayerData'ya çek
    /// </summary>
    public void SyncFromSystems()
    {
        // Bu referansları sahnede bul veya cache'le
        var health = FindObjectOfType<PlayerHealth>();
        var stamina = FindObjectOfType<StaminaSystem>();
        var weapon = FindObjectOfType<WeaponController>();
        var heal = FindObjectOfType<HealSystem>();
        
        if (health != null)
        {
            Data.currentHP = health.Current;
            Data.maxHP = health.Max;
        }
        
        if (stamina != null)
        {
            Data.currentStamina = stamina.Current;
            Data.maxStamina = stamina.Max;
        }
        
        if (weapon != null)
        {
            Data.revolverAmmo = weapon.CurrentAmmo;
            Data.hasKnife = weapon.HasKnife;
            Data.hasBelt = weapon.HasBelt;
        }
        
        if (heal != null)
        {
            Data.cigaretteCount = heal.CurrentCigarettes;
        }
    }
    
    /// <summary>
    /// Yükledikten sonra çağır - PlayerData'dan sistemlere verileri aktar
    /// TODO: Mevcut sistemlere Initialize metodları eklenince aktif et
    /// </summary>
    public void SyncToSystems()
    {
        // Şimdilik boş - sistemler kendi Awake'lerinde default değer alıyor
        // İleride her sisteme Initialize(value) metodu eklenince burası doldurulacak
        Debug.Log("[GameManager] SyncToSystems çağrıldı - henüz implement edilmedi");
    }
    
    // -------------------------------------------------------------------------
    // Playtime Tracking
    // -------------------------------------------------------------------------
    private void Update()
    {
        if (Data != null)
        {
            Data.totalPlayTime += Time.deltaTime;
        }
    }
    
    // -------------------------------------------------------------------------
    // Ölüm & Respawn
    // -------------------------------------------------------------------------
    public void OnPlayerDeath()
    {
        Data.totalDeaths++;
        
        // Checkpoint'e dön - sahneyi yeniden yükle
        // veya pozisyonu resetle
        Debug.Log($"[GameManager] Ölüm #{Data.totalDeaths}. Checkpoint'e dönülüyor...");
    }
    
    public Vector2 GetCheckpointPosition()
    {
        return new Vector2(Data.checkpointPosX, Data.checkpointPosY);
    }
    
    // -------------------------------------------------------------------------
    // Kill Tracking
    // -------------------------------------------------------------------------
    public void OnEnemyKilled()
    {
        Data.totalKills++;
    }
}