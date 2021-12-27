namespace CloudAutomation.Application.Interfaces.Encryption
{
    public interface IEncryption
    {
        string Encrypt(string value);
        string Decrypt(string value);
    }
}