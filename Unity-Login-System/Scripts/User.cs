using System;
using UnityEngine;

[System.Serializable]
public class User
{
    public int id;
    public string username;
    public string passwordHash;
    public string email;
    public DateTime createdAt;
    public DateTime? lastLogin;
    public bool isActive;

    public User()
    {
        id = 0;
        username = "";
        passwordHash = "";
        email = "";
        createdAt = DateTime.Now;
        lastLogin = null;
        isActive = true;
    }

    public User(string username, string passwordHash, string email)
    {
        this.id = 0;
        this.username = username;
        this.passwordHash = passwordHash;
        this.email = email;
        this.createdAt = DateTime.Now;
        this.lastLogin = null;
        this.isActive = true;
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