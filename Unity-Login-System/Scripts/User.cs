using System;
using UnityEngine;

/// <summary>
/// 用户数据模型类
/// 功能：定义用户的基本信息结构，提供数据验证方法
/// 目的：封装用户相关数据，确保数据的一致性和有效性
/// 特性：可序列化，支持Unity Inspector显示和JSON转换
/// </summary>
[System.Serializable]
public class User
{
    // ==================== 用户基本信息字段 ====================
    [Tooltip("用户唯一标识ID（数据库自动生成）")]
    public int id;                    // 用户ID（数据库主键）
    
    [Tooltip("用户名（3-20位字母数字下划线）")]
    public string username;           // 用户名
    
    [Tooltip("密码哈希值（SHA256加密后的密码）")]
    public string passwordHash;       // 密码哈希值（不存储明文密码）
    
    [Tooltip("用户邮箱地址")]
    public string email;              // 邮箱地址
    
    [Tooltip("账户创建时间")]
    public DateTime createdAt;        // 创建时间
    
    [Tooltip("最后登录时间（可为空）")]
    public DateTime? lastLogin;       // 最后登录时间（可空类型）
    
    [Tooltip("账户是否激活状态")]
    public bool isActive;             // 账户激活状态

    /// <summary>
    /// 默认构造函数
    /// 功能：创建空的用户对象，设置默认值
    /// 目的：提供用户对象的基础初始化
    /// </summary>
    public User()
    {
        id = 0;                       // ID初始化为0（数据库会自动分配）
        username = "";                // 用户名初始化为空
        passwordHash = "";            // 密码哈希初始化为空
        email = "";                   // 邮箱初始化为空
        createdAt = DateTime.Now;     // 创建时间设置为当前时间
        lastLogin = null;             // 最后登录时间初始化为空
        isActive = true;              // 默认账户为激活状态
    }

    /// <summary>
    /// 参数化构造函数
    /// 功能：使用指定参数创建用户对象
    /// 目的：快速创建包含基本信息的用户对象（用于注册）
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="passwordHash">密码哈希值</param>
    /// <param name="email">邮箱地址</param>
    public User(string username, string passwordHash, string email)
    {
        this.id = 0;                          // ID由数据库自动分配
        this.username = username;             // 设置用户名
        this.passwordHash = passwordHash;     // 设置密码哈希
        this.email = email;                   // 设置邮箱
        this.createdAt = DateTime.Now;        // 设置创建时间为当前时间
        this.lastLogin = null;                // 新用户未登录过
        this.isActive = true;                 // 新用户默认激活
    }

    public override string ToString()
    {
        return $"User [ID: {id}, Username: {username}, Email: {email}, Created: {createdAt}, Active: {isActive}]";
    }

    // 验证用户名格式
    public static bool IsValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return false;
        
        if (username.Length < 3 || username.Length > 20)
            return false;
        
        // 只允许字母、数字和下划线
        foreach (char c in username)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;
        }
        
        return true;
    }

    // 验证密码强度
    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;
        
        if (password.Length < 6)
            return false;
        
        // 可以添加更复杂的密码规则，比如必须包含大小写字母、数字等
        return true;
    }

    // 验证邮箱格式
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
        
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

// 用户会话信息
[System.Serializable]
public class UserSession
{
    public User currentUser;
    public DateTime loginTime;
    public bool isLoggedIn;

    public UserSession()
    {
        currentUser = null;
        loginTime = DateTime.MinValue;
        isLoggedIn = false;
    }

    public void StartSession(User user)
    {
        currentUser = user;
        loginTime = DateTime.Now;
        isLoggedIn = true;
        Debug.Log($"用户 {user.username} 会话开始");
    }

    public void EndSession()
    {
        if (currentUser != null)
        {
            Debug.Log($"用户 {currentUser.username} 会话结束");
        }
        
        currentUser = null;
        loginTime = DateTime.MinValue;
        isLoggedIn = false;
    }

    public TimeSpan GetSessionDuration()
    {
        if (isLoggedIn)
        {
            return DateTime.Now - loginTime;
        }
        return TimeSpan.Zero;
    }
}