using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class GameSessionTests
{
    public GameSession session;
    public int playerOne;
    public int playerTwo;

    [SetUp]
    public void GameSessionSetup()
    {
        ResetSession();
    }

    private void ResetSession()
    {
        session = (GameSession)ScriptableObject.CreateInstance(typeof(GameSession));
        session.playerOneID = 1;
        session.playerTwoID = 2;
        session.NumStandardRounds = 5;
        session.MaxTieBreakerRounds = 0;
    }

    [Test]
    public void GameSessionStartNoPlayers()
    {
        session = (GameSession)ScriptableObject.CreateInstance(typeof(GameSession));
        Assert.Throws(typeof(GameSessionException), session.StartGame);
    }

    [Test]
    public void GameSessionStartMissingPlayerOne()
    {
        session = (GameSession)ScriptableObject.CreateInstance(typeof(GameSession));
        session.playerTwoID = 2;
        Assert.Throws(typeof(GameSessionException), session.StartGame);
    }

    [Test]
    public void GameSessionStartMissingPlayerTwo()
    {
        session = (GameSession)ScriptableObject.CreateInstance(typeof(GameSession));
        session.playerOneID = 1;
        Assert.Throws(typeof(GameSessionException), session.StartGame);
    }

    [Test]
    public void RoundResultsAreNotModifiable()
    {
        session.StartGame();
        Assert.AreEqual(5, session.RoundResults.Count);
        session.RoundResults.Add(GameSession.RoundResult.None);
        Assert.AreEqual(5, session.RoundResults.Count);
    }

    [Test]
    public void InitialRoundResultCountIsCorrect()
    {
        Assert.AreEqual(0, session.RoundResults.Count); // Should be empty before game is started

        session.StartGame();
        Assert.AreEqual(session.NumStandardRounds, session.RoundResults.Count); // Should be equal to default number of rounds

        ResetSession();
        session.NumStandardRounds = 3;
        session.StartGame();
        Assert.AreEqual(3, session.RoundResults.Count); // Should be equal to num rounds after custom set

        ResetSession();
        session.NumStandardRounds = 3;
        session.MaxTieBreakerRounds = 1;
        session.StartGame();
        Assert.AreEqual(4, session.RoundResults.Count); // Should be equal to number of rounds plus number of tie breakers.

        ResetSession();
        session.NumStandardRounds = 0;
        session.MaxTieBreakerRounds = 2;
        session.StartGame();
        Assert.AreEqual(2, session.RoundResults.Count); // Should be equal to number of tie breakers (rounds is 0)
    }

    [Test]
    public void ErrorOnlyOnBadNumberOfRounds()
    {
        session.NumStandardRounds = 0;
        session.MaxTieBreakerRounds = 0;
        Assert.Throws(typeof(GameSessionException), session.StartGame);
        ResetSession();
        session.NumStandardRounds = 1;
        session.MaxTieBreakerRounds = 0;
        Assert.DoesNotThrow(session.StartGame);
        ResetSession();
        session.NumStandardRounds = 0;
        session.MaxTieBreakerRounds = 1;
        Assert.DoesNotThrow(session.StartGame);
    }

    [Test]
    public void StandardPlayWithNoTieBreaker()
    {
        session.StartGame();
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Rock, GameMove.Scissors));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.None, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.Tie, session.ScoreRound(GameMove.Rock, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));
        Assert.AreEqual(GameSession.GameState.CompletedPlayerOneWin, session.State);
    }

    [Test]
    public void StandardPlayWithTieBreakerAllUsed1()
    {
        session.MaxTieBreakerRounds = 1;
        session.StartGame();
        // Play five normal games that end in 2 vs 2
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.Scissors, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.None, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.Tie, session.ScoreRound(GameMove.Rock, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));

        // Check game hasn't ended
        Assert.AreEqual(GameSession.GameState.InProgress, session.State);

        // Play one more round with P1 winning
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));

        // Ensure game is over
        Assert.AreEqual(GameSession.GameState.CompletedPlayerOneWin, session.State);
    }

    [Test]
    public void StandardPlayWithTieBreakerAllUsed2()
    {
        session.MaxTieBreakerRounds = 2;
        session.StartGame();

        // Play five normal games that end in 2 vs 2
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.Scissors, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.None, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.Tie, session.ScoreRound(GameMove.Rock, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));

        // Check game hasn't ended
        Assert.AreEqual(GameSession.GameState.InProgress, session.State);

        // Play a round with a tie and a round with a player two win
        Assert.AreEqual(GameSession.RoundResult.Tie, session.ScoreRound(GameMove.Paper, GameMove.Paper));
        Assert.AreEqual(GameSession.GameState.InProgress, session.State);
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.Scissors, GameMove.Rock));

        // Ensure game is over
        Assert.AreEqual(GameSession.GameState.CompletedPlayerTwoWin, session.State);
    }

    [Test]
    public void StandardPlayWithTieBreakerSomeUsed()
    {
        session.MaxTieBreakerRounds = 2;
        session.StartGame();

        // Play five normal games that end in 2 vs 2
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.Scissors, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.None, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.Tie, session.ScoreRound(GameMove.Rock, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));

        // Check game hasn't ended
        Assert.AreEqual(GameSession.GameState.InProgress, session.State);

        // Play a round with a P2 win
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.Scissors, GameMove.Rock));

        // Ensure game is over
        Assert.AreEqual(GameSession.GameState.CompletedPlayerTwoWin, session.State);
    }

    [Test]
    public void StandardPlayWithTieBreakerNotUsed()
    {
        session.MaxTieBreakerRounds = 1;
        session.StartGame();

        // Play four rounds that end in 2 v 2
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Rock, GameMove.Scissors));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.None, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.Scissors, GameMove.Rock));

        // Play another round that ends in a P1 win
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));

        Assert.AreEqual(GameSession.GameState.CompletedPlayerOneWin, session.State);
    }

    [Test]
    public void StandardPlayEndedEarly()
    {
        session.StartGame();
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Rock, GameMove.Scissors));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.None, GameMove.Rock));
        session.EndGame(); // Game has five rounds, but we're ending it at 3.
        Assert.AreEqual(GameSession.GameState.CompletedPlayerOneWin, session.State);
        Assert.AreEqual(new GameSession.RoundResult[] { 
            GameSession.RoundResult.PlayerOneWin,
            GameSession.RoundResult.PlayerOneWin,
            GameSession.RoundResult.PlayerTwoWin,
            GameSession.RoundResult.Skipped,
            GameSession.RoundResult.Skipped,
        }, session.RoundResults.ToArray());
    }

    [Test]
    public void StandardPlayEndedEarlyWithTieBreaker()
    {
        session.MaxTieBreakerRounds = 1;
        session.StartGame();
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Paper, GameMove.Rock));
        Assert.AreEqual(GameSession.RoundResult.PlayerOneWin, session.ScoreRound(GameMove.Rock, GameMove.Scissors));
        Assert.AreEqual(GameSession.RoundResult.PlayerTwoWin, session.ScoreRound(GameMove.None, GameMove.Rock));
        session.EndGame(); // Game has five rounds, but we're ending it at 3.
        Assert.AreEqual(GameSession.GameState.CompletedPlayerOneWin, session.State);
        Assert.AreEqual(new GameSession.RoundResult[] {
            GameSession.RoundResult.PlayerOneWin,
            GameSession.RoundResult.PlayerOneWin,
            GameSession.RoundResult.PlayerTwoWin,
            GameSession.RoundResult.Skipped,
            GameSession.RoundResult.Skipped,
            GameSession.RoundResult.Skipped,
        }, session.RoundResults.ToArray());
    }
}
