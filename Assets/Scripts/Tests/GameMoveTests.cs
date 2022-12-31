using NUnit.Framework;
using System;

public class GameMoveTests
{
    [Test]
    public void StandardRPSGameMovesPlayAsExpected()
    {
        Assert.AreEqual(0, GameMove.Rock.PlayAgainst(GameMove.Rock));
        Assert.AreEqual(-1, GameMove.Rock.PlayAgainst(GameMove.Paper));
        Assert.AreEqual(1, GameMove.Rock.PlayAgainst(GameMove.Scissors));

        Assert.AreEqual(1, GameMove.Paper.PlayAgainst(GameMove.Rock));
        Assert.AreEqual(0, GameMove.Paper.PlayAgainst(GameMove.Paper));
        Assert.AreEqual(-1, GameMove.Paper.PlayAgainst(GameMove.Scissors));

        Assert.AreEqual(-1, GameMove.Scissors.PlayAgainst(GameMove.Rock));
        Assert.AreEqual(1, GameMove.Scissors.PlayAgainst(GameMove.Paper));
        Assert.AreEqual(0, GameMove.Scissors.PlayAgainst(GameMove.Scissors));

    }

    [Test]
    public void NoneGameMovePlaysAsExpected()
    {
        foreach (GameMove move in (GameMove[]) Enum.GetValues(typeof(GameMove))) 
        {
            if (move == GameMove.None)
            {
                Assert.AreEqual(0, GameMove.None.PlayAgainst(move));
                Assert.AreEqual(0, move.PlayAgainst(GameMove.None));
            }
            else
            {
                Assert.AreEqual(-1, GameMove.None.PlayAgainst(move));
                Assert.AreEqual(1, move.PlayAgainst(GameMove.None));
            }
        }
    }
}
