const canvas = document.getElementById("game");
const ctx = canvas.getContext("2d");

const scoreEl = document.getElementById("score");
const highScoreEl = document.getElementById("highScore");
const speedSelect = document.getElementById("speedSelect");
const overlay = document.getElementById("overlay");
const overlayTitle = document.getElementById("overlayTitle");
const overlayDesc = document.getElementById("overlayDesc");
const startBtn = document.getElementById("startBtn");
const restartBtn = document.getElementById("restartBtn");
const pauseBtn = document.getElementById("pauseBtn");

const dpad = document.querySelector(".dpad");

const STORAGE_KEY = "snake_high_score_v1";

const GRID_CELL_SIZE = 20;
const GRID_COLS = 24;
const GRID_ROWS = 24;

const DEVICE_PIXEL_RATIO = Math.max(1, Math.floor(window.devicePixelRatio || 1));
canvas.width = GRID_COLS * GRID_CELL_SIZE * DEVICE_PIXEL_RATIO;
canvas.height = GRID_ROWS * GRID_CELL_SIZE * DEVICE_PIXEL_RATIO;
ctx.scale(DEVICE_PIXEL_RATIO, DEVICE_PIXEL_RATIO);

const Direction = Object.freeze({ Up: "Up", Down: "Down", Left: "Left", Right: "Right" });

function getInitialState() {
  return {
    snakeCells: [ { x: 8, y: 12 }, { x: 7, y: 12 }, { x: 6, y: 12 } ],
    currentDirection: Direction.Right,
    pendingDirection: Direction.Right,
    foodCell: generateFoodCell(new Set(["8,12","7,12","6,12"])),
    score: 0,
    isRunning: false,
    isGameOver: false,
    msPerStep: Number(speedSelect.value),
    timeAccumulatorMs: 0,
    inputQueue: [],
  };
}

/**
 * Returns a random food cell not occupied by the snake
 */
function generateFoodCell(occupiedSet) {
  while (true) {
    const x = Math.floor(Math.random() * GRID_COLS);
    const y = Math.floor(Math.random() * GRID_ROWS);
    const key = `${x},${y}`;
    if (!occupiedSet.has(key)) return { x, y };
  }
}

function readHighScore() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    return saved ? Number(saved) : 0;
  } catch {
    return 0;
  }
}

function writeHighScore(value) {
  try {
    localStorage.setItem(STORAGE_KEY, String(value));
  } catch {
    // ignore storage errors
  }
}

let state = getInitialState();
let highScore = readHighScore();

function setScore(value) {
  scoreEl.textContent = String(value);
}

function setHighScore(value) {
  highScoreEl.textContent = String(value);
}

function showOverlay(title, desc) {
  overlayTitle.textContent = title;
  overlayDesc.textContent = desc;
  overlay.classList.remove("hidden");
}

function hideOverlay() {
  overlay.classList.add("hidden");
}

function startGame() {
  if (state.isRunning && !state.isGameOver) return;
  state = getInitialState();
  state.isRunning = true;
  setScore(0);
  setHighScore(highScore);
  hideOverlay();
}

function pauseGame() {
  if (!state.isRunning || state.isGameOver) return;
  state.isRunning = false;
  showOverlay("暂停中", "按空格继续，或点击开始");
}

function resumeGame() {
  if (state.isRunning || state.isGameOver) return;
  state.isRunning = true;
  hideOverlay();
}

function endGame() {
  state.isRunning = false;
  state.isGameOver = true;
  const newHigh = Math.max(highScore, state.score);
  if (newHigh !== highScore) {
    highScore = newHigh;
    writeHighScore(highScore);
  }
  setHighScore(highScore);
  showOverlay("游戏结束", `得分 ${state.score} · R 重开`);
}

function handleDirectionInput(next) {
  const current = state.pendingDirection;
  if (next === current) return;

  const invalid =
    (current === Direction.Up && next === Direction.Down) ||
    (current === Direction.Down && next === Direction.Up) ||
    (current === Direction.Left && next === Direction.Right) ||
    (current === Direction.Right && next === Direction.Left);
  if (invalid) return;

  state.inputQueue.push(next);
}

function applyNextDirection() {
  if (state.inputQueue.length === 0) return;
  state.pendingDirection = state.inputQueue.shift();
}

function stepGame() {
  applyNextDirection();
  state.currentDirection = state.pendingDirection;

  const head = state.snakeCells[0];
  const delta =
    state.currentDirection === Direction.Up ? { x: 0, y: -1 } :
    state.currentDirection === Direction.Down ? { x: 0, y: 1 } :
    state.currentDirection === Direction.Left ? { x: -1, y: 0 } : { x: 1, y: 0 };

  const newHead = { x: head.x + delta.x, y: head.y + delta.y };

  if (newHead.x < 0 || newHead.x >= GRID_COLS || newHead.y < 0 || newHead.y >= GRID_ROWS) {
    endGame();
    return;
  }

  for (let i = 0; i < state.snakeCells.length; i += 1) {
    const c = state.snakeCells[i];
    if (c.x === newHead.x && c.y === newHead.y) {
      endGame();
      return;
    }
  }

  const nextSnake = [newHead, ...state.snakeCells];

  const ateFood = newHead.x === state.foodCell.x && newHead.y === state.foodCell.y;
  if (ateFood) {
    state.score += 1;
    setScore(state.score);

    const occupied = new Set(nextSnake.map((c) => `${c.x},${c.y}`));
    state.foodCell = generateFoodCell(occupied);

    if (state.msPerStep > 60 && state.score % 5 === 0) {
      state.msPerStep = Math.max(60, state.msPerStep - 5);
    }
  } else {
    nextSnake.pop();
  }

  state.snakeCells = nextSnake;
}

function drawCell(cell, color) {
  ctx.fillStyle = color;
  ctx.fillRect(cell.x * GRID_CELL_SIZE, cell.y * GRID_CELL_SIZE, GRID_CELL_SIZE, GRID_CELL_SIZE);
}

function draw() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  ctx.fillStyle = "#0d1020";
  ctx.fillRect(0, 0, GRID_COLS * GRID_CELL_SIZE, GRID_ROWS * GRID_CELL_SIZE);

  const gridColor = "#141830";
  ctx.strokeStyle = gridColor;
  ctx.lineWidth = 1;
  for (let x = 0; x <= GRID_COLS; x += 1) {
    ctx.beginPath();
    ctx.moveTo(x * GRID_CELL_SIZE + 0.5, 0);
    ctx.lineTo(x * GRID_CELL_SIZE + 0.5, GRID_ROWS * GRID_CELL_SIZE);
    ctx.stroke();
  }
  for (let y = 0; y <= GRID_ROWS; y += 1) {
    ctx.beginPath();
    ctx.moveTo(0, y * GRID_CELL_SIZE + 0.5);
    ctx.lineTo(GRID_COLS * GRID_CELL_SIZE, y * GRID_CELL_SIZE + 0.5);
    ctx.stroke();
  }

  for (let i = state.snakeCells.length - 1; i >= 0; i -= 1) {
    const color = i === 0 ? "#6ee7b7" : "#34d399";
    drawCell(state.snakeCells[i], color);
  }

  drawCell(state.foodCell, "#60a5fa");
}

let lastTimestamp = performance.now();
function gameLoop(nowMs) {
  const dt = nowMs - lastTimestamp;
  lastTimestamp = nowMs;

  if (state.isRunning && !state.isGameOver) {
    state.timeAccumulatorMs += dt;
    while (state.timeAccumulatorMs >= state.msPerStep) {
      state.timeAccumulatorMs -= state.msPerStep;
      stepGame();
    }
  }

  draw();
  requestAnimationFrame(gameLoop);
}

requestAnimationFrame(gameLoop);

window.addEventListener("keydown", (e) => {
  const key = e.key;
  if (key === " " || key === "Spacebar") {
    e.preventDefault();
    if (state.isGameOver) return; 
    if (state.isRunning) pauseGame(); else resumeGame();
    return;
  }
  if (key === "r" || key === "R") {
    startGame();
    return;
  }

  switch (key) {
    case "ArrowUp": case "w": case "W": handleDirectionInput(Direction.Up); break;
    case "ArrowDown": case "s": case "S": handleDirectionInput(Direction.Down); break;
    case "ArrowLeft": case "a": case "A": handleDirectionInput(Direction.Left); break;
    case "ArrowRight": case "d": case "D": handleDirectionInput(Direction.Right); break;
  }
});

speedSelect.addEventListener("change", () => {
  const base = Number(speedSelect.value);
  state.msPerStep = base;
});

startBtn.addEventListener("click", () => {
  startGame();
});

restartBtn.addEventListener("click", () => {
  startGame();
});

pauseBtn.addEventListener("click", () => {
  if (state.isRunning) pauseGame(); else resumeGame();
});

function handlePad(dir) {
  switch (dir) {
    case "up": handleDirectionInput(Direction.Up); break;
    case "down": handleDirectionInput(Direction.Down); break;
    case "left": handleDirectionInput(Direction.Left); break;
    case "right": handleDirectionInput(Direction.Right); break;
  }
}

dpad.addEventListener("click", (e) => {
  const target = e.target;
  if (!(target instanceof HTMLElement)) return;
  const dir = target.getAttribute("data-dir");
  if (!dir) return;
  handlePad(dir);
});

// Initial UI
setHighScore(highScore);
showOverlay("开始游戏", "按空格或点击开始。方向键/WASD 控制，R 重开。");