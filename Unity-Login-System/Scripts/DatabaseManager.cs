using System;
using System.Collections;
using System.Data;
using MySql.Data.MySqlClient;
using UnityEngine;

/// <summary>
/// 数据库连接管理器
/// 功能：管理MySQL数据库连接，提供用户数据的增删改查操作
/// 目的：封装数据库操作，提供安全的数据访问接口，自动处理数据库初始化
/// 设计模式：单例模式，确保全局只有一个数据库连接管理器
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    // ==================== 数据库配置参数 ====================
    [Header("数据库连接配置")]
    [Tooltip("MySQL服务器地址")]
    public string server = "localhost";
    [Tooltip("数据库名称")]
    public string database = "unity_login_system";
    [Tooltip("数据库用户名")]
    public string username = "root";
    [Tooltip("数据库密码")]
    public string password = "";
    [Tooltip("数据库端口号")]
    public int port = 3306;

    // ==================== 内部变量 ====================
    private string connectionString; // 数据库连接字符串

    /// <summary>
    /// Unity生命周期 - 对象唤醒时调用
    /// 功能：实现单例模式，初始化数据库连接
    /// 目的：确保整个游戏中只有一个数据库管理器实例
    /// </summary>
    void Awake()
    {
        // ==================== 单例模式实现 ====================
        // 检查是否已存在其他DatabaseManager实例
        if (FindObjectsOfType<DatabaseManager>().Length > 1)
        {
            Destroy(gameObject); // 销毁重复的实例
            return;
        }
        
        // ==================== 持久化设置 ====================
        DontDestroyOnLoad(gameObject); // 场景切换时不销毁此对象
        
        // ==================== 初始化连接配置 ====================
        InitializeConnection(); // 构建数据库连接字符串
    }

    /// <summary>
    /// Unity生命周期 - 对象启动时调用
    /// 功能：初始化数据库结构
    /// 目的：确保数据库和表结构存在
    /// </summary>
    void Start()
    {
        InitializeDatabase(); // 创建数据库和表结构
    }

    private void InitializeConnection()
    {
        connectionString = $"Server={server};Database={database};Uid={username};Pwd={password};Port={port};";
        Debug.Log("数据库连接字符串已配置");
    }

    private void InitializeDatabase()
    {
        StartCoroutine(CreateDatabaseAndTables());
    }

    private IEnumerator CreateDatabaseAndTables()
    {
        yield return StartCoroutine(CreateDatabase());
        yield return StartCoroutine(CreateUserTable());
    }

    private IEnumerator CreateDatabase()
    {
        string createDbConnectionString = $"Server={server};Uid={username};Pwd={password};Port={port};";
        
        using (MySqlConnection connection = new MySqlConnection(createDbConnectionString))
        {
            try
            {
                connection.Open();
                string createDbQuery = $"CREATE DATABASE IF NOT EXISTS {database} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
                
                using (MySqlCommand command = new MySqlCommand(createDbQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                Debug.Log($"数据库 {database} 创建成功或已存在");
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建数据库失败: {ex.Message}");
            }
        }
        
        yield return null;
    }

    private IEnumerator CreateUserTable()
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    username VARCHAR(50) UNIQUE NOT NULL,
                    password_hash VARCHAR(255) NOT NULL,
                    email VARCHAR(100) UNIQUE NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    last_login TIMESTAMP NULL,
                    is_active BOOLEAN DEFAULT TRUE
                ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
                
                using (MySqlCommand command = new MySqlCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                Debug.Log("用户表创建成功或已存在");
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建用户表失败: {ex.Message}");
            }
        }
        
        yield return null;
    }

    public IEnumerator TestConnection(System.Action<bool, string> callback)
    {
        bool success = false;
        string message = "";
        
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                success = true;
                message = "数据库连接成功";
                Debug.Log("数据库连接测试成功");
            }
            catch (Exception ex)
            {
                success = false;
                message = $"数据库连接失败: {ex.Message}";
                Debug.LogError(message);
            }
        }
        
        yield return null;
        callback?.Invoke(success, message);
    }

    public IEnumerator CheckUserExists(string username, System.Action<bool> callback)
    {
        bool exists = false;
        
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE username = @username";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    exists = count > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"检查用户是否存在时出错: {ex.Message}");
            }
        }
        
        yield return null;
        callback?.Invoke(exists);
    }

    public IEnumerator CheckEmailExists(string email, System.Action<bool> callback)
    {
        bool exists = false;
        
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE email = @email";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    exists = count > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"检查邮箱是否存在时出错: {ex.Message}");
            }
        }
        
        yield return null;
        callback?.Invoke(exists);
    }

    public IEnumerator CreateUser(User user, System.Action<bool, string> callback)
    {
        bool success = false;
        string message = "";
        
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                string query = @"
                INSERT INTO users (username, password_hash, email) 
                VALUES (@username, @password_hash, @email)";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", user.username);
                    command.Parameters.AddWithValue("@password_hash", user.passwordHash);
                    command.Parameters.AddWithValue("@email", user.email);
                    
                    int rowsAffected = command.ExecuteNonQuery();
                    
                    if (rowsAffected > 0)
                    {
                        success = true;
                        message = "用户创建成功";
                    }
                    else
                    {
                        success = false;
                        message = "用户创建失败";
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                message = $"创建用户时出错: {ex.Message}";
                Debug.LogError(message);
            }
        }
        
        yield return null;
        callback?.Invoke(success, message);
    }

    public IEnumerator ValidateUser(string username, string passwordHash, System.Action<bool, User> callback)
    {
        bool success = false;
        User user = null;
        
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                string query = @"
                SELECT id, username, email, created_at, last_login, is_active 
                FROM users 
                WHERE username = @username AND password_hash = @password_hash AND is_active = TRUE";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password_hash", passwordHash);
                    
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                id = reader.GetInt32("id"),
                                username = reader.GetString("username"),
                                email = reader.GetString("email"),
                                createdAt = reader.GetDateTime("created_at"),
                                lastLogin = reader.IsDBNull("last_login") ? (DateTime?)null : reader.GetDateTime("last_login"),
                                isActive = reader.GetBoolean("is_active")
                            };
                            success = true;
                        }
                    }
                }
                
                // 更新最后登录时间
                if (success && user != null)
                {
                    string updateQuery = "UPDATE users SET last_login = NOW() WHERE id = @id";
                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@id", user.id);
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                Debug.LogError($"验证用户时出错: {ex.Message}");
            }
        }
        
        yield return null;
        callback?.Invoke(success, user);
    }

    public IEnumerator GetUserById(int userId, System.Action<User> callback)
    {
        User user = null;
        
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                string query = @"
                SELECT id, username, email, created_at, last_login, is_active 
                FROM users 
                WHERE id = @id AND is_active = TRUE";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", userId);
                    
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                id = reader.GetInt32("id"),
                                username = reader.GetString("username"),
                                email = reader.GetString("email"),
                                createdAt = reader.GetDateTime("created_at"),
                                lastLogin = reader.IsDBNull("last_login") ? (DateTime?)null : reader.GetDateTime("last_login"),
                                isActive = reader.GetBoolean("is_active")
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"获取用户信息时出错: {ex.Message}");
            }
        }
        
        yield return null;
        callback?.Invoke(user);
    }

    public void CloseConnection()
    {
        // MySQL连接在using块中自动关闭，这里用于清理其他资源
        Debug.Log("数据库连接已关闭");
    }

    void OnApplicationQuit()
    {
        CloseConnection();
    }
}