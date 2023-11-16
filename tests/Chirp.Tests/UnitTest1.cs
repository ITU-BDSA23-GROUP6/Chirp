

using Chirp.Models;

namespace UnitTest1
{
    public class ChirpinModelsTests
    {
        [Fact]
        public void TestingTimeStampFromModels()
        {
            // Arrange

            var a10 = new Author() { UserName = "Jacqualine Gilcoine", Email = "Jacqualine.Gilcoine@gmail.com", Cheeps = new List<Cheep>() };
            var c1 = new Cheep() { CheepId = 1, Author = a10, Text = "They were married in Chicago, with old Smith, and was expected aboard every day; meantime, the two went past me.", TimeStamp = DateTime.Parse("2023-08-01 13:14:37") };

            // Act

            string output = c1.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss");

            // Assert
            Assert.Equal("2023-08-01 13:14:37", output);
        }
        [Fact]
        public void TestingAuthorAndCheepRelations()
        {
            // Arrange
            var a10 = new Author() { UserName = "Jacqualine Gilcoine", Email = "Jacqualine.Gilcoine@gmail.com", Cheeps = new List<Cheep>() };
            var c1 = new Cheep() { CheepId = 1, Author = a10, Text = "They were married in Chicago, with old Smith, and was expected aboard every day; meantime, the two went past me.", TimeStamp = DateTime.Parse("2023-08-01 13:14:37") };

            // Act
            String output = c1.Author.UserName.ToString();

            // Assert
            Assert.Equal("Jacqualine Gilcoine", output);

        }

        [Fact]
        public void TestingCheepsListSize()
        {
            // Arrange
            var a10 = new Author() { UserName = "Jacqualine Gilcoine", Email = "Jacqualine.Gilcoine@gmail.com", Cheeps = new List<Cheep>() };
            var c1 = new Cheep() { CheepId = 1, Author = a10, Text = "They were married in Chicago, with old Smith, and was expected aboard every day; meantime, the two went past me.", TimeStamp = DateTime.Parse("2023-08-01 13:14:37") };
            var c2 = new Cheep() { CheepId = 2, Author = a10, Text = "And then, as he listened to all that''s left o'' twenty-one people.", TimeStamp = DateTime.Parse("2023-08-01 13:15:21") };
            var c3 = new Cheep() { CheepId = 3, Author = a10, Text = "In various enchanted attitudes, like the Sperm Whale.", TimeStamp = DateTime.Parse("2023-08-01 13:14:58") };

            // Act
            a10.Cheeps.Add(c1);
            a10.Cheeps.Add(c2);
            a10.Cheeps.Add(c3);
            var listSize = a10.Cheeps.Count.ToString();

            // Assert
            Assert.Equal("3", listSize);
        }
        [Fact]
        public void TestingCheepsListContainment()
        {
            // Arrange
            var a10 = new Author() { UserName = "Jacqualine Gilcoine", Email = "Jacqualine.Gilcoine@gmail.com", Cheeps = new List<Cheep>() };
            var c1 = new Cheep() { CheepId = 1, Author = a10, Text = "They were married in Chicago, with old Smith, and was expected aboard every day; meantime, the two went past me.", TimeStamp = DateTime.Parse("2023-08-01 13:14:37") };
            var c2 = new Cheep() { CheepId = 2, Author = a10, Text = "And then, as he listened to all that''s left o'' twenty-one people.", TimeStamp = DateTime.Parse("2023-08-01 13:15:21") };
            var c3 = new Cheep() { CheepId = 3, Author = a10, Text = "In various enchanted attitudes, like the Sperm Whale.", TimeStamp = DateTime.Parse("2023-08-01 13:14:58") };

            // Act
            var cheepText = "";
            a10.Cheeps.Add(c1);
            a10.Cheeps.Add(c2);
            a10.Cheeps.Add(c3);

            foreach (var cheep in a10.Cheeps)
            {
                if (cheep.CheepId == 2)
                {
                    cheepText = cheep.Text;
                }
            }


            // Assert
            Assert.Equal("And then, as he listened to all that''s left o'' twenty-one people.", cheepText);
        }
    }
}


