USE [ExpressCart]
GO

/****** Object:  Table [dbo].[Travels]    Script Date: 02-07-2024 10:01:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Travels](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TicketNo] [nvarchar](50) NULL,
	[UserId] [nvarchar](50) NOT NULL,
	[DepLoc] [nvarchar](100) NOT NULL,
	[DepTerm] [nvarchar](100) NULL,
	[DepDate] [nvarchar](100) NOT NULL,
	[Stops] [int] NOT NULL,
	[DestLoc] [nvarchar](100) NOT NULL,
	[DestTerm] [nvarchar](100) NULL,
	[DestDate] [nvarchar](100) NOT NULL,
	[Adults] [int] NOT NULL,
	[Childerns] [int] NOT NULL,
	[CarrierName] [nvarchar](100) NULL,
	[AircraftName] [nvarchar](100) NULL,
	[Code] [nvarchar](50) NULL,
	[OneWay] [bit] NOT NULL,
	[BasePrice] [nvarchar](100) NULL,
	[TotalPrice] [nvarchar](100) NULL,
	[PaymentStatus] [nvarchar](100) NULL,
	[PaymentIntentId] [nvarchar](100) NULL,
	[PaymentDate] [nvarchar](100) NULL,
	[BookCancelledYN] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO