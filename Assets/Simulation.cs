using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Simulation : MonoBehaviour
{
    [SerializeField]
    private int width = 1000;
    [SerializeField]
    private int height = 1000;
    [SerializeField]
    private int brushSize = 10;
    [SerializeField]
    private int gravity = 10;

    private Texture2D texture;
    private float horizontalBorder;
    private float verticalBorder;

    private Color32[] pixels;
    private Cell[] cells;

    private Color32 BLACK = Color.black;
    private Color32 YELLOW = Color.yellow;

    private enum Cell { Empty, Sand }

    private void Awake()
    {
        RawImage image = GetComponent<RawImage>();
        texture = new Texture2D(width, height);
        image.texture = texture;

        horizontalBorder = (Screen.width - 1000) / 2f;
        verticalBorder = (Screen.height - 1000) / 2f;

        pixels = texture.GetPixels32();
        cells = new Cell[pixels.Length];
    }

    private void Start()
    {
        ClearTexture();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2Int selectedPixel = SelectedPixel();

            if (selectedPixel.x > 0 && selectedPixel.x < width &&
                selectedPixel.y > 0 && selectedPixel.y < height)
            {
                AddSand(SelectedPixel());
            }
        }
        MoveSand();

        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private void ClearTexture()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                SetPixel(x, y, Cell.Empty);
            }
        }
    }

    private void AddSand(Vector2Int position)
    {
        if (brushSize == 1)
        {
            SetPixel(position.x, position.y, Cell.Sand);
        }
        else
        {
            for (int x = position.x - brushSize / 2; x < position.x + brushSize / 2; x++)
            {
                for (int y = position.y - brushSize / 2; y < position.y + brushSize / 2; y++)
                {
                    SetPixel(x, y, Cell.Sand);
                }
            }
        }
    }

    private void MoveSand()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (GetPixel(x, y) == Cell.Sand)
                {
                    bool success = false;
                    for (int targetY = y - gravity; targetY < y && !success; targetY++)
                    {
                        success = TryMoveSand(x, y, x, targetY);
                        if (!success)
                        {
                            success = TryMoveSand(x, y, x - 1, targetY);
                            if (!success)
                            {
                                success = TryMoveSand(x, y, x + 1, targetY);
                            }
                        }
                    }
                }
            }
        }
    }

    private Vector2Int SelectedPixel()
    {
        return new Vector2Int(
            Mathf.RoundToInt((Input.mousePosition.x - horizontalBorder) * width / 1000),
            Mathf.RoundToInt((Input.mousePosition.y - verticalBorder) * height / 1000));
    }

    private Cell GetPixel(int x, int y)
    {
        if (x + y * width < 0 || x + y * width >= cells.Length)
        {
            Debug.LogError($"{x},{y}");
        }
        return cells[x + y * width];
    }

    private void SetPixel(int x, int y, Cell cell)
    {
        cells[x + y * width] = cell;
        pixels[x + y * width] = cell == Cell.Sand ? YELLOW : BLACK;
    }

    private bool TryMoveSand(int originX, int originY, int targetX, int targetY)
    {
        bool success = targetX >= 0 && targetY >= 0 && GetPixel(targetX, targetY) == Cell.Empty;
        if (success)
        {
            SetPixel(originX, originY, Cell.Empty);
            SetPixel(targetX, targetY, Cell.Sand);
        }
        return success;
    }
}
