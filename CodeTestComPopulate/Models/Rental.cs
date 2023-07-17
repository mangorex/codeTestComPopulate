using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace codeTestCom.Models
{
    public class Rental
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }
        public string CarId { get; set; }
        public int NumOfContractedDays { get; set; }

        public int NumOfDaysUsed { get; set; }

        public Price? Price { get; set; }
        public CarType CarType { get; set; }
        public bool IsCarReturned { get; set; }
        public string UserId { get; set; }

        public Price CalculatePrice()
        {
            decimal basePrice;
            Price = new Price();

            switch (CarType)
            {
                case CarType.Premium:
                    basePrice = Utils.PREMIUM_PRICE;
                    break;
                case CarType.Suv:
                    if (NumOfContractedDays <= Utils.FIRST_INTERVAL_DAYS)
                    {
                        basePrice = Utils.SUV_PRICE;
                    }
                    else if (NumOfContractedDays <= Utils.SECOND_INTERVAL_DAYS)
                    {
                        basePrice = Utils.SUV_PRICE * Utils.SUV_PRICE_SECOND_INTERVAL;
                    }
                    else
                    {
                        basePrice = Utils.SUV_PRICE * Utils.SUV_PRICE_THIRD_INTERVAL;
                    }

                    break;
                case CarType.Small:
                    if (NumOfContractedDays <= Utils.FIRST_INTERVAL_DAYS)
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

            Price.BasePrice = basePrice * NumOfContractedDays;

            return Price;
        }

        public Price CalculatePriceAndSurcharges(int numOfDaysUsed)
        {
            decimal basePricePerDay;
            decimal extraDayPrice;
            Price = new Price();

            CalculatePrice();
            basePricePerDay = Price.BasePrice/this.NumOfContractedDays;
            this.NumOfDaysUsed = numOfDaysUsed;

            switch (CarType)
            {
                case CarType.Premium:
                    extraDayPrice = Utils.PREMIUM_PRICE_EXTRA;
                    break;
                case CarType.Suv:

                    extraDayPrice = Utils.SUV_PRICE_EXTRA;
                    break;
                case CarType.Small:
                    extraDayPrice = Utils.SMALL_PRICE_EXTRA;
                    break;
                default:
                    throw new NotImplementedException("Invalid car type.");
            }

            if(NumOfDaysUsed>NumOfContractedDays)
            {
                int extraDays = (NumOfDaysUsed - NumOfContractedDays);
                Price.Surcharges = basePricePerDay * extraDays + basePricePerDay * extraDays * extraDayPrice;
            }

            return Price;
        }

        public Rental(string carId, CarType carType, string carPartitionKey, int numOfContractedDays, string userId)
        {
            this.Id = Guid.NewGuid().ToString();
            this.CarId = carId;
            this.CarType = carType;
            this.NumOfContractedDays = numOfContractedDays;
            this.PartitionKey = carPartitionKey + "#" + numOfContractedDays.ToString();
            this.IsCarReturned = false;
            this.UserId = userId;
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
