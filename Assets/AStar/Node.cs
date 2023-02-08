using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Node
{
    public string Name => $"node_{X},{Y}";
    public bool Walkable { get; set; }
    public int X;
    public int Y;
    public int F => this.G + this.H;
    public int G { get; set; }
    public int H { get; private set; }
    public int D = 10;

    public Vector3Int Location
    {
        get
        {
            return new Vector3Int(X, Y);
        }
     
    }

    public Node ParentNode
    {
        get;
        set;
    }

    public Node()
    {
        G = Int32.MaxValue; // start at infinity
    }
    public Node(Vector3Int _pos, Vector3Int _target, int gCost)
    {
        X = _pos.x;
        Y = _pos.y;

        G = gCost;
        SetDistance(_target);
    }

    //The distance is essentially the estimated distance, ignoring walls to our target. 
    //So how many tiles left and right, up and down, ignoring walls, to get there. 
    public void SetDistance(Vector3Int target)
    {
        H = Mathf.RoundToInt(Vector3Int.Distance(Location, target));
        //H = 10 * (Mathf.Abs(X - target.x) + Mathf.Abs(Y - target.y));
    }
}