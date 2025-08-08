# Unity MySQL 登录系统

这是一个完整的Unity登录系统，配合MySQL数据库实现用户注册、登录、会话管理等功能。

## 功能特性

- ✅ 用户注册和登录
- ✅ 密码安全哈希存储
- ✅ 会话管理和自动登录
- ✅ 用户输入验证
- ✅ 数据库自动初始化
- ✅ 登录状态持久化
- ✅ 美观的UI界面设计
- ✅ 错误处理和用户反馈

## 系统要求

- Unity 2020.3 LTS 或更新版本
- MySQL 5.7 或更新版本
- TextMeshPro（Unity包管理器中安装）
- MySQL.Data.dll 依赖库

## 安装步骤

### 1. 安装MySQL数据库

确保你的系统中已安装MySQL数据库，并记住以下信息：
- 服务器地址（通常是 localhost）
- 端口号（默认3306）
- 用户名（通常是 root）
- 密码

### 2. 初始化数据库

使用提供的SQL脚本初始化数据库：

```sql
-- 在MySQL命令行或phpMyAdmin中执行
source DatabaseSetup.sql
```

或者直接复制 `Scripts/DatabaseSetup.sql` 文件中的内容到MySQL客户端执行。

### 3. 安装Unity依赖

#### 3.1 安装TextMeshPro
1. 打开Unity包管理器（Window > Package Manager）
2. 搜索 "TextMeshPro"
3. 点击安装

#### 3.2 添加MySQL连接库
1. 下载 MySQL.Data.dll
2. 将DLL文件复制到Unity项目的 `Assets/Plugins/` 目录下
3. 在Inspector中设置平台为 "Any Platform"

### 4. 导入脚本文件

将以下脚本文件复制到你的Unity项目的 `Assets/Scripts/` 目录：

- `LoginUI.cs` - 登录界面控制器
- `DatabaseManager.cs` - 数据库连接管理器
- `User.cs` - 用户数据模型
- `LoginManager.cs` - 登录业务逻辑管理器

## Unity场景设置

### 1. 创建登录界面

创建一个新的Canvas，并按以下层次结构设置UI：

```
Canvas
├── LoginPanel
│   ├── Background (Image)
│   ├── Title (TextMeshPro - Text)
│   ├── UsernameField (TMP_InputField)
│   ├── PasswordField (TMP_InputField)
│   ├── LoginButton (Button)
│   ├── RegisterButton (Button)
│   └── StatusText (TextMeshPro - Text)
└── RegisterPanel
    ├── Background (Image)
    ├── Title (TextMeshPro - Text)
    ├── UsernameField (TMP_InputField)
    ├── PasswordField (TMP_InputField)
    ├── ConfirmPasswordField (TMP_InputField)
    ├── EmailField (TMP_InputField)
    ├── ConfirmRegisterButton (Button)
    ├── BackToLoginButton (Button)
    └── RegisterStatusText (TextMeshPro - Text)
```

### 2. 设置组件

#### 2.1 创建管理器对象
1. 创建空GameObject，命名为 "DatabaseManager"
2. 添加 `DatabaseManager.cs` 脚本
3. 配置数据库连接参数：
   - Server: localhost
   - Database: unity_login_system
   - Username: root
   - Password: (你的MySQL密码)
   - Port: 3306

4. 创建另一个空GameObject，命名为 "LoginManager"
5. 添加 `LoginManager.cs` 脚本
6. 将DatabaseManager对象拖拽到LoginManager的databaseManager字段

#### 2.2 设置LoginUI组件
1. 选择Canvas或其子对象，添加 `LoginUI.cs` 脚本
2. 将对应的UI元素拖拽到相应的字段：
   - Username Input → usernameInput
   - Password Input → passwordInput
   - Login Button → loginButton
   - Register Button → registerButton
   - Status Text → statusText
   - Login Panel → loginPanel
   - Register Panel → registerPanel
   - 注册面板的各个字段也要相应连接

### 3. 配置InputField
- 将密码输入框的Content Type设置为 "Password"
- 设置合适的Placeholder文本
- 配置字符限制（如用户名20字符，密码50字符等）

### 4. 配置Button
- 设置按钮的文本内容
- 确保按钮的Interactable选项已勾选

## 数据库配置

### 数据库连接参数

在 `DatabaseManager.cs` 中修改连接参数：

```csharp
[Header("Database Configuration")]
public string server = "localhost";        // 数据库服务器地址
public string database = "unity_login_system"; // 数据库名称
public string username = "root";            // 数据库用户名
public string password = "";                // 数据库密码
public int port = 3306;                     // 数据库端口
```

### 测试账户

数据库初始化脚本包含以下测试账户（密码都是 "password123"）：
- admin / admin@example.com
- testuser / test@example.com
- demo / demo@example.com

## 使用方法

### 基本功能
1. **注册新用户**：点击注册按钮，填写用户名、密码、确认密码和邮箱
2. **用户登录**：输入用户名和密码，点击登录
3. **自动登录**：勾选"记住登录状态"会在下次启动时自动登录

### 代码使用示例

```csharp
// 获取登录管理器
LoginManager loginManager = FindObjectOfType<LoginManager>();

// 检查登录状态
if (loginManager.IsLoggedIn())
{
    User currentUser = loginManager.GetCurrentUser();
    Debug.Log($"当前用户：{currentUser.username}");
}

// 手动登出
loginManager.LogoutUser();
```

## 安全注意事项

1. **密码安全**：密码使用SHA256哈希+盐值存储，不会明文保存
2. **SQL注入防护**：使用参数化查询防止SQL注入攻击
3. **会话管理**：会话有过期时间（默认7天）
4. **输入验证**：客户端和服务端都有输入验证

## 常见问题

### Q: 连接数据库失败
A: 检查以下项目：
- MySQL服务是否启动
- 连接参数是否正确
- 防火墙是否阻止连接
- Unity是否有MySQL.Data.dll库

### Q: 注册时提示"用户名已存在"
A: 检查数据库中是否已有相同用户名，可以查询users表确认

### Q: UI界面显示异常
A: 确保已安装TextMeshPro包，并正确配置UI引用

### Q: 编译错误
A: 检查是否正确添加了MySQL.Data.dll库到Plugins目录

## 扩展功能

可以考虑添加以下功能：
- 密码重置功能
- 邮箱验证
- 用户头像上传
- 用户权限管理
- 登录日志查看
- 多语言支持

## 许可证

本项目仅供学习和参考使用。在生产环境中使用时，请确保遵循相关的安全最佳实践。

## 更新日志

- v1.0.0: 基础登录注册功能
- 支持MySQL数据库
- 密码安全哈希
- 会话管理
- UI界面集成