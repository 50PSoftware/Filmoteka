delimiter $$
create procedure years(miny int, maxy int)
begin
set @year = miny;
while @year <= maxy do
set @count = (select count(*) from rok where rok = @year);
if @count = 0 then
insert into rok(rok) values(@year);
end if;
set @year = @year + 1;
end while;
end $$
