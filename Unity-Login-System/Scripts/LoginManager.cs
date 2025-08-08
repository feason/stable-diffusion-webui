using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    [Header("References")]
    public DatabaseManager databaseManager;
    
    [Header("Session Settings")]
    public bool rememberSession = true;
    
    private UserSession currentSession;
    private const string SESSION_KEY = "UserSession";

    void Awake()
    {
        // 单例模式
        if (FindObjectsOfType<LoginManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        currentSession = new UserSession();
        
        if (databaseManager == null)
        {
            databaseManager = FindObjectOfType<DatabaseManager>();
        }
        
        if (databaseManager == null)
        {
            Debug.LogError("找不到 DatabaseManager！请确保场景中有 DatabaseManager 组件。");
        }
        
        // 尝试恢复会话
        if (rememberSession)
        {
            LoadSession();
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