-- ==========================================
-- Insertar Roles iniciales
-- ==========================================
INSERT INTO Roles (RoleId, Name, Description)
VALUES 
    (NEWID(), 'Administrator', 'Rol with administrative privileges'),
    (NEWID(), 'User', 'Standart role');

DECLARE @AdminId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Users (UserId, Email, PasswordHash, SecurityStamp, TwoFactorEnabled, IsActive)
VALUES (
    @AdminId,
    'admin@habitstracker.com',
    -- IMPORTANTE: reemplazar este valor por un hash real de contraseña usando Identity PasswordHasher
    'HASH_DE_CONTRASEÑA_AQUI',
    NEWID(), -- SecurityStamp
    0,
    1
);

INSERT INTO UserProfiles (ProfileId, UserId, FullName, PhotoUrl, TimeZone, Language)
VALUES (
    NEWID(),
    @AdminId,
    'Administrador del Sistema',
    NULL,
    'UTC',
    'es'
);

DECLARE @AdminRoleId UNIQUEIDENTIFIER;
SELECT TOP 1 @AdminRoleId = RoleId FROM Roles WHERE Name = 'Administrator';

INSERT INTO UserRoles (UserId, RoleId)
VALUES (@AdminId, @AdminRoleId);
