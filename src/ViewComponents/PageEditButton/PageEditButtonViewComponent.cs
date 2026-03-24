using Kentico.Content.Web.Mvc;
using Kentico.Membership;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XperienceCommunity.PageEditButton.Options;


namespace XperienceCommunity.PageEditButton.ViewComponents
{
    public class PageEditButtonViewComponent : ViewComponent
    {
        private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
        private readonly PageEditButtonOptions _options;

        public PageEditButtonViewComponent(
            IWebPageDataContextRetriever webPageDataContextRetriever,
            IOptions<PageEditButtonOptions> options)
        {
            _webPageDataContextRetriever = webPageDataContextRetriever;
            _options = options.Value;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_webPageDataContextRetriever.TryRetrieve(out var data))
            {
                return Content(string.Empty);
            }

            if (HttpContext.Kentico().Preview().Enabled)
            {
                return Content(string.Empty);
            }

            var authenticateResult = await HttpContext.AuthenticateAsync(AdminIdentityConstants.APPLICATION_SCHEME);

            if (authenticateResult.Succeeded &&
                authenticateResult.Principal?.Identity != null &&
                authenticateResult.Principal.Identity.IsAuthenticated)
            {
                var pageUrl = $"/webpages-{data.WebPage.WebsiteChannelID}/{data.WebPage.LanguageName}_{data.WebPage.WebPageItemID}";

                var model = BuildAdminUrl(pageUrl);
                return View(model: model);
            }

            return Content(string.Empty);
        }

        /// <summary>
        /// Builds the admin URL using the configured admin domain if available,
        /// otherwise falls back to a relative URL.
        /// </summary>
        private string BuildAdminUrl(string pageUrl)
        {
            if (string.IsNullOrWhiteSpace(_options.AdminDomain))
            {
                // Fall back to relative URL (current behavior)
                return $"/admin{pageUrl}";
            }

            // Build absolute URL to the configured admin domain
            var scheme = HttpContext.Request.Scheme;
            return $"{scheme}://{_options.AdminDomain}/admin{pageUrl}";
        }
    }
}
