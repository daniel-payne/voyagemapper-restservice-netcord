using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.Extensions.Options;

namespace WebApplication5.Services
{

  public interface IDataService {
    void   initilizeSQLCommand(String CommandName);
    void   setupSQLCommand(String Name, SqlDbType Type, Int32? Length, Object Value);
    String processSQLCommand();
  };

  //
  public partial class DataService : IDataService
  {
    private ILogger<DataService> logger;
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public DataService(IHttpContextAccessor HttpContextAccessor, IConfiguration Configration, ILogger<DataService> Logger)
    {
      String address;
      String database;
      String password;

      logger = Logger;

      try
      {
        try
        {
          address  = Configration["SQL_SERVER_ADDRESS" ];
          database = Configration["SQL_SERVER_DATABASE"];
          password = Configration["SQL_SERVER_PASSWORD"];
        }
        catch
        {
          address  = "WS030";
          database = "EatandDoNew";
          password = "123ABC";
        }

        String connectionString = "Server = "                    + "tcp:" + address + ",1433"     + "; " +
                                  "Data Source = "               + address                        + "; " +
                                  "Initial Catalog = "           + database                       + "; " +
                                  "Persist Security Info = "     + "False"                        + "; " +
                                  "User ID = "                   + "risk.service"                 + "; " +
                                  "Password = "                  + password                       + "; " +
                                  "MultipleActiveResultSets = "  + "False"                        + "; " +
                                  "Encrypt = "                   + "True"                         + "; " +
                                  "TrustServerCertificate = "    + "True"                         + "; " +
                                  "Connection Timeout = "        + "30"                           + "; " +
                                  "Pooling = "                   + "true"                         + "; " +
                                  "Connection Lifetime = "       + "86400"                        + ";";



        connection = new SqlConnection(connectionString);
        command = new SqlCommand();

        command.Connection = connection;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 600;

        builder = new StringBuilder();
      }
      catch (Exception e)
      {
        logger.LogCritical("REST Connection Error : " + e.Message);
      }

    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ~DataService()
    {

      if ((connection != null) && (connection.State != ConnectionState.Closed))
      {
        connection.Close();
        connection.Dispose();
      }

    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private SqlCommand    command;
    private SqlConnection connection;
    private StringBuilder builder;
    private SqlParameter  sqlParameterReturn;
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void resetJSONBuilder()
    {

      if (builder != null)
      {
        builder.Clear();
        //
        builder.AppendLine("[");
      }

    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void addJSONRecord()
    {

      builder.Append("{");

    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void addJSONData(String FieldName, Object FieldData)
    {

      if (FieldData.ToString() != String.Empty)
      {

        StringBuilder data = new StringBuilder(FieldData.ToString());

        data.Replace("\"", "\\\"");

        if ((FieldData is Double) || (FieldData is Int32))
        {
          builder.Append(" \"" + FieldName + "\": " + data.ToString() + ", ");
        }
        else
        {
          builder.Append(" \"" + FieldName + "\": \"" + data.ToString() + "\", ");
        };
      }

    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void finiliseJSONRecord()
    {

      builder.Remove(builder.Length - 2, 1);
      builder.AppendLine("},");

    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void finaliseJSONBuilder()
    {

      if (builder.Length > 3)
        builder.Remove(builder.Length - 3, 1);

      builder.AppendLine("]");

    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void initilizeSQLCommand(String CommandName)
    {

      resetJSONBuilder();

      command.CommandText = CommandName;

      command.Parameters.Clear();

      sqlParameterReturn = new SqlParameter();

      sqlParameterReturn.Direction = ParameterDirection.ReturnValue;

      command.Parameters.Add(sqlParameterReturn);

      resetJSONBuilder();

    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void setupSQLCommand(String Name, SqlDbType Type, Int32? Length, Object Value)
    {

      SqlParameter parmUser;

      if (!Length.HasValue)
        parmUser = new SqlParameter("@" + Name, Type);
      else
        parmUser = new SqlParameter("@" + Name, Type, Length.Value);

      parmUser.Direction = ParameterDirection.Input;

      parmUser.Value = Value;

      command.Parameters.Add(parmUser);

    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public String processSQLCommand()
    {

      SqlDataReader dataReader = null;

      if (connection.State == ConnectionState.Closed)
      {
        connection.Open();
      }

      try
      {

        dataReader = command.ExecuteReader(CommandBehavior.SequentialAccess);

        if ((sqlParameterReturn.Value != null) && ((Int32)sqlParameterReturn.Value == 0))
        {
          return String.Empty;
        };

        while (dataReader.Read())
        {

          addJSONRecord();

          for (int i = 0; i < dataReader.FieldCount; i++)
          {
            addJSONData(dataReader.GetName(i).ToString(), dataReader[i]);
          }

          finiliseJSONRecord();
        };

        finaliseJSONBuilder();
      }
      catch (Exception e)
      {
        logger.LogCritical(e.Message);

        throw e;
      }
      finally
      {

        if ( (dataReader != null) && (dataReader.IsClosed == false))
        {
          dataReader.Dispose();
          connection.Close();
        }
      };

      return builder.ToString();

    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  }
}
