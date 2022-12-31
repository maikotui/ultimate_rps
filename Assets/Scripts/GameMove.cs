using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMove
{
    None,
    Rock,
    Paper,
    Scissors,
}

public static class GameMoveExtensions
{
    private static Dictionary<GameMove, List<GameMove>> _winTable = new()
    {
        { GameMove.Rock, new List<GameMove> {GameMove.Scissors,} },
        { GameMove.Paper, new List<GameMove> {GameMove.Rock,} },
        { GameMove.Scissors, new List<GameMove> {GameMove.Paper,} },
    };

    private static Dictionary<GameMove, List<GameMove>> _lossTable = new()
    {
        { GameMove.Rock, new List<GameMove> {GameMove.Paper,} },
        { GameMove.Paper, new List<GameMove> {GameMove.Scissors,} },
        { GameMove.Scissors, new List<GameMove> {GameMove.Rock,} },
    };

    /// <summary>
    /// Plays one move against the other and returns a value depicting who won in that exchange
    /// </summary>
    /// <param name="move"></param>
    /// <param name="other"></param>
    /// <returns>-1 if the move lost to other, 1 if the move won against other, and 0 if they tied</returns>
    public static int PlayAgainst(this GameMove move, GameMove other)
    {
        // Handle ties
        if (move == other) return 0;

        // Handle None moves
        if (move == GameMove.None)
        {
            return -1;
        }
        else if (other == GameMove.None)
        {
            return 1;
        }

        // Handle win & loss table lookups
        if (_winTable[move].Contains(other))
        {
            return 1;
        }
        else if (_lossTable[move].Contains(other))
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}