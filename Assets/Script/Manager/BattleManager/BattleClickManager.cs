using UnityEngine;
using UnityEngine.InputSystem;

// Xử lý click/touch trong BattleScene — raycast tập trung 1 chỗ
// thay vì mỗi Mon tự check trong Update riêng.
public class BattleClickManager : MonoBehaviour
{
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        Vector2 screenPos;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            screenPos = Mouse.current.position.ReadValue();
        else
            return;

        Ray ray = _camera.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

        hit.collider.GetComponentInParent<MonOnSlot>()?.OnClick();
    }
}
