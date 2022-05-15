using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class TopCamera : MonoBehaviour
{
    [SerializeField] private int MoveSpeed = 20;
    [SerializeField] private int ZoomSpeed = 100;
    [SerializeField] private int MinHeight = 5;
    [SerializeField] private int MaxHeight = 100;

    Action<float> OnMouseScroll;
    Action<float> OnMoveHorizontal;
    Action<float> OnMoveVertical;

    Vector3 Move = Vector3.zero;

    private void Start()
    {
        OnMouseScroll += (float value) =>
        {
            if (value < 0f && transform.position.y < MaxHeight)
            {
                Move.y += ZoomSpeed * Time.deltaTime;
            }
            else if (value > 0f && transform.position.y > MinHeight)
            {
                Move.y -= ZoomSpeed * Time.deltaTime;
            }
        };

        OnMoveHorizontal += (float value) =>
        {
            if (value > 0f)
                Move.x += MoveSpeed * Time.deltaTime;
            else
                Move.x -= MoveSpeed * Time.deltaTime;
        };

        OnMoveVertical += (float value) =>
        {
            if (value > 0f)
                Move.z += MoveSpeed * Time.deltaTime;
            else
                Move.z -= MoveSpeed * Time.deltaTime;
        };
    }

    private void Update()
    {
        Move = Vector3.zero;
#if UNITY_EDITOR
        if (EditorWindow.focusedWindow != EditorWindow.mouseOverWindow)
            return;
#endif
        float value = Input.GetAxis("Mouse ScrollWheel");
        if (value != 0)
            OnMouseScroll(value);

        value = Input.GetAxis("Horizontal");
        if (value != 0)
            OnMoveHorizontal(value);

        value = Input.GetAxis("Vertical");
        if (value != 0)
            OnMoveVertical(value);

        if (Move != Vector3.zero)
            transform.position += Move;
    }
}