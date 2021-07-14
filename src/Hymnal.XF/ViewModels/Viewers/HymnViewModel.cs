using System;
using System.Linq;
using System.Reactive.Linq;
using Hymnal.AzureFunctions.Client;
using Hymnal.AzureFunctions.Extensions;
using Hymnal.XF.Extensions;
using Hymnal.XF.Extensions.i18n;
using Hymnal.XF.Models;
using Hymnal.XF.Models.Parameters;
using Hymnal.XF.Services;
using Hymnal.XF.Views;
using MediaManager;
using MediaManager.Library;
using MediaManager.Player;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Essentials;

namespace Hymnal.XF.ViewModels
{
    public class HymnViewModel : BaseViewModelParameter<HymnIdParameter>
    {
        private readonly IHymnsService hymnsService;
        private readonly IPreferencesService preferencesService;
        private readonly IMediaManager mediaManager;
        private readonly IDialogService dialogService;
        private readonly IStorageManager storageService;
        private readonly IAzureHymnService azureHymnService;

        #region Properties
        public int HymnTitleFontSize => preferencesService.HymnalsFontSize + 10;
        public int HymnFontSize => preferencesService.HymnalsFontSize;

        private Hymn hymn;
        public Hymn Hymn
        {
            get => hymn;
            set => SetProperty(ref hymn, value);
        }

        private HymnalLanguage language;
        public HymnalLanguage Language
        {
            get => language;
            set => SetProperty(ref language, value);
        }

        private bool isFavorite;
        public bool IsFavorite
        {
            get => isFavorite;
            set => SetProperty(ref isFavorite, value);
        }

        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set => SetProperty(ref isPlaying, value);
        }

        private HymnIdParameter hymnId;
        public HymnIdParameter HymnParameter
        {
            get => hymnId;
            set => SetProperty(ref hymnId, value);
        }
        #endregion


        public HymnViewModel(
            INavigationService navigationService,
            IHymnsService hymnsService,
            IPreferencesService preferencesService,
            IMediaManager mediaManager,
            IDialogService dialogService,
            IStorageManager storageService,
            IAzureHymnService azureHymnService
            ) : base(navigationService)
        {
            this.hymnsService = hymnsService;
            this.preferencesService = preferencesService;
            this.mediaManager = mediaManager;
            this.dialogService = dialogService;
            this.storageService = storageService;
            this.azureHymnService = azureHymnService;
            mediaManager.StateChanged += MediaManager_StateChanged;

            OpenSheetCommand = new DelegateCommand(OpenSheetAsync).ObservesCanExecute(() => NotBusy);
            FavoriteCommand = new DelegateCommand(FavoriteExecute).ObservesCanExecute(() => NotBusy);
            ShareCommand = new DelegateCommand(ShareExecute).ObservesCanExecute(() => NotBusy);
            PlayCommand = new DelegateCommand(PlayExecuteAsync).ObservesCanExecute(() => NotBusy);
            CloseCommand = new DelegateCommand(CloseAsync).ObservesCanExecute(() => NotBusy);
        }

        ~HymnViewModel()
        {
            mediaManager.StateChanged -= MediaManager_StateChanged;
        }

        public override async void OnNavigatedTo(INavigationParameters parameters, HymnIdParameter parameter)
        {
            base.OnNavigatedTo(parameters, parameter);

            HymnParameter = parameter;
            Language = HymnParameter.HymnalLanguage;

            try
            {
                Hymn = await hymnsService.GetHymnAsync(HymnParameter.Number, HymnParameter.HymnalLanguage);
            }
            catch (Exception ex)
            {
                ex.Report();
            }

            IsPlaying = mediaManager.IsPlaying();

            // Is Favorite
            IsFavorite = storageService.All<FavoriteHymn>().ToList().Exists(f => f.Number == Hymn.Number && f.HymnalLanguageId.Equals(Language.Id));

            // Record
            if (HymnParameter.SaveInRecords)
            {

                IQueryable<RecordHymn> records = storageService.All<RecordHymn>().Where(h => h.Number == Hymn.Number && h.HymnalLanguageId.Equals(Language.Id));
                storageService.RemoveRange(records);

                storageService.Add(Hymn.ToRecordHymn());
            }

            azureHymnService.ObserveSettings().Subscribe(x => { }, ex => { });
        }

        //public override async Task Initialize()
        //{
        //    try
        //    {
        //        Hymn = await hymnsService.GetHymnAsync(HymnParameter.Number, HymnParameter.HymnalLanguage);
        //    }
        //    catch (Exception ex)
        //    {
        //        var properties = new Dictionary<string, string>()
        //        {
        //            { "File", nameof(HymnViewModel) },
        //            { "Hymn Number", HymnParameter.Number.ToString() },
        //            { "Hymn Version", HymnParameter.HymnalLanguage.Id }
        //        };

        //        logger.LogError("Exception opening a hymnal", ex, properties);
        //        Crashes.TrackError(ex, properties);

        //        await navigationService.Close(this);
        //        return;
        //    }

        //    IsPlaying = mediaManager.IsPlaying();

        //    // Is Favorite
        //    IsFavorite = storageService.All<FavoriteHymn>().ToList().Exists(f => f.Number == Hymn.Number && f.HymnalLanguageId.Equals(Language.Id));

        //    // Record
        //    if (HymnParameter.SaveInRecords)
        //    {

        //        IQueryable<RecordHymn> records = storageService.All<RecordHymn>().Where(h => h.Number == Hymn.Number && h.HymnalLanguageId.Equals(Language.Id));
        //        storageService.RemoveRange(records);

        //        storageService.Add(Hymn.ToRecordHymn());
        //    }

        //    await base.Initialize();
        //}

        //public override void ViewAppeared()
        //{
        //    base.ViewAppeared();

        //    // Precarga de datos para reproduccion
        //    azureHymnService.ObserveSettings().Subscribe(x => { }, ex => { });

        //    Debug.WriteLine($"Opening Hymn: {Hymn.Number} of {Language.Id}");

        //    Analytics.TrackEvent(Constants.TrackEv.HymnOpened, new Dictionary<string, string>
        //    {
        //        { Constants.TrackEv.HymnReferenceScheme.Number, Hymn.Number.ToString() },
        //        { Constants.TrackEv.HymnReferenceScheme.HymnalVersion, Language.Id },
        //        { Constants.TrackEv.HymnReferenceScheme.CultureInfo, Constants.CurrentCultureInfo.Name },
        //        { Constants.TrackEv.HymnReferenceScheme.Time, DateTime.Now.ToLocalTime().ToString() },
        //        { "Font Size Hymnal", preferencesService.HymnalsFontSize.ToString() }
        //    });
        //}

        #region Events
        private void MediaManager_StateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
        {
            switch (e.State)
            {
                case MediaPlayerState.Playing:
                case MediaPlayerState.Buffering:
                case MediaPlayerState.Loading:
                    IsPlaying = true;
                    break;

                case MediaPlayerState.Stopped:
                case MediaPlayerState.Failed:
                case MediaPlayerState.Paused:
                    IsPlaying = false;
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region Commands
        public DelegateCommand OpenSheetCommand;
        private async void OpenSheetAsync()
        {
            await NavigationService.NavigateAsync(nameof(MusicSheetPage), HymnParameter);
        }

        public DelegateCommand FavoriteCommand;
        private void FavoriteExecute()
        {

            IQueryable<FavoriteHymn> favorites = storageService.All<FavoriteHymn>().Where(f => f.Number == Hymn.Number && f.HymnalLanguageId.Equals(Language.Id));

            if (IsFavorite || favorites.Count() > 0)
            {
                storageService.RemoveRange(favorites);

                //Analytics.TrackEvent(Constants.TrackEv.HymnRemoveFromFavorites, new Dictionary<string, string>
                //{
                //    { Constants.TrackEv.HymnReferenceScheme.Number, Hymn.Number.ToString() },
                //    { Constants.TrackEv.HymnReferenceScheme.HymnalVersion, Language.Id },
                //    { Constants.TrackEv.HymnReferenceScheme.CultureInfo, Constants.CurrentCultureInfo.Name },
                //    { Constants.TrackEv.HymnReferenceScheme.Time, DateTime.Now.ToLocalTime().ToString() }
                //});
            }
            else
            {
                storageService.Add(Hymn.ToFavoriteHymn());

                //Analytics.TrackEvent(Constants.TrackEv.HymnAddedToFavorites, new Dictionary<string, string>
                //{
                //    { Constants.TrackEv.HymnReferenceScheme.Number, Hymn.Number.ToString() },
                //    { Constants.TrackEv.HymnReferenceScheme.HymnalVersion, Language.Id },
                //    { Constants.TrackEv.HymnReferenceScheme.CultureInfo, Constants.CurrentCultureInfo.Name },
                //    { Constants.TrackEv.HymnReferenceScheme.Time, DateTime.Now.ToLocalTime().ToString() }
                //});
            }

            IsFavorite = !IsFavorite;
        }

        public DelegateCommand ShareCommand;
        private void ShareExecute()
        {
            Share.RequestAsync(
                title: hymn.Title,
                text: $"{hymn.Title}\n\n{hymn.Content}\n\n{Constants.Constants.WebLinks.DeveloperWebSite}");

            //Analytics.TrackEvent(Constants.TrackEv.HymnShared, new Dictionary<string, string>
            //{
            //    { Constants.TrackEv.HymnReferenceScheme.Number, Hymn.Number.ToString() },
            //    { Constants.TrackEv.HymnReferenceScheme.HymnalVersion, Language.Id },
            //    { Constants.TrackEv.HymnReferenceScheme.CultureInfo, Constants.CurrentCultureInfo.Name },
            //    { Constants.TrackEv.HymnReferenceScheme.Time, DateTime.Now.ToLocalTime().ToString() }
            //});
        }

        public DelegateCommand PlayCommand;
        private async void PlayExecuteAsync()
        {
            // Check internet connection
            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                await dialogService.Alert(Languages.WeHadAProblem, Languages.NoInternetConnection, Languages.Ok);
                return;
            }

            // Stop/Start playing
            if (IsPlaying)
            {
                await mediaManager.Stop();

                // IsPlaying is setted here becouse maybe the internet is not so fast enough and the song can be loading and not put play
                IsPlaying = false;
                return;
            }

            azureHymnService.ObserveSettings()
                .Where(x => x.Id == Language.Id)
                .Subscribe(hymnSettings => MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var songUrl = string.Empty;
                    var isPlayingInstrumentalMusic = false;

                    // Choose music
                    if (hymnSettings.InstrumentalMusicUrl != null && hymnSettings.SungMusicUrl != null)
                    {
                        var instrumentalTitle = Languages.Instrumental;
                        var sungTitle = Languages.Sung;

                        var result = await dialogService.DisplayActionSheet(
                            Languages.ChooseYourHymnal, Languages.Cancel,
                            null, new[] { instrumentalTitle, sungTitle });

                        if (result.Equals(instrumentalTitle))
                        {
                            songUrl = hymnSettings.GetInstrumentURL(Hymn.Number);
                            isPlayingInstrumentalMusic = true;
                        }
                        else if (result.Equals(sungTitle))
                        {
                            songUrl = hymnSettings.GetSungURL(hymn.Number);
                            isPlayingInstrumentalMusic = false;
                        }
                        // Tap on "Close"
                        else
                        {
                            return;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(songUrl))
                    {
                        isPlayingInstrumentalMusic = language.SupportInstrumentalMusic;
                        songUrl = language.SupportInstrumentalMusic ? hymnSettings.GetInstrumentURL(Hymn.Number) : hymnSettings.GetSungURL(Hymn.Number);
                    }

                    // IsPlaying is setted here becouse maybe the internet is not so fast enough and the song can be loading and not to put play from the first moment
                    IsPlaying = true;
                    IMediaItem mediaItem = new MediaItem(songUrl) { IsMetadataExtracted = true };
                    mediaItem = await mediaManager.Extractor.UpdateMediaItem(mediaItem).ConfigureAwait(false);
                    mediaItem.DisplayTitle = Hymn.Title;
                    mediaItem.Album = Language.Name;
                    mediaItem.Year = Language.Year;
                    mediaItem.MediaType = MediaType.Audio;

                    await mediaManager.Play(mediaItem);

                    //Analytics.TrackEvent(Constants.TrackEv.HymnMusicPlayed, new Dictionary<string, string>
                    //{
                    //    { Constants.TrackEv.HymnReferenceScheme.Number, Hymn.Number.ToString() },
                    //    { Constants.TrackEv.HymnReferenceScheme.HymnalVersion, Language.Id },
                    //    { Constants.TrackEv.HymnReferenceScheme.TypeOfMusicPlaying, isPlayingInstrumentalMusic ?
                    //    Constants.TrackEv.HymnReferenceScheme.InstrumentalMusic : Constants.TrackEv.HymnReferenceScheme.SungMusic },
                    //    { Constants.TrackEv.HymnReferenceScheme.CultureInfo, Constants.CurrentCultureInfo.Name },
                    //    { Constants.TrackEv.HymnReferenceScheme.Time, DateTime.Now.ToLocalTime().ToString() }
                    //});
                }),
                ex => MainThread.InvokeOnMainThreadAsync(async () => await dialogService.Alert(Languages.WeHadAProblem, Languages.Ok)));

        }

        //public DelegateCommand OpenPlayerCommand;
        //private async Task OpenPlayerExecuteAsync()
        //{
        //    await NavigationService.Close(this);
        //    await NavigationService.Navigate<PlayerViewModel, HymnIdParameter>(HymnParameter);
        //}

        public DelegateCommand CloseCommand;
        private async void CloseAsync()
        {
            await NavigationService.GoBackAsync();
        }
        #endregion
    }
}