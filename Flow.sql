CREATE DATABASE [Flow]
GO
USE [Flow]
GO
/****** Object:  Table [dbo].[Activities]    Script Date: 08/10/2015 15:42:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Activities](
	[Name] [nvarchar](40) NOT NULL,
	[Version] [bigint] NOT NULL,
	[Json] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Activities] PRIMARY KEY CLUSTERED 
(
	[Name] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Executions]    Script Date: 08/10/2015 15:42:10 ******/
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
	[JobId] [nvarchar](50) NULL,
 CONSTRAINT [PK_Executions] PRIMARY KEY CLUSTERED 
(
	[ExecutionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExecutionStates]    Script Date: 08/10/2015 15:42:10 ******/
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
/****** Object:  Table [dbo].[Histories]    Script Date: 08/10/2015 15:42:10 ******/
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
/****** Object:  Table [dbo].[Servers]    Script Date: 08/10/2015 15:42:10 ******/
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
/****** Object:  Table [dbo].[TaskLists]    Script Date: 08/10/2015 15:42:10 ******/
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
 CONSTRAINT [PK_WorkQueues] PRIMARY KEY CLUSTERED 
(
	[TaskToken] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Users]    Script Date: 08/10/2015 15:42:10 ******/
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
/****** Object:  Table [dbo].[Variables]    Script Date: 08/10/2015 15:42:10 ******/
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
/****** Object:  Table [dbo].[Workers]    Script Date: 08/10/2015 15:42:10 ******/
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
/****** Object:  Table [dbo].[Workflows]    Script Date: 08/10/2015 15:42:10 ******/
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
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'asynch', 1000000000000, N'{"objtype":"activity","name":"asynch","version":"1.0.0.0","description":"Wait for a bit, asynchronously","defaultTaskList":"asynch","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"delay":{"description":"Time to waste in milliseconds","type":"int","required":false}, "signalName":{"description":"Name of signal to send on completion","type":"string","required":true}},"outputs":{}}')
GO
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'demo', 1000000000000, N'{"objtype":"activity","name":"demo","version":"1.0.0.0","description":"Wait for a bit","defaultTaskList":"demo","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"delay":{"description":"Time to waste in milliseconds","type":"int","required":false}},"outputs":{"result":{"description":"output","type":"int"}}}')
GO
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'javascript', 1000000000000, N'{"objtype":"activity","name":"javascript","version":"1.0.0.0","description":"Run JavaScript","defaultTaskList":"javascript","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"$script":{"description":"script to execute","type":"javascript","required":true}},"outputs":{}}')
GO
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'partialCopy', 1000000000000, N'{"objtype":"activity","name":"partialCopy","version":"1.0.0.0","description":"Partial copy of clip","defaultTaskList":"partialCopy","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"source":{"description":"Path to source media","type":"string","required":true},"inpoint":{"type":"timecode","required":false,"default":"00:00:00:00"},"outpoint":{"type":"timecode","required":false,"default":"00:00:00:00"}},"outputs":{"destination":{"description":"path to output media","type":"string"},"frob":{"description":"arbitrary variable name","type":"string"}}}')
GO
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'quantelExport', 1000000000000, N'{"objtype":"activity","name":"quantelExport","version":"1.0.0.0","description":"Export media from quantel","defaultTaskList":"quantel","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"destination":{"type":"string","required":true},"profileName":{"type":"string","required":true},"renameRuleText":{"type":"string","required":true},"source":{"type":"string","required":true}},"outputs":{"job":{"description":"job details","type":"object"}}}')
GO
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'quantelJobProgress', 1000000000000, N'{"objtype":"activity","name":"quantelJobProgress","version":"1.0.0.0","description":"Check job progress on quantel","defaultTaskList":"quantel","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"jobId":{"type":"string","required":true}},"outputs":{"progress":{"description":"Percent complete","type":"integer"},"complete":{"description":"True if complete","type":"bool"}}}')
GO
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'test', 1000000000000, N'{"objtype":"activity","name":"test","version":"1.0.0.0","description":"Wait for a bit","defaultTaskList":"demo","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{},"outputs":{}}')
GO
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'transcode', 1000000000000, N'{"objtype":"activity","name":"transcode","version":"1.0.0.0","description":"Transcode clip","defaultTaskList":"transcode","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"source":{"description":"Path to source media","type":"string","required":true},"outputFormat":{"description":"output media format","type":"string","required":false,"default":"WMV"},"extra":{"description":"another variable","type":"string","required":false,"default":"WMV"}},"outputs":{"destination":{"description":"path to output media","type":"string"}}}')
GO
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'updateJob', 1000000000000, N'{"objtype":"activity","name":"updateJob","version":"1.0.0.0","description":"Update transman job","defaultTaskList":"updater","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"updateType":{"type":"string","required":true},"jobId":{"type":"integer","required":true},"message":{"type":"string","required":true},"progress":{"type":"integer","default":0},"destination":{"type":"string"}},"outputs":{}}')
GO
INSERT [dbo].[Activities] ([Name], [Version], [Json]) VALUES (N'wait', 1000000000000, N'{"objtype":"activity","name":"wait","version":"1.0.0.0","description":"Wait for a signal","defaultTaskList":"wait","defaultTaskScheduleToCloseTimeout":300,"defaultTaskScheduleToStartTimeout":150,"defaultTaskStartToCloseTimeout":450,"defaultTaskHeartbeatTimeout":null,"inputs":{"signalName":{"description":"Name of signal to wait for","type":"string","required":true}},"outputs":{}}')
GO
INSERT [dbo].[Workflows] ([Name], [Version], [Description], [Json]) VALUES (N'Adder', 1000000000000, N'Adder', N'{"objtype":"workflow","name":"Adder","version":"1.0.0.0","decisionList":"decider","defaultTaskStartToCloseTimeout":null,"variables":{"x":{"type":"any","path":"$.x","required":true},"y":{"type":"integer","path":"$.y","required":true},"sum":{"lit":"0","type":"integer","required":false},"product":{"lit":"1","type":"integer","required":false}},"tasks":[{"taskId":"start","activityName":"start","activityVersion":"0.0.0.0","outflows":[{"name":"Out","target":"add","targetPin":"In","route":[180,140,250,140]}],"taskPriority":"0","symbol":{"name":"start","label":"Start","style":"circle","locationX":100,"locationY":100}},{"taskId":"end","activityName":"end","activityVersion":"0.0.0.0","outflows":[],"taskPriority":"0","symbol":{"name":"end","label":"End","style":"circle","locationX":610,"locationY":100}},{"taskId":"add","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"result.sum = x + y;","type":"any","required":true,"description":"script to execute","userDefined":false},"x":{"var":"x","type":"integer","required":true,"userDefined":true},"y":{"var":"y","type":"integer","required":true,"userDefined":true}},"outputs":{"sum":{"var":"sum","type":"string","description":"script output","userDefined":true}},"outflows":[{"name":"Out","target":"double","targetPin":"In","route":[370,140,440,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"javascript","label":"Script","style":"box","locationX":250,"locationY":100}},{"taskId":"double","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"result.product = sum * 2;","type":"any","required":true,"description":"script to execute","userDefined":false},"sum":{"var":"sum","type":"integer","required":true,"userDefined":true}},"outputs":{"product":{"var":"product","type":"string","description":"script output","userDefined":true}},"outflows":[{"name":"Out","target":"end","targetPin":"In","route":[560,140,610,140]}],"failOutflow":{"name":"Err"},"symbol":{"name":"javascript","label":"Script","style":"box","locationX":440,"locationY":100}}]}')
GO
INSERT [dbo].[Workflows] ([Name], [Version], [Description], [Json]) VALUES (N'AdderAsynch', 2000000000000, NULL, N'{"objtype":"workflow","name":"AdderAsynch","version":"2.0.0.0","tasklist":"decider","defaultTaskStartToCloseTimeout":null,"variables":{"x":{"type":"any","path":"$.x","required":true},"y":{"type":"integer","path":"$.y","required":true},"sum":{"lit":"0","type":"integer","required":false},"product":{"lit":"1","type":"integer","required":false}},"tasks":[{"taskId":"start","activityName":"start","activityVersion":"0.0.0.0","outflows":[{"name":"Out","target":"d95d2ad6","targetPin":"In","route":[180,140,210,140]}],"taskPriority":"0","symbol":{"name":"start","label":"Start","style":"circle","locationX":100,"locationY":100}},{"taskId":"end","activityName":"end","activityVersion":"0.0.0.0","outflows":[],"taskPriority":"0","symbol":{"name":"end","label":"End","style":"circle","locationX":850,"locationY":100}},{"taskId":"add","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"result.sum = x + y;","type":"any","required":true,"description":"script to execute","userDefined":false},"x":{"var":"x","type":"integer","required":true,"userDefined":true},"y":{"var":"y","type":"integer","required":true,"userDefined":true}},"outputs":{"sum":{"var":"sum","type":"string","description":"script output","userDefined":true}},"outflows":[{"name":"Out","target":"double","targetPin":"In","route":[490,140,540,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"javascript","label":"Script","style":"box","locationX":370,"locationY":100}},{"taskId":"double","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"result.product = sum * 2;","type":"any","required":true,"description":"script to execute","userDefined":false},"sum":{"var":"sum","type":"integer","required":true,"userDefined":true}},"outputs":{"product":{"var":"product","type":"string","description":"script output","userDefined":true}},"outflows":[{"name":"Out","target":"743cafd2","targetPin":"In","route":[660,140,700,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"javascript","label":"Script","style":"box","locationX":540,"locationY":100}},{"taskId":"d95d2ad6","activityName":"asynch","activityVersion":"1.0.0.0","inputs":{"delay":{"lit":30000,"type":"integer","required":false,"description":"Time to waste in milliseconds","userDefined":false},"signalName":{"lit":"fred","type":"string","required":true,"description":"Name of signal to send on completion","userDefined":false}},"outputs":{},"outflows":[{"name":"Out","target":"add","targetPin":"In","route":[330,140,370,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"asynch","label":"Asynch","style":"box","locationX":210,"locationY":100}},{"taskId":"743cafd2","activityName":"wait","activityVersion":"1.0.0.0","inputs":{"signalName":{"lit":"fred","type":"string","required":true,"description":"Name of signal to wait for","userDefined":false}},"outputs":{},"outflows":[{"name":"Out","target":"end","targetPin":"In","route":[820,140,850,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"wait","label":"Wait","style":"box","locationX":700,"locationY":100}}]}')
GO
INSERT [dbo].[Workflows] ([Name], [Version], [Description], [Json]) VALUES (N'SampleWorkflow', 6000000000000, N'Sample Workflow', N'{"objtype":"workflow","name":"SampleWorkflow","version":"6.0.0.0","decisionList":"decider","defaultExecutionStartToCloseTimeout":"3000","defaultTaskStartToCloseTimeout":"1000","inputSchema":"SampleSchema","inputSchemaVersion":"1.0.0.0","variables":{"ensoQuantelProfileName":{"lit":"","type":"any","required":false},"ensoQuantelClientPort":{"lit":"","type":"integer","required":false},"messageType":{"type":"string","required":false},"jobId":{"path":"body.id","required":true,"description":"The Transman job identifier"},"transman":{"path":"$.body.transman","required":true,"description":"The transman instance that posted the job"},"destinationSharerPath":{"path":"body.destination.sharerpath","required":true},"quantelHostName":{"path":"$.body.source.hostname","required":true},"body":{"path":"$.body","required":true,"description":"The entire workflow start data object"},"mediagridFileName":{"type":"string","path":"$.body.destination.clip","required":true,"description":"The destination file name on the mediagrid"},"mediagridDestination":{"required":false},"quantelSource":{"required":false},"quantelJob":{"lit":"","required":false},"quantelProgressJob":{"type":"object","required":false},"transmanDestination":{"type":"string","required":false}},"tasks":[{"taskId":"start","activityName":"start","activityVersion":"0.0.0.0","outflows":[{"name":"Out","target":"getInputs","targetPin":"In","route":[130,150,180,150]}],"taskPriority":"0","symbol":{"name":"Start","label":"Start","style":"circle","locationX":50,"locationY":110}},{"taskId":"getInputs","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"var format = Xml.XPathGetOne(body.properties, \"//format\");\nvar resolution = Xml.XPathGetOne(body.properties, \"//resolution\");\n\nvar mediagridDestination = String.Format(\"\\\\\\\\{0}{1}{2}\\\\{3}\", \n  body.destination.hostname,\n  body.destination.directory.replace(/\\//g, ''\\\\''),\n  format,\n  resolution); \n\nvar quantelSource = String.Format(\"SQ:{0}::{1}\", body.source.file, body.source.directory); \n\nvar transmanDestination = format + ''\\\\'' + resolution + ''\\\\'' + body.destination.file + ''.'' + format.toLowerCase();\n\nresult.ensoQuantelProfileName = \"Export SD\"; // Environment.GetEnvironmentVariable(\"ensoQuantelProfileName\");\nresult.ensoQuantelClientPort = 8080; // parseInt(Environment.GetEnvironmentVariable(\"ensoQuantelClientPort\"), 10);\nresult.mediagridDestination = mediagridDestination;\nresult.quantelSource = quantelSource;\nresult.transmanDestination = transmanDestination;","type":"javascript","required":true,"userDefined":false},"body":{"var":"body","type":"object","required":true,"userDefined":true}},"outputs":{"ensoQuantelProfileName":{"var":"ensoQuantelProfileName","type":"string","userDefined":true},"ensoQuantelClientPort":{"var":"ensoQuantelClientPort","type":"integer","userDefined":true},"mediagridDestination":{"var":"mediagridDestination","type":"string","userDefined":true},"quantelSource":{"var":"quantelSource","type":"string","userDefined":true},"transmanDestination":{"var":"transmanDestination","type":"any","userDefined":true}},"outflows":[{"name":"Out","target":"doExport","targetPin":"In","route":[300,150,350,150]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"JavaScript","label":"JavaScript","style":"box","locationX":180,"locationY":110}},{"taskId":"end","activityName":"end","activityVersion":"0.0.0.0","taskPriority":"0","symbol":{"name":"End","label":"End","style":"circle","locationX":1260,"locationY":130}},{"taskId":"doExport","activityName":"quantelExport","activityVersion":"1.0.0.0","inputs":{"destination":{"var":"mediagridDestination","type":"any","path":"","required":true,"userDefined":false},"profileName":{"var":"ensoQuantelProfileName","type":"string","required":true,"userDefined":false},"renameRuleText":{"var":"mediagridFileName","type":"string","required":true,"userDefined":false},"source":{"var":"quantelSource","type":"string","required":true,"userDefined":false},"renameRuleType":{"lit":"REPLACE","type":"string","required":false,"userDefined":true},"host":{"var":"quantelHostName","type":"any","required":false,"userDefined":true},"port":{"var":"ensoQuantelClientPort","type":"any","required":false,"userDefined":true}},"outputs":{"QJobResponse":{"var":"quantelJob","type":"object","description":"All job data","userDefined":false}},"outflows":[{"name":"Out","target":"checkProgress","targetPin":"In","route":[470,150,530,150]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"QuantelExport","label":"Quantel Export","style":"box","locationX":350,"locationY":110}},{"taskId":"checkProgress","activityName":"quantelJobProgress","activityVersion":"1.0.0.0","inputs":{"jobId":{"var":"quantelJob","type":"any","path":"$.QJob.id","required":true,"userDefined":false},"host":{"var":"quantelHostName","type":"any","path":"","required":false,"userDefined":true},"port":{"var":"ensoQuantelClientPort","type":"any","required":false,"userDefined":true}},"outputs":{"QJobResponse":{"var":"quantelProgressJob","type":"object","description":"All job data","userDefined":false}},"outflows":[{"name":"Out","target":"updateProgress","targetPin":"In","route":[650,150,710,150]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"QuantelJobProgress","label":"Quantel Job Progress","style":"box","locationX":530,"locationY":110}},{"taskId":"checkComplete","activityName":"javascript","activityVersion":"1.0.0.0","inputs":{"$script":{"lit":"//Console.WriteLine(\"Progress = \" + progress.ToString());\n\nswitch(status) {\n  case \"FAILED\":\n  case \"ABORTED\":\n  case \"CANCELLED\":\n    throw new Error(\"Quantel export \" + status);\n\n  case \"WAITING\":\n  case \"PROCESSING\":\n  case \"ABORTING\":\n  case \"COMPLETED\":\n    break;\n\n  default:\n    throw new Error(\"Unknown status \" + status);\n}\n// Give it 5 seconds to avoid spamming quantel\nThread.Sleep(5000);\nbranch(status === \"COMPLETED\");\n","type":"any","required":true,"description":"script to execute","userDefined":false},"status":{"var":"quantelProgressJob","type":"any","path":"$.QJob.status","required":true,"userDefined":true},"progress":{"var":"quantelProgressJob","type":"integer","path":"$.QJob.progress","required":false,"userDefined":true}},"outputs":{},"outflows":[{"name":"IfFalse","target":"checkProgress","targetPin":"In","route":[1010,130,1030,130,1030,80,510,80,510,150,530,150]},{"name":"IfTrue","target":"updateComplete","targetPin":"In","route":[1010,170,1080,170]}],"failOutflow":{"name":"Err","target":"updateFailed","targetPin":"In","route":[950,190,950,300,1080,300]},"taskPriority":"0","symbol":{"name":"IfElse","label":"If Else","style":"box","locationX":890,"locationY":110}},{"taskId":"updateFailed","activityName":"updateJob","activityVersion":"1.0.0.0","inputs":{"updateType":{"lit":"errored","type":"any","required":true,"userDefined":false},"jobId":{"var":"jobId","type":"integer","required":true,"userDefined":false},"message":{"lit":"Transfer Failed","type":"string","required":true,"userDefined":false},"progress":{"type":"integer","required":false,"default":"0","userDefined":false},"destination":{"type":"string","required":false,"userDefined":false}},"outputs":{},"outflows":[{"name":"Out","target":"frob","targetPin":"In","route":[1200,300,1260,300]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"UpdateJob","label":"Update Job","style":"box","locationX":1080,"locationY":260}},{"taskId":"updateComplete","activityName":"updateJob","activityVersion":"1.0.0.0","inputs":{"updateType":{"lit":"complete","type":"any","required":true,"userDefined":false},"jobId":{"var":"jobId","type":"integer","required":true,"userDefined":false},"message":{"lit":"YEE HARR!!","type":"string","required":true,"userDefined":false},"progress":{"lit":"","type":"integer","required":false,"userDefined":false},"destination":{"var":"transmanDestination","type":"string","required":false,"userDefined":false}},"outputs":{},"outflows":[{"name":"Out","target":"end","targetPin":"In","route":[1200,170,1260,170]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"UpdateJob","label":"Update Job","style":"box","locationX":1080,"locationY":130}},{"taskId":"updateProgress","activityName":"updateJob","activityVersion":"1.0.0.0","inputs":{"updateType":{"lit":"progress","type":"any","required":true,"userDefined":false},"jobId":{"var":"jobId","type":"integer","required":true,"userDefined":false},"message":{"var":"quantelProgressJob","type":"string","path":"$.QJob.status","required":true,"userDefined":false},"progress":{"var":"quantelProgressJob","type":"integer","path":"$.QJob.progress","required":false,"userDefined":false},"destination":{"type":"string","required":false,"userDefined":false}},"outputs":{},"outflows":[{"name":"Out","target":"checkComplete","targetPin":"In","route":[830,150,890,150]}],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"UpdateJob","label":"Update Job","style":"box","locationX":710,"locationY":110}},{"taskId":"frob","activityName":"end","activityVersion":"0.0.0.0","taskPriority":"0","symbol":{"name":"End","label":"End","style":"circle","locationX":1260,"locationY":260}}]}')
GO
INSERT [dbo].[Workflows] ([Name], [Version], [Description], [Json]) VALUES (N'Test', 1000000000000, NULL, N'{"objtype":"workflow","name":"Test","version":"1.0.0.0","decisionList":"decider","defaultTaskStartToCloseTimeout":null,"variables":{},"tasks":[{"taskId":"start","activityName":"start","activityVersion":"0.0.0.0","outflows":[{"name":"Out","target":"delay","targetPin":"In","route":[180,140,260,140]}],"taskPriority":"0","symbol":{"name":"start","label":"Start","style":"circle","locationX":100,"locationY":100}},{"taskId":"delay","activityName":"test","activityVersion":"1.0.0.0","inputs":{},"outputs":{},"outflows":[{"name":"Out","target":"end","targetPin":"In","route":[380,140,460,140]}],"failOutflow":{"name":"Err","targetPin":"In"},"tasklist":"delay","taskPriority":"0","symbol":{"name":"test","label":"Test","style":"box","locationX":260,"locationY":100}},{"taskId":"end","activityName":"end","activityVersion":"0.0.0.0","outflows":[],"failOutflow":{"name":"Err","targetPin":"In"},"taskPriority":"0","symbol":{"name":"end","label":"End","style":"circle","locationX":460,"locationY":100}}]}')
GO
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
