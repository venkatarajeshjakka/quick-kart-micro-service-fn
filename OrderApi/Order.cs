using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderApi
{
    public class Order
    {
        [FunctionName("order")]
        public void Run([ServiceBusTrigger("payment-queue",
            Connection = "ConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            dynamic data = JsonConvert.DeserializeObject(myQueueItem);

            string prodCost = data.prodCost;

            string prodID = data.prodId;

            int result = 0;

            //SQL connection

            SqlConnection conObj = new SqlConnection("Data Source=quickkartserver1.database.windows.net;Initial Catalog=QuickKartDB;user id=qkadmin; password=Password@123");

            //command

            SqlCommand cmdObj = new SqlCommand("usp_AddOrder_venkatarajeshjakka", conObj);

            cmdObj.CommandType = CommandType.StoredProcedure;

            try

            {

                SqlParameter prmReturnValue = new SqlParameter();

                prmReturnValue.Direction = ParameterDirection.ReturnValue;

                cmdObj.Parameters.Add(prmReturnValue);

                conObj.Open();

                cmdObj.ExecuteNonQuery();

                int res = Convert.ToInt32(prmReturnValue.Value);

                if (res == 1)

                    result = 1;//it means added

                else

                    result = 0;//error

            }

            catch (Exception e)

            {

                result = -1;

            }

            finally

            {

                conObj.Close();

            }

            log.LogInformation("" + result);
        }
    }
}
