using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=NhakhoaDb;Trusted_Connection=True;MultipleActiveResultSets=true");
using var db = new ApplicationDbContext(optionsBuilder.Options);
var user = db.Users.First();
Console.WriteLine($"ID: {user.Id}, Username: {user.Username}, Stamp: {user.SecurityStamp}");
