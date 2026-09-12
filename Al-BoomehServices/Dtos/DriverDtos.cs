namespace Al_BoomehDAL.Classes
{
    public class CreateDriverDTO
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public enGendor Gendor { get; set; }
        public enVehicleType VehicleType { get; set; }
    }

    public class UpdateDriverInfoDTO
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public enVehicleType VehicleType { get; set; }
    }

    public class DriverInfoDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public enGendor Gendor { get; set; }
        public enVehicleType VehicleType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DriverIssueDTO
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public int IssueId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
