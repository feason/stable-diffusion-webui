'use strict';

(function(){
  const canvas = document.getElementById('game-canvas');
  const ctx = canvas.getContext('2d');

  const startButton = document.getElementById('start-btn');
  const restartButton = document.getElementById('restart-btn');
  const pauseButton = document.getElementById('pause-btn');
  const overlay = document.getElementById('overlay');
  const overlayTitle = document.getElementById('overlay-title');
  const overlaySub = document.getElementById('overlay-sub');
  const scoreEl = document.getElementById('score');
  const bestScoreEl = document.getElementById('best-score');

  const BEST_SCORE_KEY = 'snake_best_score_v1';

  function loadBestScore(){
    try{ return Number(localStorage.getItem(BEST_SCORE_KEY) || '0'); }
    catch{ return 0; }
  }
  function saveBestScore(value){
    try{ localStorage.setItem(BEST_SCORE_KEY, String(value)); }
    catch{}
  }

  let bestScore = loadBestScore();
  bestScoreEl.textContent = String(bestScore);

  // Grid configuration
  const GRID_SIZE = 24; // number of cells per side
  let cellSize = Math.floor(canvas.width / GRID_SIZE);

  function resizeCanvasToContainer(){
    const parent = canvas.parentElement;
    const rect = parent.getBoundingClientRect();
    const size = Math.min(rect.width, 560); // clamp for readability
    const devicePixelRatio = window.devicePixelRatio || 1;
    const targetCssSize = Math.max(320, Math.floor(size));
    canvas.style.width = targetCssSize + 'px';
    canvas.style.height = targetCssSize + 'px';
    canvas.width = Math.floor(targetCssSize * devicePixelRatio);
    canvas.height = Math.floor(targetCssSize * devicePixelRatio);
    cellSize = Math.floor(canvas.width / GRID_SIZE);
    ctx.setTransform(devicePixelRatio, 0, 0, devicePixelRatio, 0, 0);
  }

  window.addEventListener('resize', resizeCanvasToContainer);
  resizeCanvasToContainer();

  const Direction = Object.freeze({
    Up: {x:0, y:-1, name:'Up'},
    Down: {x:0, y:1, name:'Down'},
    Left: {x:-1, y:0, name:'Left'},
    Right: {x:1, y:0, name:'Right'},
  });

  function positionsEqual(a,b){ return a.x===b.x && a.y===b.y; }

  function randomInt(min, max){ // inclusive
    return Math.floor(Math.random() * (max - min + 1)) + min;
  }

  function generateFoodPosition(snake){
    const occupied = new Set(snake.map(p=> p.x + ':' + p.y));
    while(true){
      const pos = { x: randomInt(0, GRID_SIZE-1), y: randomInt(0, GRID_SIZE-1) };
      if(!occupied.has(pos.x + ':' + pos.y)) return pos;
    }
  }

  function willReverse(currentDir, nextDir){
    return (currentDir.x + nextDir.x === 0 && currentDir.y + nextDir.y === 0);
  }

  const initialState = () => ({
    snake: [
      {x: 8, y: 12},
      {x: 7, y: 12},
      {x: 6, y: 12},
    ],
    direction: Direction.Right,
    pendingDirections: [],
    food: {x: 14, y:12},
    score: 0,
    isRunning: false,
    isPaused: false,
    isGameOver: false,
    msPerStep: 120,
    lastStepAt: 0,
    accumulator: 0,
    lastFrameTs: 0,
  });

  let state = initialState();

  function setScore(newScore){
    state.score = newScore;
    scoreEl.textContent = String(newScore);
    if(newScore > bestScore){
      bestScore = newScore;
      bestScoreEl.textContent = String(bestScore);
      saveBestScore(bestScore);
    }
  }

  function startGame(){
    state = initialState();
    state.isRunning = true;
    overlay.hidden = true;
    scoreEl.textContent = '0';
  }

  function restartGame(){
    startGame();
  }

  function pauseOrResume(){
    if(!state.isRunning || state.isGameOver){ return; }
    state.isPaused = !state.isPaused;
    overlay.hidden = state.isPaused ? false : true;
    overlayTitle.textContent = state.isPaused ? '已暂停' : '';
    overlaySub.textContent = state.isPaused ? '空格继续' : '';
  }

  function gameOver(){
    state.isRunning = false;
    state.isGameOver = true;
    overlay.hidden = false;
    overlayTitle.textContent = '游戏结束';
    overlaySub.textContent = '点“重新开始”或按空格重来';
  }

  function step(deltaMs){
    state.accumulator += deltaMs;
    if(state.accumulator < state.msPerStep){ return; }
    const steps = Math.floor(state.accumulator / state.msPerStep);
    state.accumulator -= steps * state.msPerStep;

    for(let i=0;i<steps;i++){
      if(state.pendingDirections.length){
        const nextDir = state.pendingDirections.shift();
        if(!willReverse(state.direction, nextDir)){
          state.direction = nextDir;
        }
      }

      const head = state.snake[0];
      const nextHead = { x: head.x + state.direction.x, y: head.y + state.direction.y };

      // wall collision
      if(nextHead.x < 0 || nextHead.y < 0 || nextHead.x >= GRID_SIZE || nextHead.y >= GRID_SIZE){
        gameOver();
        return;
      }

      // self collision
      for(let k=0;k<state.snake.length;k++){
        if(positionsEqual(state.snake[k], nextHead)){
          gameOver();
          return;
        }
      }

      state.snake.unshift(nextHead);

      if(positionsEqual(nextHead, state.food)){
        setScore(state.score + 1);
        // speed up slightly every few points
        if(state.msPerStep > 70 && state.score % 5 === 0){
          state.msPerStep -= 5;
        }
        state.food = generateFoodPosition(state.snake);
      }else{
        state.snake.pop();
      }
    }
  }

  function drawGrid(){
    const size = canvas.width; // already scaled via DPR
    ctx.fillStyle = '#0f1337';
    ctx.fillRect(0, 0, size, size);

    ctx.save();
    ctx.globalAlpha = 0.12;
    ctx.strokeStyle = '#7ba8ff';
    ctx.lineWidth = 1;
    for(let i=0;i<=GRID_SIZE;i++){
      const p = i * cellSize;
      ctx.beginPath(); ctx.moveTo(p, 0); ctx.lineTo(p, size); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(0, p); ctx.lineTo(size, p); ctx.stroke();
    }
    ctx.restore();
  }

  function drawSnake(){
    for(let i=0;i<state.snake.length;i++){
      const seg = state.snake[i];
      const x = seg.x * cellSize;
      const y = seg.y * cellSize;
      const isHead = i === 0;

      const colorBody = '#7bffb5';
      const colorHead = '#6dd6ff';

      ctx.fillStyle = isHead ? colorHead : colorBody;
      const inset = isHead ? 2 : 3;
      roundRect(ctx, x+inset, y+inset, cellSize-2*inset, cellSize-2*inset, 6, true, false);
    }
  }

  function drawFood(){
    const x = state.food.x * cellSize;
    const y = state.food.y * cellSize;
    ctx.save();
    ctx.fillStyle = '#ff9f6b';
    const r = Math.floor(cellSize/2) - 2;
    circle(ctx, x + cellSize/2, y + cellSize/2, r);
    ctx.fill();
    ctx.restore();
  }

  function drawOverlayText(){
    if(!state.isRunning || state.isPaused || state.isGameOver){
      return; // overlay DOM handles it
    }
  }

  function render(){
    drawGrid();
    drawFood();
    drawSnake();
    drawOverlayText();
  }

  function loop(ts){
    if(!state.lastFrameTs) state.lastFrameTs = ts;
    const delta = ts - state.lastFrameTs;
    state.lastFrameTs = ts;

    if(state.isRunning && !state.isPaused && !state.isGameOver){
      step(delta);
    }

    render();
    requestAnimationFrame(loop);
  }

  requestAnimationFrame(loop);

  // Helpers
  function roundRect(ctx, x, y, width, height, radius, fill, stroke){
    if(typeof radius === 'number'){
      radius = {tl: radius, tr: radius, br: radius, bl: radius};
    } else {
      radius = Object.assign({tl: 0, tr: 0, br: 0, bl: 0}, radius);
    }
    ctx.beginPath();
    ctx.moveTo(x + radius.tl, y);
    ctx.lineTo(x + width - radius.tr, y);
    ctx.quadraticCurveTo(x + width, y, x + width, y + radius.tr);
    ctx.lineTo(x + width, y + height - radius.br);
    ctx.quadraticCurveTo(x + width, y + height, x + width - radius.br, y + height);
    ctx.lineTo(x + radius.bl, y + height);
    ctx.quadraticCurveTo(x, y + height, x, y + height - radius.bl);
    ctx.lineTo(x, y + radius.tl);
    ctx.quadraticCurveTo(x, y, x + radius.tl, y);
    if(fill) ctx.fill();
    if(stroke) ctx.stroke();
    ctx.closePath();
  }

  function circle(ctx, x, y, r){
    ctx.beginPath();
    ctx.arc(x, y, r, 0, Math.PI * 2);
    ctx.closePath();
  }

  // Input management
  function enqueueDirection(dir){
    const last = state.pendingDirections.length ? state.pendingDirections[state.pendingDirections.length - 1] : state.direction;
    if(willReverse(last, dir)) return;
    state.pendingDirections.push(dir);
  }

  const keyToDir = new Map([
    ['ArrowUp', Direction.Up],
    ['KeyW', Direction.Up],
    ['ArrowDown', Direction.Down],
    ['KeyS', Direction.Down],
    ['ArrowLeft', Direction.Left],
    ['KeyA', Direction.Left],
    ['ArrowRight', Direction.Right],
    ['KeyD', Direction.Right],
  ]);

  window.addEventListener('keydown', (e)=>{
    if(keyToDir.has(e.code)){
      enqueueDirection(keyToDir.get(e.code));
      e.preventDefault();
    } else if(e.code === 'Space'){
      if(!state.isRunning || state.isGameOver){
        restartGame();
      } else {
        pauseOrResume();
      }
      e.preventDefault();
    }
  }, { passive: false });

  // Touch controls (swipe)
  let touchStart = null;
  const SWIPE_THRESHOLD = 18; // px

  canvas.addEventListener('touchstart', (e)=>{
    if(e.touches.length === 1){
      touchStart = { x: e.touches[0].clientX, y: e.touches[0].clientY };
    }
  }, { passive: true });
  canvas.addEventListener('touchmove', (e)=>{
    if(!touchStart || e.touches.length !== 1) return;
    const dx = e.touches[0].clientX - touchStart.x;
    const dy = e.touches[0].clientY - touchStart.y;
    if(Math.abs(dx) < SWIPE_THRESHOLD && Math.abs(dy) < SWIPE_THRESHOLD){
      return;
    }
    if(Math.abs(dx) > Math.abs(dy)){
      enqueueDirection(dx > 0 ? Direction.Right : Direction.Left);
    } else {
      enqueueDirection(dy > 0 ? Direction.Down : Direction.Up);
    }
    touchStart = null;
  }, { passive: true });

  // Buttons
  startButton.addEventListener('click', ()=>{
    if(!state.isRunning){ startGame(); }
  });
  restartButton.addEventListener('click', ()=>{
    restartGame();
  });
  pauseButton.addEventListener('click', ()=>{
    pauseOrResume();
  });

  // Show initial overlay
  overlay.hidden = false;
  overlayTitle.textContent = '按开始';
  overlaySub.textContent = '方向键 / WASD 控制，手机支持滑动。';
})();