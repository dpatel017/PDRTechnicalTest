using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    public class CancelBookingRequestValidatorTests
    {
        private IFixture _fixture;

        private PatientBookingContext _context;

        private CancelBookingRequestValidator _cancelBookingRequestValidator;

        private AddBookingRequest _request;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _cancelBookingRequestValidator = new CancelBookingRequestValidator(
                _context
            );
        }

        private void SeedData()
        {
            var clinics = new List<Clinic>
            {
                new Clinic
                {
                    Id = 12,
                    Name = "Mr Docs Healthcare Bonanza",
                    SurgeryType = SurgeryType.SystemOne
                }
            };

            _context.Clinic.AddRange(clinics);
            _context.SaveChanges();


            var patients = new List<Patient>
            {
                new Patient
                {
                    Id = 100,
                    Gender = 1,
                    FirstName = "Bill",
                    LastName = "Bagly",
                    Email = "BToTheB@gmail.com",
                    DateOfBirth = new DateTime(1912, 1, 17),
                    Clinic = clinics[0],
                    ClinicId = clinics[0].Id,
                    Created = DateTime.UnixEpoch
                }
            };

            _context.Patient.AddRange(patients);
            _context.SaveChanges();

            var doctors = new List<Doctor>
            {
                new Doctor()
                {
                    Id = 1,
                    DateOfBirth = new DateTime(1980, 1, 1),
                    Email = "DrMg@docworld.com",
                    FirstName = "Mac",
                    LastName = "Guffin",
                    Gender = 1,
                    Created = DateTime.UtcNow
                }
            };
            _context.Doctor.AddRange(doctors);
            _context.SaveChanges();

            var orders = new List<Order>
            {
                new Order
                {
                    Id = Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9"),
                    StartTime = new DateTime(2021, 1, 12, 12, 15, 00),
                    EndTime = new DateTime(2021, 1, 12, 12, 30, 00),
                    PatientId = 100,
                    DoctorId = 1,
                    SurgeryType = (int)SurgeryType.SystemOne
                }
            };

            _context.Order.AddRange(orders);
            _context.SaveChanges();

            _request = new AddBookingRequest
            {
                Id = Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9"),
                StartTime = new DateTime(2021, 1, 12, 12, 15, 00),
                EndTime = new DateTime(2021, 1, 12, 12, 30, 00),
                DoctorId = 1,
                PatientId = 100
            };
        }

        private void SetupMockDefaults()
        {
            
        }

        private AddBookingRequest GetValidRequest()
        {
            var request = _fixture.Build<AddBookingRequest>()
                .With(x => x.Id, Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9"))
                .Create();
            return request;
        }

        [Test]
        public void ValidateRequest_AllChecksPass_ReturnsPassedValidationResult()
        {
            //arrange
            SeedData();
            var request = GetValidRequest();
            
            //act
            var res = _cancelBookingRequestValidator.ValidateRequest(request.Id);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        

    }
}
