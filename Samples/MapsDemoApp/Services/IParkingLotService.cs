namespace MapsDemoApp.Services
{
    public interface IParkingLotService
    {
        Task<ParkingLot[]> GetAllAsync(CancellationToken ct = default);
    }
}