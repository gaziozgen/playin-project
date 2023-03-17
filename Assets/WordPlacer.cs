using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static WordPlacer;

public class WordPlacer : MonoBehaviour
{
    [SerializeField] private Grid grid = null;
    [SerializeField] private List<string> wordList = new List<string>();

    private List<Word> possibleWords { get => remainingWords.Where((word) => !placedWords.Peek().FailedWords.Contains(word)).ToList(); }
    private List<Word> remainingWords = new();
    private Stack<Word> placedWords = new();

    public class Word
    {
        public string word;
        public Vector2 StartPosition;
        public Vector2 Direction;
        public int RootIntersectionIndex = -1;
        public int IntersectionIndex = -1;
        public List<(int, int)> PossibleLinks = new List<(int, int)>();
        public List<Word> FailedWords = new List<Word>();

        public Word(string word)
        {
            this.word = word;
            StartPosition = Vector2.zero;
            Direction = Vector2.zero;
        }
    }

    public void Start()
    {
        Refresh();
    }



    public void Refresh()
    {
        ClearData();
        PlaceFistWord();

        while (remainingWords.Count > 0)
        {
            if (possibleWords.Count == 0)
            {
                if (placedWords.Count == 1) break;
                Word word = RemoveLastAddedWord();
                if (!TryDifferentConnectionWithRemovedWord(word))
                    placedWords.Peek().FailedWords.Add(word);
            }
            else
            {
                Word newWord = possibleWords[Random.Range(0, possibleWords.Count)];
                Word oldWord = placedWords.Peek();
                AssignConnectionsBetweenTwoWord(oldWord, newWord);

                if (!ConnectFromRandomLink(newWord))
                    placedWords.Peek().FailedWords.Add(newWord);
            }
        }
        if (placedWords.Count != wordList.Count) print("Unplaceable words!");
        else print("Success");
    }


    private bool TryDifferentConnectionWithRemovedWord(Word word)
    {
        bool success = false;
        while (word.PossibleLinks.Count > 0)
        {
            int index = Random.Range(0, word.PossibleLinks.Count);
            (int, int) link = word.PossibleLinks[index];
            word.PossibleLinks.RemoveAt(index);

            if (PlaceWordToOtherWord(placedWords.Peek(), word, link.Item1, link.Item2))
                success = true; break;
        }
        return success;

    }

    private bool ConnectFromRandomLink(Word word)
    {
        bool success = false;
        while (word.PossibleLinks.Count > 0)
        {
            int index = Random.Range(0, word.PossibleLinks.Count);
            (int, int) link = word.PossibleLinks[index];
            word.PossibleLinks.RemoveAt(index);

            if (PlaceWordToOtherWord(placedWords.Peek(), word, link.Item1, link.Item2))
            {
                success = true;
                break;
            }
        }
        return success;
    }

    private void AssignConnectionsBetweenTwoWord(Word oldWord, Word newWord)
    {
        newWord.PossibleLinks.Clear();

        for (int i = 0; i < oldWord.word.Length; i++)
        {
            for (int j = 0; j < newWord.word.Length; j++)
            {
                if (newWord.word[j] == oldWord.word[i])
                    newWord.PossibleLinks.Add((i, j));
            }
        }
    }

    private Word RemoveLastAddedWord()
    {
        Word word = placedWords.Pop();
        for (int i = 0; i < word.word.Length; i++)
        {
            if (word.IntersectionIndex != i)
                grid.GetTile(word.StartPosition + i * word.Direction).ClearTile();
        }
        remainingWords.Add(word);
        word.FailedWords.Clear();
        return word;
    }

    private bool PlaceWordToOtherWord(Word oldWord, Word newWord, int indexInOldWord, int indexInNewWord)
    {
        Tile tile = grid.GetTile(oldWord.StartPosition + oldWord.Direction * indexInOldWord);
        Vector2 newWordDirection = GetDirectionOfNewPlacement(tile);
        if (CheckSpace(tile.Position, newWord.word.Length, newWordDirection, indexInNewWord))
        {
            Vector2 startPosition = tile.Position - indexInNewWord * newWordDirection;
            newWord.RootIntersectionIndex = indexInOldWord;
            newWord.IntersectionIndex = indexInNewWord;
            PlaceWord(newWord, startPosition, newWordDirection);
            return true;
        }
        return false;
    }

    private bool CheckSpace(Vector2 tilePos, int length, Vector2 dir, int charIndex)
    {
        for (int wordIndex = 0; wordIndex < length; wordIndex++)
        {
            if (wordIndex == charIndex) continue;

            Vector2 targetPos = tilePos + dir * (wordIndex - charIndex);
            if (!grid.GetTile(targetPos).IsEmpty()) return false;

            Vector2 otherDir = new Vector2(1, -1) - dir;
            if (!(grid.GetTile(targetPos + otherDir).IsEmpty() && grid.GetTile(targetPos - otherDir).IsEmpty())) return false;
        }
        if (!(grid.GetTile(tilePos + dir * (-charIndex - 1)).IsEmpty() && grid.GetTile(tilePos + dir * (length - charIndex)).IsEmpty())) return false;

        return true;
    }

    private Vector2 GetDirectionOfNewPlacement(Tile tile)
    {
        if (grid.GetTile(tile.Position + Vector2.right).IsEmpty() && grid.GetTile(tile.Position - Vector2.right).IsEmpty()) return Vector2.right;
        else return Vector2.down;
    }

    private void PlaceWord(Word word, Vector2 pos, Vector2 dir)
    {
        word.Direction = dir;
        word.StartPosition = pos;
        for (int i = 0; i < word.word.Length; i++)
        {
            Tile tile = grid.GetTile(pos + i * dir);
            tile.SetChar(word.word[i]);
        }
        remainingWords.Remove(word);
        placedWords.Push(word);
    }

    private void PlaceFistWord()
    {
        Vector2[] directionList = { Vector2.down, Vector2.right };
        int selectedWordIndex = Random.Range(0, remainingWords.Count);
        Word firstWord = remainingWords[selectedWordIndex];
        PlaceWord(firstWord, grid.GetCenter(), directionList[Random.Range(0, 2)]);
    }

    private void ClearData()
    {
        remainingWords.Clear();
        placedWords.Clear();
        for (int i = 0; i < wordList.Count; i++)
        {
            Word word = new(wordList[i].ToUpper());
            remainingWords.Add(word);
        }
        grid.ClearGrid();
    }

}
