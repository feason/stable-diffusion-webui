// 游戏配置
const GAME_CONFIG = {
    CANVAS_SIZE: 400,
    GRID_SIZE: 20,
    INITIAL_SPEED: 200,
    SPEED_INCREASE: 10,
    FOOD_SCORE: 10
};

// 游戏状态
let gameState = {
    isRunning: false,
    isPaused: false,
    score: 0,
    highScore: localStorage.getItem('snakeHighScore') || 0,
    speed: GAME_CONFIG.INITIAL_SPEED,
    direction: { x: 1, y: 0 },
    nextDirection: { x: 1, y: 0 }
};

// 游戏对象
let snake = [{ x: 10, y: 10 }];
let food = { x: 15, y: 15 };

// DOM 元素
const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');
const scoreElement = document.getElementById('score');
const highScoreElement = document.getElementById('high-score');
const gameOverlay = document.getElementById('gameOverlay');
const gameMessage = document.getElementById('gameMessage');
const startBtn = document.getElementById('startBtn');

// 初始化游戏
function initGame() {
    // 设置画布
    canvas.width = GAME_CONFIG.CANVAS_SIZE;
    canvas.height = GAME_CONFIG.CANVAS_SIZE;
    
    // 显示最高分
    highScoreElement.textContent = gameState.highScore;
    
    // 绑定事件
    startBtn.addEventListener('click', startGame);
    document.addEventListener('keydown', handleKeyPress);
    
    // 初始化游戏显示
    drawGame();
}

// 开始游戏
function startGame() {
    // 重置游戏状态
    gameState.isRunning = true;
    gameState.isPaused = false;
    gameState.score = 0;
    gameState.speed = GAME_CONFIG.INITIAL_SPEED;
    gameState.direction = { x: 1, y: 0 };
    gameState.nextDirection = { x: 1, y: 0 };
    
    // 重置蛇和食物
    snake = [{ x: 10, y: 10 }];
    generateFood();
    
    // 隐藏覆盖层
    gameOverlay.classList.add('hidden');
    
    // 更新分数显示
    updateScore();
    
    // 开始游戏循环
    gameLoop();
}

// 游戏循环
function gameLoop() {
    if (!gameState.isRunning || gameState.isPaused) return;
    
    // 更新蛇的方向
    gameState.direction = { ...gameState.nextDirection };
    
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
    
    // 绘制游戏
    drawGame();
    
    // 继续游戏循环
    setTimeout(gameLoop, gameState.speed);
}

// 移动蛇
function moveSnake() {
    const head = { ...snake[0] };
    head.x += gameState.direction.x;
    head.y += gameState.direction.y;
    
    snake.unshift(head);
    
    // 如果没有吃到食物，移除尾部
    if (!checkFoodCollision()) {
        snake.pop();
    }
}

// 检查碰撞
function checkCollision() {
    const head = snake[0];
    
    // 检查墙壁碰撞
    if (head.x < 0 || head.x >= GAME_CONFIG.CANVAS_SIZE / GAME_CONFIG.GRID_SIZE ||
        head.y < 0 || head.y >= GAME_CONFIG.CANVAS_SIZE / GAME_CONFIG.GRID_SIZE) {
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
    // 增加分数
    gameState.score += GAME_CONFIG.FOOD_SCORE;
    updateScore();
    
    // 增加速度
    if (gameState.speed > 100) {
        gameState.speed -= GAME_CONFIG.SPEED_INCREASE;
    }
    
    // 生成新食物
    generateFood();
    
    // 添加视觉效果
    addEatingEffect();
}

// 生成食物
function generateFood() {
    const gridSize = GAME_CONFIG.CANVAS_SIZE / GAME_CONFIG.GRID_SIZE;
    
    do {
        food = {
            x: Math.floor(Math.random() * gridSize),
            y: Math.floor(Math.random() * gridSize)
        };
    } while (snake.some(segment => segment.x === food.x && segment.y === food.y));
}

// 更新分数
function updateScore() {
    scoreElement.textContent = gameState.score;
    
    // 更新最高分
    if (gameState.score > gameState.highScore) {
        gameState.highScore = gameState.score;
        highScoreElement.textContent = gameState.highScore;
        localStorage.setItem('snakeHighScore', gameState.highScore);
    }
}

// 结束游戏
function endGame() {
    gameState.isRunning = false;
    
    // 显示游戏结束消息
    gameMessage.innerHTML = `
        <h2 style="color: #ff6b6b;">游戏结束!</h2>
        <p>你的分数: <strong>${gameState.score}</strong></p>
        <p>最高分: <strong>${gameState.highScore}</strong></p>
        ${gameState.score === gameState.highScore && gameState.score > 0 ? 
            '<p style="color: #4ecdc4;">🎉 新纪录!</p>' : ''}
        <button id="restartBtn" class="game-btn">重新开始</button>
    `;
    
    gameOverlay.classList.remove('hidden');
    
    // 添加重新开始按钮事件
    document.getElementById('restartBtn').addEventListener('click', startGame);
    
    // 添加游戏结束动画
    canvas.style.animation = 'gameOverShake 0.5s ease-in-out';
    setTimeout(() => {
        canvas.style.animation = '';
    }, 500);
}

// 绘制游戏
function drawGame() {
    // 清空画布
    ctx.fillStyle = '#2c3e50';
    ctx.fillRect(0, 0, GAME_CONFIG.CANVAS_SIZE, GAME_CONFIG.CANVAS_SIZE);
    
    // 绘制网格
    drawGrid();
    
    // 绘制蛇
    drawSnake();
    
    // 绘制食物
    drawFood();
}

// 绘制网格
function drawGrid() {
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.1)';
    ctx.lineWidth = 1;
    
    for (let i = 0; i <= GAME_CONFIG.CANVAS_SIZE; i += GAME_CONFIG.GRID_SIZE) {
        ctx.beginPath();
        ctx.moveTo(i, 0);
        ctx.lineTo(i, GAME_CONFIG.CANVAS_SIZE);
        ctx.stroke();
        
        ctx.beginPath();
        ctx.moveTo(0, i);
        ctx.lineTo(GAME_CONFIG.CANVAS_SIZE, i);
        ctx.stroke();
    }
}

// 绘制蛇
function drawSnake() {
    snake.forEach((segment, index) => {
        const x = segment.x * GAME_CONFIG.GRID_SIZE;
        const y = segment.y * GAME_CONFIG.GRID_SIZE;
        
        if (index === 0) {
            // 绘制蛇头
            const gradient = ctx.createLinearGradient(x, y, x + GAME_CONFIG.GRID_SIZE, y + GAME_CONFIG.GRID_SIZE);
            gradient.addColorStop(0, '#4ecdc4');
            gradient.addColorStop(1, '#44a08d');
            ctx.fillStyle = gradient;
            
            ctx.fillRect(x + 1, y + 1, GAME_CONFIG.GRID_SIZE - 2, GAME_CONFIG.GRID_SIZE - 2);
            
            // 绘制眼睛
            ctx.fillStyle = 'white';
            const eyeSize = 3;
            const eyeOffset = 5;
            
            if (gameState.direction.x === 1) { // 向右
                ctx.fillRect(x + GAME_CONFIG.GRID_SIZE - eyeOffset, y + 4, eyeSize, eyeSize);
                ctx.fillRect(x + GAME_CONFIG.GRID_SIZE - eyeOffset, y + GAME_CONFIG.GRID_SIZE - 7, eyeSize, eyeSize);
            } else if (gameState.direction.x === -1) { // 向左
                ctx.fillRect(x + 2, y + 4, eyeSize, eyeSize);
                ctx.fillRect(x + 2, y + GAME_CONFIG.GRID_SIZE - 7, eyeSize, eyeSize);
            } else if (gameState.direction.y === 1) { // 向下
                ctx.fillRect(x + 4, y + GAME_CONFIG.GRID_SIZE - eyeOffset, eyeSize, eyeSize);
                ctx.fillRect(x + GAME_CONFIG.GRID_SIZE - 7, y + GAME_CONFIG.GRID_SIZE - eyeOffset, eyeSize, eyeSize);
            } else { // 向上
                ctx.fillRect(x + 4, y + 2, eyeSize, eyeSize);
                ctx.fillRect(x + GAME_CONFIG.GRID_SIZE - 7, y + 2, eyeSize, eyeSize);
            }
        } else {
            // 绘制蛇身
            const alpha = Math.max(0.3, 1 - (index - 1) * 0.1);
            ctx.fillStyle = `rgba(76, 205, 196, ${alpha})`;
            ctx.fillRect(x + 2, y + 2, GAME_CONFIG.GRID_SIZE - 4, GAME_CONFIG.GRID_SIZE - 4);
        }
    });
}

// 绘制食物
function drawFood() {
    const x = food.x * GAME_CONFIG.GRID_SIZE;
    const y = food.y * GAME_CONFIG.GRID_SIZE;
    
    // 创建渐变
    const gradient = ctx.createRadialGradient(
        x + GAME_CONFIG.GRID_SIZE / 2, y + GAME_CONFIG.GRID_SIZE / 2, 0,
        x + GAME_CONFIG.GRID_SIZE / 2, y + GAME_CONFIG.GRID_SIZE / 2, GAME_CONFIG.GRID_SIZE / 2
    );
    gradient.addColorStop(0, '#ff6b6b');
    gradient.addColorStop(1, '#ee5a52');
    
    ctx.fillStyle = gradient;
    
    // 绘制圆形食物
    ctx.beginPath();
    ctx.arc(
        x + GAME_CONFIG.GRID_SIZE / 2,
        y + GAME_CONFIG.GRID_SIZE / 2,
        GAME_CONFIG.GRID_SIZE / 2 - 2,
        0,
        2 * Math.PI
    );
    ctx.fill();
    
    // 添加高光
    ctx.fillStyle = 'rgba(255, 255, 255, 0.6)';
    ctx.beginPath();
    ctx.arc(
        x + GAME_CONFIG.GRID_SIZE / 2 - 3,
        y + GAME_CONFIG.GRID_SIZE / 2 - 3,
        3,
        0,
        2 * Math.PI
    );
    ctx.fill();
}

// 处理键盘输入
function handleKeyPress(event) {
    if (!gameState.isRunning && event.code !== 'Space') return;
    
    switch (event.code) {
        case 'ArrowUp':
            if (gameState.direction.y !== 1) {
                gameState.nextDirection = { x: 0, y: -1 };
            }
            event.preventDefault();
            break;
        case 'ArrowDown':
            if (gameState.direction.y !== -1) {
                gameState.nextDirection = { x: 0, y: 1 };
            }
            event.preventDefault();
            break;
        case 'ArrowLeft':
            if (gameState.direction.x !== 1) {
                gameState.nextDirection = { x: -1, y: 0 };
            }
            event.preventDefault();
            break;
        case 'ArrowRight':
            if (gameState.direction.x !== -1) {
                gameState.nextDirection = { x: 1, y: 0 };
            }
            event.preventDefault();
            break;
        case 'Space':
            if (gameState.isRunning) {
                togglePause();
            }
            event.preventDefault();
            break;
    }
}

// 暂停/继续游戏
function togglePause() {
    gameState.isPaused = !gameState.isPaused;
    
    if (gameState.isPaused) {
        gameMessage.innerHTML = `
            <h2>游戏暂停</h2>
            <p>按空格键继续游戏</p>
            <button id="resumeBtn" class="game-btn">继续游戏</button>
        `;
        gameOverlay.classList.remove('hidden');
        
        // 添加继续按钮事件
        document.getElementById('resumeBtn').addEventListener('click', togglePause);
    } else {
        gameOverlay.classList.add('hidden');
        gameLoop();
    }
}

// 添加吃食物效果
function addEatingEffect() {
    // 创建粒子效果
    const particles = [];
    const particleCount = 8;
    
    for (let i = 0; i < particleCount; i++) {
        particles.push({
            x: food.x * GAME_CONFIG.GRID_SIZE + GAME_CONFIG.GRID_SIZE / 2,
            y: food.y * GAME_CONFIG.GRID_SIZE + GAME_CONFIG.GRID_SIZE / 2,
            vx: (Math.random() - 0.5) * 4,
            vy: (Math.random() - 0.5) * 4,
            life: 30,
            maxLife: 30
        });
    }
    
    // 动画粒子
    function animateParticles() {
        ctx.save();
        
        particles.forEach((particle, index) => {
            if (particle.life <= 0) {
                particles.splice(index, 1);
                return;
            }
            
            particle.x += particle.vx;
            particle.y += particle.vy;
            particle.life--;
            
            const alpha = particle.life / particle.maxLife;
            ctx.fillStyle = `rgba(255, 107, 107, ${alpha})`;
            ctx.beginPath();
            ctx.arc(particle.x, particle.y, 3, 0, 2 * Math.PI);
            ctx.fill();
        });
        
        ctx.restore();
        
        if (particles.length > 0) {
            requestAnimationFrame(animateParticles);
        }
    }
    
    animateParticles();
}

// 触摸控制支持
let touchStartX = 0;
let touchStartY = 0;

canvas.addEventListener('touchstart', (e) => {
    touchStartX = e.touches[0].clientX;
    touchStartY = e.touches[0].clientY;
    e.preventDefault();
});

canvas.addEventListener('touchend', (e) => {
    if (!gameState.isRunning) return;
    
    const touchEndX = e.changedTouches[0].clientX;
    const touchEndY = e.changedTouches[0].clientY;
    
    const deltaX = touchEndX - touchStartX;
    const deltaY = touchEndY - touchStartY;
    
    const minSwipeDistance = 30;
    
    if (Math.abs(deltaX) > Math.abs(deltaY)) {
        // 水平滑动
        if (Math.abs(deltaX) > minSwipeDistance) {
            if (deltaX > 0 && gameState.direction.x !== -1) {
                gameState.nextDirection = { x: 1, y: 0 };
            } else if (deltaX < 0 && gameState.direction.x !== 1) {
                gameState.nextDirection = { x: -1, y: 0 };
            }
        }
    } else {
        // 垂直滑动
        if (Math.abs(deltaY) > minSwipeDistance) {
            if (deltaY > 0 && gameState.direction.y !== -1) {
                gameState.nextDirection = { x: 0, y: 1 };
            } else if (deltaY < 0 && gameState.direction.y !== 1) {
                gameState.nextDirection = { x: 0, y: -1 };
            }
        }
    }
    
    e.preventDefault();
});

// 初始化游戏
initGame();
