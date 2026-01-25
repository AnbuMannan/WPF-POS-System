using System.IO;
using System.Text.Json;

namespace POS.UI.Core.Navigation;

public static class MenuService
{
    public static List<MenuItemModel> LoadMenu()
    {
        //var json = File.ReadAllText("menu.json");
        var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "menu.json"));

        return JsonSerializer.Deserialize<List<MenuItemModel>>(json);
    }
}
