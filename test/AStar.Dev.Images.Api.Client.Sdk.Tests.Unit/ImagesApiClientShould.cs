using AStar.Dev.Images.Api.Client.Sdk.Helpers;
using AStar.Dev.Images.Api.Client.Sdk.ImagesApi;
using AStar.Dev.Images.Api.Client.Sdk.MockMessageHandlers;
using JetBrains.Annotations;

namespace AStar.Dev.Images.Api.Client.Sdk;

[TestSubject(typeof(ImagesApiClient))]
public sealed class ImagesApiClientShould
{
    [Fact]
    public async Task ReturnExpectedFailureFromGetHealthAsyncWhenTheApiIsUnreachableVersion()
    {
        var handler = new MockHttpRequestExceptionErrorHttpMessageHandler();
        var sut     = ImagesApiClientFactory.Create(handler);

        var response = await sut.GetHealthCheckAsync();

        response.IsFailure.ShouldBeTrue();
        response.Value?.Description.ShouldBe("Unable to retrieve the description of the Health Status");
        response.Value?.Status.ShouldBe("Could not get a response from the AStar.Dev.Admin.Api.");
    }

    [Fact]
    public async Task ReturnExpectedFailureMessageFromGetHealthAsyncWhenCheckFails()
    {
        var sut = ImagesApiClientFactory.CreateInternalServerErrorClient("Health Check failed.");

        var response = await sut.GetHealthCheckAsync();

        response.IsFailure.ShouldBeTrue();
        response.Value?.Description.ShouldBe("Health Check failed - Internal Server Error.");
    }

    [Fact]
    public async Task ReturnExpectedMessageFromGetHealthAsyncWhenCheckSucceeds()
    {
        var handler = new MockSuccessHttpMessageHandler("Health");
        var sut     = ImagesApiClientFactory.Create(handler);

        var response = await sut.GetHealthCheckAsync();

        response.IsSuccess.ShouldBeTrue();
    }
}
