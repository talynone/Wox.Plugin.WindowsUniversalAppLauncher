using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wox.Plugin.WindowsUniversalAppLauncher
{
    using System.Diagnostics;
    using System.IO;

    using StoreAppLauncher.Models;

    using Wox.Infrastructure;
    using Wox.Plugin;

    public class Main : IPlugin
    {
        private List<PackageInfoEx> _storeItems = new List<PackageInfoEx>();

        private PluginInitContext _pluginInitContext;

        private bool _isLoading = true;
        private bool _iconsProcessed = false;

        private string _defaultIcon;

        public void Init(PluginInitContext pluginInitContext)
        {
            _pluginInitContext = pluginInitContext;
            _defaultIcon = Path.Combine(pluginInitContext.CurrentPluginMetadata.PluginDirectory, "Images\\\\pic.png");

            Task.Run(LoadStoreItems);
        }

        private async Task LoadStoreItems()
        {
            _isLoading = true;

            // Do we really need to late process
            // icon paths? Seems pretty fast already
            var storeItems = await StoreAppLauncher.StoreAppLauncher.GetAppListForCurrentUserAsync();

            _iconsProcessed = true;

            _storeItems = storeItems.ToList();

            _isLoading = false;
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            if (_isLoading)
            {
                return results;                
            }

            foreach (var storeItem in _storeItems)
            {
                var score = Score(storeItem, query.Search);

                if (score <= 0)
                {
                    continue;
                }

                var icoPath = storeItem.FullLogoPath;
                
                // Icon image can't be found
                // default to using plugin icon
                if (string.IsNullOrEmpty(icoPath))
                {
                    if (_iconsProcessed)
                    {
                        storeItem.FullLogoPath = _defaultIcon;
                    }

                    icoPath = _defaultIcon;
                }

                results.Add(new Result()
                {
                    Title = storeItem.DisplayName,
                    SubTitle = storeItem.Description,
                    Score = score,
                    IcoPath = icoPath,
                    Action = (ActionContext e) =>
                    {                        
                        Task.Run(() => LaunchApp(storeItem));
                        return true;
                    }
                });
            }

            return (results.OrderByDescending(o => o.Score)).ToList();           
        }

        private void LaunchApp(PackageInfoEx storeItem)
        {
            try
            {
                StoreAppLauncher.StoreAppLauncher.LaunchApp(storeItem);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        private int Score(PackageInfoEx item, string query)
        {
            var scores = new List<int>();

            if (item.DisplayName != null)
            {
                int score1 = StringMatcher.Score(item.DisplayName, query);
                int socre2 = StringMatcher.ScoreForPinyin(item.DisplayName, query);

                scores.Add(Math.Max(score1, socre2));
            }

            if (item.Description != null && item.Description != item.DisplayName)
            {
                scores.Add(StringMatcher.Score(item.Description, query));
            }

            return scores.Max();
        }
    }
}
