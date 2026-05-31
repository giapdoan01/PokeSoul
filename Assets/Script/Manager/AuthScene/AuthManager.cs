using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Cysharp.Threading.Tasks;
using System;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    public event Action OnLoginSuccess;
    public event Action<string> OnAuthError;

    public bool IsInitialized { get; private set; }

    private const string KeyUsername = "saved_username";
    private const string KeyPassword = "saved_password";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAsync().Forget();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async UniTaskVoid InitializeAsync()
    {
        await UnityServices.InitializeAsync();
        IsInitialized = true;
        await TryAutoLoginAsync();
    }

    // Tự động đăng nhập nếu có thông tin đã lưu
    private async UniTask TryAutoLoginAsync()
    {
        string username = PlayerPrefs.GetString(KeyUsername, "");
        string password = PlayerPrefs.GetString(KeyPassword, "");
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;

        await LoginAsync(username, password, saveCredentials: false);
    }

    public void ClearSavedLogin()
    {
        PlayerPrefs.DeleteKey(KeyUsername);
        PlayerPrefs.DeleteKey(KeyPassword);
        PlayerPrefs.Save();
    }

    public async UniTask RegisterAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            await PlayerDataManager.Instance.LoadOrCreatePlayerDataAsync(username);
            SaveCredentials(username, password);
            OnLoginSuccess?.Invoke();
        }
        catch (AuthenticationException e)
        {
            OnAuthError?.Invoke(GetErrorMessage(e.ErrorCode));
        }
        catch (Exception e)
        {
            OnAuthError?.Invoke(e.Message);
        }
    }

    public async UniTask LoginAsync(string username, string password, bool saveCredentials = true)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            await PlayerDataManager.Instance.LoadOrCreatePlayerDataAsync(username);
            if (saveCredentials) SaveCredentials(username, password);
            OnLoginSuccess?.Invoke();
        }
        catch (AuthenticationException e)
        {
            OnAuthError?.Invoke(GetErrorMessage(e.ErrorCode));
        }
        catch (Exception e)
        {
            OnAuthError?.Invoke(e.Message);
        }
    }

    public void Logout()
    {
        ClearSavedLogin();
        AuthenticationService.Instance.SignOut();
    }

    private static void SaveCredentials(string username, string password)
    {
        PlayerPrefs.SetString(KeyUsername, username);
        PlayerPrefs.SetString(KeyPassword, password);
        PlayerPrefs.Save();
    }

    private static string GetErrorMessage(int errorCode) => errorCode switch
    {
        10002 => "Email hoặc mật khẩu không đúng.",
        10003 => "Email này đã được đăng ký.",
        10009 => "Mật khẩu phải có ít nhất 8 ký tự.",
        _     => "Có lỗi xảy ra. Vui lòng thử lại."
    };
}
