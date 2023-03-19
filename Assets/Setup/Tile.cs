using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI content;
    public Vector2 Position { get; private set; }
    private char c;

    public char GetChar()
    {
        return c;
    }

    public bool IsEmpty()
    {
        return content.text == default;
    }

    public void SetChar(char text)
    {
        c = text;
        content.text = c.ToString();
        gameObject.SetActive(true);
    }

    public void ClearTile()
    {
        c = default;
        content.text = default;
        gameObject.SetActive(false);
    }

    public void SetPosition(Vector2 position)
    {
        Position= position;
    }
}
