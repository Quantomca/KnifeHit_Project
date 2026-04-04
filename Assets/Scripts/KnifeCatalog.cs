using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Knife Hit/Knife Catalog", fileName = "KnifeCatalog")]
public class KnifeCatalog : ScriptableObject
{
    public string defaultKnifeId = "knife_0";
    public List<KnifeData> knives = new List<KnifeData>();
}
