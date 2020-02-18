using System;
using System.Collections.Generic;
using System.Linq;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Abstractions.Setup;
using RapidCMS.Core.Extensions;
using RapidCMS.Core.Helpers;
using RapidCMS.Core.Models.Config;

namespace RapidCMS.Core.Models.Setup
{
    internal class CmsSetup : ICms, ICollectionResolver, ILogin
    {
        private Dictionary<string, CollectionSetup> _collectionMap { get; set; } = new Dictionary<string, CollectionSetup>();

        internal CmsSetup(CmsConfig config)
        {
            SiteName = config.SiteName;
            IsDevelopment = config.IsDevelopment;

            Collections = ConfigProcessingHelper.ProcessCollections(config);

            Pages = config.Pages.ToList(x => new PageRegistrationSetup(x));
            if (config.CustomLoginScreenRegistration != null)
            {
                CustomLoginScreenRegistration = new CustomTypeRegistrationSetup(config.CustomLoginScreenRegistration);
            }
            if (config.CustomLoginStatusRegistration != null)
            {
                CustomLoginStatusRegistration = new CustomTypeRegistrationSetup(config.CustomLoginStatusRegistration);
            }

            MapCollections(Collections);

            void MapCollections(IEnumerable<CollectionSetup> collections)
            {
                foreach (var collection in collections.Where(col => !col.Recursive))
                {
                    if (!_collectionMap.TryAdd(collection.Alias, collection))
                    {
                        throw new InvalidOperationException($"Duplicate collection alias '{collection.Alias}' not allowed.");
                    }

                    if (collection.Collections.Any())
                    {
                        MapCollections(collection.Collections);
                    }
                }
            }
        }

        internal string SiteName { get; set; }
        internal bool IsDevelopment { get; set; }

        public List<CollectionSetup> Collections { get; set; }
        internal List<PageRegistrationSetup> Pages { get; set; }
        internal CustomTypeRegistrationSetup? CustomLoginScreenRegistration { get; set; }
        internal CustomTypeRegistrationSetup? CustomLoginStatusRegistration { get; set; }

        string ICms.SiteName => SiteName;
        bool ICms.IsDevelopment
        {
            get => IsDevelopment;
            set => IsDevelopment = value;
        }
        
        ICollectionSetup ICollectionResolver.GetCollection(string alias)
        {
            return _collectionMap.FirstOrDefault(x => x.Key == alias).Value
                ?? throw new InvalidOperationException($"Failed to find collection with alias {alias}.");
        }

        IPageSetup ICollectionResolver.GetPage(string alias)
        {
            return Pages.First(x => x.Alias == alias);
        }

        IEnumerable<(ICollectionSetup, IPageSetup)> ICollectionResolver.GetRootCollections()
        {
            return Collections.Select(x => (x as ICollectionSetup, default(IPageSetup)))!
                .Union(Pages.Skip(1).Select(x => (default(ICollectionSetup), x as IPageSetup))!);
        }

        ITypeRegistration? ILogin.CustomLoginScreenRegistration => CustomLoginScreenRegistration;
        ITypeRegistration? ILogin.CustomLoginStatusRegistration => CustomLoginStatusRegistration;
    }
}
