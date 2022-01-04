CREATE TABLE [dbo].[Scrim] (
    [Id]       INT  NOT NULL IDENTITY(1,1),
    [Team1]    INT  NULL,
    [Team2]    INT  NULL,
    [Datetime] DATE NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

