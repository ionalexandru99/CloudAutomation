using System;
using CloudAutomation.Resources.VirtualMachine.Processors.Interfaces;

namespace CloudAutomation.Resources.VirtualMachine.Processors
{
    public class PasswordProcessor : IPasswordProcessor
    {
        private static string LowerCase { get; } = "abcdefghijklmnopqursuvwxyz";
        private static string UpperCase { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static string Numbers { get; } = "123456789";
        private static string Specials { get; } = @"!@£$%^&*()#€";

        public string GeneratePassword(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial, int passwordSize)
        {
            var password = new char[passwordSize];
            var charSet = string.Empty; 
            var random = new Random();
            int counter;

            if (useLowercase) charSet += LowerCase;

            if (useUppercase) charSet += UpperCase;

            if (useNumbers) charSet += Numbers;

            if (useSpecial) charSet += Specials;

            for (counter = 0; counter < passwordSize; counter++)
            {
                password[counter] = charSet[random.Next(charSet.Length - 1)];

                switch (counter)
                {
                    case 0 when useUppercase:
                        password[counter] = UpperCase[random.Next(UpperCase.Length - 1)];
                        break;
                    case 1 when useLowercase:
                        password[counter] = LowerCase[random.Next(LowerCase.Length - 1)];
                        break;
                    case 2 when useSpecial:
                        password[counter] = Specials[random.Next(Specials.Length - 1)];
                        break;
                    case 3 when useNumbers:
                        password[counter] = Numbers[random.Next(Numbers.Length - 1)];
                        break;
                }
            }
            
            return string.Join(null, password);
        }
    }
}