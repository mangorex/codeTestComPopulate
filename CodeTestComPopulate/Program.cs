﻿using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using codeTestCom.Models;

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
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "RentalDB";
        private string containerId = "Items";

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
            await this.QueryItemsAsync<Car>("BMW");
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
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
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
                int? throughput = await this.container.ReadThroughputAsync();
                if (throughput.HasValue)
                {
                    Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
                    int newThroughput = throughput.Value + 100;
                    // Update throughput
                    await this.container.ReplaceThroughputAsync(newThroughput);
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

            await PopulateItem(car1, car1.Id, car1.PartitionKey);
            await PopulateItem(car2, car2.Id, car2.PartitionKey);
            await PopulateItem(car3, car3.Id, car3.PartitionKey);
            await PopulateItem(car4, car4.Id, car4.PartitionKey);
            await PopulateItem(car5, car5.Id, car5.PartitionKey);
            await PopulateItem(car6, car6.Id, car6.PartitionKey);
            await PopulateItem(car7, car7.Id, car7.PartitionKey);
            await PopulateItem(car8, car8.Id, car8.PartitionKey);
        }
        // </AddItemsToContainerAsync>

        // <PopulateItem>
        /// <summary>
        /// Add one item to the container
        /// </summary>
        private async Task PopulateItem<T>(T item, string id, string partitionKey)
        {
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<T> itemResponse = await this.container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
                Console.WriteLine("Item in database with id: {0} already exists\n", id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<T> itemResponse = await this.container.CreateItemAsync<T>(item, new PartitionKey(partitionKey));

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
        private async Task QueryItemsAsync<T>(string partitionKey)
        {
            
            var sqlQueryText = "SELECT * FROM c WHERE c.partitionKey = '" + partitionKey +"'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<T> queryResultSetIterator = this.container.GetItemQueryIterator<T>(queryDefinition);

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
            ItemResponse<Car> bmwfieldCarResponse = await this.container.ReadItemAsync<Car>("0000BBB", new PartitionKey("BMW"));
            var car = bmwfieldCarResponse.Resource;

            // update rented status from false to true
            car.IsRented = rented;

            // replace the item with the updated content
            bmwfieldCarResponse = await this.container.ReplaceItemAsync<Car>(car, car.Id, new PartitionKey(car.PartitionKey));
            Console.WriteLine("Updated Car [{0},{1}].\n \tBody is now: {2}\n", car.Name, car.Id, bmwfieldCarResponse.Resource);

            if (rented)
            {
                Rental rental = new Rental("1", car.Id, car.Type, car.PartitionKey, 10);
                rental.CalculatePrice();
                await PopulateItem(rental, rental.Id, rental.PartitionKey);
                await QueryItemsAsync<Rental>(rental.PartitionKey);
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
            var id = "1";

            // Delete an item. Note we must provide the partition key value and id of the item to delete
            ItemResponse<Rental> BMWRentalResponse = await this.container.DeleteItemAsync<Rental>(id, new PartitionKey(partitionKeyValue));
            Console.WriteLine("Deleted Rental [{0},{1}]\n", partitionKeyValue, id);

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
    }
}
