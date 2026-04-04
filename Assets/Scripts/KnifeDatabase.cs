using System.Collections.Generic;
using UnityEngine;

public static class KnifeDatabase
{
    const string CatalogResourcePath = "KnifeCatalog";
    const string SelectedKnifeKey = "SELECTED_KNIFE";

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
                return "knife_0";

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
        EnsureLookup();

        if (!string.IsNullOrWhiteSpace(selectedId) && knifeLookup != null && knifeLookup.ContainsKey(selectedId))
            return selectedId;

        return DefaultKnifeId;
    }

    public static KnifeData GetSelectedKnife()
    {
        return GetKnifeByID(GetSelectedKnifeId());
    }

    public static void SelectKnife(string id)
    {
        KnifeData knife = GetKnifeByID(id);
        if (knife == null)
            return;

        PlayerPrefs.SetString(SelectedKnifeKey, knife.id);
        PlayerPrefs.Save();
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
}
