using GuestsLogix.Business.Repository.Exseptions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GuestsLogix.Business.Repository.Routes
{
    public class MySqlRoutesRepository : IRoutesRepository
    {
        public MySqlRoutesRepository()
        { }

        public GuestLogix.Business.Models.Routes.Routes GetRoutes(string orign, string derection)
        {
            const string Sql = "dbo.usp_Dijkstra";
            GuestLogix.Business.Models.Routes.Routes result = null;
            try
            {
                using (var sqlConnection = new SqlConnection("Data Source=(localdb)\\ProjectsV13;Initial Catalog=Guestlogix;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))// more elegant to injest this with DI from Bootstrapper
                {
                    sqlConnection.Open();

                    using (var sqlCommand = new SqlCommand(Sql, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@StartNode", orign);
                        sqlCommand.Parameters.AddWithValue("@EndNode", derection);
                        
                        using (var sqlDataReader = sqlCommand.ExecuteReader())
                        {
                          
                            if (sqlDataReader.Read())
                            {
                                result = new GuestLogix.Business.Models.Routes.Routes { Origin = orign, Destination = derection, Path = sqlDataReader.GetString(0) };


                                }
                            }
                        }
                    }
                }

               
            
            catch (SqlException me)
            {
                if(me.Message == "50005")
                    throw new InvalidOriginExseption();
                else if (me.Message == "50006")
                    throw new InvalidDestinationExseption();
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;


            
        }

    }
}
