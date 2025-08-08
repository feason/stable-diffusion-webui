using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoginUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public TextMeshProUGUI statusText;
    public GameObject loginPanel;
    public GameObject registerPanel;
    
    [Header("Register Panel")]
    public TMP_InputField regUsernameInput;
    public TMP_InputField regPasswordInput;
    public TMP_InputField regConfirmPasswordInput;
    public TMP_InputField regEmailInput;
    public Button confirmRegisterButton;
    public Button backToLoginButton;
    public TextMeshProUGUI registerStatusText;

    private LoginManager loginManager;

    void Start()
    {
        loginManager = FindObjectOfType<LoginManager>();
        
        // 设置按钮事件
        loginButton.onClick.AddListener(OnLoginClick);
        registerButton.onClick.AddListener(OnRegisterClick);
        confirmRegisterButton.onClick.AddListener(OnConfirmRegisterClick);
        backToLoginButton.onClick.AddListener(OnBackToLoginClick);
        
        // 初始状态
        ShowLoginPanel();
        ClearStatusText();
        
        // 回车键登录
        usernameInput.onSubmit.AddListener(delegate { OnLoginClick(); });
        passwordInput.onSubmit.AddListener(delegate { OnLoginClick(); });
    }

    public void OnLoginClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatus("请输入用户名和密码！", Color.red);
            return;
        }

        ShowStatus("正在登录...", Color.yellow);
        loginButton.interactable = false;

        StartCoroutine(LoginCoroutine(username, password));
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        yield return StartCoroutine(loginManager.LoginUser(username, password, OnLoginResult));
    }

    private void OnLoginResult(bool success, string message)
    {
        loginButton.interactable = true;
        
        if (success)
        {
            ShowStatus("登录成功！", Color.green);
            Debug.Log("登录成功，跳转到主界面");
            // 这里可以加载主场景或切换到主界面
            // SceneManager.LoadScene("MainScene");
        }
        else
        {
            ShowStatus(message, Color.red);
        }
    }

    public void OnRegisterClick()
    {
        ShowRegisterPanel();
    }

    public void OnConfirmRegisterClick()
    {
        string username = regUsernameInput.text.Trim();
        string password = regPasswordInput.text;
        string confirmPassword = regConfirmPasswordInput.text;
        string email = regEmailInput.text.Trim();

        // 验证输入
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
        {
            ShowRegisterStatus("请填写所有必填字段！", Color.red);
            return;
        }

        if (password != confirmPassword)
        {
            ShowRegisterStatus("两次输入的密码不一致！", Color.red);
            return;
        }

        if (password.Length < 6)
        {
            ShowRegisterStatus("密码长度至少6位！", Color.red);
            return;
        }

        if (!IsValidEmail(email))
        {
            ShowRegisterStatus("邮箱格式不正确！", Color.red);
            return;
        }

        ShowRegisterStatus("正在注册...", Color.yellow);
        confirmRegisterButton.interactable = false;

        StartCoroutine(RegisterCoroutine(username, password, email));
    }

    private IEnumerator RegisterCoroutine(string username, string password, string email)
    {
        yield return StartCoroutine(loginManager.RegisterUser(username, password, email, OnRegisterResult));
    }

    private void OnRegisterResult(bool success, string message)
    {
        confirmRegisterButton.interactable = true;
        
        if (success)
        {
            ShowRegisterStatus("注册成功！", Color.green);
            // 延迟1秒后返回登录界面
            StartCoroutine(DelayedBackToLogin());
        }
        else
        {
            ShowRegisterStatus(message, Color.red);
        }
    }

    private IEnumerator DelayedBackToLogin()
    {
        yield return new WaitForSeconds(1.5f);
        ShowLoginPanel();
        // 自动填充用户名
        usernameInput.text = regUsernameInput.text;
    }

    public void OnBackToLoginClick()
    {
        ShowLoginPanel();
    }

    private void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        ClearStatusText();
        ClearRegisterStatusText();
    }

    private void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        ClearStatusText();
        ClearRegisterStatusText();
        ClearRegisterInputs();
    }

    private void ShowStatus(string message, Color color)
    {
        statusText.text = message;
        statusText.color = color;
    }

    private void ShowRegisterStatus(string message, Color color)
    {
        registerStatusText.text = message;
        registerStatusText.color = color;
    }

    private void ClearStatusText()
    {
        statusText.text = "";
    }

    private void ClearRegisterStatusText()
    {
        registerStatusText.text = "";
    }

    private void ClearRegisterInputs()
    {
        regUsernameInput.text = "";
        regPasswordInput.text = "";
        regConfirmPasswordInput.text = "";
        regEmailInput.text = "";
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}