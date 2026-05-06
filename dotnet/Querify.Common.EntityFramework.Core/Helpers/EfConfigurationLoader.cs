using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Querify.Common.EntityFramework.Core.Helpers;

public class EfConfigurationLoader<T> where T : class
{
    public void LoadFromNameSpace(ModelBuilder modelBuilder, string @namespace)
    {
        //Get all config types the given namespace
        var typesToRegister = typeof(T).Assembly.GetTypes()
            .Where(type => !String.IsNullOrEmpty(type.Namespace)
                           && type.MemberType != MemberTypes.NestedType
                           && type.Namespace == @namespace).ToList();

        foreach (var type in typesToRegister)
        {
            dynamic configInstance = Activator.CreateInstance(type)!;
            modelBuilder.ApplyConfiguration(configInstance);
        }
    }
}