using System;
using UnityEngine;

/// <summary>
/// RAJON — PlayerData
/// Runtime'da tutulan tüm oyuncu verileri.
/// JSON'a serialize edilebilir yapıda.
/// </summary>
[Serializable]
public class PlayerData
{
    // -------------------------------------------------------------------------
    // Temel Statlar
    // -------------------------------------------------------------------------
    public float currentHP;
public float maxHP;
    public float currentStamina;
    public float maxStamina;
    
    // -------------------------------------------------------------------------
    // Silah & Envanter
    // -------------------------------------------------------------------------
    public int  revolverAmmo;
    public int  revolverMaxAmmo;
    public int  magazineCount;      // Yedek şarjör
    public int  cigaretteCount;     // Sigara = heal hakkı
    public int  maxCigarettes;
    public bool hasKnife;
    public bool hasBelt;
    
    // -------------------------------------------------------------------------
    // İlerleme
    // -------------------------------------------------------------------------
    public int    currentChapter;       // 1-8 arası
    public string lastCheckpointID;     // Nargile noktası ID'si
    public float  checkpointPosX;
    public float  checkpointPosY;
    
    // -------------------------------------------------------------------------
    // Upgrade Flags
    // -------------------------------------------------------------------------
    public int  staminaRegenLevel;      // 0-3 arası
    public int  revolverCapacityLevel;  // 0 = 5 mermi, 1 = 6, 2 = 7
    public int  cigaretteCapacityLevel; // Sigara taşıma kapasitesi
    public bool hasInfiniteKnife;       // Bıçak geri dönüyor mu
    
    // -------------------------------------------------------------------------
    // Oyun İstatistikleri (opsiyonel, epilog için)
    // -------------------------------------------------------------------------
    public int   totalKills;
    public int   totalDeaths;
    public float totalPlayTime;
    
    // -------------------------------------------------------------------------
    // Default Değerlerle Yeni Oyun
    // -------------------------------------------------------------------------
    public static PlayerData NewGame()
    {
        return new PlayerData
        {
            // Temel
            currentHP      = 100,
            maxHP          = 100,
            currentStamina = 100f,
            maxStamina     = 100f,
            
            // Silah
            revolverAmmo    = 5,
            revolverMaxAmmo = 5,
            magazineCount   = 3,
            cigaretteCount  = 3,
            maxCigarettes   = 5,
            hasKnife        = false,
            hasBelt         = false,
            
            // İlerleme
            currentChapter    = 1,
            lastCheckpointID  = "chapter1_start",
            checkpointPosX    = 0f,
            checkpointPosY    = 0f,
            
            // Upgrades
            staminaRegenLevel      = 0,
            revolverCapacityLevel  = 0,
            cigaretteCapacityLevel = 0,
            hasInfiniteKnife       = false,
            
            // Stats
            totalKills    = 0,
            totalDeaths   = 0,
            totalPlayTime = 0f
        };
    }
}