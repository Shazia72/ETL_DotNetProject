using ETLBox;
using ETLBox.Csv;
using ETLBox.DataFlow;
using ETLBox.Json;
using ETLBox.SqlServer;
using System.Dynamic;
//using System.Security.AccessControl;

namespace ETLProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SqlConnectionManager sqlConnMan =
             new SqlConnectionManager("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CodeFirst_DB;Integrated Security=True");
            var source =
              new JsonSource<OrderRow>("https://www.etlbox.net/demo/api/orders",
              ResourceType.Http);

            var rowTransformation = new RowTransformation<OrderRow>();
            rowTransformation.TransformationFunc = row => {
                row.Quantity = int.Parse(row.Description.Split(":").ElementAt(1));
                return row;
            };

            var lookup = new LookupTransformation<OrderRow, ExpandoObject>();
            lookup.Source = new CsvSource("files/customer.csv");
            lookup.MatchColumns = new[] {
                new MatchColumn() { LookupSourcePropertyName = "Id",
                        InputPropertyName = "CustomerId" }
                };
            lookup.RetrieveColumns = new[] {
                new RetrieveColumn() { LookupSourcePropertyName = "Name",
                           InputPropertyName = "CustomerName" }
                };

            var multicast = new Multicast<OrderRow>();

            var dbDest = new DbDestination<OrderRow>(sqlConnMan, "Orders");
            var textDest = new TextDestination<OrderRow>("files/order_data.log");
            textDest.WriteLineFunc = row => {
                return $"{row.OrderNumber}\t{row.CustomerName}\t{row.Quantity}";
            };

            //Step2 - linking components
            source.LinkTo(rowTransformation);
            rowTransformation.LinkTo(lookup);
            lookup.LinkTo(multicast);
            multicast.LinkTo(dbDest);
            multicast.LinkTo(textDest, row => row.CustomerName == "Clark Kent", row => row.CustomerName != "Clark Kent");

            //Step3 - executing the network
            Network.Execute(source);  //Shortcut for Network.ExecuteAsync(source).Wait();

        }
    }
}