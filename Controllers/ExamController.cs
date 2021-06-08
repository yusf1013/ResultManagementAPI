using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using TodoApp.Data;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [EnableCors("MyPolicy")]


    public class ExamController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ExamController(ApiDbContext context)
        {   
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _context.Exams.ToListAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(Exam data)
        {
            if(ModelState.IsValid)
            {
                var item = await _context.Exams.FirstOrDefaultAsync(x => x.Id == data.Id);
                if(item != null)
                {
                    return BadRequest("Record already exists in Database!");
                }

                await _context.Exams.AddAsync(data);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetItem", new {data.Id}, data);
            }

            return new JsonResult("Something went wrong") {StatusCode = 500};
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id);

            if(item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, Exam exam)
        {
            if(id != exam.Id)
                return BadRequest();

            var existItem = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id);

            if(existItem == null)
                return NotFound();

            existItem.Id = exam.Id;
            existItem.Semester = exam.Semester;
            existItem.Name = exam.Name;
            existItem.Marks = exam.Marks;
            
            // Implement the changes on the database level
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var existItem = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id);

            if(existItem == null)
                return NotFound();

            _context.Exams.Remove(existItem);
            await _context.SaveChangesAsync();

            return Ok(existItem);
        }
    }
}