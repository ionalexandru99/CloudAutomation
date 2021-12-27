namespace CloudAutomation.Resources.VirtualMachine.Processors.Interfaces
{
    public interface IPasswordProcessor
    {
        string GeneratePassword(
            bool useLowercase,
            bool useUppercase,
            bool useNumbers,
            bool useSpecial,
            int passwordSize);
    }
}