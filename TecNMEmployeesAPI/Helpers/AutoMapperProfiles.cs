using AutoMapper;
using Microsoft.AspNetCore.Identity;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using System.Security.Claims;

namespace TecNMEmployeesAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {


        public AutoMapperProfiles()
        {

            CreateMap<IncidentDTO, Incident>().ReverseMap();
            CreateMap<IncidentCreateDTO, Incident>().ReverseMap();
            CreateMap<Incident, IncidentDTO>().ReverseMap();

            // Crear Mapa de Genre hasta GenreDTO y viseversa
            // Convertir los datos de tipo Genre a un tipo GenreDTO y viseversa
            CreateMap<Period, PeriodDTO>().ReverseMap();

            // Crear Mapa de GenreCreateDTO hasta Genre
            // Convertir los datos de tipo GenreCreateDTO a un tipo Genre
            CreateMap<PeriodCreateDTO, Period>();



            CreateMap<StaffType, StaffTypeDTO>().ReverseMap();
            CreateMap<StaffTypeCreateDTO, StaffType>();



            CreateMap<ContractType, ContractTypeDTO>().ReverseMap();
            CreateMap<ContractTypeCreateDTO, ContractType>();


            CreateMap<Degree, DegreeDTO>().ReverseMap();
            CreateMap<DegreeCreateDTO, Degree>();


            CreateMap<GeneralNotice, GeneralNoticeDTO>().ReverseMap();
            CreateMap<GeneralNoticeCreateDTO, GeneralNotice>();



            CreateMap<Employee, EmployeeDTO>().ReverseMap();
            CreateMap<EmployeeCreateDTO, Employee>();
            CreateMap<Employee, EmployeeWithoutDetailsDTO>();
            CreateMap<Employee, EmployeeForDepartmentDTO>();
            CreateMap<EmployeePatchDTO, Employee>().ReverseMap();



            CreateMap<Department, DepartmentDTO>().ReverseMap();
            CreateMap<DepartmentCreateDTO, Department>();

            CreateMap<WorkStation, WorkStationDTO>().ReverseMap();
            CreateMap<WorkStationCreateDTO, WorkStation>();


            CreateMap<Notice, NoticeDTO>()
                .ForMember(notice => notice.Employees, opt => opt.MapFrom(MapNoticeDTOEmployees));
            CreateMap<NoticeCreateDTO, Notice>()
              .ForMember(notice => notice.NoticeEmployee, opt => opt.MapFrom(MapNoticesEmployees));


            CreateMap<Permit, PermitDTO>();
            CreateMap<PermitCreateDTO, Permit>();


            CreateMap<WorkPermit, WorkPermitDTO>();
            CreateMap<WorkPermitCreateDTO, WorkPermit>();


            CreateMap<Station, StationDTO>().ReverseMap();
            CreateMap<StationCreateDTO, Station>();

            CreateMap<NonWorkingDay, NonWorkingDayDTO>().ReverseMap();
            CreateMap<NonWorkingDayCreateDTO, NonWorkingDay>();

            CreateMap<WorkSchedule, WorkScheduleDTO>().ReverseMap();
            CreateMap<WorkScheduleCreateDTO, WorkSchedule>();
             

            CreateMap<Attendance, AttendanceDTO>().ReverseMap();
            CreateMap<AttendanceCreateDTO, Attendance>();
            CreateMap<AttendanceCreateDateRequiredDTO, Attendance>();


            CreateMap<AttendanceLog, AttendanceLogDTO>().ReverseMap();
            CreateMap<AttendanceLogCreateDTO, AttendanceLog>();


            CreateMap<Time, TimeDTO>().ReverseMap();
            CreateMap<TimeCreateDTO, Time>();


            CreateMap<UI, UiDTO>().ReverseMap();
            CreateMap<UiUpdateDTO, UI>().ReverseMap();


            CreateMap<IdentityUser, UserDTO>();


            CreateMap<Claim, RoleDetailDTO>().ReverseMap();

        }


       


        private List<EmployeeWithoutDetailsDTO> MapNoticeDTOEmployees(Notice notice, NoticeDTO noticeDto)
        {
            var result = new List<EmployeeWithoutDetailsDTO>();


            if (notice.NoticeEmployee == null) { return result; }


            foreach (var noticeEmployee in notice.NoticeEmployee)
            {
                result.Add(new EmployeeWithoutDetailsDTO()
                {
                    Id = noticeEmployee.EmployeeId,
                    Name = noticeEmployee.Employee.Name,
                    Lastname = noticeEmployee.Employee.Lastname,
                    Picture = noticeEmployee.Employee.Picture,
                    CardCode = noticeEmployee.Employee.CardCode
                });
            }

            return result;

        }




        private List<NoticeEmployee> MapNoticesEmployees(NoticeCreateDTO noticeCreateDTO, Notice notice)
        {
            var result = new List<NoticeEmployee>();

            if (noticeCreateDTO.EmployeesIds == null)
            {
                return result;
            }


            foreach (var employeeId in noticeCreateDTO.EmployeesIds)
            {
                result.Add(new NoticeEmployee() { EmployeeId = employeeId });
            }

            return result;
        }


    }
}
