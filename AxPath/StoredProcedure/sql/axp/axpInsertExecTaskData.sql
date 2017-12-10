CREATE PROCEDURE axpInsertExecTaskData
	-- Add the parameters for the stored procedure here
    @EXECTASKDATAID_VAL varchar(50),
	@CREATETIME_VAL bigint,
	@PROGID_VAL varchar(50),
	@RESULTDATA_VAL nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    Insert into AXAEXECTASKDATA(EXECTASKDATAID,CREATETIME,PROGID,RESULTDATA) 
    values(@EXECTASKDATAID_VAL,@CREATETIME_VAL,@PROGID_VAL,@RESULTDATA_VAL);

END


