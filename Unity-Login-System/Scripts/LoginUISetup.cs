using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 这个脚本用于帮助快速设置登录UI的样式和布局
// 在Editor模式下运行，不会包含在最终构建中
public class LoginUISetup : MonoBehaviour
{
    [Header("UI Style Configuration")]
    [SerializeField] private Color primaryColor = new Color(0.2f, 0.6f, 1f, 1f);      // 主色调
    [SerializeField] private Color secondaryColor = new Color(0.8f, 0.8f, 0.8f, 1f);  // 次要色调
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.9f); // 背景色
    [SerializeField] private Color textColor = Color.white;                            // 文字颜色
    [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f, 1f);       // 错误提示颜色
    [SerializeField] private Color successColor = new Color(0.3f, 1f, 0.3f, 1f);     // 成功提示颜色

    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset titleFont;
    [SerializeField] private TMP_FontAsset normalFont;
    [SerializeField] private int titleFontSize = 24;
    [SerializeField] private int normalFontSize = 16;
    [SerializeField] private int buttonFontSize = 18;

    [Header("Layout Settings")]
    [SerializeField] private Vector2 panelSize = new Vector2(400, 500);
    [SerializeField] private float elementSpacing = 20f;
    [SerializeField] private float buttonHeight = 50f;
    [SerializeField] private float inputFieldHeight = 40f;

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