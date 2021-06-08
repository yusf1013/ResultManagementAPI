using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using TodoApp.Data;

namespace Controllers
{
    [EnableCors("MyPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentDetailsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public StudentDetailsController(ApiDbContext context)
        {   
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetItems()
        {
            var items = await _context.Students.ToListAsync();
            return Ok(items);
        }

        // [HttpPost]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> CreateItem(StudentDetails data)
        // {
        //     if(ModelState.IsValid)
        //     {
        //         var item = await _context.Students.FirstOrDefaultAsync(x => x.Id == data.Id);
        //         if(item != null)
        //         {
        //             return BadRequest("Record already exists in Database!");
        //         }
                
        //         await _context.Students.AddAsync(data);
        //         await _context.SaveChangesAsync();

        //         return CreatedAtAction("GetItem", new {data.Id}, data);
        //     }

        //     return new JsonResult("Something went wrong") {StatusCode = 500};
        // }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(string id)
        {
            var item = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);

            if(item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateItem(string id, StudentDetails student)
        {
            if(id != student.Id || !ModelState.IsValid)
                return BadRequest();

            var existItem = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);

            if(existItem == null)
                return NotFound();

            existItem.Id = student.Id;
            existItem.DOB = student.DOB;
            existItem.FirstName = student.FirstName;
            existItem.LastName = student.LastName;
            existItem.Semester = student.Semester;
            
            
            // Implement the changes on the database level
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // [HttpPut("suspend/{id}")]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> Suspend(string id, StudentDetails student)
        // {
        //     if(id != student.Id)
        //         return BadRequest();

        //     var existItem = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);

        //     if(existItem == null)
        //         return NotFound();

        //     existItem.AccountSuspended = student.AccountSuspended;
            
            
        //     // Implement the changes on the database level
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            var existItem = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);

            if(existItem == null)
                return NotFound();

            _context.Students.Remove(existItem);
            await _context.SaveChangesAsync();

            return Ok(existItem);
        }
    }
}