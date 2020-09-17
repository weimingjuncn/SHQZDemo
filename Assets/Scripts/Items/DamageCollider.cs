using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHQZ
{
    public class DamageCollider : MonoBehaviour
    {
        StateManager states;

        public void Init(StateManager st)
        {
            states = st;
        }
        void OnTriggerEnter(Collider other)
        {
            EnemyStates eStates = other.transform.root.GetComponentInParent<EnemyStates>();

            if (eStates == null)
                return;

            eStates.DoDamage(states.currentAction);

        }
    }
}
