ALTER TABLE usuarios ADD COLUMN username VARCHAR(100) NULL AFTER email;
CREATE UNIQUE INDEX idx_usuarios_username_unique ON usuarios (username);
DESCRIBE usuarios;