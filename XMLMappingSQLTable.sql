USE [Config]
GO

/****** Object:  Table [dbo].[XMLMapping]    Script Date: 01/04/2020 2:39:08 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[XMLMapping](
	[MappingType] [nvarchar](50) NULL,
	[TableName] [nvarchar](50) NULL,
	[TableId] [nvarchar](50) NULL,
	[FLD_DATA_CL_ID] [nvarchar](50) NULL,
	[FLD_ID] [nvarchar](50) NULL
) ON [PRIMARY]

GO

