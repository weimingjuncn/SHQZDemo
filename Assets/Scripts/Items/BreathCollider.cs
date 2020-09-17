using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHQZ
{
    public class BreathCollider : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            EnemyStates es = other.GetComponentInParent<EnemyStates>();
            if(es != null)
            {
                es.DoDamage_();
                SpellEffectsManager.singleton.UseSpellEffect("onFire", null, es);
            }
        }
    }
}
