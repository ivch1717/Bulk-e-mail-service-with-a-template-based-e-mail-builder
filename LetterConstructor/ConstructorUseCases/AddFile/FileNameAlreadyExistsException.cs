namespace ConstructorUseCases.AddFile;

public sealed class FileNameAlreadyExistsException : Exception
{
    public FileNameAlreadyExistsException(string name)
        : base($"Файл с именем '{name}' уже существует")
    {
    }
}
