using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainState
{
    int currRound;
    int strength;
    int inspiration;
    int manuscriptStatusSummary;

    public int CurrRound
    {
        get
        {
            return currRound;
        }
        set
        {
            if (value >= 0)
            {
                currRound = value;
            }
            else
            {
                currRound = 0;
            }
        }
    }

    public int Strength
    {
        get
        {
            return strength;
        }
        set
        {
            if (value >= 0)
            {
                strength = value;
            }
            else
            {
                strength = 0;
            }
        }
    }

    public int Inspiration
    {
        get
        {
            return inspiration;
        }
        set
        {
            if (value >= 0)
            {
                inspiration = value;
            }
            else
            {
                inspiration = 0;
            }
        }
    }

    public int ManuscriptStatusSummary
    {
        get
        {
            return manuscriptStatusSummary;
        }
        set
        {
            if (value >= 0)
            {
                manuscriptStatusSummary = value;
            }
            else
            {
                manuscriptStatusSummary = 0;
            }
        }
    }

}
