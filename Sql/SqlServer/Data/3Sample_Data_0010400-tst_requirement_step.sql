/***************************************************************
**	Insert script for table TST_REQUIREMENT_STEP
***************************************************************/
SET IDENTITY_INSERT TST_REQUIREMENT_STEP ON; 

INSERT INTO TST_REQUIREMENT_STEP
(
REQUIREMENT_STEP_ID, REQUIREMENT_ID, POSITION, DESCRIPTION, IS_DELETED, CREATION_DATE, LAST_UPDATE_DATE, CONCURRENCY_DATE
)
VALUES
(
1, 30, 1, 'User logs into the system', 0, DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME())
),
(
2, 30, 2, 'User chooses option to create new book', 0, DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME())
),
(
3, 30, 3, 'User enters books name and author', 0, DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME())
),
(
4, 30, 4, 'User chooses book''s genre and sub-genre from list', 0, DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME())
),
(
5, 30, 5, 'User commits the changes and the new book is added to the system', 0, DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME())
),
(
6, 31, 6, 'User logs into the system', 0, DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME())
),
(
7, 31, 1, 'User chooses option to view existing books', 0, DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME())
),
(
8, 31, 2, 'User selects a specific book to edit', 0, DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME())
),
(
9, 31, 3, 'User modifies the various fields', 0, DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME())
),
(
10, 31, 4, 'User selects the option to commit the updates', 0, DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -138, SYSUTCDATETIME())
),
(
11, 32, 1, 'User logs into the system', 0, DATEADD(day, -137, SYSUTCDATETIME()), DATEADD(day, -137, SYSUTCDATETIME()), DATEADD(day, -137, SYSUTCDATETIME())
),
(
12, 32, 2, 'User chooses option to view existing books', 0, DATEADD(day, -137, SYSUTCDATETIME()), DATEADD(day, -137, SYSUTCDATETIME()), DATEADD(day, -137, SYSUTCDATETIME())
),
(
13, 32, 3, 'User selects a specific book to delete from the list', 0, DATEADD(day, -137, SYSUTCDATETIME()), DATEADD(day, -137, SYSUTCDATETIME()), DATEADD(day, -137, SYSUTCDATETIME())
),
(
14, 32, 4, 'User confirms the deletion and the book is deleted', 0, DATEADD(day, -137, SYSUTCDATETIME()), DATEADD(day, -137, SYSUTCDATETIME()), DATEADD(day, -137, SYSUTCDATETIME())
)
GO

SET IDENTITY_INSERT TST_REQUIREMENT_STEP OFF; 

