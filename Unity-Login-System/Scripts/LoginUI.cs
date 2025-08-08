using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 登录界面UI控制器
/// 功能：管理登录和注册界面的用户交互，处理用户输入验证，显示状态信息
/// 目的：提供用户友好的登录注册界面，连接UI操作与后端登录逻辑
/// </summary>
public class LoginUI : MonoBehaviour
{
    // ==================== 登录面板UI引用 ====================
    [Header("登录面板UI引用")]
    [Tooltip("用户名输入框")]
    public TMP_InputField usernameInput;
    [Tooltip("密码输入框")]
    public TMP_InputField passwordInput;
    [Tooltip("登录按钮")]
    public Button loginButton;
    [Tooltip("跳转注册按钮")]
    public Button registerButton;
    [Tooltip("登录状态提示文本")]
    public TextMeshProUGUI statusText;
    [Tooltip("登录面板容器")]
    public GameObject loginPanel;
    [Tooltip("注册面板容器")]
    public GameObject registerPanel;
    
    // ==================== 注册面板UI引用 ====================
    [Header("注册面板UI引用")]
    [Tooltip("注册用户名输入框")]
    public TMP_InputField regUsernameInput;
    [Tooltip("注册密码输入框")]
    public TMP_InputField regPasswordInput;
    [Tooltip("确认密码输入框")]
    public TMP_InputField regConfirmPasswordInput;
    [Tooltip("邮箱输入框")]
    public TMP_InputField regEmailInput;
    [Tooltip("确认注册按钮")]
    public Button confirmRegisterButton;
    [Tooltip("返回登录按钮")]
    public Button backToLoginButton;
    [Tooltip("注册状态提示文本")]
    public TextMeshProUGUI registerStatusText;

    // ==================== 内部引用 ====================
    private LoginManager loginManager; // 登录管理器引用，用于处理登录注册逻辑

    /// <summary>
    /// 初始化方法 - 设置UI组件引用和事件监听
    /// 目的：建立UI与逻辑的连接，配置用户交互响应
    /// </summary>
    void Start()
    {
        // 查找并获取登录管理器组件
        loginManager = FindObjectOfType<LoginManager>();
        
        // ==================== 设置按钮点击事件 ====================
        loginButton.onClick.AddListener(OnLoginClick);                    // 登录按钮事件
        registerButton.onClick.AddListener(OnRegisterClick);              // 跳转注册按钮事件
        confirmRegisterButton.onClick.AddListener(OnConfirmRegisterClick); // 确认注册按钮事件
        backToLoginButton.onClick.AddListener(OnBackToLoginClick);        // 返回登录按钮事件
        
        // ==================== 初始化界面状态 ====================
        ShowLoginPanel();    // 默认显示登录面板
        ClearStatusText();   // 清空状态文本
        
        // ==================== 设置键盘交互 ====================
        // 在输入框中按回车键也能触发登录
        usernameInput.onSubmit.AddListener(delegate { OnLoginClick(); });
        passwordInput.onSubmit.AddListener(delegate { OnLoginClick(); });
    }

    /// <summary>
    /// 登录按钮点击处理方法
    /// 功能：验证用户输入，调用登录逻辑，更新UI状态
    /// 目的：处理用户登录请求，提供即时反馈
    /// </summary>
    public void OnLoginClick()
    {
        // ==================== 获取用户输入 ====================
        string username = usernameInput.text.Trim(); // 去除首尾空格的用户名
        string password = passwordInput.text;         // 密码（保持原样）

        // ==================== 前端输入验证 ====================
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatus("请输入用户名和密码！", Color.red);
            return;
        }

        // ==================== 开始登录流程 ====================
        ShowStatus("正在登录...", Color.yellow);  // 显示登录中状态
        loginButton.interactable = false;        // 禁用登录按钮防止重复点击

        // 启动异步登录协程
        StartCoroutine(LoginCoroutine(username, password));
    }

    /// <summary>
    /// 登录协程 - 异步处理登录请求
    /// 功能：调用登录管理器执行登录逻辑，等待结果返回
    /// 目的：避免阻塞主线程，提供流畅的用户体验
    /// </summary>
    private IEnumerator LoginCoroutine(string username, string password)
    {
        // 等待登录管理器处理登录请求并返回结果
        yield return StartCoroutine(loginManager.LoginUser(username, password, OnLoginResult));
    }

    /// <summary>
    /// 登录结果回调方法
    /// 功能：处理登录成功或失败的结果，更新UI状态
    /// 目的：根据登录结果给用户相应的反馈
    /// </summary>
    /// <param name="success">登录是否成功</param>
    /// <param name="message">返回的消息</param>
    private void OnLoginResult(bool success, string message)
    {
        // ==================== 恢复UI交互状态 ====================
        loginButton.interactable = true; // 重新启用登录按钮
        
        if (success)
        {
            // ==================== 登录成功处理 ====================
            ShowStatus("登录成功！", Color.green);
            Debug.Log("登录成功，跳转到主界面");
            // 这里可以加载主场景或切换到主界面
            // SceneManager.LoadScene("MainScene");
        }
        else
        {
            // ==================== 登录失败处理 ====================
            ShowStatus(message, Color.red); // 显示错误信息
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