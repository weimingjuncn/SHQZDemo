using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SHQZ
{
    public class SpellEffectsManager : MonoBehaviour
    {
        Dictionary<string, int> s_effect = new Dictionary<string, int>();

        public void UseSpellEffect(string id, StateManager c, EnemyStates e = null)
        {
            int index = GetEffect(id);

            if(index == -1)
            {
                Debug.Log("Spell effect doesn't exit.");
                return;
            }

            switch (index)
            {
                case 0: FireBreath(c); break;
                case 1: DarkShield(c); break;
                case 2: HealingSmall(c); break;
            }
        }

        int GetEffect(string id)
        {
            int index = -1;
            if (s_effect.TryGetValue(id, out index))
            {
                return index;
            }
            return index;
        }

        void FireBreath(StateManager c)
        {
            c.spellCast_start = c.inventoryManager.OpenBreathCollider;
            c.spellCast_loop = c.inventoryManager.EmitSpellParticle;
            c.spellCast_stop = c.inventoryManager.CloseBreathCollider;
        }

        void DarkShield(StateManager c)
        {
            c.spellCast_start = c.inventoryManager.OpenBlockCollider;
            c.spellCast_loop = c.inventoryManager.EmitSpellParticle;
            c.spellCast_stop = c.inventoryManager.CloseBlockCollider;
        }

        void HealingSmall(StateManager c)
        {
            c.spellCast_loop = c.AddHealth;
        }

        public static SpellEffectsManager singleton;
        private void Awake()
        {
            singleton = this;
            s_effect.Add("firebreath", 0);
            s_effect.Add("darkshield", 1);
            s_effect.Add("healingSmall", 2);
        }
    }
}