# .NET 11 upgrade and Azure Functions build workaround

Published 2026-11-11.

For the updated samples with these changes already applied, clone the latest repository into a new folder. If you are updating your own implementation, follow the actions below.

| Chapters | What changed | Reader action |
| --- | --- | --- |
| All | Target framework now .NET 11, with updated dependency versions. | Install the .NET 11 SDK and use the updated chapter project files. |
| 1 | MailKit upgraded to 5.0.0. | Replace `smtpClient.Send(message)` with `smtpClient.Send(new MailKit.SendRequest(message))`. |
| 4–14 | Shared test packages updated. bUnit upgraded to 2.15.2 in chapters 9–14. | Update [shared test dependencies](../../Directory.Build.props) and the chapter's test-project references. |
| 6–14 | Refit upgraded to 16.4.0. Mock API responses now require their originating request. | Set `HttpResponseMessage.RequestMessage` before constructing `ApiResponse<string>`, as shown in the updated [payment gateway tests](../../ch06/CleanCart.NET/tst/Infrastructure.UnitTests/Services/PaymentGatewayTests.cs). |
| 7–14 | Testcontainers.MsSql 4.16.0 resolves BouncyCastle.Cryptography 2.7.0 automatically. | After upgrading Testcontainers.MsSql, remove the direct BouncyCastle reference. |
| 7–14 | Testcontainers deprecated the parameterless `ContainerBuilder` constructor. | In `TestInitializer.cs`, pass the existing image name to `new ContainerBuilder(image)` and remove `.WithImage(image)`. |
| 1, 6–14 | EF Core packages upgraded to 11.0.0. The local `dotnet-ef` tool also upgraded in chapters 8–14. | Align EF package versions. In chapters 8–14, run `dotnet tool restore` from the chapter's `CleanCart.NET` folder. |
| 8–14 | MudBlazor upgraded to 9.12.0 and Microsoft.Identity.Web to 4.18.0. CodeBeam extensions upgraded to 9.3.1 in chapters 9–14. | Update the UI package references together. |
| 9–14 | MudBlazor deprecated `MudForm.Validate()`. | In `Checkout.razor.cs`, replace `await _form.Validate()` with `await _form.ValidateAsync()`. |
| 9–14 | A targeted audit-warning suppression was added for pinned AutoMapper 14.0.0. | Review the [dependency audit note](#dependency-audit-warning) before applying the suppression. |
| 10–14 | ServiceComposition.NET upgraded from 3.0.0 to 3.0.1. | Update the package reference. |
| 11–14 | Azure.Identity deprecated the implicit managed-identity constructor. | In Key Vault configuration, replace `new ManagedIdentityCredential()` with `new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned)`. |
| 12–14 | EntityAxis upgraded from 2.0.0 to 3.0.2. | Update all EntityAxis package references together. |
| 14 | KubernetesClient override upgraded to 21.0.8 to avoid the vulnerable dependency supplied by HealthChecks.UI. | Update the direct KubernetesClient reference in `Presentation.BSA.csproj` to 21.0.8. |
| 14, pp. 412–415 | Function project migrated to Azure.Functions.Sdk. | Apply the [project setup changes](#chapter-14-project-setup). |
| 14, p. 418 | Parallel solution builds need an SDK-version mapping. | Add the [global.json workaround](#parallel-solution-build-workaround). |

## Dependency audit warning

[Directory.Build.props](../../Directory.Build.props) suppresses warning output for the advisory affecting the book's pinned AutoMapper 14.0.0. The package is still affected. Review the advisory before adopting this suppression in your own application. [AutoMapper advisory](https://github.com/advisories/GHSA-rvv3-g6hj-g44x)

## Chapter 14 project setup

Use Azure Functions tools and templates that support .NET 11. For an existing Function project, change its SDK declaration to the following and remove the Microsoft.Azure.Functions.Worker.Sdk package reference. Reference Microsoft.Azure.Functions.Worker explicitly. [Microsoft migration guidance](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#net-11)

~~~xml
<Project Sdk="Azure.Functions.Sdk/1.0.2">
~~~

Remove the explicit `AzureFunctionsVersion` property because the new SDK sets it to `v4` automatically. Configuration-file entries now use `Content Update` because the SDK includes JSON files as content automatically. See the updated [Function project](../../ch14/CleanCart.NET/src/Presentation.Functions/Presentation.Functions.csproj).

## Parallel solution build workaround

With .NET SDK 11.0.100 and Azure.Functions.Sdk 1.0.2, chapter 14's parallel solution build can fail with `MSB4236`, reporting that `Azure.Functions.Sdk` could not be found.

The SDK generates an extensions project without an SDK version. During parallel builds, MSBuild can evaluate it without access to the outer project's resolved version. The related upstream report covers the same missing-version failure when building the generated project independently. [Azure Functions worker issue #3506](https://github.com/Azure/azure-functions-dotnet-worker/issues/3506)

The repository supplies [global.json](../../ch14/CleanCart.NET/global.json) beside the chapter 14 solution. If you create your own Function project, add this file at your solution root. The template does not generate it.

~~~json
{
  "msbuild-sdks": {
    "Azure.Functions.Sdk": "1.0.2"
  }
}
~~~

Merge this entry into any existing `global.json`, matching the version in your Function project's SDK declaration. The mapping supplies the missing version to the generated project and allows the ordinary parallel solution build to succeed. [MSBuild SDK resolution](https://learn.microsoft.com/en-us/visualstudio/msbuild/how-to-use-project-sdk#how-project-sdks-are-resolved)

**Workaround active as of 2026-11-11.** This note will be updated after an SDK fix has been verified.
