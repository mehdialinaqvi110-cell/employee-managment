
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Employee
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        return await _context.Employees.ToListAsync();
    }
    // POST: api/Employee

[HttpPost]
public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
{
    var emailExists = await _context.Employees
        .AnyAsync(e => e.Email == employee.Email);

    if (emailExists)
    {
        return Conflict(new
        {
            message = "An employee with this email already exists."
        });
    }

    _context.Employees.Add(employee);

    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateException)
    {
        // The unique database index also protects against
        // two requests attempting the same email simultaneously.
        return Conflict(new
        {
            message = "An employee with this email already exists."
        });
    }

    return CreatedAtAction(
        nameof(GetEmployees),
        new { id = employee.Id },
        employee
    );
}



    // PUT: api/Employee/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest("Employee ID mismatch.");
        }

        var existingEmployee = await _context.Employees.FindAsync(id);

        if (existingEmployee == null)
        {
            return NotFound("Employee not found.");
        }

        existingEmployee.FullName = employee.FullName;
        existingEmployee.Email = employee.Email;
        existingEmployee.Department = employee.Department;
        existingEmployee.Salary = employee.Salary;

        await _context.SaveChangesAsync();

        return NoContent();
    }


    // DELETE: api/Employee/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
        {
            return NotFound("Employee not found.");
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }



}
