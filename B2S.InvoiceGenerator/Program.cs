using B2S.Core.Interfaces;
using B2S.Infrastructure.Data;
using B2S.Infrastructure.Utilities;
using B2S.InvoiceGenerator.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace B2S.InvoiceGenerator
{
    class Program
    {
        private static AppDbContext appDBContext;
        private static INLogger logger;
        static void Main(string[] args)
        {
            GenerateInvoices();
        }

        private static void GenerateInvoices()
        {
            string sqlConn = "Server=.\\sqlexpress;Database=b2s8;Trusted_Connection=True;MultipleActiveResultSets=true";
            var dbOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(sqlConn)
               .Options;
            appDBContext = new AppDbContext(dbOptions);
            logger = new NLogger();


            try
            {
                Console.Write("Invoice is generating...");

                var invoiceService = new InvoiceService(logger, appDBContext);
                //invoiceService.GeneratePayments();
                invoiceService.GenerateAllInvoices();

                Console.Write("Complete!!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message ?? "Something went wrong");
                Console.ReadLine();
                logger.LogError(ex);
            }

        }
    }
}
