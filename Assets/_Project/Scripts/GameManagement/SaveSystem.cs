using System;
using System.IO;
using UnityEngine;

/// <summary>
/// RAJON — SaveSystem
/// JSON tabanlı save/load sistemi.
/// Steam Cloud uyumlu - dosya bazlı çalışır.
/// </summary>
public static class SaveSystem
{
    private const string SAVE_FILENAME = "rajon_save.json";
    private const string BACKUP_FILENAME = "rajon_save_backup.json";
    
    /// <summary>
    /// Save dosyasının tam yolu.
    /// Windows: C:/Users/[User]/AppData/LocalLow/[Company]/[Product]/
    /// Steam Cloud bu klasörü sync eder.
    /// </summary>
    private static string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
    private static string BackupPath => Path.Combine(Application.persistentDataPath, BACKUP_FILENAME);
    
    // -------------------------------------------------------------------------
    // Kaydet
    // -------------------------------------------------------------------------
    public static bool Save(PlayerData data)
    {
        try
        {
            // Önce mevcut save'i backup'a kopyala
            if (File.Exists(SavePath))
            {
                File.Copy(SavePath, BackupPath, overwrite: true);
            }
            
            // JSON'a çevir
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            
            // Dosyaya yaz
            File.WriteAllText(SavePath, json);
            
            Debug.Log($"[SaveSystem] Oyun kaydedildi: {SavePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Kaydetme hatası: {e.Message}");
            return false;
        }
    }
    
    // -------------------------------------------------------------------------
    // Yükle
    // -------------------------------------------------------------------------
    public static PlayerData Load()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[SaveSystem] Save dosyası yok, yeni oyun başlatılıyor.");
                return null;
            }
            
            string json = File.ReadAllText(SavePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            
            Debug.Log($"[SaveSystem] Oyun yüklendi: Bölüm {data.currentChapter}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Yükleme hatası: {e.Message}");
            
            // Ana save bozuksa backup'tan dene
            return LoadBackup();
        }
    }
    
    // -------------------------------------------------------------------------
    // Backup'tan Yükle
    // -------------------------------------------------------------------------
    private static PlayerData LoadBackup()
    {
        try
        {
            if (!File.Exists(BackupPath))
            {
                Debug.LogWarning("[SaveSystem] Backup da yok.");
                return null;
            }
            
            string json = File.ReadAllText(BackupPath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            
            Debug.Log("[SaveSystem] Backup'tan yüklendi.");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Backup yükleme hatası: {e.Message}");
            return null;
        }
    }
    
    // -------------------------------------------------------------------------
    // Save Var mı?
    // -------------------------------------------------------------------------
    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }
    
    // -------------------------------------------------------------------------
    // Save Sil (Yeni Oyun için)
    // -------------------------------------------------------------------------
    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
            
            if (File.Exists(BackupPath))
                File.Delete(BackupPath);
            
            Debug.Log("[SaveSystem] Save dosyaları silindi.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Silme hatası: {e.Message}");
        }
    }
    
    // -------------------------------------------------------------------------
    // Debug: Save Konumunu Aç
    // -------------------------------------------------------------------------
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void OpenSaveFolder()
    {
        Application.OpenURL(Application.persistentDataPath);
    }
}