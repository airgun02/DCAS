using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DCAS.Models;

namespace DCAS.Data
{
    public class DCASContext : DbContext
    {
        public DCASContext(DbContextOptions<DCASContext> options)
            : base(options)
        {
        }

        public DbSet<DCAS.Models.TodaySchedule> TodaySchedule { get; set; } = default!;
        public DbSet<DCAS.Models.PersonInfo> PersonInfo { get; set; } = default!;
        public DbSet<Services> Services { get; set; }
        public DbSet<DCAS.Models.MedicineInventory> MedicineInventory { get; set; } = default!;
        public DbSet<DCAS.Models.Users> Users { get; set; } = default!;
        public DbSet<DCAS.Models.Payment> Payments { get; set; } = default!;
        public DbSet<DCAS.Models.PaymentMedicine> PaymentMedicine { get; set; } = default!;
        public DbSet<DCAS.Models.PaymentService> PaymentServices { get; set; } = default!;
        public DbSet<DCAS.Models.LatestUpdate> LatestUpdates { get; set; } = default!;
    }
}
