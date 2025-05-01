using AStar.Dev.Images.Api.Client.Sdk.ImagesApi;
using AStar.Dev.Images.Api.Client.Sdk.MockMessageHandlers;
using AStar.Dev.Logging.Extensions;
using Microsoft.Identity.Web;
using NSubstitute;

namespace AStar.Dev.Images.Api.Client.Sdk.Helpers;

internal static class ImagesApiClientFactory
{
    private                 const string                        IrrelevantUrl = "https://doesnot.matter.com";
    private static readonly       ILoggerAstar<ImagesApiClient> DummyLogger   = Substitute.For<ILoggerAstar<ImagesApiClient>>();

    public static ImagesApiClient Create(HttpMessageHandler mockHttpMessageHandler)
    {
        var tokenAcquisitionServiceMock = Substitute.For<ITokenAcquisition>();
        var httpClient                  = new HttpClient(mockHttpMessageHandler) { BaseAddress = new(IrrelevantUrl) };

        return new(httpClient, tokenAcquisitionServiceMock, DummyLogger);
    }

    public static ImagesApiClient CreateInternalServerErrorClient(string errorMessage)
    {
        var tokenAcquisitionServiceMock = Substitute.For<ITokenAcquisition>();
        var handler                     = new MockInternalServerErrorHttpMessageHandler(errorMessage);
        var httpClient                  = new HttpClient(handler) { BaseAddress = new(IrrelevantUrl) };

        return new(httpClient, tokenAcquisitionServiceMock, DummyLogger);
    }
}
