using System.Collections.Generic;
using RapidCMS.Core.Abstractions.Setup;

namespace RapidCMS.Core.Abstractions.Resolvers
{
    internal interface ICollectionResolver
    {
        ICollectionSetup GetCollection(string alias);
        IPageSetup GetPage(string alias);
        IEnumerable<(ICollectionSetup collection, IPageSetup page)> GetRootCollections();
    }
}
