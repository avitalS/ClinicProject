use ClinicDB
---Function---
--1
create function GetAge(@id int)
returns int
as begin
declare @age int
select @age=DATEDIFF(year,DateOfBirth,GETDATE())from Clients
where ClientId=@id
return @age
end
select dbo.GetAge(121212121)

--2
CREATE FUNCTION GetDoctorsbySpecialization(@spec nvarchar(50)) 
RETURNS TABLE 
AS 
RETURN 
( 
    SELECT * FROM Doctors WHERE Specialization = @spec 
)
select * from dbo.GetDoctorsbySpecialization('Eyes')

--3
create function GetLastVisitToClient(@id int)
returns date
as begin
declare @date date
select top 1 @date= DateAndHour from Visits
where ClientId=@id
order by DateAndHour desc
return @date
end
select dbo.GetLastVisitToClient(121212121)

---procedures---
--4
drop procedure GetCountVisitToDoctor
create procedure GetCountVisitToDoctor(@id int)
as begin
select d.FirstName,d.LastName,d.Specialization,count(v.visitid) as CountVisits
from Doctors d join Visits v
on d.DoctorId=v.DoctorId
where d.DoctorId=@id
group by d.FirstName,d.LastName,d.Specialization
end

exec GetCountVisitToDoctor 333333333

--5 
create procedure GetDoctorsByCity(@city nvarchar(50))
as begin
select * from Doctors
where City=@city
end

exec getDoctorsByCity 'London'

--6--העיר עם הכי הרבה מקרים דחופים וכמות המקרים לתאריך מסוים
create procedure GetPriorityCity(@date date, @cnt int output,@city nvarchar(50) output)
as begin
select top 1 @city = d.City ,@cnt=count(*)
from Visits v JOIN Doctors d 
on v.DoctorId = d.DoctorId 
WHERE CAST(v.DateAndHour AS DATE) = @date
AND v.Priority = 3
group by d.City
order by COUNT(v.VisitId) desc
end

DECLARE @out_cnt INT;
DECLARE @out_city NVARCHAR(50);

EXEC GetPriorityCity @date = '2026-12-25',@cnt = @out_cnt OUTPUT, @city = @out_city OUTPUT;
SELECT @out_cnt AS [Count], @out_city AS [City];

---View---
create view AvgHourToDoctorInMonth as
select d.DoctorId, FirstName,LastName, avg(Duration) as AvgDuration
from Doctors d join Visits v
on v.DoctorId = d.DoctorId
where month(v.DateAndHour)=month(GETDATE())
and year(v.DateAndHour)=year(GETDATE())
group by d.DoctorId, FirstName,LastName

select * from AvgHourToDoctorInMonth



