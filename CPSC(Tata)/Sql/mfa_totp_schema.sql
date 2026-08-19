-- ============================================================================
-- CPSC(Tata) - production DB script to enable app-level TOTP MFA
-- (Users.cs GetTotpSecret / SetTotpSecret; App_Code/TotpHelper.cs)
--
-- Verified against a restored copy of the actual "Tata" database (SQLEXPRESS2025
-- instance, local): dbo.ValidateUser reads from the view dbo.vw_Users, which for
-- every employee-derived login is backed by dbo.tblEmployees, keyed on [LOGIN]
-- (varchar(50), NOT NULL). This script adds the TOTP secret there.
--
-- KNOWN GAP: vw_Users also UNIONs in several hardcoded logins that do NOT exist
-- as rows in tblEmployees (as of this backup: cpsc, NP_1008870, HKUMAR_1002850,
-- SUMANC_1009000, YA_1009590, duvesh, Monika - see vw_Users' definition). MFA
-- enrollment (SetTotpSecret) will affect 0 rows for those specific logins since
-- there's nothing to UPDATE - the app already treats rowsAffected=0 as "enrollment
-- failed" (Users.SetTotpSecret), so this fails safely, but those accounts need a
-- real backing row (or a separate fix) before they can complete MFA enrollment.
--
-- Idempotent: safe to run again if partially applied already.
-- ============================================================================

USE [Tata];
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.tblEmployees') AND name = 'TotpSecret'
)
BEGIN
    ALTER TABLE dbo.tblEmployees ADD TotpSecret varchar(64) NULL;
END
GO

-- CREATE OR ALTER needs compat level 130+ (SQL Server 2016+); using the
-- drop-then-create form instead so this also runs on older production instances.
IF OBJECT_ID('dbo.GetTotpSecret', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetTotpSecret;
GO

CREATE PROCEDURE dbo.GetTotpSecret
    @Username varchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TotpSecret
    FROM dbo.tblEmployees
    WHERE [LOGIN] = @Username;
END
GO

IF OBJECT_ID('dbo.SetTotpSecret', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SetTotpSecret;
GO

CREATE PROCEDURE dbo.SetTotpSecret
    @Username varchar(50),
    @TotpSecret varchar(64)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.tblEmployees
    SET TotpSecret = @TotpSecret
    WHERE [LOGIN] = @Username;
END
GO
