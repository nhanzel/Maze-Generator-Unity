using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject WallPrefab;

    public int Dimensions; //TODO adjust ot not just be a square
    private float width;
    private float length;
    private Cell[] grid;
    private int rowSize;
    private int columnSize;
    private float offset;
    private float scale;

    public void Start()
    {
        width = transform.localScale.x * 10;
        length = transform.localScale.z * 10;
        UnityEngine.Random.InitState(3466);
        GenerateMaze();
    }

    /// <summary>
    /// Generates a maze of the given dimensions
    /// </summary>
    private void GenerateMaze()
    {
        columnSize = Convert.ToInt32(Math.Floor(length / Dimensions));
        rowSize = Convert.ToInt32(Math.Floor(width / Dimensions));
        grid = new Cell[Dimensions * Dimensions];
        Stack stack = new Stack();
        offset = -(width / 2); //TODO adjust for non-square offsets

        int counter = 0;
        for (int r=0; r<Dimensions; r++) 
        { 
            for (int c=0; c<Dimensions; c++)
            {
                Cell newCell = new Cell(r, c, counter, Dimensions);
                grid[counter++] = newCell;
            }
        }

        for (int i = 0; i < grid.Length; i++)
        {
            CreateWalls(i);
        }

        Cell current = grid[0];
        current.visited = true;
        int numVisited = 1;

        while (numVisited < grid.Length)
        {
            Cell next = CheckNeighbors(current.i);
            if (next != null)
            {
                next.visited = true;
                numVisited++;
                stack.Push(current);
                RemoveWalls(current, next);
                current = next;
            }
            else if (stack.Count > 0)
            {
                current = (Cell)stack.Pop();
            }
        }
    }

    /// <summary>
    /// Generate four walls around a given cell
    /// </summary>
    /// <param name="cellIndex"></param>
    private void CreateWalls(int cellIndex)
    {
        GameObject newTopWall = Instantiate(WallPrefab);
        newTopWall.transform.parent = transform;
        newTopWall.transform.localPosition = new Vector3((grid[cellIndex].r * rowSize) + .5f + offset, .5f, (grid[cellIndex].c * columnSize) + 1f + offset);

        GameObject newRightWall = Instantiate(WallPrefab);
        newRightWall.transform.parent = transform;
        newRightWall.transform.localPosition = new Vector3((grid[cellIndex].r * rowSize) + 1f + offset, .5f, (grid[cellIndex].c * columnSize) + .5f + offset);
        newRightWall.transform.localRotation = Quaternion.Euler(0, 90, 0);

        GameObject newBottomWall = Instantiate(WallPrefab);
        newBottomWall.transform.parent = transform;
        newBottomWall.transform.localPosition = new Vector3((grid[cellIndex].r * rowSize) + .5f + offset, .5f, (grid[cellIndex].c * columnSize) + offset);

        GameObject newLeftWall = Instantiate(WallPrefab);
        newLeftWall.transform.parent = transform;
        newLeftWall.transform.localPosition = new Vector3((grid[cellIndex].r * rowSize) + offset, .5f, (grid[cellIndex].c * columnSize) + .5f + offset);
        newLeftWall.transform.localRotation = Quaternion.Euler(0, 90, 0);
    }

    /// <summary>
    /// Determines which walls needs to be removed between two cells
    /// </summary>
    /// <param name="current"></param>
    /// <param name="next"></param>
    private void RemoveWalls(Cell current, Cell next)
    {
        List<GameObject> toBeRemoved = new List<GameObject>();

        int rDiff = current.r - next.r;
        Debug.Log(rDiff);
        if (rDiff == 1)
        {
            //bottom of current, top of next
            foreach (Transform child in transform)
            {
                if (child.localPosition == new Vector3((current.c * rowSize) + .5f + offset, .5f, (current.r * columnSize) + offset))
                {
                    toBeRemoved.Add(child.gameObject);
                }
            }
        }
        if (rDiff == -1)
        {
            //top of current, bottom of next
            foreach (Transform child in transform)
            {
                if (child.localPosition == new Vector3((current.c * rowSize) + .5f + offset, .5f, (current.r * columnSize) + 1f + offset))
                {
                    toBeRemoved.Add(child.gameObject);
                }
            }
        }
        int cDiff = current.c - next.c;
        Debug.Log(cDiff);
        if (cDiff == 1)
        {
            //left of current, right of next
            foreach (Transform child in transform)
            {
                if (child.localPosition == new Vector3((current.c * rowSize) + offset, .5f, (current.r * columnSize) + .5f + offset))
                {
                    toBeRemoved.Add(child.gameObject);
                }
            }
        }
        if (cDiff == -1)
        {
            //right of current, left of next
            foreach (Transform child in transform)
            {
                if (child.localPosition == new Vector3((current.c * rowSize) + 1f + offset, .5f, (current.r * columnSize) + .5f + offset))
                {
                    toBeRemoved.Add(child.gameObject);
                }
            }
        }

        foreach (GameObject g in toBeRemoved)
        {
            Destroy(g);
        }
    }

    /// <summary>
    /// Checks all the neighbors of a given cell and returns a random unvisited neighbor
    /// </summary>
    /// <param name="cell"></param>
    /// <returns>an unvisited neighbor</returns>
    private Cell CheckNeighbors(int cell)
    {
        int r = grid[cell].r;
        int c = grid[cell].c;
        List<Cell> unvisitedNeighbors = new List<Cell>();

        if (Index(r + 1, c) != -1 && !grid[Index(r + 1, c)].visited)
        {
            unvisitedNeighbors.Add(grid[Index(r + 1, c)]);
        }
        if (Index(r, c + 1) != -1 && !grid[Index(r, c + 1)].visited)
        {
            unvisitedNeighbors.Add(grid[Index(r, c + 1)]);
        }
        if (Index(r - 1, c) != -1 && !grid[Index(r - 1, c)].visited)
        {
            unvisitedNeighbors.Add(grid[Index(r - 1, c)]);
        }
        if (Index(r, c - 1) != -1 && !grid[Index(r, c - 1)].visited)
        {
            unvisitedNeighbors.Add(grid[Index(r, c - 1)]);
        }

        if (unvisitedNeighbors.Count > 0)
        {
            return unvisitedNeighbors[UnityEngine.Random.Range(0, unvisitedNeighbors.Count)];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Determines the index of a cell in the grid based on its row and column
    /// </summary>
    /// <param name="_r"></param>
    /// <param name="_c"></param>
    /// <returns>the index of a given row and column, returns -1 if no such cell exists</returns>
    private int Index(int _r, int _c)
    {
        if (_r < 0 || _c < 0 || _r > Dimensions - 1 || _c > Dimensions - 1)
        {
            return -1;
        }
        return Convert.ToInt32((_r * Dimensions) + _c); 
    }
}

public class Cell
{
    public int r { get; set; }
    public int c { get; set; }
    public int i { get; set; }

    public float w { get; set; }
    public bool visited { get; set; }

    public Cell(int _r, int _c, int _i, float _size)
    {
        r = _r;
        c = _c;
        i = _i;
        w = _size;
        visited = false;
    }
}

//-------------------------------------------------------------------------//

public class MazeParams
{
    public int width { get; set; }

    public int height { get; set; }

    public int seed { get; set; } = 0;

    public bool useWindows { get; set; } = false;

    public double windowProbability { get; set; } = 0.0;

    public Algorithm algorithm { get; set; }

    public bool useRooms { get; set; } = false;

    public double roomProbability { get; set; } = 0.0;

    public bool fixedPlaneSize { get; set; } = false;
}

public enum Algorithm
{
    DepthFirst,
    BreadthFirst
}
