namespace Gley.UrbanSystem.Editor
{
    internal readonly struct WindowProperties
    {
        internal string NameSpace { get; }
        internal string ClassName { get; }
        internal string Title { get; }
        internal string TutorialLink { get; }
        internal bool ShowBack { get; }
        internal bool ShowTitle { get; }
        internal bool ShowTop { get; }
        internal bool ShowScroll { get; }
        internal bool ShowBottom { get; }
        internal bool BlockClicks { get; }


        internal WindowProperties(string nameSpace, string className, string title, bool showBack, bool showTitle, bool showTop, bool showScroll, bool showBottom, bool blockClicks, string tutorialLink)
        {
            NameSpace = nameSpace;
            ClassName = className;
            Title = title;
            ShowBack = showBack;
            ShowTitle = showTitle;
            ShowTop = showTop;
            ShowScroll = showScroll;
            ShowBottom = showBottom;
            BlockClicks = blockClicks;
            TutorialLink = tutorialLink;
        }
    }
}
