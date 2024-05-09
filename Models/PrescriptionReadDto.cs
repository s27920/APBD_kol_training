namespace kolWebApp.MOdels;

public class Prescription
{
    public int IdPrescription { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    
}