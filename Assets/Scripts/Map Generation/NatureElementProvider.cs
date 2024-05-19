using UnityEngine;

public class NatureElementPlacer : MonoBehaviour
{
    public enum ElementType
    {
        Tree,
        Rock,
        Grass,
        Bush,
        Flower
    }

    public enum BiomeType
    {
        Forest,
        DeadForest,
        Beach
    }

    [System.Serializable]
    public class NatureElement
    {
        public ElementType type;
        public GameObject[] prefabs;
        [Min(0)] public float weight;
    }

    [System.Serializable,]
    public class Biome
    {
        public BiomeType type;
        public NatureElement[] elements;
    }

    public Biome[] biomes;

    public GameObject PlaceElement(Vector3 position, float cellScale, BiomeType biomeType)
    {
        NatureElement[] elements = GetBiomeElements(biomeType);

        NatureElement elementType = WeightedSelectElementType(elements);

        int variationIndex = (int) Mathf.Floor(Random.Range(0, elementType.prefabs.Length));
        
        GameObject elementPrefab = elementType.prefabs[variationIndex];

        Quaternion rotation = Quaternion.Euler(elementPrefab.transform.rotation.eulerAngles.x, Random.Range(0, 360), 0);
        Vector3 offset = new(Random.Range(-0.4f * cellScale, 0.4f * cellScale), 0, Random.Range(-0.4f * cellScale, 0.4f * cellScale));

        GameObject instance = Instantiate(elementPrefab, position + offset, rotation);
        instance.transform.SetParent(transform, false);
        return instance;
    }

    private NatureElement WeightedSelectElementType(NatureElement[] elements)
    {
        float totalWeight = 0;
        foreach (NatureElement element in elements)
        {
            totalWeight += element.weight;
        }

        float diceRoll = Random.Range(0, totalWeight);

        float cumulative = 0f;
        for (int i = 0; i < elements.Length; i++)
        {
            cumulative += elements[i].weight;
            if (diceRoll < cumulative)
            {
                return elements[i]; ;
            }
        }
        return null;
    }

    private NatureElement[] GetBiomeElements(BiomeType biomeType)
    {
        foreach(Biome biome in biomes) 
        { 
            if(biome.type == biomeType)
            {
                return biome.elements;
            }
        }
        return new NatureElement[0];
    }
}

