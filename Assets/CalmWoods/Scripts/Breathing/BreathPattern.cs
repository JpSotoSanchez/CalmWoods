using System;
using UnityEngine;

namespace CalmWoods.Breathing
{
    public enum BreathPhase
    {
        Inhale = 0,
        HoldIn = 1,
        Exhale = 2,
        HoldOut = 3
    }

    // Seconds for each phase of one breath. A phase with 0 seconds is skipped.
    [Serializable]
    public class BreathPattern
    {
        public string name = "Calm 4-1-5";
        [Min(0f)] public float inhale = 4f;
        [Min(0f)] public float holdIn = 1f;
        [Min(0f)] public float exhale = 5f;
        [Min(0f)] public float holdOut = 0f;

        public float CycleSeconds => inhale + holdIn + exhale + holdOut;

        // Default pattern = 10 s per breath = 6 breaths per minute (calm target 6-10).
        public float BreathsPerMinute => CycleSeconds > 0f ? 60f / CycleSeconds : 0f;

        public float Duration(BreathPhase phase) => phase switch
        {
            BreathPhase.Inhale => inhale,
            BreathPhase.HoldIn => holdIn,
            BreathPhase.Exhale => exhale,
            _ => holdOut
        };
    }
}
