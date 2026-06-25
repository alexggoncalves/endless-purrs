using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TileBitmask
{
    public ulong m0, m1, m2, m3;

    public void Clear() { m0 = m1 = m2 = m3 = 0; }
    
    public void Set(int id)
    {
        if (id < 64) m0 |= (1UL << id);
        else if (id < 128) m1 |= (1UL << (id - 64));
        else if (id < 192) m2 |= (1UL << (id - 128));
        else if (id < 256) m3 |= (1UL << (id - 192));
    }

    public void Unset(int id)
    {
        if (id < 64) m0 &= ~(1UL << id);
        else if (id < 128) m1 &= ~(1UL << (id - 64));
        else if (id < 192) m2 &= ~(1UL << (id - 128));
        else if (id < 256) m3 &= ~(1UL << (id - 192));
    }

    public readonly bool IsSet(int id)
    {
        if (id < 64) return (m0 & (1UL << id)) != 0;
        if (id < 128) return (m1 & (1UL << (id - 64))) != 0;
        if (id < 192) return (m2 & (1UL << (id - 128))) != 0;
        if (id < 256) return (m3 & (1UL << (id - 192))) != 0;
        return false;
    }

    public readonly int Count
    {
        get
        {
            return PopCount(m0) + PopCount(m1) + PopCount(m2) + PopCount(m3);
        }
    }

    public readonly bool Equals(TileBitmask other)
    {
        return m0 == other.m0 && m1 == other.m1 && m2 == other.m2 && m3 == other.m3;
    }

    public readonly bool IsEmpty => (m0 | m1 | m2 | m3) == 0;

    public static TileBitmask Or(TileBitmask a, TileBitmask b)
    {
        return new TileBitmask { m0 = a.m0 | b.m0, m1 = a.m1 | b.m1, m2 = a.m2 | b.m2, m3 = a.m3 | b.m3 };
    }

    public static TileBitmask And(TileBitmask a, TileBitmask b)
    {
        return new TileBitmask { m0 = a.m0 & b.m0, m1 = a.m1 & b.m1, m2 = a.m2 & b.m2, m3 = a.m3 & b.m3 };
    }  

    public readonly int GetFirstSetBit()
    {
        if (m0 != 0) return TrailingZeroCount(m0);
        if (m1 != 0) return 64 + TrailingZeroCount(m1);
        if (m2 != 0) return 128 + TrailingZeroCount(m2);
        if (m3 != 0) return 192 + TrailingZeroCount(m3);
        return -1;
    }

    private static int PopCount(ulong x)
    {
        x -= (x >> 1) & 0x5555555555555555UL;
        x = (x & 0x3333333333333333UL) + ((x >> 2) & 0x3333333333333333UL);
        x = (x + (x >> 4)) & 0x0f0f0f0f0f0f0f0fUL;
        return (int)((x * 0x0101010101010101UL) >> 56);
    }

    public static int TrailingZeroCount(ulong v)
    {
        if (v == 0) return 64;
        int n = 0;
        if ((v & 0xFFFFFFFFUL) == 0) { n += 32; v >>= 32; }
        if ((v & 0xFFFFUL) == 0) { n += 16; v >>= 16; }
        if ((v & 0xFFUL) == 0) { n += 8; v >>= 8; }
        if ((v & 0xFUL) == 0) { n += 4; v >>= 4; }
        if ((v & 0x3UL) == 0) { n += 2; v >>= 2; }
        if ((v & 0x1UL) == 0) { n += 1; }
        return n;
    }
}

public class Cell : MonoBehaviour
{
    public bool collapsed;
    public int x, y;
    public TileBitmask tileOptionsMask;
    public GameObject tileInstance;
    public bool preset = false;

    TileLoader tileLoader;
    public GameObject decoration;

    public void CreateCell(bool collapseState, int x, int y, TileLoader tileLoader)
    {
        this.x = x;
        this.y = y;

        collapsed = collapseState;

        this.tileLoader = tileLoader;

        tileOptionsMask = tileLoader.GetPossibleTilesMask();

        tileInstance = null;
        decoration = null;
    }

    public void RecreateCell(TileBitmask mask)
    {
        tileOptionsMask = mask;
    }

    public GameObject InstantiateTile()
    {
        Tile tile = tileLoader.GetTileByID(tileOptionsMask.GetFirstSetBit());
        GameObject selectedOption = tile.SelectOption();
        tileInstance = Instantiate(selectedOption, transform.position, Quaternion.Euler(0, selectedOption.transform.rotation.y + 90 * tile.rotation, 0));
        return tileInstance;
    }

    public void ResetCell()
    {
        collapsed = false;

        tileOptionsMask = tileLoader.GetPossibleTilesMask();

        tileInstance = null;
        decoration = null;
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public void SetDecoration(GameObject decoration)
    {
        this.decoration = decoration;
    }
}


