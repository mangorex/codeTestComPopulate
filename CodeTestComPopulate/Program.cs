using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using codeTestCom.Models;
using User = codeTestCom.Models.User;

namespace CodeTestComPopulate
{
    class Program
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container containerCar;
        private Container containerRental;
        private Container containerUser;

        // The name of the database and container we will create
        private string databaseId = "RentalDB";
        private string containerIdCar = "Cars";
        private string containerIdRental = "Rentals";
        private string containerIdUser = "Users";

        private static string _rentalId;

        // <Main>
        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Beginning operations...\n");
                Program p = new Program();
                await p.GetStartedPopulateAsync();

            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            finally
            {
                Console.WriteLine("End of population, press any key to exit.");
                Console.ReadKey();
            }
        }
        // </Main>
        // <GetStartedPopulateAsync>
        /// <summary>
        /// Entry point to call methods that populate on Azure Cosmos DB resources in this project
        /// </summary>
        public async Task GetStartedPopulateAsync()
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CodeTestComPopulate" });

            await this.DeleteDatabaseAndCleanupAsync();
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.ScaleContainerAsync();
            await this.AddItemsToContainerAsync();
            await this.QueryItemsAsync<Car>(this.containerCar, "BMW");
            await this.UpdateCarRented(true);
            await this.DeleteRentalItemAsync();
            //Dispose of CosmosClient
            this.cosmosClient.Dispose();
        }
        // </GetStartedDemoAsync>

        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }
        // </CreateDatabaseAsync>

        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/partitionKey" as the partition key path since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.containerCar = await this.database.CreateContainerIfNotExistsAsync(containerIdCar, "/partitionKey");
            this.containerRental = await this.database.CreateContainerIfNotExistsAsync(containerIdRental, "/partitionKey");
            this.containerUser = await this.database.CreateContainerIfNotExistsAsync(containerIdUser, "/partitionKey");

            Console.WriteLine("Created Container: {0}\n", this.containerCar.Id);
            Console.WriteLine("Created Container: {0}\n", this.containerRental.Id);
            Console.WriteLine("Created Container: {0}\n", this.containerUser.Id);
        }
        // </CreateContainerAsync>

        // <ScaleContainerAsync>
        /// <summary>
        /// Scale the throughput provisioned on an existing Container.
        /// You can scale the throughput (RU/s) of your container up and down to meet the needs of the workload. Learn more: https://aka.ms/cosmos-request-units
        /// </summary>
        /// <returns></returns>
        private async Task ScaleContainerAsync()
        {
            // Read the current throughput
            try
            {
                int? throughput = await this.containerCar.ReadThroughputAsync();
                if (throughput.HasValue)
                {
                    Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
                    int newThroughput = throughput.Value + 100;
                    // Update throughput
                    await this.containerCar.ReplaceThroughputAsync(newThroughput);
                    Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
                }
            }
            catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.BadRequest)
            {
                Console.WriteLine("Cannot read container throuthput.");
                Console.WriteLine(cosmosException.ResponseBody);
            }

        }
        // </ScaleContainerAsync>

        // <AddItemsToContainerAsync>
        /// <summary>
        /// Add Cars items to the container
        /// </summary>
        private async Task AddItemsToContainerAsync()
        {
            // Create a car object for the rental DB
            Car car1 = new Car("0000AAA", "BMW 7", "BMW", CarType.Premium);
            Car car2 = new Car("0000BBB", "BMW 6", "BMW", CarType.Premium);
            Car car3 = new Car("1111AAA", "Nissan Juke", "Nissan", CarType.Suv);
            Car car4 = new Car("1111BBB", "Nissan Juke 2", "Nissan", CarType.Suv);
            Car car5 = new Car("2222AAA", "Skoda Fabia", "Skoda", CarType.Small);
            Car car6 = new Car("3333AAA", "Mercedes Class A", "Mercedes", CarType.Premium);
            Car car7 = new Car("4444AAA", "Dacia Duster", "Dacia", CarType.Suv);
            Car car8 = new Car("5555AAA", "Volkswagen Polo", "Volkswagen", CarType.Small);

            await PopulateItem(car1, car1.Id, car1.PartitionKey, this.containerCar);
            await PopulateItem(car2, car2.Id, car2.PartitionKey, this.containerCar);
            await PopulateItem(car3, car3.Id, car3.PartitionKey, this.containerCar);
            await PopulateItem(car4, car4.Id, car4.PartitionKey, this.containerCar);
            await PopulateItem(car5, car5.Id, car5.PartitionKey, this.containerCar);
            await PopulateItem(car6, car6.Id, car6.PartitionKey, this.containerCar);
            await PopulateItem(car7, car7.Id, car7.PartitionKey, this.containerCar);
            await PopulateItem(car8, car8.Id, car8.PartitionKey, this.containerCar);

            User user1 = new User("Manuel", "Gomez", "5334369R", 33, EnumSex.Male);
            User user2 = new User("Claudia", "Lafita", "5331369R", 29, EnumSex.Female);
            User user3 = new User("Josep", "Monrabà", "5314369R", 34, EnumSex.Male);
            User user4 = new User("Jesus", "Capote", "5313369R", 34, EnumSex.Male);
            User user5 = new User("Paca", "Pepa", "5331319R", 21, EnumSex.Female);
            User user6 = new User("Pepa", "Pujol", "5324329R", 40, EnumSex.Other);

            await PopulateItem(user1, user1.Dni, user1.PartitionKey, this.containerUser);
            await PopulateItem(user2, user2.Dni, user2.PartitionKey, this.containerUser);
            await PopulateItem(user3, user3.Dni, user3.PartitionKey, this.containerUser);
            await PopulateItem(user4, user4.Dni, user4.PartitionKey, this.containerUser);
            await PopulateItem(user5, user5.Dni, user5.PartitionKey, this.containerUser);
            await PopulateItem(user6, user6.Dni, user6.PartitionKey, this.containerUser);
        }
        // </AddItemsToContainerAsync>

        // <PopulateItem>
        /// <summary>
        /// Add one item to the container
        /// </summary>
        private async Task PopulateItem<T>(T item, string id, string partitionKey, Container container)
        {
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<T> itemResponse = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
                Console.WriteLine("Item in database with id: {0} already exists\n", id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container. Note we provide the value of the partition key for this item
                ItemResponse<T> itemResponse = await container.CreateItemAsync<T>(item, new PartitionKey(partitionKey));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", id, itemResponse.RequestCharge);
            }
        }
        // </PopulateItem>


        // <QueryItemsAsync>
        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of brand in the WHERE filter results in a more efficient query
        /// </summary>
        private async Task QueryItemsAsync<T>(Container container, string partitionKey)
        {
            
            var sqlQueryText = "SELECT * FROM c WHERE c.partitionKey = '" + partitionKey +"'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<T> queryResultSetIterator = container.GetItemQueryIterator<T>(queryDefinition);

            List<T> items = new List<T>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (T item in currentResultSet)
                {
                    items.Add(item);
                    Console.WriteLine("\tRead {0}\n", item);
                }
            }
        }
        // </QueryItemsAsync>

        // <ReplaceFamilyItemAsync>
        /// <summary>
        /// Replace an item in the container
        /// </summary>
        private async Task UpdateCarRented(bool rented)
        {
            ItemResponse<Car> bmwfieldCarResponse = await this.containerCar.ReadItemAsync<Car>("0000BBB", new PartitionKey("BMW"));
            var car = bmwfieldCarResponse.Resource;

            // update rented status from false to true
            car.IsRented = rented;

            // replace the item with the updated content
            bmwfieldCarResponse = await this.containerCar.ReplaceItemAsync<Car>(car, car.Id, new PartitionKey(car.PartitionKey));
            Console.WriteLine("Updated Car [{0},{1}].\n \tBody is now: {2}\n", car.Name, car.Id, bmwfieldCarResponse.Resource);

            if (rented)
            {
                User user = await GetUserAsyncByNameSurname("Manuel", "Gomez");
                Rental rental = new Rental(car.Id, car.Type, car.PartitionKey, 10, user.Dni);
                rental.CalculatePrice();
                await PopulateItem(rental, rental.Id, rental.PartitionKey, this.containerRental);
                await QueryItemsAsync<Rental>(this.containerRental, rental.PartitionKey);
                _rentalId = rental.Id;
            }
            
        }
        // </ReplaceCarItemAsync>

        // <DeleteRentalItemAsync>
        /// <summary>
        /// Delete an item in the container
        /// </summary>
        private async Task DeleteRentalItemAsync()
        {
            var partitionKeyValue = "BMW#10";

            // Delete an item. Note we must provide the partition key value and id of the item to delete
            ItemResponse<Rental> BMWRentalResponse = await this.containerRental.DeleteItemAsync<Rental>(_rentalId, new PartitionKey(partitionKeyValue));
            Console.WriteLine("Deleted Rental [{0},{1}]\n", partitionKeyValue, _rentalId);

            await UpdateCarRented(false);
        }
        // </DeleteRentalItemAsync>

        // <DeleteDatabaseAndCleanupAsync>
        /// <summary>
        /// Delete the database
        /// </summary>
        private async Task DeleteDatabaseAndCleanupAsync()
        {
            Database databaseDelete = (Database)this.cosmosClient.GetDatabase(databaseId);
            try
            {
                await databaseDelete.ReadAsync();
                Console.WriteLine("The database exists. Delete database");
                DatabaseResponse databaseResourceResponse = await databaseDelete.DeleteAsync();
                // Also valid: await this.cosmosClient.Databases["FamilyDatabase"].DeleteAsync();

                Console.WriteLine("Deleted Database: {0}\n", this.databaseId);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
            }
        }
        // </DeleteDatabaseAndCleanupAsync>

        private async Task<User> GetUserAsyncByNameSurname(string name, string surname)
        {
            var sqlQueryText = "SELECT * FROM c WHERE c.Name = '" + name + "' AND c.Surname = '" + surname + "'";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<User> queryResultSetIterator = containerUser.GetItemQueryIterator<User>(queryDefinition);

            User user = new User();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<User> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (User item in currentResultSet)
                {
                    user = item;
                    break;
                }
            }

            return user;
        }
    }
}
