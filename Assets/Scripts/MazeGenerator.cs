using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject WallPrefab;

    [Header("Dimensions")]
    public int Rows = 10;
    public int Columns = 10;
    [Header("Walls")]
    public float WallHeight = 1f;
    [Range(.25f, 10f)]
    public float WallThickness = 1f;
    [Header("Seed")]
    public bool UseSeed = false;
    public int Seed = 0;

    private float width;
    private float length;
    private Cell[] grid;
    private float rowSize;
    private float columnSize;
    private float offset;
    private float rowScale;
    private float columnScale;
    private float wallHeight;
    private bool hasError;

    public void Start()
    {
        hasError = false;
        width = 10;
        length = 10;
        if (Rows < 1)
        {
            hasError = true;
            Debug.LogException(new Exception("the 'Rows' property is invalid in Maze Generator"));
        }
        if (Columns < 1)
        {
            hasError = true;
            Debug.LogException(new Exception("the 'Columns' property is invalid in Maze Generator"));
        }
        rowScale = 10f / Convert.ToSingle(Rows);
        columnScale = 10f / Convert.ToSingle(Columns);
        wallHeight = WallHeight / 2;
        if (UseSeed) 
        { 
            if (Seed < 1)
            {
                hasError = true;
                Debug.LogException(new Exception("the 'Seed' property is invalid in Maze Generator"));
            }
            try
            {
                UnityEngine.Random.InitState(Seed);
            }
            catch (Exception e)
            {
                hasError = true;
                Debug.LogException(e);
            }
        }
        if (WallHeight <= 0)
        {
            hasError = true;
            Debug.LogException(new Exception("the 'WallHeight' property is invalid in Maze Generator"));
        }
        if (!hasError)
        {
            GenerateMaze();
        }
    }

    /// <summary>
    /// Generates a maze of the given dimensions
    /// </summary>
    private void GenerateMaze()
    {
        columnSize = length / Columns;
        rowSize = width / Rows;
        grid = new Cell[Rows * Columns];
        Stack stack = new Stack();
        offset = -(width / 2); //TODO adjust for non-square offsets

        int counter = 0;
        for (int r=0; r<Rows; r++) 
        { 
            for (int c=0; c<Columns; c++)
            {
                Cell newCell = new Cell(r, c, counter);
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

        Vector3 scalar = newTopWall.transform.localScale;

        newTopWall.transform.parent = transform;
        newTopWall.transform.localPosition = new Vector3((grid[cellIndex].c * columnSize) + .5f + offset - ((1 - columnScale) / 2), wallHeight, (grid[cellIndex].r * rowSize) + 1f + offset - (1 - rowScale));
        newTopWall.transform.localScale = new Vector3(scalar.x * columnScale, WallHeight, scalar.z * WallThickness);

        GameObject newRightWall = Instantiate(WallPrefab);
        newRightWall.transform.parent = transform;
        newRightWall.transform.localPosition = new Vector3((grid[cellIndex].c * columnSize) + 1f + offset - (1 - columnScale), wallHeight, (grid[cellIndex].r * rowSize) + .5f + offset - ((1 - rowScale) / 2));
        newRightWall.transform.localRotation = Quaternion.Euler(0, 90, 0);
        newRightWall.transform.localScale = new Vector3(scalar.x * rowScale, WallHeight, scalar.z * WallThickness);

        GameObject newBottomWall = Instantiate(WallPrefab);
        newBottomWall.transform.parent = transform;
        newBottomWall.transform.localPosition = new Vector3((grid[cellIndex].c * columnSize) + .5f + offset - ((1 - columnScale) / 2), wallHeight, (grid[cellIndex].r * rowSize) + offset);
        newBottomWall.transform.localScale = new Vector3(scalar.x * columnScale, WallHeight, scalar.z * WallThickness);

        GameObject newLeftWall = Instantiate(WallPrefab);
        newLeftWall.transform.parent = transform;
        newLeftWall.transform.localPosition = new Vector3((grid[cellIndex].c * columnSize) + offset, wallHeight, (grid[cellIndex].r * rowSize) + .5f + offset - ((1 - rowScale) / 2));
        newLeftWall.transform.localRotation = Quaternion.Euler(0, 90, 0);
        newLeftWall.transform.localScale = new Vector3(scalar.x * rowScale, WallHeight, scalar.z * WallThickness);
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
                if (child.localPosition == new Vector3((current.c * columnSize) + .5f + offset - ((1 - columnScale) / 2), wallHeight, (current.r * rowSize) + offset))
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
                if (child.localPosition == new Vector3((current.c * columnSize) + .5f + offset - ((1 - columnScale) / 2), wallHeight, (current.r * rowSize) + 1f + offset - (1 - rowScale)))
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
                if (child.localPosition == new Vector3((current.c * columnSize) + offset, wallHeight, (current.r * rowSize) + .5f + offset - ((1 - rowScale) / 2)))
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
                if (child.localPosition == new Vector3((current.c * columnSize) + 1f + offset - (1 - columnScale), wallHeight, (current.r * rowSize) + .5f + offset - ((1 - rowScale) / 2)))
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
        if (_r < 0 || _c < 0 || _r > Rows - 1 || _c > Columns - 1)
        {
            return -1;
        }
        return Convert.ToInt32((_r * Columns) + _c); 
    }
}

public class Cell
{
    public int r { get; set; }
    public int c { get; set; }
    public int i { get; set; }
    public bool visited { get; set; }

    public Cell(int _r, int _c, int _i)
    {
        r = _r;
        c = _c;
        i = _i;
        visited = false;
    }
}