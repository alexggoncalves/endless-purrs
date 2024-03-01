using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public Tile[] upNeighbours;
    public Tile[] downNeighbours;
    public Tile[] leftNeighbours;
    public Tile[] rightNeighbours;

    string pX, pY, nX, nY;

    public Tile()
    { }

    Tile(Tile[] upNeighbours, Tile[] downNeighbours, Tile[] leftNeighbours, Tile[] rightNeighbours)
    {
        this.upNeighbours = upNeighbours;
        this.downNeighbours = downNeighbours;
        this.leftNeighbours = leftNeighbours;
        this.rightNeighbours = rightNeighbours;
    }
}
