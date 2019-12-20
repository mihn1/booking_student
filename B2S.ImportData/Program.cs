using B2S.Core.Entities;
using B2S.Core.Interfaces;
using B2S.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace B2S.ImportData
{
    class Program
    {
        private static IRepository<BookingData> bookingDataRepo;
        private static IRepository<Room> roomRepo;
        private static IRepository<Building> buildingRepo;
        private static IRepository<Bed> bedRepo;
        private static IRepository<Booking> bookingRepo;
        private static IRepository<User> userRepo;
        private static IRepository<Agent> agentRepo;
        private static IRepository<RoomType> roomTypeRepo;
        private static AppDbContext appDBContext;
        static void Main(string[] args)
        {
            ImportDataAsync();

        }

        private static void ImportDataAsync()
        {
            string sqlConn = "Server=.\\sqlexpress;Database=studenthome;Trusted_Connection=True;MultipleActiveResultSets=true";
            var dbOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(sqlConn)
               .Options;
            appDBContext = new AppDbContext(dbOptions);

            bookingDataRepo = new Repository<BookingData>(appDBContext);
            roomRepo = new Repository<Room>(appDBContext);
            bedRepo = new Repository<Bed>(appDBContext);
            bookingRepo = new Repository<Booking>(appDBContext);
            buildingRepo = new Repository<Building>(appDBContext);
            userRepo = new Repository<User>(appDBContext);
            agentRepo = new Repository<Agent>(appDBContext);
            roomTypeRepo = new Repository<RoomType>(appDBContext);

            try
            {
                Console.Write("Importing ...");

                var agent = agentRepo.FindEntity(a => a.BusinessName == "ELC");
                var user = userRepo.FindEntity(u => u.Email == "super@admin.com");
                var bookingData = bookingDataRepo.GetAllEntity();

                List<Booking> bookings = new List<Booking>();

                foreach (var data in bookingData)
                {
                    var booking = GetBedInfo(data);

                    if (!string.IsNullOrEmpty(booking.BedId))
                    {
                        booking.AgentId = agent.AgentId;
                        booking.BookingFrom = data.StartDate;
                        booking.BookingTo = data.FinishDate;
                        booking.CreatedAt = DateTime.Today;
                        booking.CreatedBy = user.Id;
                        booking.IsConfirmTC = true;
                        booking.IsConfirmOA = true;
                        booking.Email = "TBA@email.com";
                        booking.Nationality = data.Country;

                        string firstName = string.Empty;
                        string lastName = string.Empty;
                        SetName(data.CustomerName, ref firstName, ref lastName);
                        booking.FirstName = firstName;
                        booking.LastName = lastName;
                        booking.Status = Core.Enums.BookingStatus.Confirmed;


                        if (data.Gender == "M")
                        {
                            booking.Gender = Core.Enums.Gender.Male;
                        }
                        else if (data.Gender == "F")
                        {
                            booking.Gender = Core.Enums.Gender.Female;
                        }
                        else
                        {
                            booking.Gender = Core.Enums.Gender.Other;
                        }

                        bookings.Add(booking);
                    }                    

                }

                appDBContext.AddRange(bookings);
                appDBContext.SaveChanges();

                Console.Write("Complete!!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message ?? "Something went wrong");
                Console.ReadLine();

            }

        }

        private static Booking GetBedInfo(BookingData data)
        {
            Booking booking = new Booking();

            if (data.Property.Contains("617 Harris St Level 1"))
            {
                var building = buildingRepo.FindEntity(b => b.Name == "Level 1");
                if (building != null)
                {
                    var room = roomRepo.FindEntity(b => b.RoomName == data.Room && b.BuildingId == building.BuildingId);
                    if (room != null)
                    {
                        var bed = bedRepo.FindEntity(b => b.RoomId == room.RoomId && b.Name == data.Bed);
                        var roomType = roomTypeRepo.FindEntity(r => r.RoomTypeId == room.RoomTypeId && r.PropertyId == room.PropertyId);
                        if (bed != null && roomType != null)
                        {
                            booking.BedId = bed.BedId;
                            booking.Price = roomType.Price;
                            booking.DiscountAmount = roomType.DiscountAmount;
                            booking.DepositAmount = roomType.DepositAmount;
                        }

                    }
                }


            }
            else if (data.Property.Contains("617 Harris St Level 2"))
            {
                var building = buildingRepo.FindEntity(b => b.Name == "Level 2");
                if (building != null)
                {
                    var room = roomRepo.FindEntity(b => b.RoomName == data.Room && b.BuildingId == building.BuildingId);
                    if (room != null)
                    {
                        var bed = bedRepo.FindEntity(b => b.RoomId == room.RoomId && b.Name == data.Bed);
                        var roomType = roomTypeRepo.FindEntity(r => r.RoomTypeId == room.RoomTypeId && r.PropertyId == room.PropertyId);
                        if (bed != null && roomType != null)
                        {
                            booking.BedId = bed.BedId;
                            booking.Price = roomType.Price;
                            booking.DiscountAmount = roomType.DiscountAmount;
                            booking.DepositAmount = roomType.DepositAmount;
                        }

                    }
                }
            }
            else if (data.Property.Contains("617 Harris St Level 3"))
            {
                var building = buildingRepo.FindEntity(b => b.Name == "Level 3");
                if (building != null)
                {
                    var room = roomRepo.FindEntity(b => b.RoomName == data.Room && b.BuildingId == building.BuildingId);
                    if (room != null)
                    {
                        var bed = bedRepo.FindEntity(b => b.RoomId == room.RoomId && b.Name == data.Bed);
                        var roomType = roomTypeRepo.FindEntity(r => r.RoomTypeId == room.RoomTypeId && r.PropertyId == room.PropertyId);
                        if (bed != null && roomType != null)
                        {
                            booking.BedId = bed.BedId;
                            booking.Price = roomType.Price;
                            booking.DiscountAmount = roomType.DiscountAmount;
                            booking.DepositAmount = roomType.DepositAmount;
                        }

                    }
                }
            }
            else if (data.Property.Contains("617 Harris St Level Ground Floor"))
            {
                var building = buildingRepo.FindEntity(b => b.Name == "Ground");
                if (building != null)
                {
                    var room = roomRepo.FindEntity(b => b.RoomName == data.Room && b.BuildingId == building.BuildingId);
                    if (room != null)
                    {
                        var bed = bedRepo.FindEntity(b => b.RoomId == room.RoomId && b.Name == data.Bed);
                        var roomType = roomTypeRepo.FindEntity(r => r.RoomTypeId == room.RoomTypeId && r.PropertyId == room.PropertyId);
                        if (bed != null && roomType != null)
                        {
                            booking.BedId = bed.BedId;
                            booking.Price = roomType.Price;
                            booking.DiscountAmount = roomType.DiscountAmount;
                            booking.DepositAmount = roomType.DepositAmount;
                        }

                    }
                }
            };

            return booking;
        }
        private static void SetName(string FullName, ref string firstName, ref string lastName)
        {
            if (!string.IsNullOrEmpty(FullName))
            {
                string[] names = FullName.Split(' ');
                if (names.Length > 1)
                {
                    lastName = names[0];
                    firstName = FullName.Replace(names[0] + " ", "");

                }

            }
        }
       
    }
}
