For DB if Using SQL server then run this Scripts 


IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'JiraPocTicket'
)
BEGIN
    CREATE TABLE dbo.JiraPocTicket (
        Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title       NVARCHAR(500) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Priority    NVARCHAR(50) NOT NULL CONSTRAINT DF_JiraPocTicket_Priority DEFAULT ('Medium'),
        JiraKey     NVARCHAR(50) NOT NULL,
        Status      NVARCHAR(100) NOT NULL CONSTRAINT DF_JiraPocTicket_Status DEFAULT ('To Do'),
        CreatedAt   DATETIME2 NOT NULL CONSTRAINT DF_JiraPocTicket_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2 NULL
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.JiraPoc_Upsert
    @JiraKey     NVARCHAR(50),
    @Title       NVARCHAR(500),
    @Description NVARCHAR(MAX) = NULL,
    @Priority    NVARCHAR(50) = 'Medium',
    @Status      NVARCHAR(100) = 'To Do'
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.JiraPocTicket WHERE JiraKey = @JiraKey)
    BEGIN
        UPDATE dbo.JiraPocTicket
        SET Title = @Title,
            Description = ISNULL(@Description, Description),
            Priority = @Priority,
            Status = @Status,
            UpdatedAt = SYSUTCDATETIME()
        WHERE JiraKey = @JiraKey;

        SELECT Id, Title, Description, Priority, JiraKey, Status, CreatedAt, UpdatedAt
        FROM dbo.JiraPocTicket WHERE JiraKey = @JiraKey;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.JiraPocTicket (Title, Description, Priority, JiraKey, Status)
        VALUES (@Title, @Description, @Priority, @JiraKey, @Status);

        SELECT Id, Title, Description, Priority, JiraKey, Status, CreatedAt, UpdatedAt
        FROM dbo.JiraPocTicket WHERE Id = SCOPE_IDENTITY();
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.JiraPoc_GetByJiraKey
    @JiraKey NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, Description, Priority, JiraKey, Status, CreatedAt, UpdatedAt
    FROM dbo.JiraPocTicket WHERE JiraKey = @JiraKey;
END
GO

CREATE OR ALTER PROCEDURE dbo.JiraPoc_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, Description, Priority, JiraKey, Status, CreatedAt, UpdatedAt
    FROM dbo.JiraPocTicket WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.JiraPoc_ListAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, Description, Priority, JiraKey, Status, CreatedAt, UpdatedAt
    FROM dbo.JiraPocTicket ORDER BY Id DESC;
END
GO




//Verify
EXEC dbo.JiraPoc_ListAll;
