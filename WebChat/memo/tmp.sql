-- 插入一筆 3 封鎖 1 的聯絡人關係
insert into A_UserRelationShip (userId, userId2, type, createTime)
values(3,1,2,GETDATE())

-- 選出把 1 封鎖的使用者
select userId from A_UserRelationShip where userId2=1 and type=2;

-- 選出使用者 1 提交的清單(1,2,3)中，可以拿到的使用者
SELECT * FROM S10_users 
WHERE 
id IN (1,2,3) AND
id NOT IN
(
    select userId from A_UserRelationShip where userId2=1 and type=2
)

-- 刪除匿名使用者、聊天室並重置pk 
  UPDATE A_ChatRoomUser SET readMsgId=null WHERE roomId>5;
  DELETE FROM A_ChatRoomUser  WHERE roomId>5;
  DELETE FROM A_ChatMsg WHERE roomId > 5;
  DELETE FROM A_ChatRoom WHERE id>5;
  DELETE FROM A_UserAnonymous;
  DBCC CHECKIDENT (A_ChatRoom, RESEED, 5);
  DBCC CHECKIDENT (A_UserAnonymous, RESEED, 1);

-- 把這些 userId 互相加好友
declare @ids table(idx int identity(1,1), id int)

insert into @ids (id)
    select 1 union
    select 3 union
    select 5 union
    select 7 union
    select 8 union
    select 9 union
    select 10 union
    select 12 union
    select 13 union
    select 14 union
    select 15

delete from A_UserRelationShip where 
userId in ( select id from @ids ) OR
userId2 in ( select id from @ids ) 


declare @i int
declare @ii int
declare @cnt int
declare @icnt int
declare @id int
declare @iid int

select @i = min(idx) - 1, @cnt = max(idx) from @ids

while @i < @cnt
begin
     select @i = @i + 1
     select @id = id from @ids where idx = @i

     select @ii = min(idx) - 1, @icnt = max(idx) from @ids
	 while @ii < @icnt
	 begin
	     select @ii = @ii + 1
         select @iid = id from @ids where idx = @ii
		 if(@iid <> @id)
		 insert into A_UserRelationShip (userId, userId2, type) VALUES (@id, @iid, 1)
	 end
end

-- 選出兩位使用私人聊天室
SELECT * FROM A_ChatRoom WHERE type=3 and id in (
select t.roomId from (SELECT * FROM A_ChatRoomUser WHERE status <> 2 AND userId=1) t
INNER JOIN (SELECT * FROM A_ChatRoomUser WHERE status <> 2 AND userId=3) t1 on t.roomId=t1.roomId
)

declare @userId int=3;
-- 選出使用者和封鎖對象所在的私人聊天室
SELECT * FROM A_ChatRoomUser WHERE status=1 
AND userId IN (
	-- 選出使用者封鎖的對象
	SELECT userId2 FROM A_UserRelationShip WHERE userId=@userId AND type=2
)
AND roomId IN (
	-- 選出使用者的 1v1 聊天室
	SELECT id FROM A_ChatRoom WHERE type=3 AND id IN (
		SELECT DISTINCT roomId from A_ChatRoomUser WHERE userId=@userId AND status=1
	)
);

-- 刪除某聊天室
declare @roomId int= 10;
delete from [A_ChatRoomUser] where roomId = @roomId;
delete from [A_ChatRoom] where id = @roomId;
