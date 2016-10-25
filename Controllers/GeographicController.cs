using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;

using WebApplication5.Services;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace WebApplication5.Controllers
{

  [Route("[controller]")]
  public class GeographicController : Controller
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public GeographicController(IHttpContextAccessor HttpContextAccessor, IDataService DataService)
    {
      dataService = DataService;

      context = HttpContextAccessor.HttpContext;
      remoteIpAddress = context.Connection?.RemoteIpAddress?.ToString();

      context.Response.ContentType = "application/json";
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private IDataService dataService;
    private HttpContext context;
    private String remoteIpAddress;
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // TEST /search
    [HttpGet]
    public String GetPing()
    {
      return "[{ data: 'geography', currentTime: '" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss") + "} ]";
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // TEST /geographic/countries
    [HttpGet("countries")]
    public String GetUncuratedDocuments()
    {

      dataService.initilizeSQLCommand("Geographic.ListCountries");

      dataService.setupSQLCommand("CountryList",    SqlDbType.VarChar, 20, "ALL"        );
      dataService.setupSQLCommand("CoverageFormat", SqlDbType.VarChar, 20, "GOOGLEARRAY");

      return dataService.processSQLCommand();
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  }
}
