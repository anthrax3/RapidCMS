using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using RapidCMS.Core.Abstractions.Config;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Repositories;
using RapidCMS.Core.Exceptions;
using RapidCMS.Core.Extensions;

namespace RapidCMS.Core.Models.Config
{
    internal class CmsConfig : ICmsConfig
    {
        internal CmsAdvancedConfig AdvancedConfig { get; set; } = new CmsAdvancedConfig
        {
            SemaphoreCount = 1
        };
        internal string SiteName { get; set; } = "RapidCMS";
        internal bool IsDevelopment { get; set; }
        internal bool AllowAnonymousUsage { get; set; } = false;

        public string Alias => "__root";

        internal static List<string> CollectionAliases = new List<string>();

        internal CustomTypeRegistrationConfig? CustomLoginScreenRegistration { get; set; }
        internal CustomTypeRegistrationConfig? CustomLoginStatusRegistration { get; set; }

        public IEnumerable<ICollectionConfig> Collections => CollectionsAndPages.SelectNotNull(x => x.collection);
        public IEnumerable<IPageConfig> Pages => CollectionsAndPages.SelectNotNull(x => x.page);

        public List<(ICollectionConfig? collection, IPageConfig? page)> CollectionsAndPages { get; set; } = new List<(ICollectionConfig?, IPageConfig?)>
        {
            (default, new PageConfig("Dashboard", "apps"))
        };

        public IAdvancedCmsConfig Advanced => AdvancedConfig;

        public IPageConfig Dashboard => CollectionsAndPages.First().page!;

        public ICmsConfig SetCustomLoginScreen(Type loginType)
        {
            if (!loginType.IsSameTypeOrDerivedFrom(typeof(ComponentBase)))
            {
                throw new InvalidOperationException($"{nameof(loginType)} must be derived of {nameof(ComponentBase)}.");
            }

            CustomLoginScreenRegistration = new CustomTypeRegistrationConfig(loginType);

            return this;
        }

        public ICmsConfig SetCustomLoginStatus(Type loginType)
        {
            if (!loginType.IsSameTypeOrDerivedFrom(typeof(ComponentBase)))
            {
                throw new InvalidOperationException($"{nameof(loginType)} must be derived of {nameof(ComponentBase)}.");
            }

            CustomLoginStatusRegistration = new CustomTypeRegistrationConfig(loginType);

            return this;
        }

        public ICmsConfig SetSiteName(string siteName)
        {
            SiteName = siteName;

            return this;
        }

        public ICmsConfig AllowAnonymousUser()
        {
            AllowAnonymousUsage = true;

            return this;
        }

        public ICollectionConfig<TEntity> AddCollection<TEntity, TRepository>(string alias, string name, Action<ICollectionConfig<TEntity>> configure)
            where TEntity : class, IEntity
            where TRepository : IRepository
        {
            return AddCollection<TEntity, TRepository>(alias, default, name, configure);
        }

        public ICollectionConfig<TEntity> AddCollection<TEntity, TRepository>(string alias, string? icon, string name, Action<ICollectionConfig<TEntity>> configure)
            where TEntity : class, IEntity
            where TRepository : IRepository
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            if (alias != alias.ToUrlFriendlyString())
            {
                throw new ArgumentException($"Use lowercase, hyphened strings as alias for collections, '{alias.ToUrlFriendlyString()}' instead of '{alias}'.");
            }
            if (CollectionAliases.Contains(alias))
            {
                throw new NotUniqueException(nameof(alias));
            }

            CollectionAliases.Add(alias);

            var configReceiver = new CollectionConfig<TEntity>(
                alias,
                icon,
                name,
                typeof(TRepository),
                new EntityVariantConfig(typeof(TEntity).Name, typeof(TEntity)));

            configure.Invoke(configReceiver);

            CollectionsAndPages.Add((configReceiver, default));

            return configReceiver;
        }

        public ICmsConfig AddPage(string name, Action<IPageConfig> configure)
        {
            return AddPage("document", name, configure);
        }

        public ICmsConfig AddPage(string icon, string name, Action<IPageConfig> configure)
        {
            if (Pages.Any(x => x.Name == name))
            {
                throw new InvalidOperationException($"Page with name {name} already exists.");
            }

            var page = new PageConfig(name, icon);

            configure.Invoke(page);

            CollectionsAndPages.Add((default, page));

            return this;
        }
    }
}
