using System.ComponentModel.DataAnnotations;

namespace kolWebApp.Dtos;

public class DoctorDto
{
    [MaxLength(100)]
    public string FirstName { get; set; }
    [MaxLength(100)]
    public string LastName { get; set; }
    [EmailAddress]
    public string email { get; set; }
}