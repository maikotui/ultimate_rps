using System;
using System.Collections.Generic;
using UnityEngine;

// Make it so player handles it's own move inventory (decouple from game session)
public class Player: ScriptableObject
{
    public int id;
    public Dictionary<GameMove, int> moves = new Dictionary<GameMove, int>();
}