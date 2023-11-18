                   //[..] imports ^
[ApiController]                     
[Authorize(Policy="tracr-default",AuthenticationSchemes=NegotiateDefaults.AuthenticationScheme)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/v1/[controller]")]
internal sealed partial class UserController : ControllerBase
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
    
    //// <summary>
    /// GET: api/{version}/Diary/GetSkills
    /// </summary>
    /// <response code="200">{skill view objects}</response>
    /// <response code="204">missing skill objects</response>
    [Authorize(Policy="tracr-trainee//tracr-reviewer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(IEnumerable<Skill>))]
    [ActionName("GetSkills"),HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<Skill?>?>> GetSkills()
    {
        IEnumerable<Skill?> skills = await _diaryService.GetSkills();
        return (skills != null) && (typeof(List<Skill>) == skills!.GetType()) ? Ok(skills) : StatusCode(204);
    }

    /// <summary>
    /// GET: api/{version}/Diary/GetDiariesPfid/{pfid}
    /// </summary>
    /// <param name="pfid">PFID of diary objects</param>
    /// <response code="200">{diary view objects}</response>
    /// <response code="204">missing diary objects</response>
    [Authorize(Policy="tracr-trainee//tracr-reviewer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(IEnumerable<DiaryViewModel>))]
    [ActionName("GetDiariesPfid"),HttpGet("[action]/{pfid:int}")]
    public async Task<ActionResult<IEnumerable<DiaryViewModel?>?>> GetDiariesPfid([FromRoute] [ValidPfid] int pfid)
    {
        IEnumerable<Diary?> diaries = await _diaryService.GetDiariesAsync(pfid);
        IEnumerable<DiaryViewModel?> diaryVM = _mapper.Map<IEnumerable<Diary?>, IEnumerable<DiaryViewModel>>(diaries!);
        return (diaryVM != null) && (typeof(List<Diary>) == diaries!.GetType()) ? Ok(diaryVM) : StatusCode(204);
    }

    /// <summary>
    /// GET: api/{version}/User/GetTraineesByReviewer/{pfid}
    /// </summary>
    // / <param name="pfid">trainee reviwer PFID</param>
    /// <response code="200">{trainee view objects}</response>
    /// <response code="404">missing trainee objects</response>
    /// <response code="500">operation failed</response>
    [Authorize(Policy="tracr-reviewer")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(IEnumerable<TraineeViewModel>))]
    [ActionName("GetTraineesByReviewer"),HttpGet("[action]/{pfid:int}")]
    public async Task<ActionResult<IEnumerable<TraineeViewModel?>?>> GetTraineesByReviewer([FromRoute] [ValidPfid] int pfid)
    {
        IEnumerable<PeopleFinderUser?> users = await _userService.GetPFUsersAsync();
        IEnumerable<Trainee?> trainees = await _userService.TraineesByReviewerAsync(pfid);
        if ((users is null)||(trainees is null)) return StatusCode(404);
        IEnumerable<TraineeViewModel?> traineesVM = _mapper.Map<IEnumerable<Trainee?>,IEnumerable<TraineeViewModel>>(
            trainees.Where(
                trainee => users.Any(user => user?.PFID.ToString() == trainee?.TRAINEE_PFID)
            ).OfType<Trainee>().ToList()!).OfType<TraineeViewModel>().ToList();
        foreach (PeopleFinderUser? user in users) user!.Photo = (bnetUrl + user.Photo?.ToString()) ?? "../../../assets/profilePic.png";
        foreach (TraineeViewModel? trainee in traineesVM) _mapper.Map(users.FirstOrDefault(user => trainee?.TRAINEE_PFID == user?.PFID.ToString())!, trainee);
        return (trainees.GetType() == typeof(List<Trainee>)) && traineesVM != null ? Ok(traineesVM) : StatusCode(500);
    }

    /// <summary>
    /// PUT: api/{version}/User/SetPair/{pfid}
    /// </summary>
    /// <param name="pfid">PFID of trainee</param>
    /// <param name="addReq">AddModifyTraineeReq DTO</param>
    /// <response code="201">{ new trainee object }</response>
    /// <response code="204">no content for arguments</response>
    /// <response code="500">object not created</response>
    [Consumes(MediaTypeNames.Application.Json)]
    [Authorize(Policy="tracr-admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status201Created,Type=typeof(TraineeViewModel))]
    [ActionName("SetPair"),HttpPut("[action]/{pfid:int}")]
    public async Task<ActionResult<TraineeViewModel>?> SetPair([FromRoute] [ValidPfid] int pfid, [FromBody] AddModifyTraineeReq addReq)
    {
        Trainee? currentTrainee = await _userService.GetTraineeByPfidAsync(pfid);
        if ((currentTrainee is null)||(addReq is null)||(await _userService.GetPFUserAsync(pfid) is null)) return StatusCode(204);
        _userService.SetPair(_mapper.Map(addReq, currentTrainee!));
        TraineeViewModel traineeVM = _mapper.Map<Trainee,TraineeViewModel>(currentTrainee!);
        return traineeVM != null ? CreatedAtAction(nameof(GetTraineesByReviewer), new { pfid = currentTrainee?.REVIEWER_PFID }, traineeVM) : StatusCode(500);
    }

    /// <summary>
    /// GET: api/{version}/User/GetUserReviewer
    /// </summary>
    /// <param name="pfid">PFID of trainee</param>
    /// <response code="200">{ reviewer view object }</response>
    /// <response code="400">missing reviewer object</response>
    /// <response code="500">operation failed</response>
    [Authorize(Policy="tracr-trainee//tracr-reviewer")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(UserViewModel))]
    [ActionName("GetUserReviewer"),HttpGet("[action]/{pfid:int}")]
    public async Task<ActionResult<UserViewModel?>> GetUserReviewer([FromRoute] [ValidPfid] int pfid)
    {
        Trainee? trainee = await _userService.GetTraineeByPfidAsync(pfid);
        PeopleFinderUser? reviewer = await _userService.ReviewerByTraineeAsync(pfid);
        if ((trainee is null)||(reviewer is null)) return StatusCode(400);
        UserViewModel userVM = _mapper.Map<PeopleFinderUser?,UserViewModel>(reviewer);
        userVM!.Role = "Reviewer";
        userVM!.Photo = (bnetUrl + userVM.Photo?.ToString()) ?? "../../../assets/profilePic.png";    
        return userVM != null ? Ok(userVM) : StatusCode(500);
    }

    /// <summary>
    /// GET: api/{version}/User/GetReviewers
    /// </summary>
    /// <response code="200">{reviewer view objects}</response>
    /// <response code="404">missing reviewer objects</response>
    [Authorize(Policy="tracr-admin")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(IEnumerable<UserViewModel>))]
    [ActionName("GetReviewers"),HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<UserViewModel?>?>> GetReviewers()
    {
        IEnumerable<PeopleFinderUser?> reviewers = await _userService.GetReviewersAsync();
        IEnumerable<UserViewModel?> reviewersVM = _mapper.Map<IEnumerable<PeopleFinderUser?>,IEnumerable<UserViewModel>>(reviewers!);
        foreach(UserViewModel? rev in reviewersVM) 
        {
            rev!.Role = "reviewer";
            rev!.Photo = (bnetUrl + rev!.Photo?.ToString()) ?? "../../../assets/profilePic.png";
        }
        return (reviewersVM != null) && (typeof(List<PeopleFinderUser>) == reviewers!.GetType()) ? Ok(reviewersVM) : StatusCode(204);
    }

    /// <summary>
    /// POST: api/{version}/User/AssignTrainees/{pfid}
    /// </summary>
    /// <param name="pfid">PFID of trainee</param>
    /// <param name="addReq">AddModifyTraineeReq DTO</param>
    /// <response code="201">{ new trainee object }</response>
    /// <response code="400">object not created</response>
    /// <response code="500">operation failed</response>
    [Authorize(Policy="tracr-admin")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status201Created,Type=typeof(TraineeViewModel))]
    [ActionName("AssignTrainees"),HttpPost("[action]/{pfid:int}")]
    public async Task<ActionResult<TraineeViewModel?>> AssignTrainees([FromRoute] [ValidPfid] int pfid, [FromBody] AddModifyTraineeReq addReq)
    {
        if ((await _userService.GetPFUserAsync(pfid) is null)||(addReq is null)) return StatusCode(400);
        Trainee? newTrainee = _mapper.Map<AddModifyTraineeReq,Trainee>(addReq!);
        newTrainee.TRAINEE_PFID = pfid.ToString();
        _userService.AssignTrainees(newTrainee);   
        TraineeViewModel traineeVM = _mapper.Map<Trainee,TraineeViewModel>(newTrainee!);
        return traineeVM != null ? CreatedAtAction(nameof(GetTraineesByReviewer), new { pfid = newTrainee?.REVIEWER_PFID }, traineeVM) : StatusCode(500);
    }

    /// <summary>
    /// PUT: api/{version}/User/EditTrainee/{pfid}
    /// </summary>
    /// <param name="pfid">PFID of trainee</param>
    /// <param name="modifyReq">AddModifyTraineeReq DTO</param>
    /// <response code="200">{AddModifyTraineeReq DTO}</response>
    /// <response code="400">object not modified</response>
    [Consumes(MediaTypeNames.Application.Json)]
    [Authorize(Policy="tracr-admin//tracr-reviewer")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(TraineeViewModel))]
    [ActionName("EditTrainee"),HttpPut("[action]/{pfid:int}")]
    public async Task<ActionResult<TraineeViewModel?>> EditTrainee([FromRoute] [ValidPfid] int pfid, [FromBody] AddModifyTraineeReq modifyReq)
    {
        Trainee? trainee = await _userService.GetTraineeByPfidAsync(pfid);
        if ((trainee is null)||(modifyReq is null)) return StatusCode(400);
        _mapper.Map<AddModifyTraineeReq?,Trainee>(modifyReq, trainee);
        this._userService.UpdateTrainee(trainee);
        return StatusCode(200);
    }

    /// <summary>
    /// GET: api/{version}/User/GetUserType
    /// </summary>
    /// <response code="200">{user view objects}</response>
    /// <response code="511">unauthorized client</response>
    [ProducesResponseType(StatusCodes.Status511NetworkAuthenticationRequired)]
    [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(UserViewModel))]
    [ActionName("GetUserType"),HttpGet("[action]")]
    public async Task<ActionResult<UserViewModel?>> GetUserType([FromServices] IWebHostEnvironment webHostEnvironment)
    {
        IWebHostEnvironment env = webHostEnvironment ?? NullArg<IWebHostEnvironment>(webHostEnvironment!);
        if (_claimsPrincipal.Identity?.IsAuthenticated == true)
        {
            Claim? usernameClaim = _claimsPrincipal.FindFirst("DomainUsername");
            if (usernameClaim?.Value != null)
            {
                PeopleFinderUser? user = await _userService.GetByDomainAsync(usernameClaim.Value);
                if (user != null && user?.PFID != null)
                {
                    string? role = await _userService.GetRoleByPfidAsync((int)user.PFID);
                    UserViewModel? userVM = role != null ? _mapper.Map<PeopleFinderUser,UserViewModel>(user) : null;
                    userVM!.Photo = (bnetUrl + user.Photo?.ToString()) ?? "../../../assets/profilePic.png";
                    userVM!.Role = role ?? "Unauthorized";
                    return userVM != null ? Ok(userVM) : StatusCode(StatusCodes.Status511NetworkAuthenticationRequired);
                }
                return StatusCode(StatusCodes.Status511NetworkAuthenticationRequired);
            }
         }
        return env.IsDevelopment() ? throw new Exception() : StatusCode(StatusCodes.Status500InternalServerError);
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
