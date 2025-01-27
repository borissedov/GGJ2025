using GlobalGameJam2025_Bubbles.Models;
using GlobalGameJam2025_Bubbles.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace GlobalGameJam2025_Bubbles.Components.Pages;

public partial class Game
{
    [Inject] private ProtectedSessionStorage _protectedSessionStore { get; set; }
    [Inject] private GameService _gameService { get; set; }

    private bool IsLoading = false;

    private GameViewModel? _gameViewModel;
    
    private ElementReference _tweetInput;

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
        try
        {
            // Turn on the "loading" indicator
            IsLoading = true;
            // Let Blazor re-render so the overlay becomes visible right away
            StateHasChanged();

            await Task.Delay(1000);

            await _gameService.NextStep(_gameViewModel);
        }
        finally
        {
            // Turn off the loading indicator
            IsLoading = false;
            if (_gameViewModel.GameState == GameState.TweetEntry)
            {
                await _tweetInput.FocusAsync();
            }
            // Re-render so the overlay goes away
            StateHasChanged();
        }
    }
}