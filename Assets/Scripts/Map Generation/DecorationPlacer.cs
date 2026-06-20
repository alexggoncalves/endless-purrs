using UnityEngine;


public enum BiomeType
{
    Forest,
    Beach
}

public enum ElementType
{
    Tree,
    Rock,
    Grass,
    Bush,
    Flower
}

public class DecorationPlacer : MonoBehaviour
{
    [System.Serializable]
    public class NatureElement
    {
        public ElementType type;
        public GameObject[] prefabs;
        [Min(0)] public float weight;
    }

    [System.Serializable]
    public class Biome
    {
        public BiomeType type;
        public NatureElement[] elements;
    }

    public Biome[] biomes;

    [SerializeField] private float decorationRate = 0.5f;

    public GameObject PlaceElement(Vector3 position, float cellScale, BiomeType biomeType)
    {
        NatureElement[] elements = GetBiomeElements(biomeType);
        NatureElement elementType = WeightedSelectElementType(elements);

        if (elementType == null) return null;
        if (elementType.prefabs == null || elementType.prefabs.Length == 0) return null;

        int variationIndex = Random.Range(0, elementType.prefabs.Length);
        GameObject elementPrefab = elementType.prefabs[variationIndex];

        Quaternion rotation = Quaternion.Euler(
            elementPrefab.transform.rotation.eulerAngles.x,
            Random.Range(0, 360),
            0);
        Vector3 offset = new(
            Random.Range(-0.4f * cellScale, 0.4f * cellScale),
            0,
            Random.Range(-0.4f * cellScale, 0.4f * cellScale));

        GameObject instance = Instantiate(elementPrefab, position + offset, rotation);
        instance.transform.SetParent(transform, true);
        return instance;
    }

    private NatureElement WeightedSelectElementType(NatureElement[] elements)
    {
        if (elements == null || elements.Length == 0) return null;

        float totalWeight = 0;
        foreach (NatureElement element in elements)
        {
            totalWeight += element.weight;
        }

        if (totalWeight <= 0f) return null;

        float diceRoll = Random.Range(0f, totalWeight);

        float cumulative = 0f;
        for (int i = 0; i < elements.Length; i++)
        {
            cumulative += elements[i].weight;
            if (diceRoll < cumulative)
            {
                return elements[i]; ;
            }
        }

        return elements[^1]; // return last element
    }

    private NatureElement[] GetBiomeElements(BiomeType biomeType)
    {
        foreach (Biome biome in biomes)
        {
            if (biome.type == biomeType)
            {
                return biome.elements;
            }
        }
        return new NatureElement[0];
    }

    public float GetDecorationRate() { return decorationRate; }
}

