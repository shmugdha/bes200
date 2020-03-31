using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using LibraryApi;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

namespace LibraryApiIntegrationTests
{
    public class ResourceSmokeTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient Client;
        public ResourceSmokeTests(CustomWebApplicationFactory<Startup> factory)
        {
            Client = factory.CreateClient();
        }
        [Theory]
        [InlineData("/books")]
        [InlineData("/books/1")]
        public async Task CheckIfResourceIsAlive(string resource)
        {
            var response = await Client.GetAsync(resource);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CanGetABook()
        {
            var response = await Client.GetAsync("/books/1");
            var book = await response.Content.ReadAsAsync<BookDetailsResponse>();
            Assert.Equal(1, book.id);
            Assert.Equal("Walden", book.title);
            Assert.Equal("Philosophy", book.genre);

        }

        [Fact]

        public async Task CanAddABook()
        {
            var bookToAdd = new PostBookRequest
            {
                title = "Music",
                author = "Smith",
                genre = "Non Fiction",
                numberOfPages = 252
            };

            var response = await Client.PostAsJsonAsync("/books", bookToAdd);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var location = response.Headers.Location.LocalPath; // books/4
            var getItResponse = await Client.GetAsync(location);
            var responseData = await getItResponse.Content.ReadAsAsync<BookDetailsResponse>();
            Assert.Equal(bookToAdd.title, responseData.title);
            Assert.Equal(bookToAdd.author, responseData.author);


        }



    }


    public class BookDetailsResponse
    {
        public int id { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public string genre { get; set; }
        public int numberOfPages { get; set; }
    }

    public class PostBookRequest
    {
        public string title { get; set; }
        public string author { get; set; }
        public string genre { get; set; }
        public int numberOfPages { get; set; }
    }



}
