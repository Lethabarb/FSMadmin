CREATE TABLE [dbo].[Organisation] (
    [Id]        INT            NOT NULL,
    [Name]      NVARCHAR (MAX) NOT NULL,
    [GuildId]   NUMERIC (20)   NOT NULL,
    [BotConfig] NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

