namespace UseCases.SmtpProfile.DeleteSmtpProfile;

public interface IDeleteSmtpProfileRequestHandler
{
    public Task<bool> HandleAsync(Guid profileId);
}