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

        public void Init(PluginInitContext pluginInitContext)
        {
            _pluginInitContext = pluginInitContext;

            _storeItems = StoreAppLauncher.StoreAppLauncher.GetAppListForCurrentUser().ToList();
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            foreach (var storeItem in _storeItems)
            {
                var score = Score(storeItem, query.Search);

                if (score <= 0)
                {
                    continue;
                }

                var icoPath = storeItem.FullLogoPath;

                // Icon image can't be fine
                // default to using plugin icon
                if (!File.Exists(icoPath))
                {
                    icoPath = Path.Combine(_pluginInitContext.CurrentPluginMetadata.PluginDirectory, "Images\\\\pic.png");
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
