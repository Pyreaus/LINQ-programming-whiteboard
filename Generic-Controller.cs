                 //[..] imports ^
[ApiController]                     
[Authorize(Policy="tracr-default",AuthenticationSchemes=NegotiateDefaults.AuthenticationScheme)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/v1/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IMapper _mapper;
    private static T NullArg<T>(T arg) => throw new ArgumentNullException(nameof(arg));
    private readonly ILogger<EmployeeController> _logger;
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IMapper mapper, ILogger<EmployeeController> logger, IEmployeeService empService)
    {
        (_employeeService, _mapper, _logger) = (empService ?? NullArg<IEmployeeService>(empService!), mapper, logger);
    }
    
    /// <summary>
    /// GET: api/{version}/Employee/GetEmployees
    /// </summary>
    /// <response code="200">{employee view objects}</response>
    /// <response code="404">missing employee objects</response>
    [Authorize(Policy="tracr-admin")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(IEnumerable<EmployeeViewModel>))]
    [ActionName("GetEmployees"),HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<EmployeeViewModel>?>> GetEmployees()
    {
        IEnumerable<Employee?> employees = await _employeeService.GetEmployeesAsync();
        IEnumerable<EmployeeViewModel> employeesVM = _mapper.Map<IEnumerable<Employee?>, IEnumerable<EmployeeViewModel>>(employees!);
        return (employees != null) && (typeof(List<Employee>) == employees.GetType()) ? Ok(employeesVM) : StatusCode(404);
    }
    
    /// <summary>
    /// GET: api/{version}/User/GetTrainees
    /// </summary>
    /// <response code="200">{trainee view objects}</response>
    /// <response code="404">missing employee objects</response>
    [Authorize(Policy="tracr-admin")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(IEnumerable<TraineeViewModel>))]
    [ActionName("GetTrainees"),HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<TraineeViewModel>?>> GetTrainees()
    {
        IEnumerable<Trainee?> trainees = await _userService.GetTraineesAsync();
        IEnumerable<PeopleFinderUser?> users = await _userService.GetPFUsersAsync();
        IEnumerable<TraineeViewModel> traineesVM = _mapper.Map<IEnumerable<Trainee?>,IEnumerable<TraineeViewModel>>(trainees.Where(
        trainee => users.Any(user => user?.OtherPfid == trainee?.TraineePfid)).OfType<Trainee>().ToList()!).OfType<TraineeViewModel>().ToList();
        foreach (TraineeViewModel trainee in traineesVM)
        {
            trainee!.Photo = (bnetUrl + trainee.Photo?.ToString()) ?? "../../../assets/profilePic.png";
            _mapper.Map(users.FirstOrDefault(user => trainee.TraineePfid == user?.OtherPfid)!, trainee);
        }
        return (trainees.GetType() == typeof(List<Trainee>)) && traineesVM != null ? Ok(traineesVM) : StatusCode(404);
    }

    /// <summary>
    /// PUT: api/{version}/Employee/EditEmployee/{id}
    /// </summary>
    /// <param name="id">Guid of employee</param>
    /// <response code="200">{employee view object}</response>
    /// <response code="204">invlaid id</response>
    [Consumes(MediaTypeNames.Application.Json)]
    [Authorize(Policy="tracr-admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(EmployeeViewModel))]
    [ActionName("EditEmployee"),HttpPut("[action]/{id:guid}")]
    public async Task<ActionResult<EmployeeViewModel?>> EditEmployee([FromRoute] Guid id, [FromBody] AddModifyEmpReq modifyReq)
    {
        Employee? empEntry = await _employeeService.GetEmployeeByIdAsync(id);
        if ((empEntry is null) || (modifyReq is null)) return StatusCode(204);
        _mapper.Map(modifyReq, empEntry);
        EmployeeViewModel employeeVM = _mapper.Map<Employee, EmployeeViewModel>(empEntry!);
        this._employeeService.UpdateEmployee(empEntry);
        return Ok(employeeVM);
    }

    /// <summary>
    /// POST: api/{version}/Employee/AddEmployee
    /// </summary>
    /// <param name="employeeReq">AddModifyEmpReq DTO</param>
    /// <response code="201">{employee view objects}</response>
    /// <response code="400">not created</response>
    [Consumes(MediaTypeNames.Application.Json)]
    [Authorize(Policy="tracr-admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created,Type=typeof(EmployeeViewModel))]
    [ActionName("AddEmployee"),HttpPost("[action]")]
    public ActionResult<EmployeeViewModel?> AddEmployee([FromBody] AddModifyEmpReq employeeReq)
    {
        if (employeeReq is null) return BadRequest(employeeReq);
        Employee createdEmployee = _employeeService.CreateEmployee(_mapper.Map<AddModifyEmpReq, Employee>(employeeReq));
        EmployeeViewModel employeeVM = _mapper.Map<Employee, EmployeeViewModel>(createdEmployee);
        return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployee.Id }, employeeVM);
    }

    /// <summary>
    /// DELETE: api/{version}/Employee/DeleteEmployee/{id}
    /// </summary>
    /// <param name="id">Guid of employee</param>
    /// <response code="204">invlaid id</response>
    /// <response code="200">deleted successfully</response>
    [Authorize(Policy="tracr-admin")]
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
    /// <response code="200">{employee view object}</response>
    /// <response code="204">invlaid id</response>
    [Authorize(Policy="tracr-admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(EmployeeViewModel))]
    [ActionName("GetEmployee"),HttpGet("[action]/{id:guid}")]
    public async Task<ActionResult<EmployeeViewModel?>> GetEmployee([FromRoute] Guid id)
    {
        Employee? employee = await _employeeService.GetEmployeeByIdAsync(id);
        EmployeeViewModel employeeVM = _mapper.Map<Employee, EmployeeViewModel>(employee!);
        return (employee != null) && (typeof(Employee) == employee.GetType()) ? Ok(employeeVM) : StatusCode(204);
    }
    

    //[..]
}
