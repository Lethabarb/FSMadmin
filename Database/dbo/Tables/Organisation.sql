CREATE TABLE [dbo].[Organisation] (
    [Id]        UNIQUEIDENTIFIER            NOT NULL,
    [Name]      NVARCHAR (MAX) NOT NULL,
    [ShortName] NVARCHAR (MAX) NOT NULL,
    [GuildId]   NUMERIC (20)   NOT NULL,
    [BotConfig] NVARCHAR (MAX) NOT NULL DEFAULT '{}',
    [ImageName] NVARCHAR(MAX) NOT NULL, 
    [ImagePath] NVARCHAR(MAX) NOT NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

