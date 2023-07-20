using codeTestCom.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CodeTestComPopulate.Models
{
    public class RentalRQ
    {
        public string CarId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ContractDeliveryDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ContractReturnDate { get; set; } 
        public string UserId { get; set; }

        public RentalRQ(string carId, DateTime contractDeliveryDate, DateTime contractReturnDate, string userId)
        {
            CarId = carId;
            ContractDeliveryDate = contractDeliveryDate;
            ContractReturnDate = contractReturnDate;
            UserId = userId;
        }
    }
}
