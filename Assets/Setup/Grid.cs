using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;

    private Tile[,] grid;

    private void Awake()
    {
        grid = new Tile[width,height];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity, transform);
                grid[x, y].SetPosition(new Vector2(x, y));
            }
        }
        AdjustCamera();
    }

    public Tile GetTile(Vector2 pos)
    {
        return grid[(int)pos.x, (int)pos.y];
    }

    public Vector2 GetCenter()
    {
        return new Vector2(width / 2 - 0.5f, height / 2 - 0.5f);
    }

    public void ClearGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y].ClearTile();
            }
        }
    }

    private void AdjustCamera()
    {
        Camera.main.transform.position = (Vector3)GetCenter() + Vector3.back;
    }
}
