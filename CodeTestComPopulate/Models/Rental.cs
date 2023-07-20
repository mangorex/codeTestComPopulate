using CodeTestComPopulate.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace codeTestCom.Models
{
    public class Rental
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string CarId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ContractDeliveryDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ContractReturnDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ActualReturnDate { get; set; }

        public Price Price { get; set; }
        public CarType CarType { get; set; }
        public bool IsCarReturned { get; set; }
        public string UserId { get; set; }

        public Price CalculatePrice()
        {
            decimal basePrice;
            Price = new Price();
            int numOfContractedDays = (int)(ContractReturnDate - ContractDeliveryDate).TotalDays;
            switch (CarType)
            {
                case CarType.Premium:
                    basePrice = Utils.PREMIUM_PRICE;
                    break;
                case CarType.Suv:
                    if (numOfContractedDays <= Utils.FIRST_INTERVAL_DAYS)
                    {
                        basePrice = Utils.SUV_PRICE;
                    }
                    else if (numOfContractedDays <= Utils.SECOND_INTERVAL_DAYS)
                    {
                        basePrice = Utils.SUV_PRICE * Utils.SUV_PRICE_SECOND_INTERVAL;
                    }
                    else
                    {
                        basePrice = Utils.SUV_PRICE * Utils.SUV_PRICE_THIRD_INTERVAL;
                    }

                    break;
                case CarType.Small:
                    if (numOfContractedDays <= Utils.FIRST_INTERVAL_DAYS)
                    {
                        basePrice = Utils.SMALL_PRICE;
                    }
                    else
                    {
                        basePrice = Utils.SMALL_PRICE * Utils.SMALL_PRICE_SECOND_INTERVAL;
                    }

                    break;
                default:
                    throw new NotImplementedException("Invalid car type.");
            }

            Price.BasePrice = basePrice * numOfContractedDays;

            return Price;
        }

     
        [JsonConstructor]

        public Rental(string carId, CarType carType, string carPartitionKey, DateTime contractDeliveryDate, DateTime contractReturnDate, string userId)
        {
            this.Id = Guid.NewGuid().ToString();
            this.CarId = carId;
            this.CarType = carType;
            this.ContractDeliveryDate = contractDeliveryDate;
            this.ContractReturnDate = contractReturnDate;
            this.PartitionKey = carPartitionKey;
            this.IsCarReturned = false;
            this.UserId = userId;
            Price = CalculatePrice();
        }

        public Rental(RentalRQ rentalRq, Car car)
        {
            this.Id = Guid.NewGuid().ToString();
            this.CarId = car.Id;
            this.CarType = car.Type;
            this.ContractDeliveryDate = rentalRq.ContractDeliveryDate;
            this.ContractReturnDate = rentalRq.ContractReturnDate;
            this.PartitionKey = car.PartitionKey;
            this.IsCarReturned = false;
            this.UserId = rentalRq.UserId;
            Price = CalculatePrice();
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class Price
    {
        public decimal BasePrice { get; set; }
        public decimal Surcharges { get; set; }
    }
}
