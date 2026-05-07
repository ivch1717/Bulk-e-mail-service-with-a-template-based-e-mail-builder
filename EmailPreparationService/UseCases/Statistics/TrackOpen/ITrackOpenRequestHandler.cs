namespace UseCases.Statistics.TrackOpen;

public interface ITrackOpenRequestHandler
{
    Task<byte[]> HandleAsync(TrackOpenRequest request);
}