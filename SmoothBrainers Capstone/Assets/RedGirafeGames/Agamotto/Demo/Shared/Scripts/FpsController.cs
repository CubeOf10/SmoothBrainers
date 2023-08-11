using UnityEngine;

namespace RedGirafeGames.Agamotto.Demo.Shared.Scripts
{
    /// <summary>
    /// Simple FPS Controller from <link>https://sharpcoderblog.com/blog/unity-3d-fps-controller</link>
    /// Modified for the Agamotto demos.
    /// </summary>
    public class FpsController : MonoBehaviour
    {
        public CharacterController controller;
    
        public Camera playerCamera;
        
        public float walkingSpeed = 7.5f;
        public float jumpSpeed = 8.0f;
        public float gravity = 20.0f;
        public float lookSpeed = 2.0f;
        public float lookXLimit = 45.0f;

        Vector3 moveDirection = Vector3.zero;
        float rotationX = 0;
    
        [HideInInspector]
        public bool canMove = true;
    
        void Start()
        {
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            
        }

        void Update()
        {
            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            float curSpeedX = canMove ? walkingSpeed * Input.GetAxisRaw("Vertical") : 0;
            float curSpeedY = canMove ? walkingSpeed * Input.GetAxisRaw("Horizontal") : 0;
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            if (Input.GetButton("Jump") && canMove && controller.isGrounded)
            {
                moveDirection.y = jumpSpeed;
            }
            else
            {
                moveDirection.y = movementDirectionY;
            }

            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            if (!controller.isGrounded)
            {
                moveDirection.y -= gravity * Time.unscaledDeltaTime;
            }

            // controller.Move(moveDirection);
            
            // Move the controller
            controller.Move(moveDirection * Time.unscaledDeltaTime);

            // Player and Camera rotation
            if (canMove)
            {
                rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
            }
        }

        public Transform GetLookTransform()
        {
            return playerCamera.transform;
        }
    }
}
