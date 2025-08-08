using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// 登录管理器 - 核心业务逻辑控制器
/// 功能：处理用户登录、注册、会话管理等核心业务逻辑
/// 目的：作为UI和数据库之间的中间层，封装复杂的业务逻辑
/// 特性：单例模式、密码加密、会话持久化、异步操作
/// 安全性：SHA256密码哈希、会话过期管理、输入验证
/// </summary>
public class LoginManager : MonoBehaviour
{
    // ==================== 组件引用配置 ====================
    [Header("组件引用")]
    [Tooltip("数据库管理器引用")]
    public DatabaseManager databaseManager;
    
    // ==================== 会话管理配置 ====================
    [Header("会话管理设置")]
    [Tooltip("是否记住用户登录状态（重启后自动登录）")]
    public bool rememberSession = true;
    
    // ==================== 内部变量 ====================
    private UserSession currentSession;              // 当前用户会话
    private const string SESSION_KEY = "UserSession"; // 会话数据存储键名

    /// <summary>
    /// Unity生命周期 - 对象唤醒时调用
    /// 功能：实现单例模式，确保全局唯一性
    /// 目的：防止多个LoginManager实例同时存在
    /// </summary>
    void Awake()
    {
        // ==================== 单例模式实现 ====================
        if (FindObjectsOfType<LoginManager>().Length > 1)
        {
            Destroy(gameObject); // 销毁重复实例
            return;
        }
        
        // ==================== 跨场景持久化 ====================
        DontDestroyOnLoad(gameObject); // 场景切换时保持存在
    }

    /// <summary>
    /// Unity生命周期 - 对象启动时调用
    /// 功能：初始化会话系统，建立组件引用
    /// 目的：准备登录系统的运行环境
    /// </summary>
    void Start()
    {
        // ==================== 初始化用户会话 ====================
        currentSession = new UserSession();
        
        // ==================== 自动查找数据库管理器 ====================
        if (databaseManager == null)
        {
            databaseManager = FindObjectOfType<DatabaseManager>();
        }
        
        // ==================== 验证组件依赖 ====================
        if (databaseManager == null)
        {
            Debug.LogError("找不到 DatabaseManager！请确保场景中有 DatabaseManager 组件。");
        }
        
        // ==================== 恢复上次登录会话 ====================
        if (rememberSession)
        {
            LoadSession(); // 尝试从本地存储恢复用户会话
        }
    }

    // 用户登录
    public IEnumerator LoginUser(string username, string password, System.Action<bool, string> callback)
    {
        if (databaseManager == null)
        {
            callback?.Invoke(false, "数据库管理器未初始化");
            yield break;
        }

        // 验证输入
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            callback?.Invoke(false, "用户名和密码不能为空");
            yield break;
        }

        // 对密码进行哈希处理
        string passwordHash = HashPassword(password);
        
        bool loginSuccess = false;
        User user = null;
        
        // 验证用户
        yield return StartCoroutine(databaseManager.ValidateUser(username, passwordHash, (success, userData) =>
        {
            loginSuccess = success;
            user = userData;
        }));

        if (loginSuccess && user != null)
        {
            // 开始用户会话
            currentSession.StartSession(user);
            
            // 保存会话信息
            if (rememberSession)
            {
                SaveSession();
            }
            
            callback?.Invoke(true, "登录成功");
            Debug.Log($"用户 {username} 登录成功");
        }
        else
        {
            callback?.Invoke(false, "用户名或密码错误");
            Debug.Log($"用户 {username} 登录失败");
        }
    }

    // 用户注册
    public IEnumerator RegisterUser(string username, string password, string email, System.Action<bool, string> callback)
    {
        if (databaseManager == null)
        {
            callback?.Invoke(false, "数据库管理器未初始化");
            yield break;
        }

        // 验证输入数据
        if (!User.IsValidUsername(username))
        {
            callback?.Invoke(false, "用户名格式不正确（3-20位字母数字下划线）");
            yield break;
        }

        if (!User.IsValidPassword(password))
        {
            callback?.Invoke(false, "密码格式不正确（至少6位）");
            yield break;
        }

        if (!User.IsValidEmail(email))
        {
            callback?.Invoke(false, "邮箱格式不正确");
            yield break;
        }

        // 检查用户名是否已存在
        bool usernameExists = false;
        yield return StartCoroutine(databaseManager.CheckUserExists(username, (exists) =>
        {
            usernameExists = exists;
        }));

        if (usernameExists)
        {
            callback?.Invoke(false, "用户名已存在");
            yield break;
        }

        // 检查邮箱是否已存在
        bool emailExists = false;
        yield return StartCoroutine(databaseManager.CheckEmailExists(email, (exists) =>
        {
            emailExists = exists;
        }));

        if (emailExists)
        {
            callback?.Invoke(false, "邮箱已被注册");
            yield break;
        }

        // 创建新用户
        string passwordHash = HashPassword(password);
        User newUser = new User(username, passwordHash, email);
        
        bool createSuccess = false;
        string createMessage = "";
        
        yield return StartCoroutine(databaseManager.CreateUser(newUser, (success, message) =>
        {
            createSuccess = success;
            createMessage = message;
        }));

        if (createSuccess)
        {
            callback?.Invoke(true, "注册成功");
            Debug.Log($"用户 {username} 注册成功");
        }
        else
        {
            callback?.Invoke(false, createMessage);
            Debug.Log($"用户 {username} 注册失败: {createMessage}");
        }
    }

    // 用户登出
    public void LogoutUser()
    {
        if (currentSession.isLoggedIn)
        {
            Debug.Log($"用户 {currentSession.currentUser.username} 登出");
            currentSession.EndSession();
            
            // 清除保存的会话
            ClearSavedSession();
        }
    }

    // 获取当前用户信息
    public User GetCurrentUser()
    {
        return currentSession?.currentUser;
    }

    // 检查是否已登录
    public bool IsLoggedIn()
    {
        return currentSession != null && currentSession.isLoggedIn;
    }

    // 获取会话持续时间
    public TimeSpan GetSessionDuration()
    {
        return currentSession?.GetSessionDuration() ?? TimeSpan.Zero;
    }

    // 密码哈希处理
    private string HashPassword(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // 添加盐值增强安全性
            string saltedPassword = password + "UnityLoginSalt2024";
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    // 保存会话信息
    private void SaveSession()
    {
        if (currentSession.isLoggedIn && currentSession.currentUser != null)
        {
            string sessionData = JsonUtility.ToJson(currentSession.currentUser);
            PlayerPrefs.SetString(SESSION_KEY, sessionData);
            PlayerPrefs.SetString(SESSION_KEY + "_LoginTime", currentSession.loginTime.ToBinary().ToString());
            PlayerPrefs.Save();
            Debug.Log("会话信息已保存");
        }
    }

    // 加载会话信息
    private void LoadSession()
    {
        if (PlayerPrefs.HasKey(SESSION_KEY))
        {
            try
            {
                string sessionData = PlayerPrefs.GetString(SESSION_KEY);
                string loginTimeData = PlayerPrefs.GetString(SESSION_KEY + "_LoginTime");
                
                if (!string.IsNullOrEmpty(sessionData) && !string.IsNullOrEmpty(loginTimeData))
                {
                    User user = JsonUtility.FromJson<User>(sessionData);
                    DateTime loginTime = DateTime.FromBinary(Convert.ToInt64(loginTimeData));
                    
                    // 检查会话是否过期（例如：7天）
                    if ((DateTime.Now - loginTime).TotalDays < 7)
                    {
                        currentSession.currentUser = user;
                        currentSession.loginTime = loginTime;
                        currentSession.isLoggedIn = true;
                        
                        Debug.Log($"会话恢复成功：用户 {user.username}");
                    }
                    else
                    {
                        ClearSavedSession();
                        Debug.Log("会话已过期，已清除");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载会话失败: {ex.Message}");
                ClearSavedSession();
            }
        }
    }

    // 清除保存的会话
    private void ClearSavedSession()
    {
        PlayerPrefs.DeleteKey(SESSION_KEY);
        PlayerPrefs.DeleteKey(SESSION_KEY + "_LoginTime");
        PlayerPrefs.Save();
        Debug.Log("已清除保存的会话信息");
    }

    // 更新用户最后登录时间
    public IEnumerator UpdateLastLoginTime(System.Action<bool> callback = null)
    {
        if (currentSession.isLoggedIn && currentSession.currentUser != null)
        {
            // 这里可以调用数据库更新最后登录时间
            // 在 DatabaseManager 的 ValidateUser 方法中已经自动更新了
            callback?.Invoke(true);
        }
        else
        {
            callback?.Invoke(false);
        }
        
        yield return null;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && rememberSession)
        {
            SaveSession();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && rememberSession)
        {
            SaveSession();
        }
    }

    void OnApplicationQuit()
    {
        if (rememberSession)
        {
            SaveSession();
        }
    }
}