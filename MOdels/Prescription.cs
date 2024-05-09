namespace kolWebApp.MOdels;

public class Prescription
{
    public int IdPrescription { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public String? DoctorName { get; set; }
    public String? PatientName { get; set; }
    
}