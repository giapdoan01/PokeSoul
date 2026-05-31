using UnityEngine;
using UnityEngine.InputSystem;

// Gắn vào bất kỳ GameObject nào trong BattleScene (VD: BattleSceneInitializer)
// Xử lý raycast 1 lần duy nhất thay vì mỗi Mon tự check trong Update
public class BattleClickManager : MonoBehaviour
{
    private void Update()
    {
        Vector2 screenPos;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            screenPos = Mouse.current.position.ReadValue();
        else
            return;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

        var mon = hit.collider.GetComponentInParent<MonOnSlot>();
        mon?.OnClick();
    }
}
