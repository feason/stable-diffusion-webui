# 贪吃蛇 Snake

一个零依赖的原生 HTML/CSS/JS 版贪吃蛇小游戏，支持键盘和移动端方向按钮，带分数、最高分（本地存储）、暂停/重开、速度调节。

## 运行

在项目目录下开启一个静态服务器即可预览：

- Python:

```bash
python3 -m http.server 8000
```

然后在浏览器打开 http://localhost:8000

也可使用任意静态服务器（如 `npx serve`）。

## 操作
- 方向键或 WASD 控制方向
- 空格 暂停/继续
- R 重开
- 顶部下拉选择速度

## 文件结构
- `index.html`: 页面结构
- `styles.css`: 样式
- `script.js`: 游戏逻辑