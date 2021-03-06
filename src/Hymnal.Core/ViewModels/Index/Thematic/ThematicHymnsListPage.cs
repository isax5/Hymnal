using System.Collections.Generic;
using System.Threading.Tasks;
using Hymnal.Core.Extensions;
using Hymnal.Core.Models;
using Hymnal.Core.Models.Parameter;
using Hymnal.Core.Services;
using Microsoft.AppCenter.Analytics;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace Hymnal.Core.ViewModels
{
    public class ThematicHymnsListViewModel : MvxViewModel<Ambit>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly IHymnsService hymnsService;
        private readonly IPreferencesService preferencesService;
        private Ambit ambit;
        public Ambit Ambit
        {
            get => ambit;
            set => SetProperty(ref ambit, value);
        }

        public MvxObservableCollection<Hymn> Hymns { get; set; } = new MvxObservableCollection<Hymn>();

        public Hymn SelectedHymn
        {
            get => null;
            set
            {
                if (value == null)
                    return;

                SelectedHymnExecuteAsync(value).ConfigureAwait(true);
                RaisePropertyChanged(nameof(SelectedHymn));
            }
        }

        private readonly HymnalLanguage language;

        public ThematicHymnsListViewModel(
            IMvxNavigationService navigationService,
            IHymnsService hymnsService,
            IPreferencesService preferencesService
            )
        {
            this.navigationService = navigationService;
            this.hymnsService = hymnsService;
            this.preferencesService = preferencesService;

            language = this.preferencesService.ConfiguratedHymnalLanguage;
        }

        public override void Prepare(Ambit parameter)
        {
            Ambit = parameter;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Hymns.AddRange((await hymnsService.GetHymnListAsync(language)).GetRange(Ambit.Star, Ambit.End));
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            Analytics.TrackEvent(Constants.TrackEv.Navigation, new Dictionary<string, string>
            {
                { Constants.TrackEv.NavigationReferenceScheme.PageName, nameof(ThematicHymnsListViewModel) },
                { "Ambit", Ambit.AmbitName },
                { Constants.TrackEv.NavigationReferenceScheme.CultureInfo, Constants.CurrentCultureInfo.Name },
                { Constants.TrackEv.NavigationReferenceScheme.HymnalVersion, preferencesService.ConfiguratedHymnalLanguage.Id }
            });
        }

        private async Task SelectedHymnExecuteAsync(Hymn hymn)
        {
            await navigationService.Navigate<HymnViewModel, HymnIdParameter>(new HymnIdParameter
            {
                Number = hymn.Number,
                HymnalLanguage = language
            });
        }
    }
}
