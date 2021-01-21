using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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
            _factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder => builder.UseSetting("https_port", "5001").UseEnvironment("Testing"));
        }

        [TestMethod]
        public async Task ShouldReturnSuccessResponse_SingleFileForm()
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
        public async Task ShouldReturnBadRequestIfFileFormatIsNotPdf_SingleFileForm()
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

        [TestMethod]
        public async Task ShouldReturnSuccessResponse_MultipleFiles()
        {
            var client = _factory.CreateClient();

            const string testFile1 = "test.pdf";
            await File.WriteAllTextAsync(testFile1, "test1111");
            const string testFile2 = "test2.txt";
            await File.WriteAllTextAsync(testFile2, "test2222222");
            const string testFile3 = "test3.xyz";
            await File.WriteAllTextAsync(testFile3, "test33333333");

            using var form = new MultipartFormDataContent();
            using var fileContent1 = new ByteArrayContent(await File.ReadAllBytesAsync(testFile1));
            using var fileContent2 = new ByteArrayContent(await File.ReadAllBytesAsync(testFile2));
            using var fileContent3 = new ByteArrayContent(await File.ReadAllBytesAsync(testFile3));
            form.Add(fileContent1, "certificates", Path.GetFileName(testFile1));
            form.Add(fileContent2, "certificates", Path.GetFileName(testFile2));
            form.Add(fileContent3, "certificates", Path.GetFileName(testFile3));

            var response = await client.PostAsync("api/students/123/certificates", form);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
            var json = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("[{\"fileName\":\"test.pdf\",\"fileSize\":8},{\"fileName\":\"test2.txt\",\"fileSize\":11},{\"fileName\":\"test3.xyz\",\"fileSize\":12}]", json);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory.Dispose();
        }
    }
}
