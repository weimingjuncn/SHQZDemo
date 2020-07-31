﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SHQZ
{
    public class AnimatorHook : MonoBehaviour
    {
        Animator anim;
        StateManager states;

        public float rootMotionMultiplier;
        bool rolling;
        float roll_t;

        //public AnimationCurve rollCurve;

        public void Init(StateManager st)
        {
            states = st;
            anim = st.anim;
            //rollCurve = st.roll_curve;
        }
        public void InitForRoll()
        {
            rolling = true;
            roll_t = 0;
        }
        public void CloseRoll()
        {
            if (rolling == false)
                return;
            rootMotionMultiplier = 1;
            roll_t = 0;
            rolling = false;
        }

        private void OnAnimatorMove()
        {
            if (states.canMove)
                return;
            states.rigidbody.drag = 0;

            if (rootMotionMultiplier == 0)
                rootMotionMultiplier = 1;

            if(rolling == false)
            {
                Vector3 delta = anim.deltaPosition;
                delta.y = 0;
                Vector3 v = (delta * rootMotionMultiplier) / states.delta;
                states.rigidbody.velocity = v;
            }
            else
            {
                roll_t += states.delta / 0.65f;
                if (roll_t > 1)
                    roll_t = 1;

                float zValue = states.roll_curve.Evaluate(roll_t);
                Vector3 v1 = Vector3.forward * zValue;
                Vector3 relative = transform.TransformDirection(v1);
                Vector3 v2 = (relative * rootMotionMultiplier);
                states.rigidbody.velocity = v2;
            }
        }
    }
}