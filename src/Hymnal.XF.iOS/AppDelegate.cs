using System.Diagnostics;
using Foundation;
using MvvmCross.Forms.Platforms.Ios.Core;
using UIKit;

namespace Hymnal.XF.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : MvxFormsApplicationDelegate<Setup, Core.App, UI.App>
    {
        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
        {
            //base.PerformActionForShortcutItem(application, shortcutItem, completionHandler);

            if (shortcutItem == null || Hymnal.Core.App.Current == null)
            {
                Debug.WriteLine("No shortcut or app not deployed");
                return;
            }

            Debug.WriteLine($"Shortcut ${shortcutItem.Type}");

            switch (shortcutItem.Type)
            {
                case "SearchAction":
                    Core.App.Current?.PerformPageRequest(Core.Models.PageRequest.Search);
                    break;

                case "HistoryAction":
                    Core.App.Current?.PerformPageRequest(Core.Models.PageRequest.Records);
                    break;

                default:
                    break;
            }
        }
    }
}