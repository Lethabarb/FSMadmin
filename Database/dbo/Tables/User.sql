CREATE TABLE [dbo].[User]
(
    [Id] int NOT NULL PRIMARY KEY IDENTITY(1,1),
	[EmailAddress] NVARCHAR(MAX) NOT NULL, 
    [Username] NVARCHAR(MAX) NOT NULL, 
    [Password] NVARCHAR(MAX) NOT NULL, 
    [Role] NVARCHAR(MAX) NULL, 
    [DiscordUserId] NUMERIC(20) NULL
)
