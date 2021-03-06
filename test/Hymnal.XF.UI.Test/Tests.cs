using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Hymnal.XF.UI.Test
{
    [TestFixture(Platform.Android)]
#if !Windows
    [TestFixture(Platform.iOS)]
#endif
    public class Tests
    {
        private IApp app;
        private readonly Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void AppLaunches()
        {
            app.Screenshot("First screen.");
        }

        [Test]
        public void OpenHymnUsingNumber()
        {
            //app.Repl();

            // Arranque
            app.WaitForElement("NumberPage");

            // Act
            app.Tap("HymnNumberEntry");
            app.EnterText("133");
            app.DismissKeyboard();
            app.Tap("OpenHymnButton");
            app.WaitForElement("HymnPage");
            back();
            app.WaitForElement("NumberPage");

            // Assert
            var result = app.Query(e => e.Marked("NumberPage")).Any();
            Assert.IsTrue(result);
        }

        [Test]
        public void OpenHymnUsingSearch()
        {
            //app.Repl();

            // Arranque
            app.WaitForElement("NumberPage");

            // Act
            app.Tap("OpenSearchPageToolbarItem");

            app.WaitForElement("SearchPage");
            app.Tap("HymnSearchBar");
            app.EnterText("13");
            app.DismissKeyboard();
            app.Tap(e => e.Class("ItemContentView").Index(0));

            app.WaitForElement("HymnPage");
            back();

            app.WaitForElement("SearchPage");
            app.DismissKeyboard();
            back();

            app.WaitForElement("NumberPage");

            // Assert
            var result = app.Query(e => e.Marked("NumberPage")).Any();
            Assert.IsTrue(result);
        }

        [Test]
        public void ChangeLanguage()
        {
            app.Repl();
            AppResult a = app.Query(x => x.Id("icon")).Last();
        }

        private void back()
        {
            if (app.Query(e => e.Text("ClosePageToolbar")).Any())
                app.Tap("ClosePageToolbar");
            else
                app.Back();
        }
    }
}

