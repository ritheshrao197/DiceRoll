// using UnityEngine;

// namespace TRAIBAG
// {
//     public class CameraOrbit : MonoBehaviour
//     {
//         [Header("Target Settings")]
//         public Transform target; // ī�޶� �߽����� �ѷ��� ���

//         [Header("Rotation Settings")]
//         public float rotationSpeed = 3f; // ���콺 �巡�� ȸ�� �ӵ�

//         [Header("Vertical Limits (-12, 60 / 10, 10)")]
//         public float minVerticalAngle = -12f; // ���� ����
//         public float maxVerticalAngle = 60f;  // �Ʒ��� ����

//         [Header("Auto Rotate (15)")]
//         public float autoRotateSpeed = 0f; // �ڵ� ȸ�� �ӵ� (0 = ȸ�� ����)

//         private float yaw;
//         private float pitch;

//         void Start()
//         {
//             if (target == null)
//             {
//                 Debug.LogWarning("Ÿ���� �������� �ʾҽ��ϴ�.");
//                 enabled = false;
//                 return;
//             }

//             Vector3 angles = transform.eulerAngles;
//             yaw = angles.y;
//             pitch = angles.x;
//         }

//         void LateUpdate()
//         {
//             var mouse = Mouse.current;

//             // ���콺 �巡�� ȸ��
//             if (mouse.leftButton.isPressed)
//             {
//                 float mouseX = mouse.delta.x.ReadValue();
//                 float mouseY = mouse.delta.y.ReadValue();

//                 yaw += mouseX * rotationSpeed * Time.deltaTime * 10f;
//                 pitch -= mouseY * rotationSpeed * Time.deltaTime * 10f;
//             }
//             else
//             {
//                 // �ڵ� ȸ�� (�¿�)
//                 if (autoRotateSpeed != 0f)
//                     yaw += autoRotateSpeed * Time.deltaTime;
//             }

//             // ���Ʒ� ���� ����
//             pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

//             // ī�޶� ȸ������ġ ����
//             transform.rotation = Quaternion.Euler(pitch, yaw, 0);
//             transform.position = target.position - transform.forward * Vector3.Distance(transform.position, target.position);
//         }
//     }
// }
