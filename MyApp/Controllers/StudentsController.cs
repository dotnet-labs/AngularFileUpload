﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace MyApp.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(ILogger<StudentsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id:int}/forms/{formId:int}")]
        [ProducesResponseType(typeof(StudentFormSubmissionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<StudentFormSubmissionResult> ViewForm(int id, int formId)
        {
            _logger.LogInformation($"viewing the form#{formId} for Student ID={id}");
            await Task.Delay(1000);
            return new StudentFormSubmissionResult { FormId = formId, StudentId = id };
        }

        [HttpPost("{id:int}/forms")]
        [ProducesResponseType(typeof(StudentFormSubmissionResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<ActionResult<StudentFormSubmissionResult>> SubmitForm(int id, [FromForm] StudentForm form)
        {
            _logger.LogInformation($"Validating the form#{form.FormId} for Student ID={id}");

            if (form.Courses == null || form.Courses.Length == 0)
            {
                return BadRequest("Please enter at least one course.");
            }

            if (form.StudentFile == null || form.StudentFile.Length < 1)
            {
                return BadRequest("The uploaded file is empty.");
            }

            if (Path.GetExtension(form.StudentFile.FileName) != ".pdf")
            {
                return BadRequest($"The uploaded file {form.StudentFile.Name} is not a PDF file.");
            }

            var filePath = Path.Combine(@"App_Data", $"{DateTime.Now:yyyyMMddHHmmss}.pdf");
            new FileInfo(filePath).Directory?.Create();
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                _logger.LogInformation($"Saving file [{form.StudentFile.FileName}]");
                await form.StudentFile.CopyToAsync(stream);
                _logger.LogInformation($"\t The uploaded file is saved as [{filePath}].");
            }

            var result = new StudentFormSubmissionResult { FormId = form.FormId, StudentId = id, FileSize = form.StudentFile.Length };
            return CreatedAtAction(nameof(ViewForm), new { id, form.FormId }, result);
        }

        /// <summary>
        /// An Example API Endpoint Accepting Multiple Files
        /// </summary>
        /// <param name="id"></param>
        /// <param name="certificates"></param>
        /// <returns></returns>
        [HttpPost("{id:int}/certificates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<ActionResult<List<CertificateSubmissionResult>>> SubmitCertificates(int id, [Required] List<IFormFile> certificates)
        {
            var result = new List<CertificateSubmissionResult>();

            if (certificates == null || certificates.Count == 0)
            {
                return BadRequest("No file is uploaded.");
            }

            foreach (var certificate in certificates)
            {
                var filePath = Path.Combine(@"App_Data", id.ToString(), @"Certificates", certificate.FileName);
                new FileInfo(filePath).Directory?.Create();

                await using var stream = new FileStream(filePath, FileMode.Create);
                await certificate.CopyToAsync(stream);
                _logger.LogInformation($"The uploaded file [{certificate.FileName}] is saved as [{filePath}].");

                result.Add(new CertificateSubmissionResult { FileName = certificate.FileName, FileSize = certificate.Length });
            }

            return Ok(result);
        }

        [HttpGet("files/{id:int}")]
        public async Task<ActionResult> DownloadFile(int id)
        {
            // validation and get the file

            var filePath = $"{id}.txt";
            if (!System.IO.File.Exists(filePath))
            {
                await System.IO.File.WriteAllTextAsync(filePath, "Hello World!");
            }
            
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contentType, Path.GetFileName(filePath));
        }
    }

    public class StudentForm
    {
        [Required] public int FormId { get; set; }
        [Required] public string[] Courses { get; set; }
        [Required] public IFormFile StudentFile { get; set; }
    }

    public class StudentFormSubmissionResult
    {
        public int StudentId { get; set; }
        public int FormId { get; set; }
        public long FileSize { get; set; }
    }

    public class CertificateSubmissionResult
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }
}
