
/****** Object:  Table [dbo].[User]    Script Date: 1/12/2014 9:44:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[User](
	[user_id] [bigint] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](max) NOT NULL,
	[password] [nvarchar](50) NOT NULL,
	[first_name] [nvarchar](50) NOT NULL,
	[last_name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[User_Status]    Script Date: 1/12/2014 9:44:11 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[User_Status](
	[status_id] [bigint] IDENTITY(1,1) NOT NULL,
	[status_name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_User_Status] PRIMARY KEY CLUSTERED 
(
	[status_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Login_History]    Script Date: 1/12/2014 9:44:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Login_History](
	[login_history_id] [bigint] IDENTITY(1,1) NOT NULL,
	[user_id] [bigint] NOT NULL,
	[status_id] [bigint] NOT NULL,
	[date_time] [datetime] NOT NULL,
 CONSTRAINT [PK_Login_History] PRIMARY KEY CLUSTERED 
(
	[login_history_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Question]    Script Date: 1/12/2014 9:44:11 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Question](
	[question_id] [bigint] IDENTITY(1,1) NOT NULL,
	[question] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_Security_Question] PRIMARY KEY CLUSTERED 
(
	[question_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Security]    Script Date: 1/12/2014 9:44:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Security](
	[security_id] [bigint] IDENTITY(1,1) NOT NULL,
	[user_id] [bigint] NOT NULL,
	[question_id] [bigint] NOT NULL,
	[answer] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Security_Answer] PRIMARY KEY CLUSTERED 
(
	[security_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

---Login History Foreign Keys
ALTER TABLE [dbo].[Login_History]  WITH CHECK ADD  CONSTRAINT [FK_StatusLoginHistory] FOREIGN KEY([status_id])
REFERENCES [dbo].[User_Status] ([status_id])
GO

ALTER TABLE [dbo].[Login_History] CHECK CONSTRAINT [FK_StatusLoginHistory]
GO

ALTER TABLE [dbo].[Login_History]  WITH CHECK ADD  CONSTRAINT [FK_UserLoginHistory] FOREIGN KEY([user_id])
REFERENCES [dbo].[User] ([user_id])
GO

ALTER TABLE [dbo].[Login_History] CHECK CONSTRAINT [FK_UserLoginHistory]
GO


---Security Questions Foreign Keys
ALTER TABLE [dbo].[Security] WITH CHECK ADD  CONSTRAINT [FK_User_Security] FOREIGN KEY([user_id])
REFERENCES [dbo].[User] ([user_id])
GO

ALTER TABLE [dbo].[Security] CHECK CONSTRAINT [FK_User_Security]
GO

ALTER TABLE [dbo].[Security] WITH CHECK ADD  CONSTRAINT [FK_Question_Security] FOREIGN KEY([question_id])
REFERENCES [dbo].[Question] ([question_id])
GO

ALTER TABLE [dbo].[Security] CHECK CONSTRAINT [FK_Question_Security]
GO

--Insert User Status
insert into UserUser_Status (status_name) values ('New User')
insert into UserUser_Status (status_name) values ('Login')
insert into UserUser_Status (status_name) values ('Logout')
insert into UserUser_Status (status_name) values ('Password Reset')


--Insert Security Questions
insert into Question (question) values ('What was your childhood nickname?')
insert into Question (question) values ('In what city did you meet your spouse/significant other?')
insert into Question (question) values ('What is the name of your favorite childhood friend?')
insert into Question (question) values ('What street did you live on in third grade?')
insert into Question (question) values ('What is your oldest sibling’s birthday month and year? (e.g., January 1900)')
insert into Question (question) values ('What is the middle name of your oldest child?')
insert into Question (question) values ('What is your oldest sibling''s middle name?')
insert into Question (question) values ('What school did you attend for sixth grade?')
insert into Question (question) values ('What was your childhood phone number including area code? (e.g., 000-000-0000)')
insert into Question (question) values ('What is your oldest cousin''s first and last name?')
insert into Question (question) values ('What was the name of your first stuffed animal?')
insert into Question (question) values ('In what city or town did your mother and father meet?')
insert into Question (question) values ('Where were you when you had your first kiss?')
insert into Question (question) values ('What is the first name of the boy or girl that you first kissed?')
insert into Question (question) values ('What was the last name of your third grade teacher?')
insert into Question (question) values ('In what city does your nearest sibling live?')
insert into Question (question) values ('What is your oldest brother’s birthday month and year? (e.g., January 1900)')
insert into Question (question) values ('What is your maternal grandmother''s maiden name?')
insert into Question (question) values ('In what city or town was your first job?')
insert into Question (question) values ('What is the name of the place your wedding reception was held?')
insert into Question (question) values ('What is the name of a college you applied to but didn''t attend?')
insert into Question (question) values ('Where were you when you first heard about 9/11?')
