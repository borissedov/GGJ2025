using GlobalGameJam2025_Bubbles.Models;
using GlobalGameJam2025_Bubbles.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace GlobalGameJam2025_Bubbles.Components.Pages;

public partial class Game
{
    [Inject] private ProtectedSessionStorage _protectedSessionStore { get; set; }
    [Inject] private GameService _gameService { get; set; }

    private GameViewModel? _gameViewModel;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var result = await _protectedSessionStore.GetAsync<GameViewModel>("game");
            if (result.Success)
            {
                _gameViewModel = result.Value;
                StateHasChanged();
            }
        }
    }

    private async Task Continue()
    {
       await _gameService.NextStep(_gameViewModel);
       StateHasChanged();
    }
}