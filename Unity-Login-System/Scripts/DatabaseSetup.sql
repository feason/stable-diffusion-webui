-- Unity登录系统数据库初始化脚本
-- 创建日期: 2024年

-- 创建数据库
CREATE DATABASE IF NOT EXISTS unity_login_system 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

-- 使用数据库
USE unity_login_system;

-- 创建用户表
CREATE TABLE IF NOT EXISTS users (
    id INT PRIMARY KEY AUTO_INCREMENT COMMENT '用户ID',
    username VARCHAR(50) UNIQUE NOT NULL COMMENT '用户名',
    password_hash VARCHAR(255) NOT NULL COMMENT '密码哈希值',
    email VARCHAR(100) UNIQUE NOT NULL COMMENT '邮箱地址',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    last_login TIMESTAMP NULL COMMENT '最后登录时间',
    is_active BOOLEAN DEFAULT TRUE COMMENT '是否激活',
    INDEX idx_username (username),
    INDEX idx_email (email),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci 
COMMENT='用户表';

-- 创建用户登录日志表（可选）
CREATE TABLE IF NOT EXISTS user_login_logs (
    id INT PRIMARY KEY AUTO_INCREMENT COMMENT '日志ID',
    user_id INT NOT NULL COMMENT '用户ID',
    login_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '登录时间',
    ip_address VARCHAR(45) COMMENT 'IP地址',
    user_agent TEXT COMMENT '用户代理',
    login_status ENUM('success', 'failed') DEFAULT 'success' COMMENT '登录状态',
    INDEX idx_user_id (user_id),
    INDEX idx_login_time (login_time),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci 
COMMENT='用户登录日志表';

-- 创建用户会话表（可选）
CREATE TABLE IF NOT EXISTS user_sessions (
    id INT PRIMARY KEY AUTO_INCREMENT COMMENT '会话ID',
    user_id INT NOT NULL COMMENT '用户ID',
    session_token VARCHAR(255) UNIQUE NOT NULL COMMENT '会话令牌',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    expires_at TIMESTAMP NOT NULL COMMENT '过期时间',
    is_active BOOLEAN DEFAULT TRUE COMMENT '是否活跃',
    INDEX idx_user_id (user_id),
    INDEX idx_session_token (session_token),
    INDEX idx_expires_at (expires_at),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci 
COMMENT='用户会话表';

-- 插入测试用户（密码为 'password123' 的哈希值）
-- 注意：实际使用时应该删除这些测试数据
INSERT IGNORE INTO users (username, password_hash, email) VALUES 
('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'admin@example.com'),
('testuser', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'test@example.com'),
('demo', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'demo@example.com');

-- 创建存储过程：清理过期会话
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS CleanExpiredSessions()
BEGIN
    DELETE FROM user_sessions 
    WHERE expires_at < NOW() OR is_active = FALSE;
    
    SELECT ROW_COUNT() as cleaned_sessions;
END //
DELIMITER ;

-- 创建存储过程：获取用户统计信息
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS GetUserStats()
BEGIN
    SELECT 
        COUNT(*) as total_users,
        COUNT(CASE WHEN is_active = TRUE THEN 1 END) as active_users,
        COUNT(CASE WHEN last_login >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 END) as recent_active_users,
        COUNT(CASE WHEN created_at >= DATE_SUB(NOW(), INTERVAL 7 DAY) THEN 1 END) as new_users_this_week
    FROM users;
END //
DELIMITER ;

-- 创建触发器：记录用户登录日志
DELIMITER //
CREATE TRIGGER IF NOT EXISTS after_user_login_update
AFTER UPDATE ON users
FOR EACH ROW
BEGIN
    IF NEW.last_login != OLD.last_login AND NEW.last_login IS NOT NULL THEN
        INSERT INTO user_login_logs (user_id, login_time, login_status) 
        VALUES (NEW.id, NEW.last_login, 'success');
    END IF;
END //
DELIMITER ;

-- 创建视图：活跃用户信息
CREATE VIEW IF NOT EXISTS active_users AS
SELECT 
    id,
    username,
    email,
    created_at,
    last_login,
    DATEDIFF(NOW(), last_login) as days_since_last_login
FROM users 
WHERE is_active = TRUE
ORDER BY last_login DESC;

-- 授权（根据实际需求调整）
-- CREATE USER IF NOT EXISTS 'unity_user'@'localhost' IDENTIFIED BY 'unity_password';
-- GRANT SELECT, INSERT, UPDATE, DELETE ON unity_login_system.* TO 'unity_user'@'localhost';
-- FLUSH PRIVILEGES;

-- 显示创建的表
SHOW TABLES;

-- 显示用户表结构
DESCRIBE users;