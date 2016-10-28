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
  public class RiskController : Controller
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public RiskController(IHttpContextAccessor HttpContextAccessor, IDataService DataService)
    {
      dataService  = DataService;

      context         = HttpContextAccessor.HttpContext;
      remoteIpAddress = context.Connection?.RemoteIpAddress?.ToString();

      context.Response.ContentType = "application/json";
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private IDataService    dataService;
    private HttpContext     context;
    private String          remoteIpAddress;
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // TEST /search
    [HttpGet]
    public String GetPing()
    {
      return "[{ data: 'risk', currentTime: '" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss")  + "} ]";
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // TEST /risk/documents/uncurated
    [HttpGet("documents/uncurated")]
    public String GetUncuratedDocuments()
    {

      dataService.initilizeSQLCommand("Risk.ListDocumentsForUncurated");

      //dataService.setupSQLCommand("CoverageFormat", SqlDbType.VarChar, 20, "GOOGLEARRAY");

      return dataService.processSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // TEST /risk/documents/uncurated
    [HttpGet("documents/{DocumentID}/facts")]
    public String GetDocumentFacts(Int32 DocumentID)
    {

      dataService.initilizeSQLCommand("Risk.ListFactsForDocument");

      dataService.setupSQLCommand("DocumentID",     SqlDbType.Int,     null,  DocumentID  );
      dataService.setupSQLCommand("CoverageFormat", SqlDbType.VarChar,   20, "GOOGLEARRAY");

      return dataService.processSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    public class FactUpdateJSON
    {
      public String  category { get; set; }
      public Boolean isMerged { get; set; }
      public String  text     { get; set; }
    }
    // TEST /risk/facts/4828?FactCategory=DISCARDED
    [HttpPut("facts/{FactID}")]
    public String PutFactCategory(Int32 FactID, [FromBody] FactUpdateJSON Update)
    {

      if (Update.category != null)
      {

        dataService.initilizeSQLCommand("Risk.PutFactCategory");

        dataService.setupSQLCommand("FactID",             SqlDbType.Int,     null,  FactID         );
        dataService.setupSQLCommand("Category",           SqlDbType.VarChar,  255,  Update.category);

        return dataService.processSQLCommand();

      }
      else if (Update.isMerged == true)
      {

        dataService.initilizeSQLCommand("Risk.PutFactMerge");

        dataService.setupSQLCommand("FactID", SqlDbType.Int, null, FactID);

        return dataService.processSQLCommand();

      }
      else if (Update.text != null)
      {

        dataService.initilizeSQLCommand("Risk.PutFactText");

        dataService.setupSQLCommand("FactID",       SqlDbType.Int,    null, FactID     );
        dataService.setupSQLCommand("Text",         SqlDbType.VarChar,  -1, Update.text);

        return dataService.processSQLCommand();

      }

      return null;

    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  }
}
