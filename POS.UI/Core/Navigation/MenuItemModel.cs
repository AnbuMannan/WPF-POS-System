namespace POS.UI.Core.Navigation;

public class MenuItemModel
{
    public string Header { get; set; }
    public string View { get; set; }
    public List<MenuItemModel> Children { get; set; } = new();
}
