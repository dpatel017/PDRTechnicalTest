using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public interface IAddBookingRequestValidator
    {
        PdrValidationResult ValidateRequest(AddBookingRequest request);
    }
}
