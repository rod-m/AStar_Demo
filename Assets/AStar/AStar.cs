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
    [FormerlySerializedAs("start")] [SerializeField] private Node startNode;
    [SerializeField] private Node targetNode;
    [SerializeField] private Tile pathTile;
    [SerializeField] private Tile startTile;
    [SerializeField] private Tile checkTile;
    [SerializeField] private Tile visitedTile;

    [SerializeField] private Tile targetTile;
    private Node[,] fullNodeGrid;
    private List<Node> openList = new List<Node>();
    private List<Node> closedeList = new List<Node>();
    private int maxGridX = 0;
    private int maxGridY = 0;
    private int cycleCount = 0;
    void Start()
    {
        startNode.G = 0;
        startNode.SetDistance(targetNode.Location);
        openList.Add(startNode);
        
        BoundsInt bounds = groundMap.cellBounds;
        Vector3Int pos = new Vector3Int(bounds.position.x, bounds.position.y, 0);
        targetMap.SetTile(startNode.Location, startTile);
        targetMap.SetTile(targetNode.Location, targetTile);
        maxGridX = bounds.size.x;
        maxGridY = bounds.size.y;
        Debug.Log($"start {startNode.Location} {maxGridX} {maxGridY}");
        fullNodeGrid = new Node[maxGridX,maxGridY];
        for(int i=0; i<groundMap.size.x; i++)
        {
            for(int j=0; j < groundMap.size.y; j++)
            {
                
                fullNodeGrid[i,j] = new Node(new Vector3Int(i, j, 0), targetNode.Location, Int32.MaxValue, null);
            }
        }
        pathFinder = PathFinderUpdater();
        StartCoroutine(pathFinder);
    }

   
    IEnumerator PathFinderUpdater()
    {
        while (openList.Any())
        {
            var currentNode = openList.OrderBy(x => x.F).First();
            pathMap.SetTile(currentNode.Location, visitedTile);
            if (currentNode.Location == targetNode.Location)
            {
                Debug.Log("Found Target!");
                StopCoroutine(pathFinder);
            }
            else
            {
                
                // draw tile
                
                closedeList.Add(currentNode);
                openList.Remove(currentNode);
                var walkableTiles = GetWalkableTiles(currentNode);
                Debug.Log($"{cycleCount} Visiting {currentNode.Location} F: {currentNode.F} H: {currentNode.H}");
                foreach (var neighbor in walkableTiles)
                {

                    cycleCount++;
                    Debug.Log($"neighbor {neighbor.Location} F: {neighbor.F} H: {neighbor.H} G {neighbor.G}");
                    //It's already in the active list, but that's OK, maybe this new tile has a better value (e.g. We might zigzag earlier but this is now straighter). 
                    if(openList.Any(x => x.X == neighbor.X && x.Y == neighbor.Y))
                    {
                        var existingTile = openList.First(x => x.X == neighbor.X && x.Y == neighbor.Y);
                        if(existingTile.F > currentNode.F)
                        {
                            openList.Remove(existingTile);
                            openList.Add(neighbor);
                        }
                    }else
                    {
                        //We've never seen this tile before so add it to the list. 
                        openList.Add(neighbor);
                        
                    }
                    pathMap.SetTile(neighbor.Location, checkTile);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private List<Node> GetWalkableTiles(Node currentNode)
    {
        var neighbours = new List<Node>();
        Vector3Int cur = currentNode.Location;
        Node aNode;
        if (cur.x + 1 <= maxGridX)
        {
            aNode = fullNodeGrid[cur.x + 1, cur.y]; // R
            aNode.ParentNode = currentNode;
            aNode.G = currentNode.G + 10;
            neighbours.Add(aNode);
        }

        if (cur.x + 1 <= maxGridX && cur.y + 1 < maxGridY)
        {
            aNode = fullNodeGrid[cur.x + 1, cur.y + 1]; // TR
            aNode.ParentNode = currentNode;
            aNode.G = currentNode.G + 14;
            neighbours.Add(aNode);
        }

        if (cur.y + 1 < maxGridY)
        {
            aNode = fullNodeGrid[cur.x, cur.y + 1]; // T
            aNode.ParentNode = currentNode;
            aNode.G = currentNode.G + 10;
            neighbours.Add(aNode);
        }

        if (cur.x - 1 >= 0 && cur.y + 1 < maxGridY)
        {
            aNode = fullNodeGrid[cur.x - 1, cur.y + 1]; // TL
            aNode.ParentNode = currentNode;
            aNode.G = currentNode.G + 14;
            neighbours.Add(aNode);
        }

        if (cur.x - 1 >= 0)
        {
            aNode = fullNodeGrid[cur.x - 1, cur.y]; // L
            aNode.ParentNode = currentNode;
            aNode.G = currentNode.G + 10;
            neighbours.Add(aNode);
        }

        if (cur.x - 1 >= 0 && cur.y - 1 >= 0)
        {
            aNode = fullNodeGrid[cur.x - 1, cur.y - 1]; // BL
            aNode.ParentNode = currentNode;
            aNode.G = currentNode.G + 10;
            neighbours.Add(aNode);
        }
        if (cur.y - 1 >= 0)
        {
            aNode = fullNodeGrid[cur.x, cur.y - 1]; // B
            aNode.ParentNode = currentNode;
            aNode.G = currentNode.G + 10;
            neighbours.Add(aNode);
        }
        if (cur.x + 1 <= maxGridX && cur.y - 1 >= 0)
        {
            aNode = fullNodeGrid[cur.x + 1, cur.y - 1]; // BR
            aNode.ParentNode = currentNode;
            aNode.G = currentNode.G + 10;
            neighbours.Add(aNode);
        }
        var possible = new List<Node>();
        foreach (var node in neighbours)
        {
            var obstacle = obstacleMap.GetTile(node.Location);
          
            if (obstacle != null)
            {
                Debug.Log("un walkable!");
                continue;
                
            }
            
            //Debug.Log($"node f {node.F},{node.G},{node.H} current f: {currentNode.F},{currentNode.G},{currentNode.H}");
            if(node.H > currentNode.H) continue;  // is further away!
            
            possible.Add(node);
   
        }

        return possible;
    }
}