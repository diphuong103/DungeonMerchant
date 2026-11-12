using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class BossBase : EnemyBase
    {
        [Serializable]
        public sealed class Phase
        {
            [Range(0f, 1f)] public float EnterAtOrBelowHPPercent = 0.66f;
            [Min(0f)] public float MoveSpeed = 1.8f;
            [Min(0.1f)] public float MeleeRange = 1.1f;
            [Min(1)] public int AttackDamage = 2;
            [Min(0.05f)] public float AttackCooldown = 0.8f;
            public UnityEvent OnEnter;
        }

        [Tooltip("List thresholds from highest to lowest. Each matching lower threshold advances the phase.")]
        public List<Phase> Phases = new List<Phase>();

        private readonly HashSet<int> enteredPhases = new HashSet<int>();

        protected override void Awake()
        {
            base.Awake();
            ApplyHealthPhase();
        }

        protected override void OnHealthChanged() => ApplyHealthPhase();

        private void ApplyHealthPhase()
        {
            while (true)
            {
                int selectedPhase = -1;
                float selectedThreshold = -1f;
                for (int i = 0; i < Phases.Count; i++)
                {
                    Phase phase = Phases[i];
                    if (phase == null || enteredPhases.Contains(i)) continue;
                    if (HealthPercent > phase.EnterAtOrBelowHPPercent) continue;
                    if (phase.EnterAtOrBelowHPPercent <= selectedThreshold) continue;
                    selectedThreshold = phase.EnterAtOrBelowHPPercent;
                    selectedPhase = i;
                }

                if (selectedPhase < 0) return;
                enteredPhases.Add(selectedPhase);
                Phase next = Phases[selectedPhase];
                MoveSpeed = next.MoveSpeed;
                MeleeRange = next.MeleeRange;
                AttackDamage = next.AttackDamage;
                AttackCooldown = next.AttackCooldown;
                next.OnEnter?.Invoke();
            }
        }
    }
}
