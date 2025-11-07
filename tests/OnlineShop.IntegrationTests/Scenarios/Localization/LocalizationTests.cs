using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Localization
{
    public class LocalizationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public LocalizationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Farsi_Static_Page_Should_Inject_RTL_CSS_And_Meta()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/fa/index.html");
            req.Headers.AcceptLanguage.ParseAdd("fa-IR");

            var res = await _client.SendAsync(req);
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            res.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
            res.Content.Headers.ContentType!.CharSet.Should().Be("utf-8");

            res.Headers.Contains("Content-Language").Should().BeTrue();
            res.Headers.GetValues("Content-Language").Should().ContainSingle().Which.Should().Be("fa-IR");

            var body = await res.Content.ReadAsStringAsync();
            body.Should().Contain("/css/rtl-global.css");
            body.Should().Contain("charset=\"utf-8\"");
            body.Should().Contain("Content-Language\"\" content=\"fa\"");
        }

        [Fact]
        public async Task Json_Response_With_Farsi_AcceptLanguage_Should_Set_UTF8_And_ContentLanguage()
        {
            var testId = "00000000-0000-0000-0000-000000000001";
            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/productinventory/{testId}");
            req.Headers.AcceptLanguage.Clear();
            req.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fa-IR"));

            var res = await _client.SendAsync(req);

            res.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
            res.Content.Headers.ContentType!.CharSet.Should().Be("utf-8");

            res.Headers.Contains("Content-Language").Should().BeTrue();
            res.Headers.GetValues("Content-Language").Should().ContainSingle().Which.Should().Be("fa-IR");
        }
    }
}

