using Microsoft.EntityFrameworkCore;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDR.PatientBooking.Service.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly PatientBookingContext _context;
        private readonly IAddBookingRequestValidator _validator;
        private readonly ICancelBookingRequestValidator _cancelValidator;

        public BookingService(PatientBookingContext context, 
                                IAddBookingRequestValidator validator,
                                ICancelBookingRequestValidator cancelValidator)
        {
            _context = context;
            _validator = validator;
            _cancelValidator = cancelValidator;
        }

        public void AddBooking(AddBookingRequest request)
        {
            var validationResult = _validator.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            var bookingPatient = _context.Patient.FirstOrDefault(x => x.Id == request.PatientId);
            var bookingDoctor = _context.Doctor.FirstOrDefault(x => x.Id == request.DoctorId);
            var bookingSurgeryType = _context.Patient.FirstOrDefault(x => x.Id == request.PatientId).Clinic.SurgeryType;

            var myBooking = new Order
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                Patient = bookingPatient,
                Doctor = bookingDoctor,
                SurgeryType = (int)bookingSurgeryType
            };

            _context.Order.AddRange(new List<Order> { myBooking });
            _context.SaveChanges();

        }

        public void CancelBooking(Guid id)
        {
            var validationResult = _cancelValidator.ValidateRequest(id);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            var booking = _context.Order.Where(x => x.Id == id).FirstOrDefault();

            //if (booking == null)
            //{
            //    throw new ArgumentException("Booking Not Found");
            //    //Raise Booking not found error
            //}

            booking.IsCancelled = true;
            _context.Order.Update(booking);
            _context.SaveChanges();
        }

        public GetAllBookingResponse GetAllBookings(long patientId)
        {
            var orders = _context
                .Order
                .Where(x => x.PatientId == patientId)
                .Select(x => new GetAllBookingResponse.Order
                {
                    Id = x.Id,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    PatientId = x.PatientId,
                    DoctorId = x.DoctorId,
                    SurgeryType = (int)x.SurgeryType,
                    IsCancelled = x.IsCancelled
                })
                .AsNoTracking()
                .ToList();

            return new GetAllBookingResponse
            {
                Orders = orders
            };
        }


    }
}
