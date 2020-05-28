using Hymnal.Core.ViewModels;
using MvvmCross.Platforms.Tvos.Views;
using System;

namespace Hymnal.Native.TvOS
{
    [MvxFromStoryboard("Main")]
    public partial class SearchViewController : MvxViewController<SearchViewModel>
    {
        public SearchViewController (IntPtr handle) : base (handle)
        {
        }
    }
}