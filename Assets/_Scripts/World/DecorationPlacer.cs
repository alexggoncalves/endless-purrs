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
    public float decorationRate = 0.5f;
    public float TotalElementWeight { get; set; }
}

public class DecorationPlacer : MonoBehaviour
{
    [SerializeField] private Biome[] biomes;
    private Transform decorationContainer;

    public float CellScale { get; set; } = 2f;

    private void Start()
    {
        decorationContainer = new GameObject("Decoration Instance Container").transform;

        // Calculate total weight for all biomes
        foreach (Biome biome in biomes)
        {
            float totalWeight = 0;
            foreach (NatureElement element in biome.elements)
                totalWeight += element.weight;
            biome.TotalElementWeight = totalWeight;
        }
    }

    public void TryDecorate(Cell cell, Tile tile)
    {
        Biome biome = GetBiomeForTile(tile.tileType);
        if (biome == null) return;

        if (Random.value >= biome.decorationRate) return;

        PlaceElement(
            cell,
            biome,
            tile.tileType
        );
    }

    private void PlaceElement(Cell cell, Biome biome, TileType tileType)
    {
        Vector3 basePos = cell.transform.position;

        // Choose decoration type
        NatureElement elementType = WeightedSelectElementType(biome);
        if (elementType?.prefabs == null || elementType.prefabs.Length == 0) return;

        // Choose decoration prefab from selected type
        GameObject elementPrefab = elementType.prefabs[Random.Range(0, elementType.prefabs.Length)];

        // Randomize rotation
        Quaternion rotation = Quaternion.Euler(
            elementPrefab.transform.rotation.eulerAngles.x,
            Random.Range(0, 360),
            0);

        // Get y offset based on tile height level
        float yOffset = tileType switch
        {
            TileType.grass_L1 => 1.6f,
            TileType.grass_L2 => 2.8f,
            TileType.sand => -0.1f,
            _ => 0
        };

        // Randomize offset
        Vector3 offset = new(
            Random.Range(-0.4f * CellScale, 0.4f * CellScale),
            yOffset,
            Random.Range(-0.4f * CellScale, 0.4f * CellScale));

        // Instantiate decoration
        GameObject instance = Instantiate(elementPrefab, basePos + offset, rotation);
        instance.transform.SetParent(decorationContainer, true);

        cell.SetDecoration(instance);
    }

    private Biome GetBiomeForTile(TileType tileType)
    {
        BiomeType? type = tileType switch
        {
            TileType.grass or TileType.grass_L1 or TileType.grass_L2 => BiomeType.Forest,
            TileType.sand => BiomeType.Beach,
            _ => null
        };

        if (type == null) return null;

        foreach (Biome biome in biomes)
            if (biome.type == type.Value)
                return biome;

        return null;
    }

    private NatureElement WeightedSelectElementType(Biome biome)
    {
        if (biome.elements == null || biome.elements.Length == 0) return null;
        if (biome.TotalElementWeight <= 0f) return null;

        // Perform random weighted selection
        float diceRoll = Random.Range(0f, biome.TotalElementWeight);
        float cumulative = 0f;
        for (int i = 0; i < biome.elements.Length; i++)
        {
            cumulative += biome.elements[i].weight;
            if (diceRoll < cumulative)
            {
                return biome.elements[i];
            }
        }

        return biome.elements[^1]; // return last element
    }

    private void OnDestroy()
    {
        if (decorationContainer != null)
            Destroy(decorationContainer.gameObject);
    }
}

