using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Helpers;
using Hymnal.XF.Extensions;
using Hymnal.XF.Models;
using Hymnal.XF.Models.Parameter;
using Hymnal.XF.Services;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace Hymnal.XF.ViewModels
{
    public class SearchViewModel : MvxViewModel
    {
        private readonly INavigationService navigationService;
        private readonly IHymnsService hymnsService;
        private readonly IPreferencesService preferencesService;
        private readonly ILogger<SearchViewModel> logger;

        public MvxObservableCollection<Hymn> Hymns { get; set; } = new MvxObservableCollection<Hymn>();

        public Hymn SelectedHymn
        {
            get => null;
            set
            {
                if (value == null)
                    return;

                RaisePropertyChanged(nameof(SelectedHymn));

                SelectedHymnExecuteAsync(value).ConfigureAwait(true);
            }
        }

        private string textSearchBar;
        public string TextSearchBar
        {
            get => textSearchBar;
            set
            {
                SetProperty(ref textSearchBar, value);
                ObservableTextSearchBar.NextValue(value);
                //TextSearchExecuteAsync(value).ConfigureAwait(true);
            }
        }
        private ObservableValue<string> ObservableTextSearchBar = new ObservableValue<string>(false);

        private HymnalLanguage _language;

        public SearchViewModel(
            INavigationService navigationService,
            IHymnsService hymnsService,
            IPreferencesService preferencesService,
            ILogger<SearchViewModel> logger
            )
        {
            this.navigationService = navigationService;
            this.hymnsService = hymnsService;
            this.preferencesService = preferencesService;
            this.logger = logger;
            _language = this.preferencesService.ConfiguratedHymnalLanguage;

            ObservableTextSearchBar
                .Throttle(TimeSpan.FromSeconds(0.3))
                .Subscribe(text => InvokeOnMainThreadAsync(async () => await TextSearchExecuteAsync(text)));
        }

        ~SearchViewModel()
        {
            preferencesService.HymnalLanguageConfiguratedChanged -= PreferencesService_HymnalLanguageConfiguratedChangedAsync;
        }

        public override async Task Initialize()
        {
            preferencesService.HymnalLanguageConfiguratedChanged += PreferencesService_HymnalLanguageConfiguratedChangedAsync;
            await TextSearchExecuteAsync(string.Empty);

            await base.Initialize();
        }

        private void PreferencesService_HymnalLanguageConfiguratedChangedAsync(object sender, HymnalLanguage e)
        {
            _language = e;
            TextSearchExecuteAsync(string.Empty).ConfigureAwait(true);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            Analytics.TrackEvent(Constants.TrackEv.Navigation, new Dictionary<string, string>
            {
                { Constants.TrackEv.NavigationReferenceScheme.PageName, nameof(SearchViewModel) },
                { Constants.TrackEv.NavigationReferenceScheme.CultureInfo, Constants.CurrentCultureInfo.Name },
                { Constants.TrackEv.NavigationReferenceScheme.HymnalVersion, preferencesService.ConfiguratedHymnalLanguage.Id }
            });
        }

        private async Task TextSearchExecuteAsync(string text)
        {
            Hymns.Clear();

            logger.LogInformation($"Search for: {text}");

            IEnumerable<Hymn> hymns = (await hymnsService.GetHymnListAsync(_language)).OrderByNumber();

            if (string.IsNullOrWhiteSpace(text))
            {
                Hymns.AddRange(hymns);
                return;
            }

            Hymns.AddRange(hymns.SearchQuery(text));
        }

        private async Task SelectedHymnExecuteAsync(Hymn hymn)
        {
            // Some devices iOS that have had problems in this place
            if (hymn == null)
                return;

            try
            {
                await navigationService.Navigate<HymnViewModel, HymnIdParameter>(new HymnIdParameter
                {
                    Number = hymn.Number,
                    HymnalLanguage = _language
                });

                if (!string.IsNullOrWhiteSpace(TextSearchBar))
                {

                    Analytics.TrackEvent(Constants.TrackEv.HymnFounded, new Dictionary<string, string>
                {
                    { Constants.TrackEv.HymnFoundedScheme.Query, TextSearchBar },
                    { Constants.TrackEv.HymnFoundedScheme.HymnFounded, hymn.Number.ToString() },
                    { Constants.TrackEv.HymnFoundedScheme.HymnalVersion, _language.Id }
                });
                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>()
                    {
                        { "File", nameof(SearchViewModel) },
                        { "Opening Hymn", hymn.Number.ToString() }
                    };

                Crashes.TrackError(ex, properties);
            }
        }
    }
}