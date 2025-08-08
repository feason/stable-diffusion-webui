using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 登录UI样式设置辅助工具
/// 功能：提供可视化的UI样式配置，快速应用统一的界面风格
/// 目的：简化UI设计流程，确保界面风格的一致性
/// 使用场景：开发阶段的UI美化，支持实时预览和批量应用样式
/// 注意：这个脚本主要用于Editor模式下的UI配置，不会影响最终构建
/// </summary>
public class LoginUISetup : MonoBehaviour
{
    // ==================== UI颜色配置 ====================
    [Header("UI颜色样式配置")]
    [SerializeField] [Tooltip("主要按钮和强调元素的颜色")]
    private Color primaryColor = new Color(0.2f, 0.6f, 1f, 1f);      // 主色调 - 蓝色
    
    [SerializeField] [Tooltip("次要按钮和辅助元素的颜色")]
    private Color secondaryColor = new Color(0.8f, 0.8f, 0.8f, 1f);  // 次要色调 - 灰色
    
    [SerializeField] [Tooltip("面板和容器的背景颜色")]
    private Color backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.9f); // 背景色 - 深蓝色
    
    [SerializeField] [Tooltip("普通文本的颜色")]
    private Color textColor = Color.white;                            // 文字颜色 - 白色
    
    [SerializeField] [Tooltip("错误提示信息的颜色")]
    private Color errorColor = new Color(1f, 0.3f, 0.3f, 1f);       // 错误提示颜色 - 红色
    
    [SerializeField] [Tooltip("成功提示信息的颜色")]
    private Color successColor = new Color(0.3f, 1f, 0.3f, 1f);     // 成功提示颜色 - 绿色

    // ==================== 字体配置 ====================
    [Header("字体样式配置")]
    [SerializeField] [Tooltip("标题文本使用的字体资源")]
    private TMP_FontAsset titleFont;
    
    [SerializeField] [Tooltip("普通文本使用的字体资源")]
    private TMP_FontAsset normalFont;
    
    [SerializeField] [Tooltip("标题文本的字体大小")]
    private int titleFontSize = 24;
    
    [SerializeField] [Tooltip("普通文本的字体大小")]
    private int normalFontSize = 16;
    
    [SerializeField] [Tooltip("按钮文本的字体大小")]
    private int buttonFontSize = 18;

    // ==================== 布局配置 ====================
    [Header("布局尺寸配置")]
    [SerializeField] [Tooltip("登录面板的整体尺寸")]
    private Vector2 panelSize = new Vector2(400, 500);
    
    [SerializeField] [Tooltip("UI元素之间的间距")]
    private float elementSpacing = 20f;
    
    [SerializeField] [Tooltip("按钮的标准高度")]
    private float buttonHeight = 50f;
    
    [SerializeField] [Tooltip("输入框的标准高度")]
    private float inputFieldHeight = 40f;

    // 应用样式配置到UI元素
    [ContextMenu("Apply UI Styling")]
    public void ApplyUIStyle()
    {
        LoginUI loginUI = GetComponent<LoginUI>();
        if (loginUI == null)
        {
            Debug.LogError("LoginUI component not found!");
            return;
        }

        StyleLoginPanel(loginUI);
        StyleRegisterPanel(loginUI);
        Debug.Log("UI styling applied successfully!");
    }

    private void StyleLoginPanel(LoginUI loginUI)
    {
        if (loginUI.loginPanel != null)
        {
            // 设置登录面板样式
            SetupPanelBackground(loginUI.loginPanel);
        }

        // 设置输入框样式
        if (loginUI.usernameInput != null)
        {
            StyleInputField(loginUI.usernameInput, "用户名");
        }

        if (loginUI.passwordInput != null)
        {
            StyleInputField(loginUI.passwordInput, "密码");
            loginUI.passwordInput.contentType = TMP_InputField.ContentType.Password;
        }

        // 设置按钮样式
        if (loginUI.loginButton != null)
        {
            StyleButton(loginUI.loginButton, "登录", primaryColor);
        }

        if (loginUI.registerButton != null)
        {
            StyleButton(loginUI.registerButton, "注册", secondaryColor);
        }

        // 设置状态文本样式
        if (loginUI.statusText != null)
        {
            StyleStatusText(loginUI.statusText);
        }
    }

    private void StyleRegisterPanel(LoginUI loginUI)
    {
        if (loginUI.registerPanel != null)
        {
            SetupPanelBackground(loginUI.registerPanel);
        }

        // 设置注册面板输入框样式
        if (loginUI.regUsernameInput != null)
        {
            StyleInputField(loginUI.regUsernameInput, "用户名");
        }

        if (loginUI.regPasswordInput != null)
        {
            StyleInputField(loginUI.regPasswordInput, "密码");
            loginUI.regPasswordInput.contentType = TMP_InputField.ContentType.Password;
        }

        if (loginUI.regConfirmPasswordInput != null)
        {
            StyleInputField(loginUI.regConfirmPasswordInput, "确认密码");
            loginUI.regConfirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
        }

        if (loginUI.regEmailInput != null)
        {
            StyleInputField(loginUI.regEmailInput, "邮箱地址");
            loginUI.regEmailInput.contentType = TMP_InputField.ContentType.EmailAddress;
        }

        // 设置注册按钮样式
        if (loginUI.confirmRegisterButton != null)
        {
            StyleButton(loginUI.confirmRegisterButton, "确认注册", primaryColor);
        }

        if (loginUI.backToLoginButton != null)
        {
            StyleButton(loginUI.backToLoginButton, "返回登录", secondaryColor);
        }

        // 设置注册状态文本样式
        if (loginUI.registerStatusText != null)
        {
            StyleStatusText(loginUI.registerStatusText);
        }
    }

    private void SetupPanelBackground(GameObject panel)
    {
        Image backgroundImage = panel.GetComponent<Image>();
        if (backgroundImage == null)
        {
            backgroundImage = panel.AddComponent<Image>();
        }

        backgroundImage.color = backgroundColor;
        backgroundImage.raycastTarget = true;

        // 设置面板大小
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = panelSize;
        }
    }

    private void StyleInputField(TMP_InputField inputField, string placeholder)
    {
        // 设置输入框高度
        RectTransform rectTransform = inputField.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, inputFieldHeight);
        }

        // 设置字体
        if (normalFont != null)
        {
            inputField.fontAsset = normalFont;
        }
        inputField.pointSize = normalFontSize;

        // 设置颜色
        inputField.textComponent.color = textColor;

        // 设置占位符文本
        if (inputField.placeholder != null)
        {
            TextMeshProUGUI placeholderText = inputField.placeholder.GetComponent<TextMeshProUGUI>();
            if (placeholderText != null)
            {
                placeholderText.text = placeholder;
                placeholderText.color = new Color(textColor.r, textColor.g, textColor.b, 0.6f);
                if (normalFont != null)
                {
                    placeholderText.font = normalFont;
                }
            }
        }

        // 设置背景颜色
        Image backgroundImage = inputField.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(1f, 1f, 1f, 0.1f);
        }
    }

    private void StyleButton(Button button, string text, Color color)
    {
        // 设置按钮高度
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, buttonHeight);
        }

        // 设置按钮颜色
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }

        // 设置按钮文本
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = text;
            buttonText.color = Color.white;
            buttonText.fontSize = buttonFontSize;
            if (normalFont != null)
            {
                buttonText.font = normalFont;
            }
        }

        // 设置按钮交互颜色
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, color.a);
        colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, color.a);
        colors.disabledColor = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, color.a);
        button.colors = colors;
    }

    private void StyleStatusText(TextMeshProUGUI statusText)
    {
        statusText.color = textColor;
        statusText.fontSize = normalFontSize;
        statusText.text = "";
        statusText.alignment = TextAlignmentOptions.Center;

        if (normalFont != null)
        {
            statusText.font = normalFont;
        }
    }

    // 创建标题文本的辅助方法
    public void CreateTitleText(Transform parent, string titleText)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent);

        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = titleText;
        title.fontSize = titleFontSize;
        title.color = textColor;
        title.alignment = TextAlignmentOptions.Center;

        if (titleFont != null)
        {
            title.font = titleFont;
        }

        RectTransform rectTransform = titleObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(300, 40);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    // 运行时颜色更新方法
    public void UpdateStatusTextColor(TextMeshProUGUI statusText, bool isError, bool isSuccess = false)
    {
        if (statusText == null) return;

        if (isError)
        {
            statusText.color = errorColor;
        }
        else if (isSuccess)
        {
            statusText.color = successColor;
        }
        else
        {
            statusText.color = textColor;
        }
    }

    // 获取预定义颜色
    public Color GetErrorColor() => errorColor;
    public Color GetSuccessColor() => successColor;
    public Color GetPrimaryColor() => primaryColor;
    public Color GetSecondaryColor() => secondaryColor;
    public Color GetTextColor() => textColor;
}