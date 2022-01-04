CREATE TABLE [dbo].[Organisation] (
    [Id]        INT            NOT NULL IDENTITY(1,1),
    [Name]      NVARCHAR (MAX) NOT NULL,
    [GuildId]   NUMERIC (20)   NOT NULL,
    [BotConfig] NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

