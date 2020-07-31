﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SHQZ
{
    public class InputHandler : MonoBehaviour
    {
        float horizontal;
        float vertical;
        
        bool b_input;
        bool a_input;
        bool x_input;
        bool y_input;

        bool rb_input;
        float rt_axis;
        bool rt_input;
        bool lb_input;
        float lt_axis;
        bool lt_input;

        bool leftAxis_down;
        bool rightAxis_down;

        StateManager states;
        CameraManager cameraManager;

        float delta;

        void Start()
        {
            states = GetComponent<StateManager>();
            states.Init();
            cameraManager = CameraManager.singleton;
            cameraManager.Init(this.transform);
        }

        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            GetInput();
            UpdateState();
            states.FixedTick(delta);
            cameraManager.Tick(delta);
        }
        
        private void Update()
        {
            delta = Time.deltaTime;
            states.Tick(delta);
        }    
        void GetInput()
        {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            b_input = Input.GetButton("B");
            a_input = Input.GetButton("A");
            y_input = Input.GetButtonUp("Y");
            x_input = Input.GetButton("X");
            
            rt_input = Input.GetButton("RT");
            rt_axis = Input.GetAxis("RT");
            if (rt_axis != 0)
                rt_input = true;

            
            lt_input = Input.GetButton("LT");
            lt_axis = Input.GetAxis("LT");
            if(lt_axis != 0)
                lt_input = true;

            rb_input = Input.GetButton("RB");
            lb_input = Input.GetButton("LB");

            rightAxis_down = Input.GetButtonUp("L");
            //Debug.Log(rightAxis_down);
        }
        void UpdateState()
        {
            states.horizontal = horizontal;
            states.vertical = vertical;
            Vector3 v = states.vertical * cameraManager.transform.forward;
            Vector3 h = states.horizontal * cameraManager.transform.right;
            states.moveDir = (v + h).normalized;
            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            states.moveAmount = Mathf.Clamp01(m);

            states.rollInput = b_input;
            if (b_input)
            {
                //states.run = (states.moveAmount > 0);
            }
            else
            {
                //states.run = false;
            }

            states.rt = rt_input;
            states.lt = lt_input;
            states.rb = rb_input;
            states.lb = lb_input;

            if (y_input)
            {
                states.isTwoHanded = !states.isTwoHanded;
                states.HandleTwoHanded();
            }
            if (rightAxis_down)
            {
                states.lockOn = !states.lockOn;
                if (states.lockOnTarget == null)
                    states.lockOn = false;
                cameraManager.lockOnTarget = states.lockOnTarget.transform;
                cameraManager.lockOn = states.lockOn;
            }
        }
    }
}
