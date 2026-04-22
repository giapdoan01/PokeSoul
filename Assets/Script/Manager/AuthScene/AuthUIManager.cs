using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AuthUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;

    [Header("Loading")]
    [SerializeField] private GameObject panelLoading;
    [SerializeField] private CanvasGroup loadingCanvasGroup;

    [Header("Login")]
    [SerializeField] private TMP_InputField loginUsername;
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button toRegisterButton;
    [SerializeField] private TMP_Text loginErrorText;

    [Header("Register")]
    [SerializeField] private TMP_InputField registerUsername;
    [SerializeField] private TMP_InputField registerPassword;
    [SerializeField] private TMP_InputField registerConfirmPassword;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button toLoginButton;
    [SerializeField] private TMP_Text registerErrorText;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "MainGame";

    async void Start()
    {
        if (panelLoading != null)
            panelLoading.SetActive(false);

        ShowLogin();

        // Chờ AuthManager init xong
        while (!AuthManager.Instance.IsInitialized)
            await Task.Yield();

        AuthManager.Instance.OnLoginSuccess += HandleLoginSuccess;
        AuthManager.Instance.OnAuthError += HandleAuthError;

        loginButton.onClick.AddListener(OnLoginClick);
        registerButton.onClick.AddListener(OnRegisterClick);
        toRegisterButton.onClick.AddListener(ShowRegister);
        toLoginButton.onClick.AddListener(ShowLogin);
    }

    void OnDestroy()
    {
        if (AuthManager.Instance == null) return;
        AuthManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
        AuthManager.Instance.OnAuthError -= HandleAuthError;
    }

    // ── SWITCH PANELS ─────────────────────────────────────────

    public void ShowLogin()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        ClearErrors();
    }

    public void ShowRegister()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        ClearErrors();
    }

    // ── BUTTONS ───────────────────────────────────────────────

    public async void OnLoginClick()
    {
        loginErrorText.text = "";

        if (string.IsNullOrEmpty(loginUsername.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            loginErrorText.text = "Vui lòng nhập Username và mật khẩu.";
            return;
        }

        SetInteractable(false);
        await AuthManager.Instance.LoginAsync(loginUsername.text.Trim(), loginPassword.text);
        SetInteractable(true);
    }

    public async void OnRegisterClick()
    {
        registerErrorText.text = "";

        if (string.IsNullOrEmpty(registerUsername.text) || string.IsNullOrEmpty(registerPassword.text))
        {
            registerErrorText.text = "Vui lòng nhập đầy đủ thông tin.";
            return;
        }

        if (registerPassword.text != registerConfirmPassword.text)
        {
            registerErrorText.text = "Mật khẩu xác nhận không khớp.";
            return;
        }

        if (registerPassword.text.Length < 8)
        {
            registerErrorText.text = "Mật khẩu phải có ít nhất 8 ký tự.";
            return;
        }

        SetInteractable(false);
        await AuthManager.Instance.RegisterAsync(registerUsername.text.Trim(), registerPassword.text);
        SetInteractable(true);
    }

    // ── CALLBACKS ─────────────────────────────────────────────

    private void HandleLoginSuccess()
    {
        StartCoroutine(ShowLoadingThenLoadScene());
    }

    private IEnumerator ShowLoadingThenLoadScene()
    {
        SetInteractable(false);

        if (panelLoading != null)
        {
            panelLoading.SetActive(true);
            yield return StartCoroutine(AnimateLoadingIn());
        }

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(gameSceneName);
    }

    private void HandleAuthError(string message)
    {
        if (loginPanel.activeSelf)
            loginErrorText.text = message;
        else
            registerErrorText.text = message;
    }

    // ── LOADING ANIMATION ─────────────────────────────────────

    private IEnumerator AnimateLoadingIn()
    {
        const float duration = 0.45f;
        float elapsed = 0f;

        RectTransform rt = panelLoading.GetComponent<RectTransform>();
        if (loadingCanvasGroup != null) loadingCanvasGroup.alpha = 0f;
        if (rt != null) rt.localScale = Vector3.one * 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutBack(t);

            if (rt != null)
                rt.localScale = Vector3.LerpUnclamped(Vector3.one * 0.6f, Vector3.one, eased);

            if (loadingCanvasGroup != null)
                loadingCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t * 2f);

            yield return null;
        }

        if (rt != null) rt.localScale = Vector3.one;
        if (loadingCanvasGroup != null) loadingCanvasGroup.alpha = 1f;
    }

    // Back ease: scale vọt lên hơi quá rồi về đúng vị trí
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float t1 = t - 1f;
        return 1f + c3 * t1 * t1 * t1 + c1 * t1 * t1;
    }

    // ── HELPERS ───────────────────────────────────────────────

    private void SetInteractable(bool state)
    {
        loginButton.interactable = state;
        registerButton.interactable = state;
        toRegisterButton.interactable = state;
        toLoginButton.interactable = state;
    }

    private void ClearErrors()
    {
        loginErrorText.text = "";
        registerErrorText.text = "";
    }
}
