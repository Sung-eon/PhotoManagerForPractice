namespace PhotoManager.Shared;

using Microsoft.AspNetCore.Components;

public class BackgroundStyle : ComponentBase 
{
    private string BackgroundStyleString = "flex flex-col justify-center w-screen h-screen absolute top-0 left-0 bg-gray-800/70";
    
    public string GetBackgroundStyle(bool HiddenSwitch) 
    {
      return HiddenSwitch ? "hidden" : BackgroundStyleString;
    }
}

public interface IPopupStyle
{
    void ClosePopup();
}