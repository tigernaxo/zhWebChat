USE [master]
GO
/****** Object:  Database [WebChat]    Script Date: 2022/3/31 下午 11:52:18 ******/
CREATE DATABASE [WebChat]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'WebChat', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\WebChat.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'WebChat_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\WebChat_log.ldf' , SIZE = 18240KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [WebChat] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [WebChat].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [WebChat] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [WebChat] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [WebChat] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [WebChat] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [WebChat] SET ARITHABORT OFF 
GO
ALTER DATABASE [WebChat] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [WebChat] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [WebChat] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [WebChat] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [WebChat] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [WebChat] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [WebChat] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [WebChat] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [WebChat] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [WebChat] SET  DISABLE_BROKER 
GO
ALTER DATABASE [WebChat] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [WebChat] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [WebChat] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [WebChat] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [WebChat] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [WebChat] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [WebChat] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [WebChat] SET RECOVERY FULL 
GO
ALTER DATABASE [WebChat] SET  MULTI_USER 
GO
ALTER DATABASE [WebChat] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [WebChat] SET DB_CHAINING OFF 
GO
ALTER DATABASE [WebChat] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [WebChat] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [WebChat] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [WebChat] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'WebChat', N'ON'
GO
ALTER DATABASE [WebChat] SET QUERY_STORE = OFF
GO
USE [WebChat]
GO
/****** Object:  Table [dbo].[A_ChatRoomUser]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[A_ChatRoomUser](
	[roomId] [bigint] NOT NULL,
	[userId] [int] NOT NULL,
	[readMsgId] [bigint] NULL,
	[status] [smallint] NOT NULL,
	[createTime] [datetime] NULL,
	[actTime] [datetime] NULL,
	[userType] [smallint] NOT NULL,
	[isAdmin] [bit] NOT NULL,
 CONSTRAINT [PK_A_ChatRoomUser] PRIMARY KEY CLUSTERED 
(
	[roomId] ASC,
	[userId] ASC,
	[userType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[A_ChatRoom]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[A_ChatRoom](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[announce] [nvarchar](max) NULL,
	[createTime] [datetime] NULL,
	[actTime] [datetime] NULL,
	[status] [smallint] NOT NULL,
	[type] [smallint] NOT NULL,
	[title] [nvarchar](100) NULL,
 CONSTRAINT [PK_A_ChatRoom] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[A_ChatRoom_ChatRoomUser]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[A_ChatRoom_ChatRoomUser]
AS
SELECT 
	t.id, t.title, t.announce, t.status, t.type, t.actTime, t.createTime,
	t1.roomId, t1.userId, t1.userType, t1.status status2, t1.readMsgId, t1.actTime actTime2, t1.createTime createTime2
FROM dbo.A_ChatRoom t 
LEFT JOIN A_ChatRoomUser t1 on t.id = t1.roomId
GO
/****** Object:  Table [dbo].[A_ChatMsg]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[A_ChatMsg](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[roomId] [bigint] NOT NULL,
	[userId] [int] NOT NULL,
	[type] [smallint] NOT NULL,
	[text] [nvarchar](max) NULL,
	[createTime] [datetime] NULL,
	[actTime] [datetime] NULL,
	[fileName] [nvarchar](max) NULL,
	[hashName] [nvarchar](max) NULL,
	[status] [smallint] NOT NULL,
	[userType] [smallint] NOT NULL,
 CONSTRAINT [PK_A_ChatMessage] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[A_ChatMsgType]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[A_ChatMsgType](
	[id] [smallint] NOT NULL,
	[typeName] [nvarchar](50) NULL,
 CONSTRAINT [PK_A_ChatMessageType] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[A_ConnectInfo]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[A_ConnectInfo](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[userId] [int] NOT NULL,
	[userType] [smallint] NOT NULL,
	[conId] [varchar](255) NOT NULL,
	[isDisConnect] [bit] NOT NULL,
	[startTime] [datetime] NOT NULL,
	[endTime] [datetime] NULL,
 CONSTRAINT [PK_A_ConnectInfo] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[A_ConnectInfoDay]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[A_ConnectInfoDay](
	[yyyyMMdd] [char](8) NOT NULL,
	[totalSec] [bigint] NOT NULL,
	[actTime] [datetime] NULL,
 CONSTRAINT [PK_A_ConnectInfoDay] PRIMARY KEY CLUSTERED 
(
	[yyyyMMdd] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[A_UserAnonymous]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[A_UserAnonymous](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[token] [varchar](500) NULL,
	[type] [smallint] NOT NULL,
	[roomId] [nchar](10) NULL,
	[isUsed] [bit] NOT NULL,
	[userName] [nvarchar](200) NULL,
	[useTime] [datetime] NULL,
	[createTime] [datetime] NULL,
	[actTime] [datetime] NULL,
	[isBaned] [bit] NOT NULL,
 CONSTRAINT [PK_A_UserAnonymous] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[A_UserRelationShip]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[A_UserRelationShip](
	[userId] [int] NOT NULL,
	[userId2] [int] NOT NULL,
	[type] [smallint] NOT NULL,
	[createTime] [datetime] NULL,
 CONSTRAINT [PK_A_UserRelationList] PRIMARY KEY CLUSTERED 
(
	[userId] ASC,
	[userId2] ASC,
	[type] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[S10_group]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[S10_group](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[groupName] [nvarchar](200) NULL,
	[status] [smallint] NOT NULL,
	[createUser] [int] NULL,
	[createTime] [datetime] NULL,
	[actUser] [int] NULL,
	[actTime] [datetime] NULL,
	[groupId] [varchar](255) NULL,
 CONSTRAINT [PK_S10_group] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[S10_userGroup]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[S10_userGroup](
	[userId] [int] NOT NULL,
	[groupId] [int] NOT NULL,
	[createTime] [datetime] NULL,
	[actTime] [datetime] NULL,
	[createUser] [int] NULL,
	[actUser] [int] NULL,
 CONSTRAINT [PK_S10_userGroup] PRIMARY KEY CLUSTERED 
(
	[userId] ASC,
	[groupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[S10_users]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[S10_users](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[loginId] [varchar](50) NOT NULL,
	[password] [varchar](50) NOT NULL,
	[userName] [nvarchar](200) NULL,
	[phone] [varchar](50) NULL,
	[status] [smallint] NOT NULL,
	[createTime] [datetime] NULL,
	[actTime] [datetime] NULL,
	[photo] [nvarchar](500) NULL,
 CONSTRAINT [PK_S10_users] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [IX_S10_users_loginId] UNIQUE NONCLUSTERED 
(
	[loginId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[A_ChatMsg]  WITH CHECK ADD  CONSTRAINT [FK_A_ChatMessage_A_ChatMessageType] FOREIGN KEY([type])
REFERENCES [dbo].[A_ChatMsgType] ([id])
GO
ALTER TABLE [dbo].[A_ChatMsg] CHECK CONSTRAINT [FK_A_ChatMessage_A_ChatMessageType]
GO
ALTER TABLE [dbo].[A_ChatMsg]  WITH CHECK ADD  CONSTRAINT [FK_A_ChatMessage_A_ChatRoom] FOREIGN KEY([roomId])
REFERENCES [dbo].[A_ChatRoom] ([id])
GO
ALTER TABLE [dbo].[A_ChatMsg] CHECK CONSTRAINT [FK_A_ChatMessage_A_ChatRoom]
GO
ALTER TABLE [dbo].[A_ChatRoomUser]  WITH CHECK ADD  CONSTRAINT [FK_A_ChatRoomUser_A_ChatMsg] FOREIGN KEY([readMsgId])
REFERENCES [dbo].[A_ChatMsg] ([id])
GO
ALTER TABLE [dbo].[A_ChatRoomUser] CHECK CONSTRAINT [FK_A_ChatRoomUser_A_ChatMsg]
GO
ALTER TABLE [dbo].[A_ChatRoomUser]  WITH CHECK ADD  CONSTRAINT [FK_A_ChatRoomUser_A_ChatRoom] FOREIGN KEY([roomId])
REFERENCES [dbo].[A_ChatRoom] ([id])
GO
ALTER TABLE [dbo].[A_ChatRoomUser] CHECK CONSTRAINT [FK_A_ChatRoomUser_A_ChatRoom]
GO
ALTER TABLE [dbo].[A_UserRelationShip]  WITH CHECK ADD  CONSTRAINT [FK_A_UserRelationShip_S10_users] FOREIGN KEY([userId])
REFERENCES [dbo].[S10_users] ([id])
GO
ALTER TABLE [dbo].[A_UserRelationShip] CHECK CONSTRAINT [FK_A_UserRelationShip_S10_users]
GO
ALTER TABLE [dbo].[A_UserRelationShip]  WITH CHECK ADD  CONSTRAINT [FK_A_UserRelationShip_S10_users1] FOREIGN KEY([userId2])
REFERENCES [dbo].[S10_users] ([id])
GO
ALTER TABLE [dbo].[A_UserRelationShip] CHECK CONSTRAINT [FK_A_UserRelationShip_S10_users1]
GO
ALTER TABLE [dbo].[S10_userGroup]  WITH CHECK ADD  CONSTRAINT [FK_S10_userGroup_S10_group] FOREIGN KEY([groupId])
REFERENCES [dbo].[S10_group] ([id])
GO
ALTER TABLE [dbo].[S10_userGroup] CHECK CONSTRAINT [FK_S10_userGroup_S10_group]
GO
ALTER TABLE [dbo].[S10_userGroup]  WITH CHECK ADD  CONSTRAINT [FK_S10_userGroup_S10_users] FOREIGN KEY([userId])
REFERENCES [dbo].[S10_users] ([id])
GO
ALTER TABLE [dbo].[S10_userGroup] CHECK CONSTRAINT [FK_S10_userGroup_S10_users]
GO
/****** Object:  StoredProcedure [dbo].[P_A0001]    Script Date: 2022/3/31 下午 11:52:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[P_A0001]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update A_ChatRoomUser set readMsgId = null;
	delete from A_ChatMsg;
END
GO
USE [master]
GO
ALTER DATABASE [WebChat] SET  READ_WRITE 
GO
