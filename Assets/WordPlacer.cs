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
        public int RootsIntersectionIndex = -1;
        public int IntersectionIndexWithRoot = -1;
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
        // setup
        grid.ClearGrid();
        remainingWords.Clear();
        placedWords.Clear();
        List<Word> possibleStartWords = new List<Word>();
        for (int i = 0; i < wordList.Count; i++)
        {
            Word word = new(wordList[i].ToUpper());
            remainingWords.Add(word);
            possibleStartWords.Add(word);
        }

        // until there is not tried word to start placement
        while (0 < possibleStartWords.Count)
        {
            // place a random possible first word
            int selectedWordIndex = Random.Range(0, possibleStartWords.Count);
            Word firstWord = possibleStartWords[selectedWordIndex];
            possibleStartWords.RemoveAt(selectedWordIndex);
            PlaceWord(firstWord, grid.GetCenter(), RandomDirection());

            // while there is an unplaced word
            while (0 < remainingWords.Count) 
            {
                // if there are possible words suitable for placement
                if (possibleWords.Count > 0)
                {
                    Word oldWord = placedWords.Peek();
                    Word newWord = possibleWords[Random.Range(0, possibleWords.Count)];

                    // assign possible connections
                    AssignLinksBetweenTwoWord(oldWord, newWord);

                    // try connect from random possible connection
                    if (!TryConnectFromRandomLink(newWord))
                        placedWords.Peek().FailedWords.Add(newWord);
                }
                else
                {
                    // if there is no option left with this start word => break
                    if (placedWords.Count == 1)
                    {
                        RemoveFirstWordToClearGrid();
                        break;
                    }

                    // remove last added word
                    Word word = RemoveLastAddedWord();

                    // try the remaining possible connections for removed word
                    if (!TryConnectRemovedWordWithDifferentConnection(word))
                        placedWords.Peek().FailedWords.Add(word);
                }
            }
            if (remainingWords.Count == 0) break;
        }

        if (remainingWords.Count == 0) print("Success");
        else print("Unplaceable words!");
    }


    #region Helper Functions
    private bool TryConnectRemovedWordWithDifferentConnection(Word word)
    {
        while (word.PossibleLinks.Count > 0)
        {
            int index = Random.Range(0, word.PossibleLinks.Count);
            (int, int) link = word.PossibleLinks[index];
            word.PossibleLinks.RemoveAt(index);

            if (TryPlaceWordToOtherWord(placedWords.Peek(), word, link.Item1, link.Item2))
                return true;
        }
        return false;
    }

    private bool TryConnectFromRandomLink(Word word)
    {
        while (word.PossibleLinks.Count > 0)
        {
            int index = Random.Range(0, word.PossibleLinks.Count);
            (int, int) link = word.PossibleLinks[index];
            word.PossibleLinks.RemoveAt(index);

            if (TryPlaceWordToOtherWord(placedWords.Peek(), word, link.Item1, link.Item2))
                return true;
        }
        return false;
    }

    private void AssignLinksBetweenTwoWord(Word oldWord, Word newWord)
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

    private bool CheckSpaceOnGrid(Vector2 startPos, int length, Vector2 dir, int intersectionIndex)
    {
        Vector2 otherDir = new Vector2(1, -1) - dir;
        for (int wordIndex = 0; wordIndex < length; wordIndex++)
        {
            if (wordIndex == intersectionIndex) continue;

            for (int neighbourDistance = -1; neighbourDistance < 2; neighbourDistance++)
            {
                Vector2 targetPos = startPos + dir * wordIndex + neighbourDistance * otherDir;
                if (!grid.GetTile(targetPos).IsEmpty())
                    return false;
            }
        }
        if (!grid.GetTile(startPos - dir).IsEmpty() || !grid.GetTile(startPos + dir * length).IsEmpty()) return false;

        return true;
    }

    #endregion


    #region Place & Remove Functions

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

    private bool TryPlaceWordToOtherWord(Word oldWord, Word newWord, int indexInOldWord, int indexInNewWord)
    {
        Vector2 intersectionPosition = oldWord.StartPosition + oldWord.Direction * indexInOldWord;
        Vector2 newWordDirection = GetDirectionOfNewPlacement(intersectionPosition);
        Vector2 newWordStartPosition = intersectionPosition - newWordDirection * indexInNewWord;
        if (CheckSpaceOnGrid(newWordStartPosition, newWord.word.Length, newWordDirection, indexInNewWord))
        {
            Vector2 startPosition = intersectionPosition - indexInNewWord * newWordDirection;
            newWord.RootsIntersectionIndex = indexInOldWord;
            newWord.IntersectionIndexWithRoot = indexInNewWord;
            PlaceWord(newWord, startPosition, newWordDirection);
            return true;
        }
        return false;
    }

    private Word RemoveLastAddedWord()
    {
        Word word = placedWords.Pop();
        for (int i = 0; i < word.word.Length; i++)
        {
            if (word.IntersectionIndexWithRoot != i)
                grid.GetTile(word.StartPosition + i * word.Direction).ClearTile();
        }
        remainingWords.Add(word);
        word.FailedWords.Clear();
        return word;
    }

    private void RemoveFirstWordToClearGrid()
    {
        Word firstWord = placedWords.Pop();
        firstWord.FailedWords.Clear();
        remainingWords.Add(firstWord);
        for (int i = 0; i < firstWord.word.Length; i++)
        {
            grid.GetTile(firstWord.StartPosition + i * firstWord.Direction).ClearTile();
        }
    }

    #endregion


    #region Direction Functions

    private Vector2 RandomDirection()
    {
        Vector2[] directionList = { Vector2.down, Vector2.right };
        return directionList[Random.Range(0, 2)];
    }

    private Vector2 GetDirectionOfNewPlacement(Vector2 tilePos)
    {
        if (grid.GetTile(tilePos + Vector2.right).IsEmpty() && grid.GetTile(tilePos - Vector2.right).IsEmpty()) return Vector2.right;
        else return Vector2.down;
    }

    #endregion


}
