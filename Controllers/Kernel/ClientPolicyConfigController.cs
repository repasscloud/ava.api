// using Ava.API.Data;
// using Ava.API.Models.Kernel;
// using Ava.API.Models.Policies;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace Ava.API.Controllers.IATA
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class ClientPolicyConfigController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public ClientPolicyConfigController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/AvaClientTravelPolicies
//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<TravelPolicy>>> GetAvaClientTravelPolicies()
//         {
//             return await _context.AvaClientTravelPolicies.ToListAsync();
//         }

//         // GET: api/AvaClientTravelPolicies/{id}
//         [HttpGet("{id}")]
//         public async Task<ActionResult<TravelPolicy>> GetTravelPolicy(int id)
//         {
//             var data = await _context.AvaClientTravelPolicies.FindAsync(id);

//             if (data == null)
//             {
//                 return NotFound();
//             }

//             return data;
//         }

//         // POST: api/AvaClientTravelPolicies
//         [HttpPost]
//         public async Task<ActionResult<TravelPolicy>> PostTravelPolicy(TravelPolicy data)
//         {
//             _context.AvaClientTravelPolicies.Add(data);
//             await _context.SaveChangesAsync();

//             return CreatedAtAction(nameof(GetTravelPolicy), new { id = data.Id }, data);
//         }

//         // PUT: api/AvaClientTravelPolicies/{id}
//         [HttpPut("{id}")]
//         public async Task<IActionResult> PutTravelPolicy(string id, TravelPolicy data)
//         {
//             if (id != data.Id)
//             {
//                 return BadRequest();
//             }

//             // Ensure unique SupportedEmaildata (if changed)
//             if (await _context.AvaClientTravelPolicies.AnyAsync(d => d.PolicyName == data.PolicyName && d.Id != id))
//             {
//                 return Conflict(new { message = "The Policy Name must be unique" });
//             }

//             _context.Entry(data).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!TravelPolicyExists(id))
//                 {
//                     return NotFound();
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         // DELETE: api/AvaClientTravelPolicies/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteTravelPolicy(string id)
//         {
//             var data = await _context.AvaClientTravelPolicies.FindAsync(id);
//             if (data == null)
//             {
//                 return NotFound();
//             }

//             _context.AvaClientTravelPolicies.Remove(data);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }

//         private bool TravelPolicyExists(string id)
//         {
//             return _context.AvaClientTravelPolicies.Any(e => e.Id == id);
//         }
//     }
// }
