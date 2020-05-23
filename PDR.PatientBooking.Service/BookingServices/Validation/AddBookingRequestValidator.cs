using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidator : IAddBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public AddBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }
            
        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            //Appointment cannot be in past
            if (IsAppoitmentInPast(request, ref result))
                return result;
            //time based validation on appointment with doctor Or Doctor is already busy for selected time
            if (IsDoctorBusy(request, ref result))
                return result;

            return result;

        }

        private bool IsDoctorBusy(AddBookingRequest request, ref PdrValidationResult result)
        {
            var order = _context.Order
                        .Where(x => x.DoctorId == request.DoctorId &&
                                    x.PatientId == request.PatientId &&
                                   (x.StartTime >= request.StartTime && x.StartTime <= request.EndTime)
                              ).ToList();

            if (order.Count() > 0)
            {
                result.PassedValidation = false;
                result.Errors.Add("Doctor is already busy for selected date & time");
                return true;
            }

            return false;
        }

        private bool IsAppoitmentInPast(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (request.StartTime <= DateTime.Now)
            {
                result.PassedValidation = false;
                result.Errors.Add("An appointment date cannot be in past");
                return true;
            }

            return false;
        }
    }
}
