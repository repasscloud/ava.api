// using Ava.API.Data;
// using Ava.API.Models.Kernel;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace Ava.API.Controllers.Kernel
// {
//     [ApiController]
//     [Route("api/sales")]
//     public class SalesController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public SalesController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // 1. Create a Salesperson
//         [HttpPost("salesperson")]
//         public async Task<IActionResult> CreateSalesperson([FromBody] CreateSalespersonDto dto)
//         {
//             if (dto == null)
//                 return BadRequest("Invalid salesperson data.");

//             var salesperson = new SalespersonProfile
//             {
//                 Name = dto.Name,
//                 Email = dto.Email,
//                 PrivateKey = dto.PrivateKey
//             };

//             _context.SalespersonProfiles.Add(salesperson);
//             await _context.SaveChangesAsync();

//             return CreatedAtAction(nameof(GetSalesperson), new { id = salesperson.Id }, salesperson);
//         }


//         // 2. Update a Salesperson
//         [HttpPut("salesperson/{id}")]
//         public async Task<IActionResult> UpdateSalesperson(string id, [FromBody] SalespersonProfile updatedSalesperson)
//         {
//             var existingSalesperson = await _context.SalespersonProfiles.FindAsync(id);
//             if (existingSalesperson == null)
//                 return NotFound("Salesperson not found.");

//             existingSalesperson.Name = updatedSalesperson.Name;
//             existingSalesperson.Email = updatedSalesperson.Email;
//             existingSalesperson.PrivateKey = updatedSalesperson.PrivateKey;
//             existingSalesperson.IsActive = updatedSalesperson.IsActive;

//             await _context.SaveChangesAsync();
//             return Ok(existingSalesperson);
//         }

//         // 3. Delete a Salesperson
//         [HttpDelete("salesperson/{id}")]
//         public async Task<IActionResult> DeleteSalesperson(string id)
//         {
//             var salesperson = await _context.SalespersonProfiles.FindAsync(id);
//             if (salesperson == null)
//                 return NotFound("Salesperson not found.");

//             _context.SalespersonProfiles.Remove(salesperson);
//             await _context.SaveChangesAsync();
//             return NoContent();
//         }

//         // 4. Retrieve a Salesperson by ID
//         [HttpGet("salesperson/{id}")]
//         public async Task<IActionResult> GetSalesperson(string id)
//         {
//             var salesperson = await _context.SalespersonProfiles
//                 .Include(s => s.AvaSalesRecords)
//                 .FirstOrDefaultAsync(s => s.Id == id);

//             if (salesperson == null)
//                 return NotFound("Salesperson not found.");

//             return Ok(salesperson);
//         }

//         // 5. Create a Sales Record
//         [HttpPost("salesrecord")]
//         public async Task<IActionResult> CreateSalesRecord([FromBody] AvaSalesRecord salesRecord)
//         {
//             if (salesRecord == null)
//                 return BadRequest("Invalid sales record data.");

//             var salesperson = await _context.SalespersonProfiles.FindAsync(salesRecord.SalesPersonId);
//             if (salesperson == null)
//                 return NotFound("Salesperson not found.");

//             var license = await _context.AvaClientLicenses.FindAsync(salesRecord.LicenseId);
//             if (license == null)
//                 return NotFound("License not found.");

//             salesRecord.SalesPerson = salesperson;
//             salesRecord.License = license;

//             _context.AvaSalesRecords.Add(salesRecord);
//             await _context.SaveChangesAsync();

//             return CreatedAtAction(nameof(GetSalesRecordsBySalesperson), new { salespersonId = salesRecord.SalesPersonId }, salesRecord);
//         }

//         // 6. Generate a License and Attach a Sale Record
//         [HttpPost("salesrecord/generate-license")]
//         public async Task<IActionResult> GenerateLicenseAndAttachSale([FromBody] SalesRecordRequest request)
//         {
//             var salesperson = await _context.SalespersonProfiles.FindAsync(request.SalesPersonId);
//             if (salesperson == null)
//                 return NotFound("Salesperson not found.");

//             var newLicense = new AvaClientLicense
//             {
//                 ClientId = request.ClientId,
//                 ExpiryDate = DateTime.UtcNow.AddYears(1),
//                 AppId = request.AppId,
//                 Signature = NanoidDotNet.Nanoid.Generate(),
//                 SpendThreshold = request.SpendThreshold
//             };

//             _context.AvaClientLicenses.Add(newLicense);
//             await _context.SaveChangesAsync();

//             var newSalesRecord = new AvaSalesRecord
//             {
//                 SalesPersonId = request.SalesPersonId,
//                 LicenseId = newLicense.Id,
//                 SalesPerson = salesperson,  // ✅ FIXED: Required properties set
//                 License = newLicense,        // ✅ FIXED: Required properties set
//                 PaymentMethod = request.PaymentMethod,
//                 SalesDate = DateTime.UtcNow
//             };

//             _context.AvaSalesRecords.Add(newSalesRecord);
//             await _context.SaveChangesAsync();

//             return Ok(new { License = newLicense, SalesRecord = newSalesRecord });
//         }


//         // 7. Retrieve All Sales Records for a Given Salesperson
//         [HttpGet("salesrecords/{salespersonId}")]
//         public async Task<IActionResult> GetSalesRecordsBySalesperson(string salespersonId)
//         {
//             var records = await _context.AvaSalesRecords
//                 .Where(sr => sr.SalesPersonId == salespersonId)
//                 .Include(sr => sr.License)
//                 .ToListAsync();

//             if (records == null || !records.Any())
//                 return NotFound("No sales records found for this salesperson.");

//             return Ok(records);
//         }

//         // 8. Retrieve License Record by Client Code
//         [HttpGet("license/{clientId}")]
//         public async Task<IActionResult> GetLicenseByClientId(string clientId)
//         {
//             var license = await _context.AvaClientLicenses
//                 .FirstOrDefaultAsync(l => l.ClientId == clientId);

//             if (license == null)
//                 return NotFound("No license found for the given client code.");

//             return Ok(license);
//         }
//     }

//     // Request DTO for Generating License and Sale
//     public class SalesRecordRequest
//     {
//         public required string SalesPersonId { get; set; }
//         public required string ClientId { get; set; }
//         public required string AppId { get; set; }
//         public required string PaymentMethod { get; set; }
//         public int SpendThreshold { get; set; } = 0;
//     }

//     public class CreateSalespersonDto
//     {
//         public required string Name { get; set; }
//         public required string Email { get; set; }
//         public required string PrivateKey { get; set; }
//     }
// }
