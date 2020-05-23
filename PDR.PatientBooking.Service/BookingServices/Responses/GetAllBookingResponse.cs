using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.BookingServices.Responses
{
    public class GetAllBookingResponse
    {
        public List<Order> Orders { get; set; }

        public class Order
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public int SurgeryType { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
        }
    }
}
