using System.Collections.Generic;
using UnityEngine;

public static class KnifeDatabase
{
    const string CatalogResourcePath = "KnifeCatalog";
    const string SelectedKnifeKey = "SELECTED_KNIFE";
    const string KnifeUnlockedKeyPrefix = "KNIFE_UNLOCKED_";

    static readonly List<KnifeData> EmptyKnives = new List<KnifeData>();

    static KnifeCatalog cachedCatalog;
    static Dictionary<string, KnifeData> knifeLookup;

    public static IReadOnlyList<KnifeData> Knives
    {
        get
        {
            KnifeCatalog catalog = LoadCatalog();
            return catalog != null && catalog.knives != null ? catalog.knives : EmptyKnives;
        }
    }

    public static string DefaultKnifeId
    {
        get
        {
            KnifeCatalog catalog = LoadCatalog();
            if (catalog == null || string.IsNullOrWhiteSpace(catalog.defaultKnifeId))
                return Knives.Count > 0 ? Knives[0].id : "knife_0";

            return catalog.defaultKnifeId;
        }
    }

    public static KnifeData GetKnifeByID(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            id = DefaultKnifeId;

        EnsureLookup();

        if (knifeLookup != null && knifeLookup.TryGetValue(id, out KnifeData knife))
            return knife;

        if (id != DefaultKnifeId && knifeLookup != null && knifeLookup.TryGetValue(DefaultKnifeId, out KnifeData defaultKnife))
            return defaultKnife;

        return Knives.Count > 0 ? Knives[0] : null;
    }

    public static string GetSelectedKnifeId()
    {
        string selectedId = PlayerPrefs.GetString(SelectedKnifeKey, DefaultKnifeId);
        if (IsKnifeUnlocked(selectedId))
            return selectedId;

        PlayerPrefs.SetString(SelectedKnifeKey, DefaultKnifeId);
        PlayerPrefs.Save();
        return DefaultKnifeId;
    }

    public static KnifeData GetSelectedKnife()
    {
        return GetKnifeByID(GetSelectedKnifeId());
    }

    public static void SelectKnife(string id)
    {
        KnifeData knife = GetKnifeByID(id);
        if (knife == null || !IsKnifeUnlocked(knife.id))
            return;

        PlayerPrefs.SetString(SelectedKnifeKey, knife.id);
        PlayerPrefs.Save();
    }

    public static bool IsKnifeUnlocked(string id)
    {
        if (!TryGetKnife(id, out KnifeData knife))
            return false;

        if (knife.id == DefaultKnifeId)
            return true;

        return PlayerPrefs.GetInt(GetUnlockKey(knife.id), 0) == 1;
    }

    public static bool UnlockKnife(string id)
    {
        if (!TryGetKnife(id, out KnifeData knife))
            return false;

        if (IsKnifeUnlocked(knife.id))
            return false;

        PlayerPrefs.SetInt(GetUnlockKey(knife.id), 1);
        PlayerPrefs.Save();
        return true;
    }

    public static string UnlockRandomLockedKnife()
    {
        List<KnifeData> lockedKnives = new List<KnifeData>();

        foreach (KnifeData knife in Knives)
        {
            if (knife == null || IsKnifeUnlocked(knife.id))
                continue;

            lockedKnives.Add(knife);
        }

        if (lockedKnives.Count == 0)
            return string.Empty;

        KnifeData randomKnife = lockedKnives[Random.Range(0, lockedKnives.Count)];
        UnlockKnife(randomKnife.id);
        return randomKnife.id;
    }

    public static int CountLockedKnives()
    {
        int lockedKnives = 0;

        foreach (KnifeData knife in Knives)
        {
            if (knife == null || IsKnifeUnlocked(knife.id))
                continue;

            lockedKnives++;
        }

        return lockedKnives;
    }

    public static bool HasLockedKnives()
    {
        return CountLockedKnives() > 0;
    }

    static KnifeCatalog LoadCatalog()
    {
        if (cachedCatalog != null)
            return cachedCatalog;

        cachedCatalog = Resources.Load<KnifeCatalog>(CatalogResourcePath);

        if (cachedCatalog == null)
            Debug.LogError("KnifeCatalog could not be loaded from Resources/KnifeCatalog.");

        EnsureLookup();
        return cachedCatalog;
    }

    static void EnsureLookup()
    {
        if (knifeLookup != null)
            return;

        knifeLookup = new Dictionary<string, KnifeData>();

        KnifeCatalog catalog = cachedCatalog != null ? cachedCatalog : Resources.Load<KnifeCatalog>(CatalogResourcePath);
        cachedCatalog = catalog;

        if (catalog == null || catalog.knives == null)
            return;

        foreach (KnifeData knife in catalog.knives)
        {
            if (knife == null || string.IsNullOrWhiteSpace(knife.id))
                continue;

            knifeLookup[knife.id] = knife;
        }
    }

    static bool TryGetKnife(string id, out KnifeData knife)
    {
        EnsureLookup();

        knife = null;
        if (string.IsNullOrWhiteSpace(id) || knifeLookup == null)
            return false;

        return knifeLookup.TryGetValue(id, out knife);
    }

    static string GetUnlockKey(string id)
    {
        return KnifeUnlockedKeyPrefix + id;
    }
}
