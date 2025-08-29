-- Migration script to safely remove Specialists table and SpecialistId column from Assignments
-- Execute this script after updating the application code

-- WARNING: This script will permanently delete the Specialists table and related data
-- Ensure you have a backup before executing these changes

-- Step 1: Remove the SpecialistId column from Assignments table
-- First, check if there are any existing assignments with specialists
SELECT COUNT(*) AS AssignmentsWithSpecialists 
FROM Assignments 
WHERE SpecialistId IS NOT NULL;

-- If the above query returns 0, proceed with the column removal
-- If it returns a number > 0, you may want to backup that data or handle it differently

-- Remove the foreign key constraint (if it exists)
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assignments_Specialists')
BEGIN
    ALTER TABLE Assignments DROP CONSTRAINT FK_Assignments_Specialists;
    PRINT 'Foreign key constraint FK_Assignments_Specialists removed';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_Assignments_Specialists not found';
END
GO

-- Remove the SpecialistId column from Assignments table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assignments') AND name = 'SpecialistId')
BEGIN
    ALTER TABLE Assignments DROP COLUMN SpecialistId;
    PRINT 'SpecialistId column removed from Assignments table';
END
ELSE
BEGIN
    PRINT 'SpecialistId column not found in Assignments table';
END
GO

-- Step 2: Drop the Specialists table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Specialists')
BEGIN
    DROP TABLE Specialists;
    PRINT 'Specialists table dropped successfully';
END
ELSE
BEGIN
    PRINT 'Specialists table not found';
END
GO

-- Verification: Check that the changes were applied successfully
PRINT 'Migration completed. Verifying changes...';

-- Verify SpecialistId column is removed
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assignments') AND name = 'SpecialistId')
BEGIN
    PRINT '✓ SpecialistId column successfully removed from Assignments table';
END
ELSE
BEGIN
    PRINT '✗ ERROR: SpecialistId column still exists in Assignments table';
END

-- Verify Specialists table is removed
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Specialists')
BEGIN
    PRINT '✓ Specialists table successfully removed';
END
ELSE
BEGIN
    PRINT '✗ ERROR: Specialists table still exists';
END

PRINT 'Migration script execution completed.';

-- Note: The application now assigns tasks directly to users without needing specialists.
-- All assignment functionality will work through the Users table only.