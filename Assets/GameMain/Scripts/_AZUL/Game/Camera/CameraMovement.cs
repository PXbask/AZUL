using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField]
        private float m_MoveSpeed = 10f;

        [SerializeField]
        private float m_ScrollSpeed = 2f;

        [SerializeField]
        private float m_MinOrthoSize = 1f;

        [SerializeField]
        private float m_MaxOrthoSize = 20f;

        private Camera m_Camera;

        private void Start()
        {
            m_Camera = GetComponent<Camera>() ?? Camera.main;
        }

        private void Update()
        {
            // Movement: W => +Z, A => -X, S => -Z, D => +X
            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                move += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                move += Vector3.back;
            }
            if (Input.GetKey(KeyCode.A))
            {
                move += Vector3.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                move += Vector3.right;
            }

            if (move != Vector3.zero)
            {
                // Move in world space relative to camera orientation
                Vector3 worldMove = move.normalized * m_MoveSpeed * Time.deltaTime;
                transform.position += new Vector3(worldMove.x, 0f, worldMove.z);
            }

            // Mouse scroll to control orthographic size
            if (m_Camera != null && m_Camera.orthographic)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0f)
                {
                    float newSize = m_Camera.orthographicSize - scroll * m_ScrollSpeed;
                    m_Camera.orthographicSize = Mathf.Clamp(newSize, m_MinOrthoSize, m_MaxOrthoSize);
                }
            }
        }
    }
}
