using System.Net.Http.Json;
using AStar.Dev.Api.HealthChecks;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.Images.Api.Client.Sdk.Models;
using AStar.Dev.Logging.Extensions;
using AStar.Dev.Technical.Debt.Reporting;
using AStar.Dev.Utilities;
using Microsoft.Identity.Web;

namespace AStar.Dev.Images.Api.Client.Sdk.ImagesApi;

/// <summary>
///     The <see href="Images.ApiClient"></see> class
/// </summary>
/// <param name="httpClient"></param>
/// <param name="tokenAcquisitionService"></param>
/// <param name="logger"></param>
[Refactor(5, 10, "This class needs to be refactored / rewritten")]
public sealed class ImagesApiClient(HttpClient httpClient, ITokenAcquisition tokenAcquisitionService, ILoggerAstar<ImagesApiClient> logger) : IApiClient
{
    /// <inheritdoc />
    public async Task<Result<string, HealthStatusResponse>> GetHealthCheckAsync(CancellationToken cancellationToken = new ())
    {
        return await GetSafelyAsync<HealthStatusResponse>("/health/ready?version=1.0");
    }

    /// <summary>
    /// </summary>
    /// <param name="imagePath"></param>
    /// <param name="maximumSizeInPixels"></param>
    /// <param name="thumbnail"></param>
    /// <returns></returns>
    [Refactor(1, 1, "Refactor the param: thumbnail as well as the passed value")]
    public async Task<Stream> GetImageAsync(string imagePath, int maximumSizeInPixels, bool thumbnail)
    {
        var requestUri = $"image?imagePath={Uri.EscapeDataString(imagePath)}&maximumSizeInPixels={maximumSizeInPixels}&thumbnail={thumbnail}&version=1.0";
        var token      = await tokenAcquisitionService.GetAccessTokenForUserAsync(["api://54861ab2-fdb0-4e18-a073-c90e7bf9f0c5/ToDoList.Write"]);

        // logger.LogDebug("Token: {Token}", token);
        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

        httpClient.Timeout = TimeSpan.FromMinutes(1); // was erroring on 30 seconds
        var response = await httpClient.GetAsync(requestUri);

        return response.IsSuccessStatusCode
                   ? await response.Content.ReadAsStreamAsync()
                   : CreateNotFoundMemoryStream(imagePath);
    }

    private async Task<Result<string, TResponse>> GetSafelyAsync<TResponse>( string uri)
    {
        try
        {
            logger.LogApiCallStart(Constants.ApiName, uri);
            var token = await tokenAcquisitionService.GetAccessTokenForUserAsync(["api://2ca26585-5929-4aae-86a7-a00c3fc2d061/ToDoList.Write"]);

            _ = httpClient.AddBearerToken(token);
            var response = await httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return logger.ReturnLoggedFailure<TResponse>( Constants.ApiName, uri, response.ReasonPhrase ?? response.StatusCode.ToString());

            var result = (await response.Content.ReadFromJsonAsync<TResponse>(Utilities.Constants.WebDeserialisationSettings))!;

            return logger.ReturnLoggedSuccess(result, Constants.ApiName, "uri");
        }
        catch (Exception ex)
        {
            logger.LogException(ex);

            return Result<string, TResponse>.Failure(ex.Message)!;
        }
    }

    private Stream CreateNotFoundMemoryStream(string fileName)
    {
        logger.LogApiCallWarning(Constants.ApiName, "image", $"The {fileName} was not found");

        return NotFound.Image;
    }
}
