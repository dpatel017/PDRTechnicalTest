using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public interface ICancelBookingRequestValidator
    {
        PdrValidationResult ValidateRequest(Guid id);
    }
}
