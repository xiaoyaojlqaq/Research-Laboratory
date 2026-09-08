using System;
using UnityEngine;

[Serializable]
public class MainState
{
    [Min(1)]
    public int currentRound = 1;

    [Min(0f)]
    public float remainingTimeSeconds;

    [Min(0)]
    public int stamina = 100;

    [Min(0)]
    public float inspiration;

    public int CurrRound
    {
        get { return currentRound; }
        set { currentRound = Mathf.Max(1, value); }
    }

    public float RemainingTimeSeconds
    {
        get { return remainingTimeSeconds; }
        set { remainingTimeSeconds = Mathf.Max(0f, value); }
    }

    public float Time
    {
        get { return RemainingTimeSeconds; }
        set { RemainingTimeSeconds = value; }
    }

    public int Strength
    {
        get { return stamina; }
        set { stamina = Mathf.Max(0, value); }
    }

    public float Inspiration
    {
        get { return inspiration; }
        set { inspiration = Mathf.Max(0f, value); }
    }
}
