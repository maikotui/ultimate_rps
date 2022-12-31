using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Session")]
public class GameSession : ScriptableObject
{
    public enum GameState
    {
        Ready = 0,
        InProgress = 1,
        CompletedPlayerOneWin = 2,
        CompletedPlayerTwoWin = 3,
        CompletedTie = 4,
    }

    public enum RoundResult
    {
        None = 0,
        PlayerOneWin = 1,
        PlayerTwoWin = 2,
        Tie = 3,
        Skipped = 4,
        Error = 5,
    }

    [field: SerializeField]
    public GameState State { get; private set; } = GameState.Ready;

    public int playerOneID = -1;
    public int playerTwoID = -1;

    [field: SerializeField]
    public int NumStandardRounds { get; set; } = 5;

    [field: SerializeField]
    public int MaxTieBreakerRounds { get; set; } = 0;

    public List<RoundResult> RoundResults { get => new(_roundResults); }
    [SerializeField] private List<RoundResult> _roundResults = new List<RoundResult>();

    public int CurrentRound { get => _currentRoundIndex + 1; }
    private int _currentRoundIndex = 0;

    public bool InTieBreakerRound { get; private set; } = false;


    public void StartGame()
    {
        if (playerOneID == -1)
        {
            throw new GameSessionException("Game session does not have Player One set");
        }
        else if (playerTwoID == -1)
        {
            throw new GameSessionException("Game session does not have Player Two set");
        }

        if (NumStandardRounds < 1 && MaxTieBreakerRounds < 1)
        {
            throw new GameSessionException("Game session requires at least one round");
        }

        State = GameState.InProgress;
        _roundResults = new(new RoundResult[NumStandardRounds + MaxTieBreakerRounds]);
        _currentRoundIndex = 0;
        InTieBreakerRound = NumStandardRounds == 0;
        InTieBreakerRound = false;
    }

    public RoundResult ScoreRound(GameMove playerOneMove, GameMove playerTwoMove)
    {
        // We're outside our maximum number of rounds and not using a tie breaker
        if (_currentRoundIndex >= _roundResults.Count)
        {
            throw new GameSessionException("Out of gameplay rounds");
        }

        // Score the player's moves then reset them
        int result = playerOneMove.PlayAgainst(playerTwoMove);
        RoundResult retval;

        // Score the round
        if (result > 0)
        {
            retval = RoundResult.PlayerOneWin;
        }
        else if (result < 0)
        {
            retval = RoundResult.PlayerTwoWin;
        }
        else
        {
            retval = RoundResult.Tie;
        }

        _roundResults[_currentRoundIndex++] = retval;

        // Check if we need to end the game or move into tiebreaker rounds
        if (_currentRoundIndex >= NumStandardRounds)
        {
            int tieBreakerRoundsAvailable = NumStandardRounds - (_currentRoundIndex) + MaxTieBreakerRounds;
            if (GradeGameToRound(_currentRoundIndex) == GameState.CompletedTie && tieBreakerRoundsAvailable > 0) // We can move to tie breaker rounds
            {
                InTieBreakerRound = true;
            }
            else
            {
                EndGame();
            }
        }

        return retval;
    }

    public void EndGame()
    {
        State = GradeGameToRound(_currentRoundIndex);
        InTieBreakerRound = false;
        for (int i = _currentRoundIndex; i < NumStandardRounds + MaxTieBreakerRounds; i++)
        {
            _roundResults[i] = RoundResult.Skipped;
        }
    }

    private GameState GradeGameToRound(int maxRoundIndex)
    {
        int playerOneWins = 0, playerTwoWins = 0;
        foreach (RoundResult roundResult in _roundResults.GetRange(0, maxRoundIndex))
        {
            if (roundResult == RoundResult.PlayerOneWin)
            {
                playerOneWins++;
            }
            else if (roundResult == RoundResult.PlayerTwoWin)
            {
                playerTwoWins++;
            }
        }

        if (playerOneWins > playerTwoWins)
        {
            return GameState.CompletedPlayerOneWin;
        }
        else if (playerTwoWins > playerOneWins)
        {
            return GameState.CompletedPlayerTwoWin;
        }
        else
        {
            return GameState.CompletedTie;
        }
    }
}

public class GameSessionException : Exception
{
    public GameSessionException()
    {
    }

    public GameSessionException(string message) : base(message)
    {
    }

    public GameSessionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
