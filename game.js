// 游戏配置
const GRID_SIZE = 20;
const CANVAS_SIZE = 400;
const INITIAL_SPEED = 150;

// 游戏状态
let canvas, ctx;
let gameRunning = false;
let gamePaused = false;
let gameSpeed = INITIAL_SPEED;
let score = 0;
let highScore = 0;

// 蛇的状态
let snake = [];
let direction = 'right';
let nextDirection = 'right';

// 食物状态
let food = {};

// 游戏循环
let gameLoop;

// 初始化游戏
function initGame() {
    canvas = document.getElementById('gameCanvas');
    ctx = canvas.getContext('2d');
    
    // 从本地存储加载最高分
    highScore = localStorage.getItem('snakeHighScore') || 0;
    document.getElementById('highScore').textContent = highScore;
    
    // 初始化蛇
    resetSnake();
    
    // 生成第一个食物
    generateFood();
    
    // 绑定键盘事件
    bindKeyEvents();
    
    // 开始游戏
    startGame();
}

// 重置蛇的状态
function resetSnake() {
    snake = [
        { x: 100, y: 200 },
        { x: 80, y: 200 },
        { x: 60, y: 200 }
    ];
    direction = 'right';
    nextDirection = 'right';
}

// 生成食物
function generateFood() {
    let validPosition = false;
    
    while (!validPosition) {
        food = {
            x: Math.floor(Math.random() * (CANVAS_SIZE / GRID_SIZE)) * GRID_SIZE,
            y: Math.floor(Math.random() * (CANVAS_SIZE / GRID_SIZE)) * GRID_SIZE
        };
        
        // 确保食物不会生成在蛇身上
        validPosition = !snake.some(segment => 
            segment.x === food.x && segment.y === food.y
        );
    }
}

// 绑定键盘事件
function bindKeyEvents() {
    document.addEventListener('keydown', (event) => {
        if (!gameRunning && event.code !== 'Space') return;
        
        switch (event.code) {
            case 'ArrowUp':
                if (direction !== 'down') nextDirection = 'up';
                event.preventDefault();
                break;
            case 'ArrowDown':
                if (direction !== 'up') nextDirection = 'down';
                event.preventDefault();
                break;
            case 'ArrowLeft':
                if (direction !== 'right') nextDirection = 'left';
                event.preventDefault();
                break;
            case 'ArrowRight':
                if (direction !== 'left') nextDirection = 'right';
                event.preventDefault();
                break;
            case 'Space':
                togglePause();
                event.preventDefault();
                break;
        }
    });
}

// 开始游戏
function startGame() {
    gameRunning = true;
    gamePaused = false;
    gameLoop = setInterval(updateGame, gameSpeed);
}

// 暂停/继续游戏
function togglePause() {
    if (!gameRunning) return;
    
    gamePaused = !gamePaused;
    
    if (gamePaused) {
        clearInterval(gameLoop);
        // 在canvas上显示暂停信息
        ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
        ctx.fillRect(0, 0, CANVAS_SIZE, CANVAS_SIZE);
        ctx.fillStyle = 'white';
        ctx.font = '30px Arial';
        ctx.textAlign = 'center';
        ctx.fillText('游戏暂停', CANVAS_SIZE / 2, CANVAS_SIZE / 2);
        ctx.font = '16px Arial';
        ctx.fillText('按空格键继续', CANVAS_SIZE / 2, CANVAS_SIZE / 2 + 40);
    } else {
        gameLoop = setInterval(updateGame, gameSpeed);
    }
}

// 更新游戏状态
function updateGame() {
    // 更新方向
    direction = nextDirection;
    
    // 移动蛇
    moveSnake();
    
    // 检查碰撞
    if (checkCollision()) {
        endGame();
        return;
    }
    
    // 检查是否吃到食物
    if (checkFoodCollision()) {
        eatFood();
    }
    
    // 渲染游戏
    render();
}

// 移动蛇
function moveSnake() {
    const head = { ...snake[0] };
    
    switch (direction) {
        case 'up':
            head.y -= GRID_SIZE;
            break;
        case 'down':
            head.y += GRID_SIZE;
            break;
        case 'left':
            head.x -= GRID_SIZE;
            break;
        case 'right':
            head.x += GRID_SIZE;
            break;
    }
    
    snake.unshift(head);
    snake.pop();
}

// 检查碰撞
function checkCollision() {
    const head = snake[0];
    
    // 检查墙壁碰撞
    if (head.x < 0 || head.x >= CANVAS_SIZE || 
        head.y < 0 || head.y >= CANVAS_SIZE) {
        return true;
    }
    
    // 检查自身碰撞
    for (let i = 1; i < snake.length; i++) {
        if (head.x === snake[i].x && head.y === snake[i].y) {
            return true;
        }
    }
    
    return false;
}

// 检查食物碰撞
function checkFoodCollision() {
    const head = snake[0];
    return head.x === food.x && head.y === food.y;
}

// 吃食物
function eatFood() {
    // 增加蛇的长度
    snake.push({ ...snake[snake.length - 1] });
    
    // 增加分数
    score += 10;
    document.getElementById('score').textContent = score;
    
    // 生成新食物
    generateFood();
    
    // 增加游戏速度
    if (score % 50 === 0 && gameSpeed > 80) {
        gameSpeed -= 10;
        clearInterval(gameLoop);
        gameLoop = setInterval(updateGame, gameSpeed);
    }
}

// 渲染游戏
function render() {
    // 清空画布
    ctx.fillStyle = 'rgba(0, 0, 0, 0.1)';
    ctx.fillRect(0, 0, CANVAS_SIZE, CANVAS_SIZE);
    
    // 绘制网格（可选，用于调试）
    drawGrid();
    
    // 绘制食物
    drawFood();
    
    // 绘制蛇
    drawSnake();
}

// 绘制网格
function drawGrid() {
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.1)';
    ctx.lineWidth = 1;
    
    for (let i = 0; i <= CANVAS_SIZE; i += GRID_SIZE) {
        ctx.beginPath();
        ctx.moveTo(i, 0);
        ctx.lineTo(i, CANVAS_SIZE);
        ctx.stroke();
        
        ctx.beginPath();
        ctx.moveTo(0, i);
        ctx.lineTo(CANVAS_SIZE, i);
        ctx.stroke();
    }
}

// 绘制食物
function drawFood() {
    // 绘制食物主体
    ctx.fillStyle = '#ff6b6b';
    ctx.fillRect(food.x, food.y, GRID_SIZE, GRID_SIZE);
    
    // 添加食物光晕效果
    ctx.shadowColor = '#ff6b6b';
    ctx.shadowBlur = 10;
    ctx.fillRect(food.x + 2, food.y + 2, GRID_SIZE - 4, GRID_SIZE - 4);
    ctx.shadowBlur = 0;
    
    // 绘制食物高光
    ctx.fillStyle = '#ffaaaa';
    ctx.fillRect(food.x + 4, food.y + 4, GRID_SIZE - 12, GRID_SIZE - 12);
}

// 绘制蛇
function drawSnake() {
    snake.forEach((segment, index) => {
        if (index === 0) {
            // 绘制蛇头
            ctx.fillStyle = '#4ecdc4';
            ctx.fillRect(segment.x, segment.y, GRID_SIZE, GRID_SIZE);
            
            // 蛇头光晕
            ctx.shadowColor = '#4ecdc4';
            ctx.shadowBlur = 8;
            ctx.fillRect(segment.x + 2, segment.y + 2, GRID_SIZE - 4, GRID_SIZE - 4);
            ctx.shadowBlur = 0;
            
            // 绘制眼睛
            ctx.fillStyle = '#333';
            const eyeSize = 3;
            const eyeOffset = 5;
            
            if (direction === 'right') {
                ctx.fillRect(segment.x + GRID_SIZE - eyeOffset, segment.y + 4, eyeSize, eyeSize);
                ctx.fillRect(segment.x + GRID_SIZE - eyeOffset, segment.y + GRID_SIZE - 7, eyeSize, eyeSize);
            } else if (direction === 'left') {
                ctx.fillRect(segment.x + 2, segment.y + 4, eyeSize, eyeSize);
                ctx.fillRect(segment.x + 2, segment.y + GRID_SIZE - 7, eyeSize, eyeSize);
            } else if (direction === 'up') {
                ctx.fillRect(segment.x + 4, segment.y + 2, eyeSize, eyeSize);
                ctx.fillRect(segment.x + GRID_SIZE - 7, segment.y + 2, eyeSize, eyeSize);
            } else if (direction === 'down') {
                ctx.fillRect(segment.x + 4, segment.y + GRID_SIZE - eyeOffset, eyeSize, eyeSize);
                ctx.fillRect(segment.x + GRID_SIZE - 7, segment.y + GRID_SIZE - eyeOffset, eyeSize, eyeSize);
            }
        } else {
            // 绘制蛇身
            const intensity = 1 - (index / snake.length) * 0.5;
            ctx.fillStyle = `rgba(150, 206, 180, ${intensity})`;
            ctx.fillRect(segment.x, segment.y, GRID_SIZE, GRID_SIZE);
            
            // 蛇身边框
            ctx.strokeStyle = 'rgba(78, 205, 196, 0.3)';
            ctx.lineWidth = 1;
            ctx.strokeRect(segment.x, segment.y, GRID_SIZE, GRID_SIZE);
        }
    });
}

// 结束游戏
function endGame() {
    gameRunning = false;
    clearInterval(gameLoop);
    
    // 检查是否创造新纪录
    let isNewRecord = false;
    if (score > highScore) {
        highScore = score;
        localStorage.setItem('snakeHighScore', highScore);
        document.getElementById('highScore').textContent = highScore;
        isNewRecord = true;
    }
    
    // 显示游戏结束界面
    document.getElementById('finalScore').textContent = score;
    if (isNewRecord) {
        document.getElementById('newRecord').style.display = 'block';
    } else {
        document.getElementById('newRecord').style.display = 'none';
    }
    document.getElementById('gameOver').style.display = 'block';
}

// 重新开始游戏
function restartGame() {
    // 隐藏游戏结束界面
    document.getElementById('gameOver').style.display = 'none';
    
    // 重置游戏状态
    score = 0;
    gameSpeed = INITIAL_SPEED;
    document.getElementById('score').textContent = score;
    
    // 重置蛇和食物
    resetSnake();
    generateFood();
    
    // 开始新游戏
    startGame();
}

// 页面加载完成后初始化游戏
window.addEventListener('load', initGame);