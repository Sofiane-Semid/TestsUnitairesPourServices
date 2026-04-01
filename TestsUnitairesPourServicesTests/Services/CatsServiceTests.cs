using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsUnitairesPourServices.Data;
using TestsUnitairesPourServices.Exceptions;
using TestsUnitairesPourServices.Models;
using TestsUnitairesPourServices.Services;

namespace TestsUnitairesPourServices.Services.Tests
{
    [TestClass()]
    public class CatsServiceTests
    {

        private DbContextOptions<ApplicationDBContext> _options;

        private const int HOUSE_1_ID = 1;
        private const int HOUSE_2_ID = 2;
        private const int OUTSIDE_CAT_ID = 1;
        private const int HOUSE_CAT_ID = 2;
       


        public CatsServiceTests()
        {
            // TODO On initialise les options de la BD, on utilise une InMemoryDatabase
            _options = new DbContextOptionsBuilder<ApplicationDBContext>()
                 // TODO il faut installer la dépendance Microsoft.EntityFrameworkCore.InMemory
                 .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseLazyLoadingProxies(true) // Active le lazy loading
                .Options;
        }

        [TestInitialize]
        public void Init()
        {
            // TODO avoir la durée de vie d'un context la plus petite possible
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            // TODO on ajoute des données de tests

            House house1 = new House
            {
                Id = HOUSE_1_ID,
                Address = "Maison Volante",
                OwnerName = "Volant",
                Cats = new List<Cat>()
            };
            House house2 = new House
            {
                Id = HOUSE_2_ID,
                Address = "Maison Au Sol",
                OwnerName = "Sol",
                Cats = new List<Cat>()
            };
            Cat cat1 = new Cat
            {
                Id = OUTSIDE_CAT_ID,
                Name = "Chat sans maison",
                Age = 2,
                House = null
            };
            Cat cat2 = new Cat
            {
                Id = HOUSE_CAT_ID,
                Name = "Chat avec maison",
                Age = 3,
                House = house1
            };
            house1.Cats.Add(cat2);

            db.AddRange(house1, house2);
            db.AddRange(cat1, cat2);
            db.SaveChanges();
        }
    

        [TestCleanup]
        public void Dispose()
        {
            //TODO on efface les données de tests pour remettre la BD dans son état initial
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            db.Cat.RemoveRange(db.Cat);
            db.House.RemoveRange(db.House);
            db.SaveChanges();
        }
        [TestMethod]
        public void Move_Success_ReturnsCatAndMovesToNewHouse() 
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);

            House from = db.House.Find(HOUSE_1_ID);
            House to = db.House.Find(HOUSE_2_ID);

            Cat? result = service.Move(HOUSE_CAT_ID, from, to);

            Assert.IsNotNull(result);
            Assert.AreEqual(HOUSE_2_ID, result.House.Id);


        }
        [TestMethod]
        public void Move_UnknownCatId_ReturnsNull()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);

            House from = db.House.Find(HOUSE_1_ID);
            House to = db.House.Find(HOUSE_2_ID);

            Cat? result = service.Move(1999, from, to);

            Assert.IsNull(result);

        }

        [TestMethod]
        public void Move_NoHouse_Exception()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);

            House from = db.House.Find(HOUSE_1_ID);
            House to = db.House.Find(HOUSE_2_ID);

            Exception e = Assert.ThrowsException<WildCatException>(() => service.Move(OUTSIDE_CAT_ID, from, to));
            Assert.AreEqual("On n'apprivoise pas les chats sauvages", e.Message);

        }

        [TestMethod]
        public void Move_InHouse_Exception()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);

            House from = db.House.Find(HOUSE_2_ID);
            House to = db.House.Find(HOUSE_1_ID);

            Exception e = Assert.ThrowsException<DontStealMyCatException>(() => service.Move(HOUSE_CAT_ID, from, to));
            Assert.AreEqual("Touche pas à mon chat!", e.Message);

        }

        


    }
}