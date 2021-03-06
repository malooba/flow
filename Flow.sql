USE [D:\WORK\REPOS\GITHUB FLOW\FLOW\FLOWSHARED\FLOW.MDF]
GO
/****** Object:  Table [dbo].[Activities]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Activities](
	[Name] [nvarchar](40) NOT NULL,
	[Version] [bigint] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Json] [nvarchar](max) NULL,
 CONSTRAINT [PK_Activities] PRIMARY KEY CLUSTERED 
(
	[Name] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Executions]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Executions](
	[ExecutionId] [uniqueidentifier] NOT NULL,
	[WorkflowName] [nvarchar](40) NOT NULL,
	[WorkflowVersion] [bigint] NOT NULL,
	[DecisionList] [nvarchar](40) NOT NULL,
	[ExecutionStartToCloseTimeout] [int] NULL,
	[TaskStartToCloseTimeout] [int] NULL,
	[TaskScheduleToStartTimeout] [int] NULL,
	[TaskScheduleToCloseTimeout] [nchar](10) NULL,
	[HistorySeen] [int] NOT NULL,
	[DeciderAlarm] [datetime2](7) NULL,
	[AwaitingDecision] [bit] NOT NULL,
	[LastSeen] [datetime2](7) NULL,
	[DeciderToken] [uniqueidentifier] NULL,
	[JobId] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Executions] PRIMARY KEY CLUSTERED 
(
	[ExecutionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExecutionStates]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExecutionStates](
	[ExecutionId] [uniqueidentifier] NOT NULL,
	[State] [nchar](10) NOT NULL,
 CONSTRAINT [PK_ExecutionState] PRIMARY KEY CLUSTERED 
(
	[ExecutionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Histories]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Histories](
	[ExecutionId] [uniqueidentifier] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventType] [nvarchar](50) NOT NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Json] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Histories_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Servers]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Servers](
	[Name] [nvarchar](50) NOT NULL,
	[UserId] [int] NULL,
	[Description] [nvarchar](50) NULL,
 CONSTRAINT [PK_Servers] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TaskLists]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaskLists](
	[ListName] [nvarchar](40) NOT NULL,
	[ExecutionId] [uniqueidentifier] NOT NULL,
	[Priority] [int] NOT NULL,
	[TaskScheduledEventId] [int] NOT NULL,
	[TaskToken] [uniqueidentifier] NOT NULL,
	[TaskAlarm] [datetime2](7) NOT NULL,
	[HeartbeatTimeout] [int] NULL,
	[HeartbeatAlarm] [datetime2](7) NULL,
	[WorkerId] [nvarchar](20) NULL,
	[Cancelling] [bit] NOT NULL,
	[ScheduledAt] [datetime2](7) NOT NULL,
	[TaskSheduleToCloseTimeout] [int] NULL,
	[TaskStartToCloseTimeout] [int] NULL,
	[Progress] [int] NULL,
	[ProgressMessage] [nvarchar](max) NULL,
	[StartedAt] [datetime2](7) NULL,
	[NotificationData] [nvarchar](max) NULL,
	[ProgressData] [nvarchar](max) NULL,
	[JobId] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_WorkQueues] PRIMARY KEY CLUSTERED 
(
	[TaskToken] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Users]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](50) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Variables]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Variables](
	[ExecutionId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](40) NOT NULL,
	[Json] [nvarchar](max) NULL,
 CONSTRAINT [PK_Variables] PRIMARY KEY CLUSTERED 
(
	[ExecutionId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Workers]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Workers](
	[WorkerId] [uniqueidentifier] NOT NULL,
	[WorkerName] [nvarchar](50) NOT NULL,
	[ServerName] [nvarchar](50) NOT NULL,
	[PackageName] [nvarchar](50) NOT NULL,
	[Config] [nvarchar](max) NULL,
	[ProcessId] [int] NULL,
 CONSTRAINT [PK_Workers] PRIMARY KEY CLUSTERED 
(
	[WorkerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WorkflowConfigs]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkflowConfigs](
	[WorkflowName] [nvarchar](40) NOT NULL,
	[WorkflowVersion] [bigint] NULL,
	[Json] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Workflows]    Script Date: 18/05/2016 14:55:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Workflows](
	[Name] [nvarchar](40) NOT NULL,
	[Version] [bigint] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Json] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Workflows] PRIMARY KEY CLUSTERED 
(
	[Name] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
INSERT [dbo].[Activities] ([Name], [Version], [Description], [Json]) VALUES (N'asynch', 1000000000000, NULL, N'{"objtype":"activity","name":"asynch","version":"1.0.0.0","description":"Wait for a bit, asynchronously","defaultTaskList":"asynch","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"delay":{"description":"Time to waste in milliseconds","type":"integer","required":false}},"outputs":{}}')
INSERT [dbo].[Activities] ([Name], [Version], [Description], [Json]) VALUES (N'delay', 1000000000000, NULL, N'{"objtype":"activity","name":"delay","version":"1.0.0.0","description":"Wait for a bit","defaultTaskList":"delay","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"delay":{"description":"Time to waste in milliseconds","type":"integer","required":false}},"outputs":{"result":{"description":"output","type":"integer"}}}')
INSERT [dbo].[Activities] ([Name], [Version], [Description], [Json]) VALUES (N'javascript', 1000000000000, NULL, N'{"objtype":"activity","name":"javascript","version":"1.0.0.0","description":"Run JavaScript","defaultTaskList":"javascript","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"$script":{"description":"script to execute","type":"javascript","required":true}},"outputs":{}}')
INSERT [dbo].[Activities] ([Name], [Version], [Description], [Json]) VALUES (N'test', 1000000000000, NULL, N'{"objtype":"activity","name":"test","version":"1.0.0.0","description":"Wait for a bit","defaultTaskList":"demo","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{},"outputs":{}}')
INSERT [dbo].[Activities] ([Name], [Version], [Description], [Json]) VALUES (N'wait', 1000000000000, NULL, N'{"objtype":"activity","name":"wait","version":"1.0.0.0","description":"Wait for a signal","defaultTaskList":"wait","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"signalName":{"description":"Name of signal to wait for","type":"string","required":true}},"outputs":{}}')
INSERT [dbo].[Workflows] ([Name], [Version], [Description], [Json]) VALUES (N'Adder', 1000000000000, N'Adder', N'{"objtype":"workflow","name":"Adder","version":"1.0.0.0","decisionList":"decider","defaultTaskStartToCloseTimeout":null,"variables":{"x":{"type":"integer","path":"$._job.x","required":true},"y":{"type":"integer","path":"$._job.y","required":true},"sum":{"lit":"","type":"integer","required":false},"product":{"lit":"1","type":"integer","required":false},"_config":{"type":"object","path":"$._config","required":false,"description":"Workflow input variable"},"_job":{"type":"object","path":"$._job","required":false,"description":"Workflow input variable"},"_jobType":{"type":"string","path":"$._jobType","required":false,"description":"Workflow input variable"},"_jobId":{"type":"string","path":"$._jobId","required":false,"description":"Workflow input variable"}},"tasks":[{"taskId":"start","activityName":"start","activityVersion":"0.0.0.0","outflows":[{"name":"Out","target":"add","targetPin":"In","route":[180,140,250,140]}],"taskPriority":0,"symbol":{"name":"start","label":"Start","style":"circle","locationX":100,"locationY":100}},{"taskId":"add","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"result.sum = x + y;","type":"any","required":true,"description":"script to execute","userDefined":false},"x":{"var":"x","type":"integer","path":"","required":true,"description":"","userDefined":true},"y":{"var":"y","type":"integer","path":"","required":true,"userDefined":true}},"outputs":{"sum":{"var":"sum","type":"string","description":"script output","userDefined":true}},"outflows":[{"name":"Out","target":"GetMetadata","targetPin":"In","route":[370,140,440,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"javascript","label":"Script","style":"box","locationX":250,"locationY":100}},{"taskId":"double","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"result.product = sum * 2;","type":"any","required":true,"description":"script to execute","userDefined":false},"sum":{"var":"sum","type":"integer","required":true,"userDefined":true}},"outputs":{"product":{"var":"product","type":"string","description":"script output","userDefined":true}},"outflows":[{"name":"Out","target":"End","targetPin":"In","route":[740,140,830,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"javascript","label":"Script","style":"box","locationX":620,"locationY":100}},{"taskId":"GetMetadata","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"result.metadata = Metadata.GetMetadataXml(file);","type":"javascript","required":true,"description":"script to execute","userDefined":false},"file":{"lit":"C:\\freeMXF-mxf1.mxf","type":"string","required":false,"description":"","userDefined":true}},"outputs":{},"outflows":[{"name":"Out","target":"double","targetPin":"In","route":[560,140,620,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"javascript","label":"Script","style":"box","locationX":440,"locationY":100}},{"taskId":"End","activityName":"end","activityVersion":"1.0.0.0","inputs":{"jobId":{"lit":12345,"type":"integer","required":true,"userDefined":false},"message":{"lit":"Done","type":"string","required":false,"userDefined":false},"progress":{"lit":100,"type":"integer","required":false,"userDefined":false},"destination":{"lit":"DESTINATION","type":"string","required":true,"userDefined":false}},"outflows":[],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"end","label":"End","style":"circle","locationX":830,"locationY":100}}]}')
INSERT [dbo].[Workflows] ([Name], [Version], [Description], [Json]) VALUES (N'AdderAsynch', 2000000000000, NULL, N'{"objtype":"workflow","name":"AdderAsynch","version":"2.0.0.0","decisionList":"decider","defaultTaskStartToCloseTimeout":null,"variables":{"x":{"type":"integer","path":"$._job.x","required":true},"y":{"type":"integer","path":"$._job.y","required":true},"sum":{"lit":"0","type":"integer","required":false},"product":{"lit":"1","type":"integer","required":false},"_config":{"type":"object","path":"$._config","required":false,"description":"Workflow input variable"},"_job":{"type":"object","path":"$._job","required":false,"description":"Workflow input variable"},"_jobType":{"type":"string","path":"$._jobType","required":false,"description":"Workflow input variable"},"_jobId":{"type":"string","path":"$._jobId","required":false,"description":"Workflow input variable"}},"tasks":[{"taskId":"start","activityName":"start","activityVersion":"0.0.0.0","outflows":[{"name":"Out","target":"Delay","targetPin":"In","route":[180,140,210,140]}],"taskPriority":0,"symbol":{"name":"start","label":"Start","style":"circle","locationX":100,"locationY":100}},{"taskId":"add","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"result.sum = x + y;","type":"any","required":true,"description":"script to execute","userDefined":false},"x":{"var":"x","type":"integer","required":true,"userDefined":true},"y":{"var":"y","type":"integer","required":true,"userDefined":true}},"outputs":{"sum":{"var":"sum","type":"string","description":"script output","userDefined":true}},"outflows":[{"name":"Out","target":"double","targetPin":"In","route":[490,140,540,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"javascript","label":"Script","style":"box","locationX":370,"locationY":100}},{"taskId":"double","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"result.product = sum * 2;","type":"any","required":true,"description":"script to execute","userDefined":false},"sum":{"var":"sum","type":"integer","required":true,"userDefined":true}},"outputs":{"product":{"var":"product","type":"string","description":"script output","userDefined":true}},"outflows":[{"name":"Out","target":"AwaitDelay","targetPin":"In","route":[660,140,700,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"javascript","label":"Script","style":"box","locationX":540,"locationY":100}},{"taskId":"Delay","activityName":"asynch","activityVersion":"1.0.0.0","inputs":{"delay":{"lit":5000,"type":"integer","required":false,"description":"Time to waste in milliseconds","userDefined":false},"signalName":{"lit":"fred","type":"string","required":true,"description":"Name of signal to send on completion","userDefined":false}},"outputs":{},"outflows":[{"name":"Out","target":"add","targetPin":"In","route":[330,140,370,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"asynch","label":"Asynch","style":"box","locationX":210,"locationY":100}},{"taskId":"AwaitDelay","activityName":"wait","activityVersion":"1.0.0.0","inputs":{"signalName":{"lit":"fred","type":"string","required":true,"description":"Name of signal to wait for","userDefined":false}},"outputs":{},"outflows":[{"name":"Out","target":"719705ea","targetPin":"In","route":[820,140,870,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"wait","label":"Wait","style":"box","locationX":700,"locationY":100}},{"taskId":"719705ea","activityName":"end","activityVersion":"1.0.0.0","inputs":{"jobId":{"var":"_jobId","type":"integer","required":true,"userDefined":false},"message":{"type":"string","required":false,"default":"COMPLETE","userDefined":false},"progress":{"type":"integer","required":false,"default":100,"userDefined":false},"destination":{"lit":"there","type":"string","required":true,"userDefined":false}},"outflows":[],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"end","label":"End","style":"circle","locationX":870,"locationY":100}}]}')
INSERT [dbo].[Workflows] ([Name], [Version], [Description], [Json]) VALUES (N'Test', 1000000000000, NULL, N'{"objtype":"workflow","name":"Test","version":"1.0.0.0","decisionList":"decider","defaultTaskStartToCloseTimeout":null,"variables":{},"tasks":[{"taskId":"start","activityName":"start","activityVersion":"0.0.0.0","outflows":[{"name":"Out","target":"delay","targetPin":"In","route":[180,140,260,140]}],"taskPriority":"0","symbol":{"name":"start","label":"Start","style":"circle","locationX":100,"locationY":100}},{"taskId":"delay","activityName":"test","activityVersion":"1.0.0.0","inputs":{},"outputs":{},"outflows":[{"name":"Out","target":"end","targetPin":"In","route":[380,140,460,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"tasklist":"delay","taskPriority":"0","symbol":{"name":"test","label":"Test","style":"box","locationX":260,"locationY":100}},{"taskId":"end","activityName":"end","activityVersion":"0.0.0.0","outflows":[],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"end","label":"End","style":"circle","locationX":460,"locationY":100}}]}')
INSERT [dbo].[Workflows] ([Name], [Version], [Description], [Json]) VALUES (N'TestHeartbeat', 1000000000000, NULL, N'{"objtype":"workflow","name":"TestHeartbeat","version":"1.0.0.0","decisionList":"decider","defaultTaskStartToCloseTimeout":null,"variables":{"_config":{"type":"object","path":"$._config","required":false,"description":"Workflow input variable"},"_job":{"type":"object","path":"$._job","required":false,"description":"Workflow input variable"},"_jobType":{"type":"string","path":"$._jobType","required":false,"description":"Workflow input variable"},"_jobId":{"type":"string","path":"$._jobId","required":false,"description":"Workflow input variable"}},"tasks":[{"taskId":"Start","activityName":"start","activityVersion":"0.0.0.0","outflows":[{"name":"Out","target":"Task1","targetPin":"In","route":[180,140,220,140]}],"taskPriority":0,"symbol":{"name":"start","label":"Start","style":"circle","locationX":100,"locationY":100}},{"taskId":"Task1","activityName":"delay","activityVersion":"1.0.0.0","inputs":{"delay":{"lit":20000,"type":"integer","required":false,"description":"Time to waste in milliseconds","userDefined":false},"progressData":{"lit":{"stage":1,"stages":3},"type":"object","required":false,"description":"","userDefined":true}},"outputs":{"result":{"type":"integer","description":"output","userDefined":false}},"outflows":[{"name":"Out","target":"Task2","targetPin":"In","route":[340,140,380,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"delay","label":"Delay","style":"box","locationX":220,"locationY":100}},{"taskId":"Task2","activityName":"delay","activityVersion":"1.0.0.0","inputs":{"delay":{"lit":20000,"type":"integer","required":false,"description":"Time to waste in milliseconds","userDefined":false},"progressData":{"lit":{"stage":2,"stages":3},"type":"object","required":false,"description":"","userDefined":true}},"outputs":{"result":{"type":"integer","description":"output","userDefined":false}},"outflows":[{"name":"Out","target":"Task3","targetPin":"In","route":[500,140,540,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"delay","label":"Delay","style":"box","locationX":380,"locationY":100}},{"taskId":"Task3","activityName":"delay","activityVersion":"1.0.0.0","inputs":{"delay":{"lit":20000,"type":"integer","required":false,"description":"Time to waste in milliseconds","userDefined":false},"progressData":{"lit":{"stage":3,"stages":3},"type":"any","required":false,"description":"","userDefined":true}},"outputs":{"result":{"type":"integer","description":"output","userDefined":false}},"outflows":[{"name":"Out","target":"Complete","targetPin":"In","route":[660,140,700,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"delay","label":"Delay","style":"box","locationX":540,"locationY":100}},{"taskId":"CleanUp","activityName":"cleanup","activityVersion":"0.0.0.0","outflows":[{"name":"Out","target":"CleanupStuff","targetPin":"In","route":[180,310,220,310]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"cleanup","label":"Clean Up","style":"circle","locationX":100,"locationY":270}},{"taskId":"CleanupStuff","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"Console.WriteLine(''Cleaning up'');","type":"javascript","required":true,"description":"script to execute","userDefined":false}},"outputs":{},"outflows":[{"name":"Out","target":"Really End","targetPin":"In","route":[340,310,390,310]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"javascript","label":"Script","style":"box","locationX":220,"locationY":270}},{"taskId":"Complete","activityName":"jobComplete","activityVersion":"1.0.0.0","inputs":{"destination":{"lit":"DONE","type":"string","required":true,"userDefined":false}},"outputs":{},"outflows":[{"name":"Out","target":"End","targetPin":"In","route":[820,140,860,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"jobComplete","label":"Job Complete","style":"box","locationX":700,"locationY":100}},{"taskId":"End","activityName":"end","activityVersion":"0.0.0.0","outflows":[],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"end","label":"End","style":"circle","locationX":860,"locationY":100}},{"taskId":"Really End","activityName":"end","activityVersion":"0.0.0.0","outflows":[],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":0,"symbol":{"name":"end","label":"End","style":"circle","locationX":390,"locationY":270}}]}')
ALTER TABLE [dbo].[TaskLists] ADD  CONSTRAINT [DF_WorkQueues_Cancelling]  DEFAULT ((0)) FOR [Cancelling]
GO
ALTER TABLE [dbo].[Executions]  WITH CHECK ADD  CONSTRAINT [FK_Executions_Workflows] FOREIGN KEY([WorkflowName], [WorkflowVersion])
REFERENCES [dbo].[Workflows] ([Name], [Version])
GO
ALTER TABLE [dbo].[Executions] CHECK CONSTRAINT [FK_Executions_Workflows]
GO
ALTER TABLE [dbo].[ExecutionStates]  WITH CHECK ADD  CONSTRAINT [FK_ExecutionState_Executions] FOREIGN KEY([ExecutionId])
REFERENCES [dbo].[Executions] ([ExecutionId])
GO
ALTER TABLE [dbo].[ExecutionStates] CHECK CONSTRAINT [FK_ExecutionState_Executions]
GO
ALTER TABLE [dbo].[Histories]  WITH CHECK ADD  CONSTRAINT [FK_Histories_Executions] FOREIGN KEY([ExecutionId])
REFERENCES [dbo].[Executions] ([ExecutionId])
GO
ALTER TABLE [dbo].[Histories] CHECK CONSTRAINT [FK_Histories_Executions]
GO
ALTER TABLE [dbo].[Servers]  WITH CHECK ADD  CONSTRAINT [FK_Servers_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Servers] CHECK CONSTRAINT [FK_Servers_Users]
GO
ALTER TABLE [dbo].[TaskLists]  WITH CHECK ADD  CONSTRAINT [FK_WorkQueues_Executions1] FOREIGN KEY([ExecutionId])
REFERENCES [dbo].[Executions] ([ExecutionId])
GO
ALTER TABLE [dbo].[TaskLists] CHECK CONSTRAINT [FK_WorkQueues_Executions1]
GO
ALTER TABLE [dbo].[Variables]  WITH CHECK ADD  CONSTRAINT [FK_Variables_Executions] FOREIGN KEY([ExecutionId])
REFERENCES [dbo].[Executions] ([ExecutionId])
GO
ALTER TABLE [dbo].[Variables] CHECK CONSTRAINT [FK_Variables_Executions]
GO
ALTER TABLE [dbo].[Workers]  WITH CHECK ADD  CONSTRAINT [FK_Workers_Servers] FOREIGN KEY([ServerName])
REFERENCES [dbo].[Servers] ([Name])
GO
ALTER TABLE [dbo].[Workers] CHECK CONSTRAINT [FK_Workers_Servers]
GO
