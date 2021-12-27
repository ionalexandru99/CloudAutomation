using Microsoft.Azure.Management.Network.Fluent.NetworkSecurityGroup.Definition;

namespace CloudAutomation.Resources.VirtualMachine.Extensions
{
    public static class NetworkSecurityGroupsExtensions
    {
        public static IWithCreate DefineRule(
            this IWithCreate networkSecurityGroup,
            string name, 
            int port,
            int priority)
        {
            return networkSecurityGroup
                .DefineRule(name)
                .AllowInbound()
                .FromAnyAddress()
                .FromAnyPort()
                .ToAnyAddress()
                .ToPort(port)
                .WithAnyProtocol()
                .WithPriority(priority)
                .Attach();
        }
    }
}