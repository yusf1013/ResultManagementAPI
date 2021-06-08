using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs.Requests;
using TodoApp.Data;

namespace Controllers
{
    [EnableCors("MyPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class ExamResultsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ExamResultsController(ApiDbContext context)
        {   
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            List<ExamResults> items = await _context.ExamResults.ToListAsync();
            List<Dictionary<string, string>> done = new List<Dictionary<string, string>>();

            foreach (ExamResults item in items)
            {
                var x = new Dictionary<string, string>();
                Exam y = await _context.Exams.FirstOrDefaultAsync(d => d.Id == item.ExamId);

                x.Add("Result ID", item.Id);
                x.Add("Student ID", item.StudentId);
                x.Add("Exam", y.Name);
                x.Add("Semester", y.Semester.ToString());
                x.Add("Full Marks", y.Marks.ToString());
                x.Add("ObtainedMark", item.ObtainedMark.ToString());
                

                done.Add(x);


            }
            return Ok(done);
        }

        // [HttpGet]
        // [Route("final")]
        // public async Task<IActionResult> GetFinalItems()
        // {
        //     List<ExamResults> items = await _context.ExamResults.ToListAsync();
        //     List<Dictionary<string, string>> done = new List<Dictionary<string, string>>();
        //     List<List<Dictionary<string, string>>> arr;

        //     foreach (ExamResults item in items)
        //     {
        //         var x = new Dictionary<string, string>();
        //         Exam y = await _context.Exams.FirstOrDefaultAsync(d => d.Id == item.ExamId);

        //         x.Add("Result ID", item.Id);
        //         x.Add("Student ID", item.StudentId);
        //         x.Add("Exam", y.Name);
        //         x.Add("Semester", y.Semester.ToString());
        //         x.Add("Full Marks", y.Marks.ToString());
        //         x.Add("ObtainedMark", item.ObtainedMark.ToString());

        //         done.Add(x);
        //     }

        //     foreach (var item in done)
        //     {
                
        //     }

            



        //     return Ok(done);
        // }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateItem(ExamResultsRequest req)
        {
            if(ModelState.IsValid)
            {
                StudentDetails student = await _context.Students.FirstOrDefaultAsync(x => x.Id == req.StudentId);
                Exam exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == req.ExamId);

                if(student == null)
                    return BadRequest(new Dictionary<string, Dictionary<string, List<string>>> {{ 
                        "errors", new Dictionary<string, List<string>> {{ 
                            "studentid", new List<string> {"No student by this email"}
                         }}
                        }});

                if(exam == null)
                    return BadRequest(new Dictionary<string, Dictionary<string, List<string>>> {{ 
                        "errors", new Dictionary<string, List<string>> {{ 
                            "examid", new List<string> {"No exams created by this id"}
                         }}
                        }});

                var newId = student.Id + "," + exam.Id;
                var item = await _context.ExamResults.FirstOrDefaultAsync(x => x.Id == newId);
                if(item != null)
                {
                    return BadRequest("Record already exists in Database!");
                }
                if(req.ObtainedMark > exam.Marks)
                    return BadRequest("Obtained marks can not be greater than full marks!");



                ExamResults data = new ExamResults{Id = newId, StudentId = student.Id, ExamId = exam.Id, ObtainedMark = req.ObtainedMark};
                await _context.ExamResults.AddAsync(data);
                await _context.SaveChangesAsync();


                return StatusCode(201);
            }

            return new JsonResult("Something went wrong") {StatusCode = 500};
        }


        [HttpGet("{sid}/{eid}")]
        public async Task<IActionResult> GetItem(string sid, string eid)
        {
            var id = sid + "," + eid;
            var item = await _context.ExamResults.FirstOrDefaultAsync(x => x.Id == id);

            if(item == null)
                return NotFound();

            return Ok(item);
        }

        
        [HttpPut("{sid}/{eid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateItem(string sid, int eid, ExamResultsRequest exam)
        {
            var id = sid + "," + eid;
            var newId = exam.StudentId + "," + exam.ExamId;

            if(id != newId)
                return BadRequest(new Dictionary<string, Dictionary<string, List<string>>> {{ 
                    "errors", new Dictionary<string, List<string>> {
                        {"examid", new List<string> {"Can't edit exam ID. Delete and add new record if needed."}},
                        {"studentid", new List<string> {"Can't edit student ID. Delete and add new record if needed."}}
                        }
                    }});

            var existItem = await _context.ExamResults.FirstOrDefaultAsync(x => x.Id == id);
            

            if(existItem == null)
                return NotFound();

            StudentDetails student = await _context.Students.FirstOrDefaultAsync(x => x.Id == sid);
            Exam ex = await _context.Exams.FirstOrDefaultAsync(x => x.Id == eid);

            if(student == null || exam == null)
                    return NotFound();
            if(exam.ObtainedMark > ex.Marks)
                    return BadRequest("Obtained marks can not be greater than full marks!");

            existItem.ObtainedMark = exam.ObtainedMark;
            
            // Implement the changes on the database level
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{sid}/{eid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItem(string sid, int eid)
        {
            var id = sid + "," + eid;
            var existItem = await _context.ExamResults.FirstOrDefaultAsync(x => x.Id == id);

            if(existItem == null)
                return NotFound();

            _context.ExamResults.Remove(existItem);
            await _context.SaveChangesAsync();

            return Ok(existItem);
        }
    }
}