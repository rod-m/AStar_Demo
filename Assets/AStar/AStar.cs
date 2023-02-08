using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class AStar : MonoBehaviour
{
    private IEnumerator pathFinder;

    [SerializeField] private Tilemap obstacleMap;
    [SerializeField] private Tilemap targetMap;
    [SerializeField] private Tilemap pathMap;
    [SerializeField] private Tilemap groundMap;

    // use the tile map grid for search
    [SerializeField] private Grid grid;

    [FormerlySerializedAs("start")] [SerializeField]
    private Node startNode;

    [SerializeField] private Node targetNode;
    [SerializeField] private Tile pathTile;
    [SerializeField] private Tile startTile;
    [SerializeField] private Tile checkTile;
    [SerializeField] private Tile visitedTile;

    [SerializeField] private Tile targetTile;
    private Node[,] fullNodeGrid;
    private List<Node> openList = new List<Node>();
    private List<Node> closedeList = new List<Node>();
    private List<Node> allWalkableNodeList = new List<Node>();
    private int maxGridX = 0;
    private int maxGridY = 0;
    private int AStarCost = 0; // count cycles
    void Start()
    {
        

        BoundsInt bounds = groundMap.cellBounds;
        Vector3Int pos = new Vector3Int(bounds.position.x, bounds.position.y, 0);
        targetMap.SetTile(startNode.Location, startTile);

        maxGridX = bounds.size.x;
        maxGridY = bounds.size.y;
        Debug.Log($"start {startNode.Location} {maxGridX} {maxGridY}");
        fullNodeGrid = new Node[maxGridX, maxGridY];
        
        for (int i = 0; i < maxGridX; i++)
        {
            for (int j = 0; j < maxGridY; j++)
            {
                var node = new Node(new Vector3Int(i, j, 0));
                var obstacle = obstacleMap.GetTile(node.Location);
                node.Walkable = false;
                if (obstacle != null)
                {
                    node.Walkable = false;
                    //Debug.Log("un walkable!");
                }
                else
                {
                    var ground = groundMap.GetTile(node.Location);
                    if (ground != null)
                    {
                        node.Walkable = true;
                        allWalkableNodeList.Add(node);
                    }
                }
                
                var target = targetMap.GetTile(node.Location);
                if (target != null)
                {
                    targetNode = node;
                    //Debug.Log("Found Target");
                }
                
                fullNodeGrid[i, j] = node;
            }
        }
        if (targetNode == null)
        {
            Debug.LogError("Target Not FOUND!");
            return;
        }

        foreach (var node in allWalkableNodeList)
        {
            
            node.SetDistance(targetNode.Location);
        }
        startNode = fullNodeGrid[startNode.X, startNode.Y];
        startNode.Walkable = false;
        startNode.G = 0;
        startNode.ParentNode = null;
        //startNode.SetDistance(targetNode.Location);
        openList.Add(startNode);
        pathFinder = PathFinderUpdater();
       
        StartCoroutine(pathFinder);
    }

    void DrawPath(Node node)
    {
        while (true)
        {
            pathMap.SetTile(node.Location, pathTile);
            node = node.ParentNode;
            if (node == null)
            {
                return;
            }
        }
    }

    IEnumerator PathFinderUpdater()
    {
        while (openList.Any())
        {
            var currentNode = openList.OrderBy(x => x.F).First();
            
            if (currentNode.Location == targetNode.Location)
            {
                Debug.Log("**************************************");
                Debug.Log($"**** Found Target in {AStarCost}!****");
                Debug.Log("**************************************");
      
                DrawPath(currentNode);
                StopCoroutine(pathFinder);
            }
            else
            {
                // draw tile

                closedeList.Add(currentNode);
                openList.Remove(currentNode);
                var walkableTiles = GetNeighbors(currentNode);
                
                foreach (var neighbor in walkableTiles)
                {
                    AStarCost++;
           
                    int tentativeG = currentNode.G + neighbor.D;
                   
                    if (tentativeG < neighbor.G)
                    {
                        neighbor.ParentNode = currentNode;
                        neighbor.G = tentativeG;
                        neighbor.F = tentativeG + neighbor.H;
                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                        
                    }

                   
                    //Debug.Log($"neighbor {neighbor.Location} F: {neighbor.F} H: {neighbor.H} G {neighbor.G}");


                    pathMap.SetTile(neighbor.Location, checkTile);
                   
                }
                Debug.Log($"Visiting {currentNode.Location} F: {currentNode.F} G: {currentNode.G} H: {currentNode.H}");
                pathMap.SetTile(currentNode.Location, visitedTile);
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    private List<Node> GetNeighbors(Node currentNode)
    {
        var neighbours = new List<Node>();
        Vector3Int cur = currentNode.Location;
        Node aNode;
        if (cur.x + 1 < maxGridX)
        {
            aNode = fullNodeGrid[cur.x + 1, cur.y]; // R
            aNode.D = 10;
            neighbours.Add(aNode);
        }

        if (cur.x + 1 < maxGridX && cur.y + 1 < maxGridY)
        {
            aNode = fullNodeGrid[cur.x + 1, cur.y + 1]; // TR
            aNode.D = 14;
            neighbours.Add(aNode);
        }

        if (cur.y + 1 < maxGridY)
        {
            aNode = fullNodeGrid[cur.x, cur.y + 1]; // T
            aNode.D = 10;
            neighbours.Add(aNode);
        }

        if (cur.x - 1 >= 0 && cur.y + 1 < maxGridY)
        {
            aNode = fullNodeGrid[cur.x - 1, cur.y + 1]; // TL
            aNode.D = 14;
            neighbours.Add(aNode);
        }

        if (cur.x - 1 >= 0)
        {
            aNode = fullNodeGrid[cur.x - 1, cur.y]; // L
            aNode.D = 10;
            neighbours.Add(aNode);
        }

        if (cur.x - 1 >= 0 && cur.y - 1 >= 0)
        {
            aNode = fullNodeGrid[cur.x - 1, cur.y - 1]; // BL
            aNode.D = 14;
            neighbours.Add(aNode);
        }

        if (cur.y - 1 >= 0)
        {
            aNode = fullNodeGrid[cur.x, cur.y - 1]; // B
            aNode.D = 10;
            neighbours.Add(aNode);
        }

        if (cur.x + 1 < maxGridX && cur.y - 1 >= 0)
        {
            aNode = fullNodeGrid[cur.x + 1, cur.y - 1]; // BR
            aNode.D = 14;
            neighbours.Add(aNode);
        }

        var possible = new List<Node>();
        foreach (var node in neighbours)
        {
            if (!node.Walkable) continue;
            
            possible.Add(node);
        }

        return possible;
    }
}