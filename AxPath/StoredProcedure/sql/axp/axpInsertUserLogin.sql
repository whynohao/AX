CREATE PROCEDURE axpInsertUserLogin
	-- Add the parameters for the stored procedure here
	@UserId varchar(20),
	@CreateTime bigint,
	@HandleType int,
	@FreeTime bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO [axpUserLogin]
           ([UserId]
           ,[CreateTime]
           ,[HandleType]
           ,[FreeTime])
     VALUES
           (@UserId
           ,@CreateTime
           ,@HandleType
           ,@FreeTime)

END
