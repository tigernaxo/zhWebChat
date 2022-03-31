-- 增加 userType 欄位
ALTER TABLE dbo.A_ChatRoomUser ADD userType smallint NULL;
UPDATE dbo.A_ChatRoomUser SET userType =1;
ALTER TABLE dbo.A_ChatRoomUser ALTER COLUMN userType smallint NOT NULL;

ALTER TABLE dbo.A_ChatMsg ADD userType smallint NULL;
UPDATE dbo.A_ChatMsg SET userType =1;
ALTER TABLE dbo.A_ChatMsg ALTER COLUMN userType smallint NOT NULL;