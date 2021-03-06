// This file has been autogenerated from a class added in the UI designer.

using System;
using Hymnal.Core.ViewModels;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;

namespace Hymnal.Native.iOS.Views
{
    [MvxFromStoryboard("Main")]
    [MvxTabPresentation(TabName = "Index", TabIconName = "tab index")]
    public partial class IndexTableViewController : MvxTableViewController<IndexViewModel>
    {
        public IndexTableViewController(IntPtr handle) : base(handle)
        {
        }
    }
}
