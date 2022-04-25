using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace acr_hello_world.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var registryURL = string.Empty;
            registryURL = Environment.GetEnvironmentVariable("DOCKER_REGISTRY");
            ViewData["REGISTRYURL"] = registryURL;
            if (registryURL != "<acrName>.azurecr.io")
            {
                var hostEntry = await System.Net.Dns.GetHostEntryAsync(registryURL);
                ViewData["HOSTENTRY"] = hostEntry.HostName;

                string region = hostEntry.HostName.Split('.')[1];
                ViewData["REGION"] = region;

                var registryIp = System.Net.Dns.GetHostAddresses(registryURL)[0].ToString();
                ViewData["REGISTRYIP"] = registryIp;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return Page();
    }
}
