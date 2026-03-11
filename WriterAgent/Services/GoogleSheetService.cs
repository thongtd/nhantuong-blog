using DailyContentWriter.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Options;

namespace DailyContentWriter.Services;

public class GoogleSheetService
{
    private readonly AppSettings _settings;

    public GoogleSheetService(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }

    private SheetsService CreateSheetsService()
    {
        var serviceAccountPath = Path.Combine(AppContext.BaseDirectory, _settings.GoogleSheet.ServiceAccountFile);

        using var stream = new FileStream(serviceAccountPath, FileMode.Open, FileAccess.Read);
        var credential = GoogleCredential.FromStream(stream)
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        return new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Daily Content Writer"
        });
    }

    public async Task<List<SheetRow>> GetAllRowsAsync()
    {
        var service = CreateSheetsService();
        var range = $"{_settings.GoogleSheet.SheetName}!A:I";

        var request = service.Spreadsheets.Values.Get(_settings.GoogleSheet.SpreadsheetId, range);
        var response = await request.ExecuteAsync();

        var values = response.Values;
        var result = new List<SheetRow>();

        if (values == null || values.Count <= 1)
            return result;

        for (int i = 1; i < values.Count; i++)
        {
            var row = values[i];

            result.Add(new SheetRow
            {
                RowNumber = i + 1,
                STT = GetCell(row, 0),
                MainContent = GetCell(row, 1),
                SEOKeywords = GetCell(row, 2),
                BlogTags = GetCell(row, 3),
                Image1 = GetCell(row, 4),
                Image2 = GetCell(row, 5),
                Image3 = GetCell(row, 6),
                Status = GetCell(row, 7)
            });
        }

        return result;
    }

    public async Task UpdateStatusAsync(int rowNumber, string status)
    {
        var service = CreateSheetsService();
        var range = $"{_settings.GoogleSheet.SheetName}!F{rowNumber}:F{rowNumber}";

        var valueRange = new ValueRange
        {
            Values = new List<IList<object>>
            {
                new List<object> { status }
            }
        };

        var updateRequest = service.Spreadsheets.Values.Update(
            valueRange,
            _settings.GoogleSheet.SpreadsheetId,
            range);

        updateRequest.ValueInputOption =
            SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

        await updateRequest.ExecuteAsync();
    }

    private static string GetCell(IList<object> row, int index)
    {
        if (index >= row.Count) return string.Empty;
        return row[index]?.ToString()?.Trim() ?? string.Empty;
    }
}