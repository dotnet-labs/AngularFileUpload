using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyApp.IntegrationTests
{
    [TestClass]
    public class StudentsControllerTests
    {
        private static WebApplicationFactory<Startup> _factory;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Console.WriteLine(testContext.TestName);
            _factory = new WebApplicationFactory<Startup>();
        }

        [TestMethod]
        public async Task ShouldReturnSuccessResponse()
        {
            var client = _factory.CreateClient();

            const string filePath = "test.pdf";
            await File.WriteAllTextAsync(filePath, "test");

            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "StudentFile", Path.GetFileName(filePath));
            form.Add(new StringContent("789"), "FormId");
            form.Add(new StringContent("Reading"), "Courses");
            form.Add(new StringContent("Math"), "Courses");

            var response = await client.PostAsync("api/students/123/forms", form);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
            Assert.AreEqual("/api/Students/123/forms/789", response.Headers.Location?.AbsolutePath);
            var json = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("{\"studentId\":123,\"formId\":789,\"fileSize\":4}", json);
        }

        [TestMethod]
        public async Task ShouldReturnBadRequestIfFileFormatIsNotPdf()
        {
            var client = _factory.CreateClient();

            const string filePath = "test.txt";
            await File.WriteAllTextAsync(filePath, "test");

            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "StudentFile", Path.GetFileName(filePath));
            form.Add(new StringContent("789"), "FormId");
            form.Add(new StringContent("Reading"), "Courses");
            form.Add(new StringContent("Math"), "Courses");

            var response = await client.PostAsync("api/students/123/forms", form);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
            Assert.IsNull(response.Headers.Location?.AbsolutePath);
            var json = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("\"The uploaded file StudentFile is not a PDF file.\"", json);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory.Dispose();
        }
    }
}
