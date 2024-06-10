namespace TecNMEmployeesAPI.DTOs
{
    public class EmployeeDTO
    {


        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Gender { get; set; }
        public string RFC { get; set; }
        public string CURP { get; set; }
        public string Email { get; set; }
        public ContractTypeDTO ContractType { get; set; }
        public StaffTypeDTO StaffType { get; set; }
        public DegreeDTO Degree { get; set; }
        public DateTime RecruitmentDate { get; set; }
        public DateTime BirthdayDate { get; set; }
        public string CardCode { get; set; }
        public string Picture { get; set; }
        public string Finger { get; set; }
        public string AnotherFinger { get; set; }
        public Boolean IsActive { get; set; }
        
        public int TotalHours { get; set; }

        public DepartmentDTO Department { get; set; }
        public WorkStationDTO WorkStation { get; set; }


    }
}
