using System;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using OnlinePastryShop.Classes;

namespace OnlinePastryShop.Handlers
{
    public class ImageHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            // Get parameters
            string idParam = context.Request.QueryString["id"];
            string typeParam = context.Request.QueryString["type"] ?? "product";
            byte[] imageData = null;

            if (string.IsNullOrEmpty(idParam) || !int.TryParse(idParam, out int id))
            {
                context.Response.StatusCode = 400;
                context.Response.End();
                return;
            }

            try
            {
                // Retrieve image from database
                using (OracleConnection connection = new OracleConnection(OracleDbContext.ConnectionString))
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand())
                    {
                        command.Connection = connection;
                        
                        // Determine which table to query based on type
                        switch (typeParam.ToLower())
                        {
                            case "product":
                                command.CommandText = "SELECT IMAGE FROM PRODUCTS WHERE PRODUCTID = :Id";
                                break;
                            case "category":
                                command.CommandText = "SELECT IMAGE FROM CATEGORIES WHERE CATEGORYID = :Id";
                                break;
                            case "user":
                                command.CommandText = "SELECT PROFILEIMAGE FROM USERS WHERE USERID = :Id";
                                break;
                            default:
                                command.CommandText = "SELECT IMAGE FROM PRODUCTS WHERE PRODUCTID = :Id";
                                break;
                        }

                        command.Parameters.Add(new OracleParameter("Id", id));

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                // Get the binary data
                                OracleBlob blob = reader.GetOracleBlob(0);
                                imageData = blob.Value;
                            }
                        }
                    }
                }

                if (imageData != null && imageData.Length > 0)
                {
                    // Set appropriate content type
                    context.Response.ContentType = "image/jpeg";
                    context.Response.BinaryWrite(imageData);
                }
                else
                {
                    // No image found, return a 404
                    context.Response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                // Log the error
                ErrorLogger.LogError(ex.Message, ex.StackTrace);
                
                // Return a 500 error
                context.Response.StatusCode = 500;
            }
            finally
            {
                context.Response.End();
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
} 