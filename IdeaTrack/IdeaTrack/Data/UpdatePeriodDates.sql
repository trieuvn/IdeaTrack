-- =====================================================
-- UPDATE INITIATIVE PERIODS TO HAVE OPEN PERIODS
-- Run this to make periods "open" for testing on 2026-01-06
-- =====================================================

-- First, check what academic years exist
SELECT Id, Name, IsCurrent FROM AcademicYears;

-- Check current periods
SELECT Id, Name, StartDate, EndDate, AcademicYearId FROM InitiativePeriods;

-- Update existing periods to be open (covering 2026-01-06)
-- Period 1: Set dates to cover today
UPDATE InitiativePeriods 
SET StartDate = '2025-09-01', EndDate = '2026-06-30'
WHERE Id = 1;

-- Period 2: Also set to cover today
UPDATE InitiativePeriods 
SET StartDate = '2026-01-01', EndDate = '2026-12-31'
WHERE Id = 2;

-- Period 3: Set to cover today as well
UPDATE InitiativePeriods 
SET StartDate = '2025-12-01', EndDate = '2026-03-31'
WHERE Id = 3;

-- Verify the update
SELECT Id, Name, StartDate, EndDate, AcademicYearId FROM InitiativePeriods;

-- Check categories linked to these periods
SELECT ic.Id, ic.Name, ic.PeriodId, ip.Name as PeriodName
FROM InitiativeCategories ic
INNER JOIN InitiativePeriods ip ON ic.PeriodId = ip.Id
WHERE ip.Id IN (1, 2, 3);

PRINT N'Periods updated to be open for testing!';
