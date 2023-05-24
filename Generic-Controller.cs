[Produces("application/json")]
[Route("api/v1/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly ILogger<EmployeeController> _logger;
    private readonly IEmployeeService _employeeService;
    public readonly IMapper _mapper;
    public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService, IMapper mapper)
    {
       (_logger, this._employeeService, _mapper) = (logger, employeeService, mapper);
    }
                                                                                            #region controllers
    /// <summary>
    /// GET: api/{version}/Employee/GetEmployees
    /// </summary>
    /// <response code="200">employee object list</response>
    /// <response code="404">missing employee objects</response>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(IEnumerable<EmployeeViewModel>))]
    [ActionName("GetEmployees"),HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<EmployeeViewModel>>> GetEmployees()
    {
        IEnumerable<Employee?> employees = await _employeeService.GetEmployeesAsync();
        IEnumerable<EmployeeViewModel> employeesVM = _mapper.Map<IEnumerable<Employee?>,IEnumerable<EmployeeViewModel>>(employees!);
        return employees is not null and IEnumerable<Employee> ? Ok(employeesVM) : StatusCode(404);
    }
    
    /// <summary>
    /// PUT: api/{version}/Employee/EditEmployee/{id}
    /// </summary>
    /// <param name="id">Guid of employee</param>
    /// <response code="200">employee object</response>
    /// <response code="204">invlaid id</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(EmployeeViewModel))]
    [ActionName("EditEmployee"),HttpPut("[action]/{id:guid}")]
    public async Task<ActionResult<EmployeeViewModel>> EditEmployee([FromRoute] Guid id, [FromBody] AddModifyEmpReq modifyReq)
    {
        Employee? empEntry = await _employeeService.GetEmployeeByIdAsync(id);
        if ((empEntry is null)||(modifyReq is null)) return StatusCode(204);
        EmployeeViewModel employeeVM = _mapper.Map<Employee,EmployeeViewModel>(empEntry!);
        _mapper.Map(modifyReq, empEntry);
        this._employeeService.UpdateEmployee(empEntry);
        return Ok(employeeVM);
    }

    /// <summary>
    /// POST: api/{version}/Employee/AddEmployee
    /// </summary>
    /// <param name="employeeReq">AddModifyEmpReq DTO</param>
    /// <response code="201">employee object</response>
    /// <response code="400">not created</response>
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created,Type=typeof(EmployeeViewModel))]
    [ActionName("AddEmployee"),HttpPost("[action]")]
    public ActionResult<EmployeeViewModel> AddEmployee([FromBody] AddModifyEmpReq employeeReq)
    {
        if (employeeReq is null) return BadRequest(employeeReq);
        Employee createdEmployee = _employeeService.CreateEmployee(_mapper.Map<AddModifyEmpReq,Employee>(employeeReq));
        EmployeeViewModel employeeVM = _mapper.Map<Employee,EmployeeViewModel>(createdEmployee);
        return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployee.Id }, employeeVM);
    }

    /// <summary>
    /// DELETE: api/{version}/Employee/DeleteEmployee/{id}
    /// </summary>
    /// <param name="id">Guid of employee</param>
    /// <response code="204">invlaid id</response>
    /// <response code="200">deleted successfully</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ActionName("DeleteEmployee"),HttpDelete("[action]/{id:guid}")]
    public async Task<IActionResult> DeleteEmployee([FromRoute] Guid id)
    {
        if (await _employeeService.GetEmployeeByIdAsync(id) is null) return StatusCode(204);
        Employee? empToDelete = await _employeeService.GetEmployeeByIdAsync(id);
        _employeeService.DeleteEmployee(empToDelete!);
        return Ok(200);
    }

    /// <summary>
    /// GET: api/{version}/Employee/GetEmployee/{id}
    /// </summary>
    /// <param name="id">Guid of employee</param>
    /// <response code="200">employee object</response>
    /// <response code="204">invlaid id</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(EmployeeViewModel))]
    [ActionName("GetEmployee"),HttpGet("[action]/{id:guid}")]
    public async Task<ActionResult<EmployeeViewModel>> GetEmployee([FromRoute] Guid id)
    {
        Employee? employee = await _employeeService.GetEmployeeByIdAsync(id);
        EmployeeViewModel employeeVM = _mapper.Map<Employee,EmployeeViewModel>(employee!);
        return employee is not null and Employee ? Ok(employeeVM) : StatusCode(204);
    }

    //[..]
    #endregion
}
