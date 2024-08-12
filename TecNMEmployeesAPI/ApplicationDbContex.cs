using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TecNMEmployeesAPI.Entities;

namespace TecNMEmployeesAPI
{


    public class ApplicationDbContext : IdentityDbContext
    {

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NoticeEmployee>()
                    .HasKey(ne => new { ne.NoticeId, ne.EmployeeId });


            modelBuilder.Entity<Period>()
                    .Property("IsCurrent")
                    .HasDefaultValue(false);

            modelBuilder.Entity<WorkPermit>()
                   .Property("IsActive")
                   .HasDefaultValue(false);

        }





        public DbSet<Period> Periods { get; set; }
        public DbSet<StaffType> StaffTypes { get; set; }
        public DbSet<ContractType> ContractTypes { get; set; }
        public DbSet<Degree> Degrees { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<GeneralNotice> GeneralNotices { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<WorkStation> WorkStations { get; set; }
        public DbSet<NonWorkingDay> NonWorkingDays { get; set; }
        public DbSet<Permit> Permits { get; set; }
        public DbSet<WorkPermit> WorkPermits { get; set; }
        public DbSet<Time> Times { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }



        public DbSet<NoticeEmployee> NoticesEmployees { get; set; }


        public DbSet<UI> UIs { get; set; }


    }


}
