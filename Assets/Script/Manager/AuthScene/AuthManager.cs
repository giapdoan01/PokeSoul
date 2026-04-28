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
    }

    public async UniTask RegisterAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            await PlayerDataManager.Instance.LoadOrCreatePlayerDataAsync(username);
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

    public async UniTask LoginAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            await PlayerDataManager.Instance.LoadOrCreatePlayerDataAsync(username);
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
        AuthenticationService.Instance.SignOut();
    }

    private static string GetErrorMessage(int errorCode) => errorCode switch
    {
        10002 => "Email hoặc mật khẩu không đúng.",
        10003 => "Email này đã được đăng ký.",
        10009 => "Mật khẩu phải có ít nhất 8 ký tự.",
        _     => "Có lỗi xảy ra. Vui lòng thử lại."
    };
}
