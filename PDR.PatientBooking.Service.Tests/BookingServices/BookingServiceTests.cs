using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDR.PatientBooking.Service.Tests.BookingServices
{
    [TestFixture]
    public class BookingServiceTests
    {
        private MockRepository _mockRepository;
        private MockRepository _mockRepositoryCancel;
        private IFixture _fixture;

        private PatientBookingContext _context;
        private Mock<IAddBookingRequestValidator> _validator;
        private Mock<ICancelBookingRequestValidator> _cancelvalidator;

        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _mockRepositoryCancel = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            //Prevent fixture from generating circular references
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _validator = _mockRepository.Create<IAddBookingRequestValidator>();
            _cancelvalidator = _mockRepositoryCancel.Create<ICancelBookingRequestValidator>();

            // Mock default
            SetupMockDefaults();

            //SeedData
            SeedData();
            // Sut instantiation
            _bookingService = new BookingService(
                _context,
                _validator.Object,
                _cancelvalidator.Object
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

        }

        private void SeedBookingData()
        {
            var booking = CreateBookingRequest();
            var orders = new List<Order>
            {
                new Order
                {
                    Id = booking.Id,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    PatientId = booking.PatientId,
                    DoctorId = booking.DoctorId,
                    SurgeryType = booking.SurgeryType
                }
            };

            _context.Order.AddRange(orders);
            _context.SaveChanges();

        }

        private void SetupMockDefaults()
        {

            _validator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>()))
                .Returns(new PdrValidationResult(true));

            _cancelvalidator.Setup(x => x.ValidateRequest(It.IsAny<Guid>()))
                .Returns(new PdrValidationResult(true));


        }

        private AddBookingRequest CreateBookingRequest()
        {
            //arrange
            var request = new AddBookingRequest
            {
                Id = Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9"),
                DoctorId = _context.Doctor.First().Id,
                PatientId = _context.Patient.First().Id,
                StartTime = new DateTime(2021, 01, 11, 11, 15, 00),
                EndTime = new DateTime(2021, 01, 11, 11, 30, 00),                
                SurgeryType = (int)SurgeryType.SystemOne
            };

            return request;
        }

        [Test]
        public void AddBooking_ValidatesRequest()
        {
            //arrange
            var request = CreateBookingRequest();

            //act
            _bookingService.AddBooking(request);

            //assert
            _validator.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public void AddBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            _validator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.AddBooking(_fixture.Create<AddBookingRequest>()));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }

        [Test]
        public void AddBooking_AddsBookingToContextWithGeneratedId()
        {
            //arrange
            //var request = _fixture.Create<AddBookingRequest>();
            var request = new AddBookingRequest
            {
                Id = Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9"),
                StartTime = new DateTime(2021, 1, 12, 12, 15, 00),
                EndTime = new DateTime(2021, 1, 12, 12, 30, 00),
                DoctorId = 1,
                PatientId = 100,
                SurgeryType = (int)SurgeryType.SystemOne
            };

            var expected = new Order
            {
                Id = Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9"),
                StartTime = new DateTime(2021, 1, 12, 12, 15, 00),
                EndTime = new DateTime(2021, 1, 12, 12, 30, 00),
                PatientId = 100,
                DoctorId = 1,
                SurgeryType = (int)SurgeryType.SystemOne,
                Doctor = _context.Doctor.FirstOrDefault(x => x.Id == request.DoctorId),
                Patient = _context.Patient.FirstOrDefault(x => x.Id == request.PatientId)
            };

            //act
            _bookingService.AddBooking(request);


            //assert
            _context.Order.Should().ContainEquivalentOf(expected, options => options.Excluding(order => order.Id));
        }

        [TestCase(0)]
        [TestCase(null)]
        public void GetAllBookings_NoOrders_ReturnsEmptyList(long patientId)
        {
            //arrange

            //act
            var res = _bookingService.GetAllBookings(patientId);

            //assert
            res.Orders.Should().BeEmpty();
        }

        [Test]
        public void GetAllBookings_ReturnsMappedOrderList()
        {
            //arrange
            var booking = _fixture.Create<Order>();

            _context.Order.Add(booking);
            _context.SaveChanges();
            var clinic = _fixture.Create<Clinic>();

            var expected = new GetAllBookingResponse
            {
                Orders = new List<GetAllBookingResponse.Order>
                {
                    new GetAllBookingResponse.Order
                    {
                        Id = booking.Id,
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime,
                        PatientId = booking.PatientId,
                        DoctorId = booking.DoctorId,
                        SurgeryType = (int)booking.SurgeryType,
                        IsCancelled = booking.IsCancelled
                    }
                }
            };

            //act
            var res = _bookingService.GetAllBookings(booking.PatientId);

            //assert
            res.Should().BeEquivalentTo(expected);
        }


        #region Cancel Booking
        
        [Test]
        public void CancelBooking_ValidatesRequest()
        {
            //arrange
            SeedBookingData();
            var request = CreateBookingRequest();

            //act
            _bookingService.CancelBooking(request.Id);

            //assert
            _cancelvalidator.Verify(x => x.ValidateRequest(request.Id), Times.Once);
        }


        [Test]
        public void CancelBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            _cancelvalidator.Setup(x => x.ValidateRequest(It.IsAny<Guid>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.CancelBooking(new Guid()));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }


        [Test]
        public void CancelBooking_CancelsBookingToContextWithGeneratedId()
        {
            //arrange
            SeedBookingData();
            var request = CreateBookingRequest();

            var expected = true;

            //act
            _bookingService.CancelBooking(request.Id);
            
            //assert
            _context.Order.Select(x => x.IsCancelled).Should().ContainEquivalentOf(expected);
        }
        #endregion

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
