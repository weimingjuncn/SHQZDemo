using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SHQZ
{
    
    public class InputHandler : MonoBehaviour
    {
        public float horizonal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;

        PlayerControls inputActions;

        Vector2 movementInput;
        Vector2 cameraInput;

        private void OnEnable()
        {
            if(inputActions == null)
            {
                inputActions = new PlayerControls();
                inputActions.PlayerMovements.Movement.performed += inputActions
                    => movementInput = inputActions.ReadValue<Vector2>();

            }

            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

    }
}