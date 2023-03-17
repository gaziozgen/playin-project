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
        return content.text[0];
    }

    public bool IsEmpty()
    {
        return content.text == default;
    }

    public void SetChar(char text)
    {
        gameObject.name = text.ToString();
        c = text;
        content.text = text.ToString();
        gameObject.SetActive(true);
    }

    public void ClearTile()
    {
        content.text = default;
        gameObject.SetActive(false);
    }

    public void SetPosition(Vector2 position)
    {
        Position= position;
    }
}
