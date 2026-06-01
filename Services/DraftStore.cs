using System.Text.Json;
using BlazorApp.Models;
using Microsoft.JSInterop;

namespace BlazorApp.Services;

/// <summary>
/// Owns the in-memory application model and persists it to the browser's localStorage
/// via JS interop. Scoped per circuit. Steps mutate <see cref="Current"/> and call
/// <see cref="SaveAsync"/>; on first render the host calls <see cref="LoadAsync"/> to
/// resume any draft (e.g. after the user's connection dropped and they reopened the page).
/// </summary>
public class DraftStore
{
    public const string DraftKey = "upsmfac_affiliation_draft";
    public const string SubmittedKey = "upsmfac_affiliation_submitted";
    public const long MaxFileBytes = 2 * 1024 * 1024; // 2 MB per file

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IJSRuntime _js;
    private bool _loaded;

    public DraftStore(IJSRuntime js) => _js = js;

    public AffiliationApplication Current { get; private set; } = new();

    /// <summary>Last time we successfully wrote to localStorage (for the "Saved ✓" footer).</summary>
    public DateTime? LastSaved { get; private set; }

    /// <summary>Load an existing draft from localStorage. Safe to call only after first render.</summary>
    public async Task LoadAsync()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            var json = await _js.InvokeAsync<string?>("appStorage.load", DraftKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var draft = JsonSerializer.Deserialize<AffiliationApplication>(json, JsonOpts);
                if (draft is not null) Current = draft;
            }
        }
        catch
        {
            // Corrupt/unreadable draft -> start fresh rather than crash the demo.
        }
    }

    public async Task SaveAsync()
    {
        Current.LastSavedUtc = DateTime.UtcNow.ToString("o");
        var json = JsonSerializer.Serialize(Current);
        await _js.InvokeVoidAsync("appStorage.save", DraftKey, json);
        LastSaved = DateTime.Now;
    }

    public async Task<bool> HasDraftAsync()
    {
        var json = await _js.InvokeAsync<string?>("appStorage.load", DraftKey);
        return !string.IsNullOrWhiteSpace(json);
    }

    public async Task StartNewAsync()
    {
        Current = new AffiliationApplication();
        await _js.InvokeVoidAsync("appStorage.remove", DraftKey);
        LastSaved = null;
    }

    /// <summary>Finalise the application: assign a registration code, mark submitted,
    /// and append it to the submitted list so a returning applicant can look it up.</summary>
    public async Task<string> SubmitAsync()
    {
        Current.Status = "submitted";
        Current.SubmittedUtc = DateTime.UtcNow.ToString("o");
        Current.RegistrationCode = $"UP-AFF-{DateTime.Now.Year}-{Random.Shared.Next(1, 9999):0000}";
        await SaveAsync();

        var submitted = await GetSubmittedAsync();
        submitted.Add(Current);
        await _js.InvokeVoidAsync("appStorage.save", SubmittedKey, JsonSerializer.Serialize(submitted));

        return Current.RegistrationCode;
    }

    /// <summary>All applications submitted on this browser/device.</summary>
    public async Task<List<AffiliationApplication>> GetSubmittedAsync()
    {
        var json = await _js.InvokeAsync<string?>("appStorage.load", SubmittedKey);
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<List<AffiliationApplication>>(json, JsonOpts) ?? new(); }
        catch { return new(); }
    }

    /// <summary>
    /// Look up a previously submitted application by Registration Code + Registered Mobile.
    /// On success, loads it into <see cref="Current"/> (read-only submitted view) and returns true.
    /// Note: with no backend, this only finds applications submitted on THIS browser/device.
    /// </summary>
    public async Task<bool> LoadSubmittedAsync(string? code, string? mobile)
    {
        var c = code?.Trim();
        var m = mobile?.Trim();
        var found = (await GetSubmittedAsync()).FirstOrDefault(a =>
            string.Equals(a.RegistrationCode, c, StringComparison.OrdinalIgnoreCase) &&
            a.Applicant.Mobile == m);
        if (found is null) return false;
        Current = found;
        return true;
    }
}
