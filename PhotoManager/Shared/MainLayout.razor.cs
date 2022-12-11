namespace PhotoManager.Shared;

public partial class MainLayout
{
    private string selectedStyle = "text-black border-b-2 border-sky-400 border-solid pb-3";
    private string filledButtonStyle = "border border-solid rounded bg-sky-400 px-1 py-2 text-white w-full hover:bg-white hover:border-sky-400 hover:text-black";
    private string plainLinkStyle() => "my-auto w-full p-2 sm:basis-1 sm:p-0 text-black rounded-lg hover:text-sky-400 border border-slate-400 rounded-lg sm:border-0 sm:rounded-0";

    private string mainContentExpanded = "w-full";
    private string mainContentShrink = "w-full sm:basis-3/4 lg:basis-4/5";
    private string sidebarShrinkHeight = "h-full";
    private string sidebarExpandedHeight = "h-5/6";

    private string hoverStyle()
    {
        return String.Join(" ", selectedStyle.Split(" ").Select(s => $"hover:{s}"));
    }

    private string selected { get; set; } = "home";

    private string SelectBarItemColor(string thisOne)
    {
        return thisOne == selected
        ? selectedStyle
        : "text-gray-400";
    }

    List<SidebarItemType> sidebarItems = new()
    {
        new SidebarItemType("/", "Home", "oi-home")
    };

    List<SidebarItemType> mainSelectBar = new()
    {
        new SidebarItemType(link: "/", text: "Home"),
        new SidebarItemType(link: "/recent", text: "Recent"),
        new SidebarItemType(link: "/directory", text: "Directory"),
        new SidebarItemType(link: "/blog", text: "Blog"),
    };

}