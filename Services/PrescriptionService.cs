using kolWebApp.Exceptions;
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
        return await _repository.GetAllPrescriptionsAsync(null);
    }

    public async Task<IEnumerable<Prescription>> GetDoctorsPrescriptionsAsync(int id)
    {
        if (!_repository.CheckIfDoctorExistsAsync(id).Result)
        {
            throw new NotFoundException("No doctor with such Id found");
        }
        return await _repository.GetAllPrescriptionsAsync(id);
    }
}