
using FileBotGPT.InterfacesServices;
using FileBotGPT.Model;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace FileBotGPT.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class FilebotTgController : ControllerBase
    {
        // Declare a private field for GPT request service
        private readonly IGptRequestService _fileBotGptService;

        // Declare a private field for file processing service
        private readonly IProcessingFileService _processingFileService;
        public FilebotTgController(IGptRequestService fileBotGptService, IProcessingFileService processingFileService)
        {
            //Inject GPT request service, file processing service through constructor
            _fileBotGptService = fileBotGptService;
            _processingFileService = processingFileService;
        }

        // Endpoint for handling Python bot requests
        [HttpPost("PythonBotRequest")]
        public async Task<IActionResult> PythonPostBotAsync([FromBody] PostBotRequest data)
        {
            Debug.WriteLine($"Received data from client: {data}");
            try
            {
                // Call GPT request service to process data and generate content
                string content = await _fileBotGptService.PostGptRequestAsync(data);

                //string content = "\"# Contents\r\n1.\tIntroduction\r\n2.\tCommon Types of Cats\r\n3.\tRare Types of Cats\r\n4.\tDomestic Cats\r\n<a name=\"introduction\"></a>��������\r\n����� - ��� ������������� �������� ������� � ���������� ��������� �����, ������ �� ������� �������� ������ ����������� ������� � ����������������. ������� ����� ����� ����� ���� ���������������� ��� ��������, ������ ���������� ��������� ������ � ������������ �����.\r\n<a name=\"common_cat\"></a>���������������� ���� �����\r\n������ ����������������� �������� ����� �������� ����������, �����������, �������� � ����-���. ��������, ���������� ����� �������� ����� ������� �������� ����� � ��������� ����������. ����� ���, ����-���� �������� ����� �� ���������� ����� ����� � �������� ������� � �������� �����.\r\n<a name=\"rare_cat\"></a>������ ���� �����\r\n��������� ������ ������ �������� � ���� �������, ������, � �����-��������. ������� - ��� ����� � ������� �������� ����, ������� �� ����� ����� �� �������. �����-��������, ��� ����� ���������� �� ��������, ����� ���� � ����� �������.\r\n<a name=\"domestic_cat\"></a>�������� �����\r\n�������� ����a, ����� ��������� ��� \"��������\" �����, �� ����������� � ������������ ������. ��� ����� ������������� �� ������, ������� � ���������, �� ������ ��� ���������� � ������ ������������ � ����� � ������.\r\n��� ����-�� ����� ������� ����������� ������ �����, ����������� �� �� ������ �����, ������������ � ������������� � ��������� �����\"\r\n";


                // Process bot response data
                MemoryStream dataStream = await _processingFileService.PostBotResponse(data, content);

                // Prepare HTTP response with file attachment
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(dataStream);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    // Set the file name for attachment
                    FileName = data.file_name + ".docx"
                };

                // Return the file stream as response
                return Ok(await response.Content.ReadAsStreamAsync());
            }
            catch
            {
                // Return a bad request response if an exception occurs
                return BadRequest("Bad");
            }
        }
    }


}