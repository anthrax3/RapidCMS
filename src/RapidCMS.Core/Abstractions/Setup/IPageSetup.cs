using System.Collections.Generic;

namespace RapidCMS.Core.Abstractions.Setup
{
    public interface IPageSetup
    {
        string Icon { get; }
        string Name { get; }
        string Alias { get; }
        List<ITypeRegistration> Sections { get; }
    }
}
