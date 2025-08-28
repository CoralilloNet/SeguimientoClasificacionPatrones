-- SeguimientoTareas Database Creation and Setup Script
-- This script creates the complete database schema and populates it with sample data

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SeguimientoTareas')
BEGIN
    CREATE DATABASE SeguimientoTareas;
END
GO

USE SeguimientoTareas;
GO

-- Drop tables if they exist (for clean recreation)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'StageEvidences')
    DROP TABLE StageEvidences;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AssignmentStages')
    DROP TABLE AssignmentStages;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Assignments')
    DROP TABLE Assignments;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskStageTemplates')
    DROP TABLE TaskStageTemplates;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskTemplates')
    DROP TABLE TaskTemplates;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Specialists')
    DROP TABLE Specialists;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    DROP TABLE Users;
GO

-- Create Users table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(256) UNIQUE NOT NULL,
    FullName NVARCHAR(256) NOT NULL,
    Password NVARCHAR(256) NOT NULL,
    IsAdmin BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Create Specialists table
CREATE TABLE Specialists (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NULL,
    Active BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Create TaskTemplates table
CREATE TABLE TaskTemplates (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Active BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Create TaskStageTemplates table
CREATE TABLE TaskStageTemplates (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaskTemplateId INT NOT NULL REFERENCES TaskTemplates(Id) ON DELETE CASCADE,
    Ordinal INT NOT NULL,
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    DurationDays INT NOT NULL
);
GO

-- Create Assignments table
CREATE TABLE Assignments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TaskTemplateId INT NOT NULL REFERENCES TaskTemplates(Id),
    Title NVARCHAR(256) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    AssignedToUserId INT NOT NULL REFERENCES Users(Id),
    AssignedByUserId INT NOT NULL REFERENCES Users(Id),
    SpecialistId INT NULL REFERENCES Specialists(Id),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    StartDate DATE NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    DueDate DATE NULL
);
GO

-- Create AssignmentStages table
CREATE TABLE AssignmentStages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AssignmentId INT NOT NULL REFERENCES Assignments(Id) ON DELETE CASCADE,
    Ordinal INT NOT NULL,
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    DurationDays INT NOT NULL,
    StartDate DATE NULL,
    TargetDate DATE NULL,
    ProgressPercent INT NOT NULL DEFAULT 0,
    IsComplete BIT NOT NULL DEFAULT 0,
    CompletedAt DATETIME2 NULL
);
GO

-- Create StageEvidences table
CREATE TABLE StageEvidences (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AssignmentStageId INT NOT NULL REFERENCES AssignmentStages(Id) ON DELETE CASCADE,
    FileName NVARCHAR(512) NOT NULL,
    FilePath NVARCHAR(1024) NOT NULL,
    Notes NVARCHAR(1024) NULL,
    UploadedByUserId INT NOT NULL REFERENCES Users(Id),
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Create indices for better performance
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Assignments_AssignedToUserId ON Assignments(AssignedToUserId);
CREATE INDEX IX_Assignments_AssignedByUserId ON Assignments(AssignedByUserId);
CREATE INDEX IX_AssignmentStages_AssignmentId ON AssignmentStages(AssignmentId);
CREATE INDEX IX_StageEvidences_AssignmentStageId ON StageEvidences(AssignmentStageId);
GO

-- Sample data insertion

-- Insert sample users with plain text passwords
-- Password for all users is "admin" stored as plain text

-- Admin user
INSERT INTO Users (Email, FullName, Password, IsAdmin, IsActive) 
VALUES ('admin@example.com', 'Administrador', 'admin', 1, 1);

-- Regular users
INSERT INTO Users (Email, FullName, Password, IsAdmin, IsActive) 
VALUES 
    ('juan.perez@example.com', 'Juan Pérez', 'admin', 0, 1),
    ('maria.gonzalez@example.com', 'María González', 'admin', 0, 1),
    ('carlos.rodriguez@example.com', 'Carlos Rodríguez', 'admin', 0, 1);
GO

-- Insert sample specialists
INSERT INTO Specialists (Name, Email, Active) 
VALUES 
    ('Dr. Ana Martínez', 'ana.martinez@specialists.com', 1),
    ('Ing. Roberto Silva', 'roberto.silva@specialists.com', 1),
    ('Lic. Laura Fernández', 'laura.fernandez@specialists.com', 1);
GO

-- Insert sample task templates
INSERT INTO TaskTemplates (Name, Description, Active) 
VALUES 
    ('Análisis de Patrones', 'Proceso completo de análisis y clasificación de patrones de datos', 1),
    ('Revisión de Calidad', 'Proceso de revisión y control de calidad de resultados', 1);
GO

-- Insert sample task stage templates for "Análisis de Patrones"
INSERT INTO TaskStageTemplates (TaskTemplateId, Ordinal, Name, Description, DurationDays) 
VALUES 
    (1, 1, 'Recolección de Datos', 'Recolectar y preparar los datos necesarios para el análisis', 3),
    (1, 2, 'Análisis Preliminar', 'Realizar análisis inicial y identificar patrones básicos', 5),
    (1, 3, 'Clasificación Avanzada', 'Aplicar algoritmos de clasificación avanzados', 7),
    (1, 4, 'Validación de Resultados', 'Validar y verificar la precisión de los resultados', 2),
    (1, 5, 'Documentación Final', 'Crear documentación final y reporte de resultados', 3);

-- Insert sample task stage templates for "Revisión de Calidad"
INSERT INTO TaskStageTemplates (TaskTemplateId, Ordinal, Name, Description, DurationDays) 
VALUES 
    (2, 1, 'Revisión Inicial', 'Revisión inicial de documentos y datos', 2),
    (2, 2, 'Análisis de Calidad', 'Análisis detallado de calidad y conformidad', 4),
    (2, 3, 'Correcciones', 'Implementar correcciones identificadas', 3),
    (2, 4, 'Revisión Final', 'Revisión final y aprobación', 1);
GO

-- Insert sample assignments
DECLARE @AdminUserId INT = (SELECT Id FROM Users WHERE Email = 'admin@example.com');
DECLARE @JuanUserId INT = (SELECT Id FROM Users WHERE Email = 'juan.perez@example.com');
DECLARE @MariaUserId INT = (SELECT Id FROM Users WHERE Email = 'maria.gonzalez@example.com');
DECLARE @Specialist1Id INT = (SELECT Id FROM Specialists WHERE Name = 'Dr. Ana Martínez');
DECLARE @Specialist2Id INT = (SELECT Id FROM Specialists WHERE Name = 'Ing. Roberto Silva');

INSERT INTO Assignments (TaskTemplateId, Title, Description, AssignedToUserId, AssignedByUserId, SpecialistId, StartDate, DueDate) 
VALUES 
    (1, 'Análisis de Patrones de Ventas Q1', 'Análisis completo de patrones de ventas del primer trimestre', @JuanUserId, @AdminUserId, @Specialist1Id, '2024-01-15', '2024-02-15'),
    (2, 'Revisión de Calidad - Proyecto Alpha', 'Revisión de calidad del proyecto Alpha antes del lanzamiento', @MariaUserId, @AdminUserId, @Specialist2Id, '2024-01-20', '2024-02-05');
GO

-- Create assignment stages for the first assignment
DECLARE @Assignment1Id INT = (SELECT Id FROM Assignments WHERE Title = 'Análisis de Patrones de Ventas Q1');
DECLARE @Assignment2Id INT = (SELECT Id FROM Assignments WHERE Title = 'Revisión de Calidad - Proyecto Alpha');

-- Copy stages from template for Assignment 1
INSERT INTO AssignmentStages (AssignmentId, Ordinal, Name, Description, DurationDays, StartDate, TargetDate, ProgressPercent, IsComplete)
SELECT 
    @Assignment1Id,
    Ordinal,
    Name,
    Description,
    DurationDays,
    CASE 
        WHEN Ordinal = 1 THEN '2024-01-15'
        ELSE DATEADD(DAY, (SELECT SUM(DurationDays) FROM TaskStageTemplates t2 WHERE t2.TaskTemplateId = t1.TaskTemplateId AND t2.Ordinal < t1.Ordinal), '2024-01-15')
    END,
    DATEADD(DAY, (SELECT SUM(DurationDays) FROM TaskStageTemplates t2 WHERE t2.TaskTemplateId = t1.TaskTemplateId AND t2.Ordinal <= t1.Ordinal), '2024-01-15'),
    CASE 
        WHEN Ordinal = 1 THEN 100
        WHEN Ordinal = 2 THEN 75
        WHEN Ordinal = 3 THEN 40
        ELSE 0
    END,
    CASE 
        WHEN Ordinal = 1 THEN 1
        ELSE 0
    END
FROM TaskStageTemplates t1
WHERE TaskTemplateId = 1;

-- Copy stages from template for Assignment 2
INSERT INTO AssignmentStages (AssignmentId, Ordinal, Name, Description, DurationDays, StartDate, TargetDate, ProgressPercent, IsComplete)
SELECT 
    @Assignment2Id,
    Ordinal,
    Name,
    Description,
    DurationDays,
    CASE 
        WHEN Ordinal = 1 THEN '2024-01-20'
        ELSE DATEADD(DAY, (SELECT SUM(DurationDays) FROM TaskStageTemplates t2 WHERE t2.TaskTemplateId = t1.TaskTemplateId AND t2.Ordinal < t1.Ordinal), '2024-01-20')
    END,
    DATEADD(DAY, (SELECT SUM(DurationDays) FROM TaskStageTemplates t2 WHERE t2.TaskTemplateId = t1.TaskTemplateId AND t2.Ordinal <= t1.Ordinal), '2024-01-20'),
    CASE 
        WHEN Ordinal = 1 THEN 100
        WHEN Ordinal = 2 THEN 30
        ELSE 0
    END,
    CASE 
        WHEN Ordinal = 1 THEN 1
        ELSE 0
    END
FROM TaskStageTemplates t1
WHERE TaskTemplateId = 2;

-- Mark first stage as completed for Assignment 1
UPDATE AssignmentStages 
SET CompletedAt = '2024-01-18 10:30:00'
WHERE AssignmentId = @Assignment1Id AND Ordinal = 1;

-- Mark first stage as completed for Assignment 2
UPDATE AssignmentStages 
SET CompletedAt = '2024-01-22 14:15:00'
WHERE AssignmentId = @Assignment2Id AND Ordinal = 1;
GO

-- Create uploads directory structure (this will be done by the application)
-- wwwroot/uploads/{assignmentId}/{stageId}/

-- Insert sample evidence files
DECLARE @Stage1Id INT = (SELECT Id FROM AssignmentStages WHERE AssignmentId = (SELECT Id FROM Assignments WHERE Title = 'Análisis de Patrones de Ventas Q1') AND Ordinal = 1);
DECLARE @Stage2Id INT = (SELECT Id FROM AssignmentStages WHERE AssignmentId = (SELECT Id FROM Assignments WHERE Title = 'Análisis de Patrones de Ventas Q1') AND Ordinal = 2);

INSERT INTO StageEvidences (AssignmentStageId, FileName, FilePath, Notes, UploadedByUserId)
VALUES 
    (@Stage1Id, 'datos_recolectados.xlsx', '/uploads/' + CAST((SELECT AssignmentId FROM AssignmentStages WHERE Id = @Stage1Id) AS VARCHAR) + '/' + CAST(@Stage1Id AS VARCHAR) + '/datos_recolectados.xlsx', 'Archivo con datos iniciales recolectados', @JuanUserId),
    (@Stage2Id, 'analisis_preliminar.pdf', '/uploads/' + CAST((SELECT AssignmentId FROM AssignmentStages WHERE Id = @Stage2Id) AS VARCHAR) + '/' + CAST(@Stage2Id AS VARCHAR) + '/analisis_preliminar.pdf', 'Reporte del análisis preliminar realizado', @JuanUserId);
GO

PRINT 'Database SeguimientoTareas created successfully with sample data!';
PRINT 'Admin credentials: admin@example.com / admin';
PRINT 'Test users: juan.perez@example.com, maria.gonzalez@example.com, carlos.rodriguez@example.com (all with password: admin)';
GO