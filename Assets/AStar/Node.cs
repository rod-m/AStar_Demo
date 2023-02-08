using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Node
{

    [SerializeField] private Vector3Int location;
    public Vector3Int Location
    {
        get => location; set => location = value;
    }

    public int X => location.x;
    public int Y => location.y;
    public int G { get; set; }
    public int H { get; private set; }
    public int F => this.G + this.H;

    public Node ParentNode
    {
        get;
        set;
    }

    public Node()
    {
        G = Int32.MaxValue; // start at infinity
    }
    public Node(Vector3Int _pos, Vector3Int _target, int gCost, Node _parent)
    {
        Location = _pos;
        ParentNode = _parent;
        G = gCost;
        SetDistance(_target);
    }

    //The distance is essentially the estimated distance, ignoring walls to our target. 
    //So how many tiles left and right, up and down, ignoring walls, to get there. 
    public void SetDistance(Vector3Int target)
    {
        // H = Mathf.RoundToInt(Vector3Int.Distance(Location, target));
        H = 10 * (Mathf.Abs(Location.x - target.x) + Mathf.Abs(Location.y - target.y));
    }
}