using B2S.Core.Entities;
using B2S.Core.Interfaces;
using B2S.Core.Common;
using B2S.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using MimeKit;
using B2S.Core.Enums;
using System.Linq;

namespace B2S.Infrastructure.Services
{
    public class UtilityService : IUtilityService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly IRoles _roles;
        private readonly SuperAdminDefaultOptions _superAdminDefaultOptions;

        public UtilityService(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            SignInManager<User> signInManager,
            IRoles roles,
            IOptions<SuperAdminDefaultOptions> superAdminDefaultOptions)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _signInManager = signInManager;
            _roles = roles;
            _superAdminDefaultOptions = superAdminDefaultOptions.Value;
        }

        public async Task CreateNewRoles()
        {
            try
            {
                foreach (var item in typeof(Core.Common.Modules).GetNestedTypes())
                {
                    var roleName = item.Name;
                    if (!await _roleManager.RoleExistsAsync(roleName)) { await _roleManager.CreateAsync(new IdentityRole(roleName)); }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateUserRoles(User user, string userProfileId)
        {
            try
            {
                if (!string.IsNullOrEmpty(userProfileId))
                {
                    //delete or add new roles if user profile is changed
                    var newRoleIds = _context.ProfileRole.Where(r => r.UserProfileId == userProfileId).Select(r => r.RoleId);
                    var oldRoleIds = _context.UserRoles.Where(r => r.UserId == user.Id).Select(r => r.RoleId);

                    //add new role to application user if the role is not existing in profile role table
                    foreach (var roleId in await newRoleIds.Except(oldRoleIds).ToListAsync())
                    {
                        var role = await _context.Roles.FindAsync(roleId);
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }

                    //delete role to application user if the role is exsting in profile role table
                    foreach (var roleId in await oldRoleIds.Except(newRoleIds).ToListAsync())
                    {
                        var role = await _context.Roles.FindAsync(roleId);
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SendEmailBySendGridAsync(string apiKey, string fromEmail, string fromFullName, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(fromEmail, fromFullName),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email, email));
            await client.SendEmailAsync(msg);

        }

        public async Task SendEmailByGmailAsync(string fromEmail,
            string fromFullName,
            string subject,
            string messageBody,
            string toEmail,
            string toFullName,
            string smtpUser,
            string smtpPassword,
            string smtpHost,
            int smtpPort,
            bool smtpSSL)
        {
            var body = messageBody;
            var message = new MailMessage();
            message.To.Add(new MailAddress(toEmail, toFullName));
            message.From = new MailAddress(fromEmail, fromFullName);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = smtpUser,
                    Password = smtpPassword
                };
                smtp.Credentials = credential;
                smtp.Host = smtpHost;
                smtp.Port = smtpPort;
                smtp.EnableSsl = smtpSSL;
                await smtp.SendMailAsync(message);
            }

        }

        public async Task SendEmailByMailKitAsync(string fromEmail,
           string fromFullName,
           string subject,
           string messageBody,
           string toEmail,
           string toFullName,
           string smtpUser,
           string smtpPassword,
           string smtpHost,
           int smtpPort,
           bool smtpSSL)
        {
            if (string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(fromEmail)) return;

            MimeMessage message = new MimeMessage();
            message.To.Add(new MailboxAddress(toFullName ?? "", toEmail));
            message.From.Add(new MailboxAddress(fromFullName ?? "", fromEmail));
            message.Subject = subject;
            var builder = new BodyBuilder
            {
                HtmlBody = messageBody,

            };
            message.Body = builder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(smtpHost, smtpPort, smtpSSL);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.AuthenticateAsync(smtpUser, smtpPassword);

                await client.SendAsync(message);

                //await client.DisconnectAsync(true);
            }
        }

        public async Task SendEmailByMailKitWithDocAsync(string fromEmail,
          string fromFullName,
          string subject,
          string messageBody,
          string toEmail,
          string toFullName,
          string smtpUser,
          string smtpPassword,
          string smtpHost,
          int smtpPort,
          bool smtpSSL,
          string documentName,
          byte[] documentBytes)
        {
            if (string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(fromEmail)) return;

            MimeMessage message = new MimeMessage();
            message.To.Add(new MailboxAddress(toFullName, toEmail));
            message.From.Add(new MailboxAddress(fromFullName, fromEmail));
            message.Subject = subject;
            var builder = new BodyBuilder
            {
                HtmlBody = messageBody,
            };
            if (documentBytes.Length > 0)
            {
                builder.Attachments.Add(documentName ?? "", documentBytes);
            }
            message.Body = builder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(smtpHost, smtpPort, smtpSSL);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.AuthenticateAsync(smtpUser, smtpPassword);

                await client.SendAsync(message);

                //await client.DisconnectAsync(true);
            }
        }

        public async Task SendEmailByMailKitWithPropertyDocsAsync(string fromEmail,
         string fromFullName,
         string subject,
         string messageBody,
         string toEmail,
         string toFullName,
         string smtpUser,
         string smtpPassword,
         string smtpHost,
         int smtpPort,
         bool smtpSSL,
         List<PropertyDocument> documents)
        {
            if (string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(fromEmail)) return;

            MimeMessage message = new MimeMessage();
            message.To.Add(new MailboxAddress(toFullName ?? "", toEmail));
            message.From.Add(new MailboxAddress(fromFullName ?? "", fromEmail));
            message.Subject = subject;
            var builder = new BodyBuilder
            {
                HtmlBody = messageBody ?? "",
            };

            foreach (var document in documents)
            {
                if (document.FileBody.Length > 0)
                {
                    builder.Attachments.Add(document.FileName, document.FileBody);
                }
            }
           
            message.Body = builder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(smtpHost, smtpPort, smtpSSL);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.AuthenticateAsync(smtpUser, smtpPassword);

                await client.SendAsync(message);

                //await client.DisconnectAsync(true);
            }
        }

        public async Task<bool> IsAccountActivatedAsync(string email, UserManager<User> userManager)
        {
            bool result = false;
            try
            {
                var user = await userManager.FindByNameAsync(email);
                if (user != null)
                {
                    //Add this to check if the email was confirmed.
                    if (await userManager.IsEmailConfirmedAsync(user))
                    {
                        result = true;
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }


        public async Task<string> UploadFile(List<IFormFile> files, IHostingEnvironment env)
        {
            var result = "";

            var webRoot = env.WebRootPath;
            var uploads = System.IO.Path.Combine(webRoot, "uploads");
            var extension = "";
            var filePath = "";
            var fileName = "";


            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    extension = System.IO.Path.GetExtension(formFile.FileName);
                    fileName = Guid.NewGuid().ToString() + extension;
                    filePath = System.IO.Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    result = fileName;

                }
            }

            return result;
        }

        public async Task UpdateRoles(User appUser,
            User currentLoginUser)
        {
            try
            {
                await _roles.UpdateRoles(appUser, currentLoginUser);

                //so no need to manually re-signIn to make roles changes effective
                if (currentLoginUser.Id == appUser.Id)
                {
                    await _signInManager.SignInAsync(appUser, false);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CreateDefaultSuperAdmin()
        {
            try
            {
                User superAdmin = new User();
                superAdmin.Email = _superAdminDefaultOptions.Email;
                superAdmin.UserName = superAdmin.Email;
                superAdmin.EmailConfirmed = true;
                superAdmin.IsSuperAdmin = true;
                superAdmin.UserType = Core.Enums.UserType.Admin;
                superAdmin.FirstName = "Quoc";
                superAdmin.LastName = "Ngo";
                superAdmin.PhoneNumber = "0412 716 870";
                superAdmin.ProfilePictureUrl = "/images/userprofile/superadmin.jpg";


                await _userManager.CreateAsync(superAdmin, _superAdminDefaultOptions.Password);

                var roles = await _roleManager.Roles.ToListAsync();

                foreach (var role in roles)
                {
                    await _userManager.AddToRoleAsync(superAdmin, role.Name);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task SetupInvoiceSettings()
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Email == "super@admin.com");

                InvoiceSetting invoiceSetting = new InvoiceSetting();
                invoiceSetting.AutoSend = true;
                invoiceSetting.BankName = "National Australia Bank";
                invoiceSetting.BSBNumber = "82-356";
                invoiceSetting.AccountNumber = "2-497-6015";
                invoiceSetting.AccountName = "HB City Home Pty Ltd";
                invoiceSetting.BusinessName = "Student Home";
                invoiceSetting.BusinessNumber = "72 838 140 148";
                invoiceSetting.ContactName = "Ann Tran";
                invoiceSetting.Phone = "(02) 9299 1133";
                invoiceSetting.Email = "ann.tran@studenthome.com.au";
                invoiceSetting.Address = "617 Harris Street";
                invoiceSetting.City = "Ultimo";
                invoiceSetting.Province = "NSW";
                invoiceSetting.Postcode = "2007";
                invoiceSetting.Country = "Australia";
                invoiceSetting.RecurringPeriod = 2;
                invoiceSetting.RecurringType = RecurringInvoiceType.Weeks;
                invoiceSetting.CreatedAt = DateTime.Today;
                invoiceSetting.CreatedBy = user.Id;
                invoiceSetting.Currency = "AUD";
                invoiceSetting.Description = "Invoice Setting for Agent";
                invoiceSetting.Name = "Agent Invoice";
                invoiceSetting.PaymentMethod = PaymentMethod.BankTransfer;
                invoiceSetting.PaymentTerm = 15;
                invoiceSetting.TaxCode = "GST";
                invoiceSetting.TaxPercentage = 10;
                invoiceSetting.Website = "www.studenthome.com.au";
                invoiceSetting.CurrentInvoiceNo = "00000001";
                invoiceSetting.IsActive = true;

                await _context.InvoiceSetting.AddAsync(invoiceSetting);
                await _context.SaveChangesAsync();



            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SetupExpense()
        {
            try
            {
                List<ExpenseCategory> expenseCategories = new List<ExpenseCategory>()
                {
                    new ExpenseCategory{CategoryName = "IT and Internet Expenses", AccountCode = "TBA", Description = "This category includes all expense for IT and Internet ", ColorHex = ""},
                    new ExpenseCategory{CategoryName = "Telephone Expense", AccountCode = "TBA", Description = "This category includes all telephone bills", ColorHex = ""},
                    new ExpenseCategory{CategoryName = "Travel Expense", AccountCode = "TBA", Description = "This category includes all travel expenses", ColorHex = ""},
                    new ExpenseCategory{CategoryName = "Laundry Expenses", AccountCode = "TBA", Description = "This category includes all laundry expenses", ColorHex = ""},
                    new ExpenseCategory{CategoryName = "Cleaning Expenses", AccountCode = "TBA", Description = "This category includes all cleaning expenses", ColorHex = ""},
                    new ExpenseCategory{CategoryName = "Linen Expenses", AccountCode = "TBA", Description = "This category includes Towels expenses, Pillow expenses, Blankets expenses, Sheets expenses", ColorHex = ""},
                    new ExpenseCategory{CategoryName = "Guests Supplies Expenses", AccountCode = "TBA", Description = "This category includes the various guest supplies provided free of charge to guests in their rooms.", ColorHex = ""},
                    new ExpenseCategory{CategoryName = "Stationary Expenses", AccountCode = "TBA", Description = "This category includes office supplies, printed manuals and guidelines, etc.", ColorHex = ""},
                    new ExpenseCategory{CategoryName = "Other Expenses", AccountCode = "TBA", Description = "", ColorHex = ""}

                };

                _context.ExpenseCategory.AddRange(expenseCategories);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task SetupUserVendor()
        {
            try
            {
                User userVendor = new User();
                userVendor.Email = "binh.tran@set-edu.com";
                userVendor.UserName = userVendor.Email;
                userVendor.EmailConfirmed = true;
                userVendor.IsSuperAdmin = false;
                userVendor.UserType = Core.Enums.UserType.Vendor;
                userVendor.FirstName = "Binh";
                userVendor.LastName = "Tran";
                userVendor.PhoneNumber = "(02) 9299 1133";
                userVendor.ProfilePictureUrl = "/images/userprofile/superadmin.jpg";


                await _userManager.CreateAsync(userVendor, _superAdminDefaultOptions.Password);

                var roles = await _roleManager.Roles.ToListAsync();

                foreach (var role in roles)
                {
                    await _userManager.AddToRoleAsync(userVendor, role.Name);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task SetupUserAgent()
        {
            try
            {
                User userAgent = new User();
                userAgent.Email = "marcia@elc.edu.au";
                userAgent.UserName = userAgent.Email;
                userAgent.EmailConfirmed = true;
                userAgent.IsSuperAdmin = false;
                userAgent.UserType = Core.Enums.UserType.Agent;
                userAgent.FirstName = "Marcia";
                userAgent.LastName = "Almeida";
                userAgent.PhoneNumber = "(02) 9267 5822";
                userAgent.ProfilePictureUrl = "/images/userprofile/elcuser.jpg";


                await _userManager.CreateAsync(userAgent, _superAdminDefaultOptions.Password);

                var roles = await _roleManager.Roles.ToListAsync();

                foreach (var role in roles)
                {
                    await _userManager.AddToRoleAsync(userAgent, role.Name);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task SetupUserProfile()
        {
            try
            {
                //Create User Profile
                List<UserProfile> userProfiles = new List<UserProfile>()
                {
                    new UserProfile{UserProfileName = B2S.Core.Common.Constants.UserProfile.Agent, Description = "User group for Agents"},
                    new UserProfile{UserProfileName = B2S.Core.Common.Constants.UserProfile.Customer, Description = "User group for Customers"},
                    new UserProfile{UserProfileName = B2S.Core.Common.Constants.UserProfile.Vendor, Description = "User group for Vendors"},
                    new UserProfile{UserProfileName = B2S.Core.Common.Constants.UserProfile.Admin, Description = "User group for Administrators"}

                };

                _context.UserProfile.AddRange(userProfiles);
                await _context.SaveChangesAsync();

                //Create Role
                foreach (var item in typeof(Core.Common.Modules).GetNestedTypes())
                {
                    var roleName = item.Name;
                    if (!await _roleManager.RoleExistsAsync(roleName)) { await _roleManager.CreateAsync(new IdentityRole(roleName)); }
                }

                //Create Extra Roles
                if (!await _roleManager.RoleExistsAsync("CheckIn")) { await _roleManager.CreateAsync(new IdentityRole("CheckIn")); }
                if (!await _roleManager.RoleExistsAsync("CheckOut")) { await _roleManager.CreateAsync(new IdentityRole("CheckOut")); }

                //Create Profile Role
                List<ProfileRole> profileRoles = new List<ProfileRole>();
                var userProfileList = await _context.UserProfile.ToListAsync();
                var roleList = await _roleManager.Roles.ToListAsync();
                foreach (var uProfile in userProfileList)
                {
                    foreach (var role in roleList)
                    {
                        profileRoles.Add(new ProfileRole { UserProfileId = uProfile.UserProfileId, RoleId = role.Id });
                    }
                }
                _context.ProfileRole.AddRange(profileRoles);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task InitApp()
        {
            await SetupUserProfile();
            await CreateDefaultSuperAdmin();
            await SetupInvoiceSettings();
            await SetupUserVendor();
            await SetupUserAgent();
            await SetUpVendors();
            await SetUpAgents();
            await SetupPropertyData();
            await SetupExpense();
            
        }

        public async Task SetUpVendors()
        {
            try
            {
                Vendor vendor = new Vendor { VendorName = "HB City Home Pty Ltd ", ContactName = "Binh Tran", Address = "617 Harris Street", City = "Sydney", Country = "Australia", Email = "", Phone = "(02) 9299 1133", Postcode = "2007", Province = "NSW", IsActive = true };
                await _context.AddAsync(vendor);

                var userVendor = await _context.User.FirstOrDefaultAsync(u => u.Email == "binh.tran@set-edu.com");
                if (userVendor != null)
                {
                    UserAccount userAccount = new UserAccount() { UserId = userVendor.Id, AccountId = vendor.VendorId };
                    await _context.AddAsync(userAccount);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task SetUpAgents()
        {
            try
            {
                string invoiceSettingId = string.Empty;
                var invoiceSetting = await _context.InvoiceSetting.FirstOrDefaultAsync();

                if (invoiceSetting != null)
                    invoiceSettingId = invoiceSetting.InvoiceSettingId;

                Agent agent = new Agent { BusinessName = "ELC", LegalName= "English Language Company", ContactName = "Marcia Almeida", Address = "495 Kent Street", City = "Sydney", Country = "Australia", Email = "binh.tran@set-edu.com", Phone = "(02) 9299 1133", Postcode = "2000", Province = "NSW", IsActive = true, InvoiceSettingId = invoiceSettingId };
                await _context.AddAsync(agent);

                var useragent = await _context.User.FirstOrDefaultAsync(u => u.Email == "marcia@elc.edu.au");
                if (useragent != null)
                {
                    UserAccount userAccount = new UserAccount() { UserId = useragent.Id, AccountId = agent.AgentId };
                    await _context.AddAsync(userAccount);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SetupPropertyData()
        {
            try
            {
                var vendor = await _context.Vendor.FirstOrDefaultAsync();
                if (vendor != null)
                {
                    var property = new Property { PropertyName = "617 Harris Street - Ultimo", PropertyType = Core.Enums.PropertyType.House, VendorId = vendor.VendorId, Address = "617 Harris Street", City = "Ultimo", Province = "NSW", Postcode = "2007", Country = "Australia" };
                    await _context.AddAsync(property);
                    await _context.SaveChangesAsync();

                    //Setup Room Type
                    List<RoomType> roomTypes = new List<RoomType>();
                    var singleRoomType = new RoomType() { PropertyId = property.PropertyId, RoomTypeName = "Single", RoomTypeValue = "S", Price = 365 };
                    var twinRoomType = new RoomType() { PropertyId = property.PropertyId, RoomTypeName = "Twin", RoomTypeValue = "TW", Price = 295 };
                    var tripleRoomType = new RoomType() { PropertyId = property.PropertyId, RoomTypeName = "Triple", RoomTypeValue = "TR", Price = 280 };
                    roomTypes.Add(singleRoomType);
                    roomTypes.Add(twinRoomType);
                    roomTypes.Add(tripleRoomType);

                    await _context.AddRangeAsync(roomTypes);
                    await _context.SaveChangesAsync();

                    //Setup Building
                    List <Building> buildings = new List<Building>();
                    buildings.Add(new Building { Name = "Level 1", Description = "This level has 3 twin room and 1 single room", PropertyId = property.PropertyId });
                    buildings.Add(new Building { Name = "Level 2", Description = "This level has 2 twin room and 6 single room", PropertyId = property.PropertyId });
                    buildings.Add(new Building { Name = "Level 3", Description = "This level has 2 twin room and 5 single room", PropertyId = property.PropertyId });
                    buildings.Add(new Building { Name = "Ground", Description = "This level has 3 twin room and 1 single room", PropertyId = property.PropertyId });

                    await _context.AddRangeAsync(buildings);
                    await _context.SaveChangesAsync();

                    // Create Ground
                    var ground = await _context.Building.FirstOrDefaultAsync(b => b.Name == "Ground");
                    if (ground != null)
                    {
                        var groundRoom1 = new Room { RoomName = "1", BuildingId = ground.BuildingId, PropertyId = ground.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(groundRoom1);
                        List<Bed> groundRoom1beds = new List<Bed>
                        {
                           new Bed{Name = "1", RoomId = groundRoom1.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "2", RoomId = groundRoom1.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(groundRoom1beds);


                        var groundRoom2 = new Room { RoomName = "2", BuildingId = ground.BuildingId, PropertyId = ground.PropertyId, RoomTypeId = twinRoomType.RoomTypeId, Description = "This room has price of $ 295" };
                        await _context.AddAsync(groundRoom2);
                        List<Bed> groundRoom2beds = new List<Bed>
                        {
                           new Bed{Name = "3", RoomId = groundRoom2.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "4", RoomId = groundRoom2.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(groundRoom2beds);

                        var groundRoom3 = new Room { RoomName = "3", BuildingId = ground.BuildingId, PropertyId = ground.PropertyId, RoomTypeId = tripleRoomType.RoomTypeId, Description = "This room has price of $ 280" };
                        await _context.AddAsync(groundRoom3);
                        List<Bed> groundRoom3beds = new List<Bed>
                        {
                           new Bed{Name = "5", RoomId = groundRoom3.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "6", RoomId = groundRoom3.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "7", RoomId = groundRoom3.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(groundRoom3beds);

                        var groundRoom4 = new Room { RoomName = "4", BuildingId = ground.BuildingId, PropertyId = ground.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(groundRoom4);
                        List<Bed> groundRoom4beds = new List<Bed>
                        {
                           new Bed{Name = "8", RoomId = groundRoom4.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "9", RoomId = groundRoom4.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(groundRoom4beds);
                    }

                    // Create Level 1
                    var level1 = await _context.Building.FirstOrDefaultAsync(b => b.Name == "Level 1");
                    if (level1 != null)
                    {
                        var level1Room1 = new Room { RoomName = "1", BuildingId = level1.BuildingId, PropertyId = level1.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level1Room1);
                        List<Bed> level1Room1beds = new List<Bed>
                        {
                           new Bed{Name = "1", RoomId = level1Room1.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "2", RoomId = level1Room1.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level1Room1beds);

                        var level1Room2 = new Room { RoomName = "2", BuildingId = level1.BuildingId, PropertyId = level1.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level1Room2);
                        List<Bed> level1Room2beds = new List<Bed>
                        {
                           new Bed{Name = "3", RoomId = level1Room2.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "4", RoomId = level1Room2.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level1Room2beds);


                        var level1Room3 = new Room { RoomName = "3", BuildingId = level1.BuildingId, PropertyId = level1.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level1Room3);
                        List<Bed> level1Room3beds = new List<Bed>
                        {
                           new Bed{Name = "5", RoomId = level1Room3.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "6", RoomId = level1Room3.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level1Room3beds);


                        var level1Room4 = new Room { RoomName = "4", BuildingId = level1.BuildingId, PropertyId = level1.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level1Room4);
                        List<Bed> level1Room4beds = new List<Bed>
                        {
                           new Bed{Name = "7", RoomId = level1Room4.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level1Room4beds);
                    }

                    // Create Level 2
                    var level2 = await _context.Building.FirstOrDefaultAsync(b => b.Name == "Level 2");
                    if (level2 != null)
                    {
                        var level2Room1 = new Room { RoomName = "1", BuildingId = level2.BuildingId, PropertyId = level2.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level2Room1);
                        List<Bed> level2Room1beds = new List<Bed>
                        {
                           new Bed{Name = "1", RoomId = level2Room1.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level2Room1beds);

                        var level2Room2 = new Room { RoomName = "2", BuildingId = level2.BuildingId, PropertyId = level2.PropertyId, RoomTypeId = tripleRoomType.RoomTypeId, Description = "This room has price of $ 280" };
                        await _context.AddAsync(level2Room2);
                        List<Bed> level2Room2beds = new List<Bed>
                        {
                           new Bed{Name = "2", RoomId = level2Room2.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "3", RoomId = level2Room2.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level2Room2beds);


                        var level2Room3 = new Room { RoomName = "3", BuildingId = level2.BuildingId, PropertyId = level2.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level2Room3);
                        List<Bed> level2Room3beds = new List<Bed>
                        {
                           new Bed{Name = "4", RoomId = level2Room3.RoomId, Status = Core.Enums.BedStatus.Ready}


                        };
                        await _context.AddRangeAsync(level2Room3beds);


                        var level2Room4 = new Room { RoomName = "4", BuildingId = level2.BuildingId, PropertyId = level2.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level2Room4);
                        List<Bed> level2Room4beds = new List<Bed>
                        {
                           new Bed{Name = "5", RoomId = level2Room4.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level2Room4beds);


                        var level2Room5 = new Room { RoomName = "5", BuildingId = level2.BuildingId, PropertyId = level2.PropertyId, RoomTypeId = twinRoomType.RoomTypeId, Description = "This room has price of $ 295" };
                        await _context.AddAsync(level2Room5);
                        List<Bed> level2Room5beds = new List<Bed>
                        {
                           new Bed{Name = "6", RoomId = level2Room4.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level2Room5beds);


                        var level2Room6 = new Room { RoomName = "6", BuildingId = level2.BuildingId, PropertyId = level2.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level2Room6);
                        List<Bed> level2Room6beds = new List<Bed>
                        {
                           new Bed{Name = "7", RoomId = level2Room6.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level2Room6beds);


                        var level2Room7 = new Room { RoomName = "7", BuildingId = level2.BuildingId, PropertyId = level2.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level2Room7);
                        List<Bed> level2Room7beds = new List<Bed>
                        {
                           new Bed{Name = "8", RoomId = level2Room7.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level2Room7beds);

                        var level2Room8 = new Room { RoomName = "8", BuildingId = level2.BuildingId, PropertyId = level2.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level2Room8);
                        List<Bed> level2Room8beds = new List<Bed>
                        {
                           new Bed{Name = "9", RoomId = level2Room8.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "10", RoomId = level2Room8.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level2Room7beds);
                    }

                    // Create Level 3
                    var level3 = await _context.Building.FirstOrDefaultAsync(b => b.Name == "Level 3");
                    if (level3 != null)
                    {
                        var level3Room1 = new Room { RoomName = "1", BuildingId = level3.BuildingId, PropertyId = level3.PropertyId, RoomTypeId = twinRoomType.RoomTypeId, Description = "This room has price of $ 295" };
                        await _context.AddAsync(level3Room1);
                        List<Bed> level3Room1beds = new List<Bed>
                        {
                           new Bed{Name = "1", RoomId = level3Room1.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "2", RoomId = level3Room1.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level3Room1beds);

                        var level3Room2 = new Room { RoomName = "2", BuildingId = level3.BuildingId, PropertyId = level3.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level3Room2);
                        List<Bed> level3Room2beds = new List<Bed>
                        {
                           new Bed{Name = "3", RoomId = level3Room2.RoomId, Status = Core.Enums.BedStatus.Ready}
                        };
                        await _context.AddRangeAsync(level3Room2beds);


                        var level3Room3 = new Room { RoomName = "3", BuildingId = level3.BuildingId, PropertyId = level3.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level3Room3);
                        List<Bed> level3Room3beds = new List<Bed>
                        {
                           new Bed{Name = "4", RoomId = level3Room3.RoomId, Status = Core.Enums.BedStatus.Ready}


                        };
                        await _context.AddRangeAsync(level3Room3beds);


                        var level3Room4 = new Room { RoomName = "4", BuildingId = level3.BuildingId, PropertyId = level3.PropertyId, RoomTypeId = twinRoomType.RoomTypeId, Description = "This room has price of $ 295" };
                        await _context.AddAsync(level3Room4);
                        List<Bed> level3Room4beds = new List<Bed>
                        {
                           new Bed{Name = "5", RoomId = level3Room4.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level3Room4beds);


                        var level3Room5 = new Room { RoomName = "5", BuildingId = level3.BuildingId, PropertyId = level3.PropertyId, RoomTypeId = singleRoomType.RoomTypeId, Description = "This room has price of $ 365" };
                        await _context.AddAsync(level3Room5);
                        List<Bed> level3Room5beds = new List<Bed>
                        {
                           new Bed{Name = "6", RoomId = level3Room4.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level3Room5beds);


                        var level3Room6 = new Room { RoomName = "6", BuildingId = level3.BuildingId, PropertyId = level3.PropertyId, RoomTypeId = tripleRoomType.RoomTypeId, Description = "This room has price of $ 280" };
                        await _context.AddAsync(level3Room6);
                        List<Bed> level3Room6beds = new List<Bed>
                        {
                           new Bed{Name = "7", RoomId = level3Room6.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level3Room6beds);

                        var level3Room7 = new Room { RoomName = "7", BuildingId = level3.BuildingId, PropertyId = level3.PropertyId, RoomTypeId = twinRoomType.RoomTypeId, Description = "This room has price of $ 295" };
                        await _context.AddAsync(level3Room7);
                        List<Bed> level3Room7beds = new List<Bed>
                        {
                           new Bed{Name = "8", RoomId = level3Room7.RoomId, Status = Core.Enums.BedStatus.Ready},
                           new Bed{Name = "9", RoomId = level3Room7.RoomId, Status = Core.Enums.BedStatus.Ready}

                        };
                        await _context.AddRangeAsync(level3Room7beds);
                        await _context.SaveChangesAsync();
                    }
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
