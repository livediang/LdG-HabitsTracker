-- ==========================================
-- 1. Tabla principal de usuarios
-- ==========================================
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    SecurityStamp NVARCHAR(100) NOT NULL,
    TwoFactorEnabled BIT DEFAULT 0,
    LockoutEnd DATETIME2 NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSDATETIME()
);

-- ==========================================
-- 2. Perfil de usuario (1:1 con Users)
-- ==========================================
CREATE TABLE UserProfiles (
    ProfileId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    FullName NVARCHAR(200) NULL,
    PhotoUrl NVARCHAR(500) NULL,
    TimeZone NVARCHAR(100) NULL,
    Language NVARCHAR(10) NULL,
    PreferencesJson NVARCHAR(MAX) NULL,
    CONSTRAINT FK_UserProfiles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserProfiles_User UNIQUE (UserId)
);

-- ==========================================
-- 3. Roles
-- ==========================================
CREATE TABLE Roles (
    RoleId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL
);

-- ==========================================
-- 4. Relación N:M entre Usuarios y Roles
-- ==========================================
CREATE TABLE UserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    AssignedAt DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE
);

-- ==========================================
-- 5. Tokens de usuario (reset password, confirmación, refresh)
-- ==========================================
CREATE TABLE UserTokens (
    TokenId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenValue NVARCHAR(500) NOT NULL,
    TokenType NVARCHAR(50) NOT NULL, -- ResetPassword, ConfirmEmail, RefreshToken
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_UserTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- ==========================================
-- 6. Historial de accesos
-- ==========================================
CREATE TABLE LoginHistory (
    LoginId BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    LoginDate DATETIME2 DEFAULT SYSDATETIME(),
    IpAddress NVARCHAR(50) NULL,
    DeviceInfo NVARCHAR(255) NULL,
    IsSuccessful BIT NOT NULL,
    CONSTRAINT FK_LoginHistory_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- ==========================================
-- 7. Auditoría de acciones de usuario
-- ==========================================
CREATE TABLE AuditLogs (
    LogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NULL,
    Action NVARCHAR(100) NOT NULL, -- Ej: PasswordChanged, ProfileUpdated
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    MetadataJson NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE SET NULL
);