CREATE PROCEDURE axpInsertApproveDataVersion
	-- Add the parameters for the stored procedure here
    @PROGID_VAL varchar(50),
	@INTERNALID_VAL varchar(50),
	@FROMROWID_VAL int,
	@CREATETIME_VAL bigint,
	@REASONID_VAL varchar(50),
	@VERSIONDATA_VAL nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    Insert into AXPAPPROVEDATAVERSION(PROGID,INTERNALID,FROMROWID,CREATETIME,REASONID,VERSIONDATA) 
    values(@PROGID_VAL,@INTERNALID_VAL,@FROMROWID_VAL,@CREATETIME_VAL,@REASONID_VAL,@VERSIONDATA_VAL);

END


