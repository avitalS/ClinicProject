using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProceduresAndFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. הוספת פונקציית GetAge
            migrationBuilder.Sql(@"
        CREATE OR ALTER FUNCTION GetAge(@id int)
        RETURNS int
        AS BEGIN
            DECLARE @age int
            SELECT @age = DATEDIFF(year, DateOfBirth, GETDATE()) FROM Clients
            WHERE ClientId = @id
            RETURN @age
        END
    ");

            // 2. הוספת פונקציית GetDoctorsbySpecialization
            migrationBuilder.Sql(@"
         CREATE OR ALTER FUNCTION GetDoctorsbySpecialization(@spec nvarchar(50)) 
        RETURNS TABLE 
        AS RETURN 
        ( 
            SELECT * FROM Doctors WHERE Specialization = @spec 
        )
    ");

            // 3. הוספת פונקציית GetLastVisitToClient
            migrationBuilder.Sql(@"
         CREATE OR ALTER FUNCTION GetLastVisitToClient(@id int)
        RETURNS date
        AS BEGIN
            DECLARE @date date
            SELECT TOP 1 @date = DateAndHour FROM Visits
            WHERE ClientId = @id
            ORDER BY DateAndHour DESC
            RETURN @date
        END
    ");

            // 4. הוספת פרוצדורת GetCountVisitToDoctor
            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetCountVisitToDoctor(@id int)
        AS BEGIN
            SELECT d.FirstName, d.LastName, d.Specialization, COUNT(v.visitid) AS CountVisits
            FROM Doctors d JOIN Visits v ON d.DoctorId = v.DoctorId
            WHERE d.DoctorId = @id
            GROUP BY d.FirstName, d.LastName, d.Specialization
        END
    ");

            // 5. הוספת פרוצדורת GetDoctorsByCity
            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetDoctorsByCity(@city nvarchar(50))
        AS BEGIN
            SELECT * FROM Doctors WHERE City = @city
        END
    ");

            // 6. הוספת פרוצדורת GetPriorityCity
            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetPriorityCity(@date date, @cnt int output, @city nvarchar(50) output)
        AS BEGIN
            SELECT TOP 1 @city = d.City, @cnt = COUNT(*)
            FROM Visits v JOIN Doctors d ON v.DoctorId = d.DoctorId 
            WHERE CAST(v.DateAndHour AS DATE) = @date
            AND v.Priority = 3
            GROUP BY d.City
            ORDER BY COUNT(v.VisitId) DESC
        END
    ");

            // 7. הוספת ה-View
            migrationBuilder.Sql(@"
        CREATE OR ALTER VIEW AvgHourToDoctorInMonth AS
        SELECT d.DoctorId, FirstName, LastName, AVG(Duration) AS AvgDuration
        FROM Doctors d JOIN Visits v ON v.DoctorId = d.DoctorId
        WHERE MONTH(v.DateAndHour) = MONTH(GETDATE())
        AND YEAR(v.DateAndHour) = YEAR(GETDATE())
        GROUP BY d.DoctorId, FirstName, LastName
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW AvgHourToDoctorInMonth");
            migrationBuilder.Sql("DROP PROCEDURE GetPriorityCity");
            migrationBuilder.Sql("DROP PROCEDURE GetDoctorsByCity");
            migrationBuilder.Sql("DROP PROCEDURE GetCountVisitToDoctor");
            migrationBuilder.Sql("DROP FUNCTION GetLastVisitToClient");
            migrationBuilder.Sql("DROP FUNCTION GetDoctorsbySpecialization");
            migrationBuilder.Sql("DROP FUNCTION GetAge");
            
        }
    }
}
