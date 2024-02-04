using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ShadmanLegal.Models;

public class DAL
{
    private readonly string connectionString;

    public DAL(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public IEnumerable<StampInstrument> GetAllInstruments()
    {
        using (IDbConnection dbConnection = new SqlConnection(connectionString))
        {
            dbConnection.Open();
            return dbConnection.Query<StampInstrument>("SELECT * FROM stampInstruments");
        }
    }

    public IEnumerable<State> GetAllStates()
    {
        using (IDbConnection dbConnection = new SqlConnection(connectionString))
        {
            dbConnection.Open();
            return dbConnection.Query<State>("SELECT * FROM State");
        }
    }

    public StampDutyDataModel GetAllDataAsync()
    {
        StampDutyDataModel stampDutyDataModel = new StampDutyDataModel();

        try
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                var states = dbConnection.Query<State>("SELECT * FROM States");
                var instruments = dbConnection.Query<StampInstrument>("SELECT * FROM StampInstruments");

                stampDutyDataModel.States = states.ToList();
                stampDutyDataModel.Instruments = instruments.ToList();
            }

            return stampDutyDataModel;
        }
        catch (Exception ex)
        {
            // Log error details here
            throw; // Rethrow to allow handling at a higher level
        }
    }

    //public void AddInstrument(StampInstrument instrument)
    //{
    //    using (SqlConnection connection = new SqlConnection(connectionString))
    //    {
    //        connection.Open();
    //        string insertQuery = "INSERT INTO stampInstruments (InstrumentName, Description) VALUES (@InstrumentName, @Description)";
    //        connection.Execute(insertQuery, instrument);
    //    }
    //}

    // Add other methods for update, delete, and other operations as needed
}
