using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class CancelBookingRequestValidator : ICancelBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public CancelBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(Guid id)
        {
            var result = new PdrValidationResult(true);


            //Check if booking exist
            if (BookingNotExist(id, ref result))
                return result;

            

            return result;
        }
                

        private bool BookingNotExist(Guid id, ref PdrValidationResult result)
        {
            var booking = _context.Order.Where(x => x.Id == id).FirstOrDefault();
            if (booking == null)
            {
                result.PassedValidation = false;
                result.Errors.Add("Doctor is already busy for selected date & time");
                return true;
            }
            return false;
        }

    }
}
