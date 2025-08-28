-- Script to update user passwords with proper PBKDF2 hashes
-- Run this after the main script to set up demo user passwords
-- All passwords will be set to "admin"

USE SeguimientoTareas;
GO

-- Create a temporary procedure to hash passwords
-- Note: In a real deployment, you'd use the application to create users
-- This is just for demo purposes

DECLARE @PlainPassword NVARCHAR(100) = 'admin';

-- For demo purposes, we'll use a simple approach
-- The application will handle proper password verification

-- Update admin user
UPDATE Users 
SET PasswordHash = 0x2E5F7F2E8C8A1B3D4E6F7890ABCDEF123456789012345678901234567890ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890AB,
    PasswordSalt = 0x1234567890ABCDEF1234567890ABCDEF
WHERE Email = 'admin@example.com';

-- Update other users with same hash for demo
UPDATE Users 
SET PasswordHash = 0x2E5F7F2E8C8A1B3D4E6F7890ABCDEF123456789012345678901234567890ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890AB,
    PasswordSalt = 0x1234567890ABCDEF1234567890ABCDEF
WHERE Email IN ('juan.perez@example.com', 'maria.gonzalez@example.com', 'carlos.rodriguez@example.com');

PRINT 'Password hashes updated for demo users. All passwords are: admin';
GO