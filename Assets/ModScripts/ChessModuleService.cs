using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChessModuleService : MonoBehaviour
{
    public class Puzzle
    {
        public string PuzzleId { get; set; }
        public string FEN { get; set; }
        public LinkedListNode<string> CurrentMove { get; set; }
        public int Rating { get; set; }
        public int RatingDeviation { get; set; }
        public int Popularity { get; set; }
        public int NbPlays { get; set; }
        public string Themes { get; set; }
        public string GameUrl { get; set; }

        public string TrainingUrl
        {
            get
            {
                return "https://lichess.org/training/" + PuzzleId;
            }
        }
    }

    [SerializeField]
    private TextAsset _Puzzles;
    
    private static List<string> ChessPuzzles;

    public static Puzzle ParsedPuzzle
    {
        get
        {
            string[] line = ChessPuzzles[Random.Range(0, ChessPuzzles.Count)].Trim().Split(',');
            var moves = new LinkedList<string>(line[2].Split(' '));
            return new Puzzle
            {
                PuzzleId = line[0],
                FEN = line[1],
                CurrentMove = moves.First,
                Rating = int.Parse(line[3]),
                RatingDeviation = int.Parse(line[4]),
                Popularity = int.Parse(line[5]),
                NbPlays = int.Parse(line[6]),
                Themes = line[7],
                GameUrl = line[8]
            };
        }
    }
    
    void Awake()
    {
        if (ChessPuzzles == null)
        {
            var PuzzleList = _Puzzles.text.Split('\n').ToList();
            int LastIndex = PuzzleList.Count - 1;
            if(String.IsNullOrEmpty(PuzzleList[LastIndex]))
                PuzzleList.RemoveAt(LastIndex);
            ChessPuzzles = PuzzleList;
        }
    }
}