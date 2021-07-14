using Hymnal.XF.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hymnal.XF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicSheetPage : BaseContentPage<MusicSheetViewModel>
    {
        public MusicSheetPage()
        {
            InitializeComponent();
        }

        private bool showingNavigationBar = true;
        private void ScrollViewTapped(object sender, System.EventArgs e)
        {
            if (showingNavigationBar)
                NavigationPage.SetHasNavigationBar(this, false);
            else
                NavigationPage.SetHasNavigationBar(this, true);

            showingNavigationBar = !showingNavigationBar;
        }
    }
}