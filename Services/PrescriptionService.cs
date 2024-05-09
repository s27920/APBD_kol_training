using kolWebApp.Repositories;

namespace kolWebApp.Services;
using MOdels;

public interface IPrescriptionService
{
    public Task<IEnumerable<Prescription>> GetAllPrescriptionAsync();
    public Task<IEnumerable<Prescription>> GetDoctorsPrescriptionsAsync(int id);
}

public class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _repository;

    public PrescriptionService(IPrescriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Prescription>> GetAllPrescriptionAsync()
    {
        return await _repository.GetAllPrescriptions(null);
    }

    public async Task<IEnumerable<Prescription>> GetDoctorsPrescriptionsAsync(int id)
    {
        return await _repository.GetAllPrescriptions(id);
    }
}