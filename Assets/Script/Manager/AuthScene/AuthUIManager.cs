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
        SceneManager.LoadScene(gameSceneName);
    }

    private void HandleAuthError(string message)
    {
        if (loginPanel.activeSelf)
            loginErrorText.text = message;
        else
            registerErrorText.text = message;
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
