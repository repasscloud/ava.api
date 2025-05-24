namespace Ava.API.Services
{
    public class TaxValidationService : ITaxValidationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;
        private const string AUBaseUrl = "https://abr.business.gov.au";
    
        public TaxValidationService(IHttpClientFactory httpClientFactory, ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }
    
        public async Task<DateTime?> ValidateTaxRegistrationAsync(string taxRegistrationId, string country)
        {
            switch (country)
            {
                case "Australia":
                {
                    var url = $"{AUBaseUrl}/ABN/View?id={taxRegistrationId}";
                    var httpClient = _httpClientFactory.CreateClient();
                    var html = await httpClient.GetStringAsync(url);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    
                    string GetField(string label)
                    {
                        // grab all <th> nodes once
                        var thNodes = doc.DocumentNode.SelectNodes("//th");
                        if (thNodes == null)
                            return "Not found";

                        foreach (var th in thNodes)
                        {
                            var decodedText = WebUtility.HtmlDecode(th.InnerText).Trim();
                            if (decodedText == label)
                            {
                                // pick up the next <td>
                                var td = th.SelectSingleNode("following-sibling::td");
                                if (td != null)
                                    return WebUtility.HtmlDecode(td.InnerText).Trim();
                            }
                        }

                        return "Not found";
                    }
                
                    var gstStatus = GetField("Goods & Services Tax (GST):");
                    return gstStatus.Contains("Not currently registered for GST") || gstStatus.Contains("Not found")
                        ? (DateTime?)null
                        : DateTime.UtcNow;
                }

                default:
                    // no support for other countries yet
                    return DateTime.MinValue;
            }
        }
    }
}
