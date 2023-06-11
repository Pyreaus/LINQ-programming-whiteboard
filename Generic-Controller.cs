                 //[..] imports ^
[ApiController]                     
[Authorize(Policy="tracr-default",AuthenticationSchemes=NegotiateDefaults.AuthenticationScheme)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/v1/[controller]")]
public partial class UserController : ControllerBase
{
    #region [Infrastructure]
    private readonly IMapper _mapper;
    private readonly ClaimsPrincipal _claimsPrincipal;
    private readonly ILogger<UserController> _logger;
    private readonly string bnetUrl = "http://source/uploads/photos/";
    private static T NullArg<T>(T arg) => throw new ArgumentNullException(nameof(arg));
    private readonly IUserService _userService;
    public UserController(ClaimsPrincipal claimsPrincipal, ILogger<UserController> logger, IUserService userService, IMapper mapper)
    {
        (_userService, _logger, _mapper, _claimsPrincipal) = (userService ?? NullArg<IUserService>(userService!), logger, mapper, claimsPrincipal);
    }
    #endregion

    /// <summary>
    /// GET: api/{version}/User/GetTraineesByReviewer/{pfid}
    /// </summary>
    /// <param name="pfid">PFID of reviwer</param>
    /// <response code="200">{trainee view objects}</response>
    /// <response code="404">missing trainee objects</response>
    [Authorize(Policy="tracr-reviewer")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(IEnumerable<TraineeViewModel>))]
    [ActionName("GetTraineesByReviewer"),HttpGet("[action]/{pfid:int}")]
    public async Task<ActionResult<IEnumerable<TraineeViewModel>?>> GetTraineesByReviewer([FromRoute] [ValidPfid] int pfid)
    {
        IEnumerable<Trainee?> trainees = await _userService.TraineesByReviewerAsync(pfid);
        IEnumerable<PeopleFinderUser?> users = await _userService.GetPFUsersAsync();
        IEnumerable<TraineeViewModel?> traineesVM = _mapper.Map<IEnumerable<Trainee?>,IEnumerable<TraineeViewModel>>(
            trainees.Where(
                trainee => users.Any(
                    user => user?.OtherPfid == trainee?.TraineePfid
                )
            ).OfType<Trainee>().ToList()!
        ).OfType<TraineeViewModel>().ToList();
        foreach (PeopleFinderUser? user in users) user!.Photo = (bnetUrl + user.Photo?.ToString()) ?? "../../../assets/profilePic.png";
        foreach (TraineeViewModel? trainee in traineesVM) _mapper.Map(users.FirstOrDefault(user => trainee?.TraineePfid == user?.OtherPfid)!, trainee);
        return (trainees.GetType() == typeof(List<Trainee>)) && traineesVM != null ? Ok(traineesVM) : StatusCode(404);
    }

    /// <summary>
    /// POST: api/{version}/User/SetPair
    /// </summary>
    /// <param name="pfid">PFID of trainee</param>
    /// <param name="addReq">AddModifyTraineeReq DTO</param>
    /// <response code="201">{ new trainee object }</response>
    /// <response code="400">object not created</response>
    [Consumes(MediaTypeNames.Application.Json)]
    [Authorize(Policy="tracr-admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created,Type=typeof(TraineeViewModel))]
    [ActionName("SetPair"),HttpPost("[action]/{pfid:int}")]
    public async Task<ActionResult<TraineeViewModel>?> SetPair([FromRoute] [ValidPfid] int pfid, [FromBody] AddModifyTraineeReq addReq)
    {
        if ((await _userService.GetPFUserAsync(pfid) is null)||(addReq is null)) return StatusCode(400);
        Trainee? newTrainee = _userService.SetPair(pfid,_mapper.Map<AddModifyTraineeReq,Trainee>(addReq));
        TraineeViewModel traineeVM = _mapper.Map<Trainee,TraineeViewModel>(newTrainee!);
        return CreatedAtAction(nameof(GetTraineesByReviewer), new { pfid = newTrainee?.ReviewerPfid }, traineeVM);
    }
    
    /// <summary>
    /// GET: api/{version}/Employee/GetEmployees
    /// </summary>
    /// <response code="200">{employee view objects}</response>
    /// <response code="404">missing employee objects</response>
       [Obsolete("Maintenance")]
    //--------------------------
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
    /// PUT: api/{version}/Employee/EditEmployee/{id}
    /// </summary>
    /// <param name="id">Guid of employee</param>
    /// <param name="modifyReq">AddModifyEmpReq DTO</param>
    /// <response code="200">{employee view object}</response>
    /// <response code="204">invlaid id</response>
       [Obsolete("Maintenance")]
    //--------------------------
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
       [Obsolete("Maintenance")]
    //--------------------------
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
       [Obsolete("Maintenance")]
    //--------------------------
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
       [Obsolete("Maintenance")]
    //--------------------------
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
