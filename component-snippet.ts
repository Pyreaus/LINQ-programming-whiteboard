enum UserType {Trainee='Trainee',Reviewer='Reviewer',Admin='Admin',Unauthorized='Unauthorized'}
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HOMEComponent implements OnInit {
  @ViewChild('sc') sc!: Scroller;
  userType$: BehaviorSubject<UserType>;
  user!: Observable<any>;
  date!: Date[];
  items: string[][] = []; // TODO: populate with user data
  visible = true;
  constructor() { this.userType$ = new BehaviorSubject<UserType>(UserType.Unauthorized) }

    ngOnInit(): void {
      this.items = Array.from({ length: 1000 }).map((_, i) =>
      Array.from({ length: 1000 }).map((_j, j) => `Item #${i}_${j}`));
      setTimeout(() => {
        this.userType$.next(UserType.Admin);
      }, 1000);
    }
    reset(): void {
        this.sc.scrollToIndex(0, 'smooth');
    }
}
                                                              //   *** Employee sub-component ts file **
export class EmployeeAddEditComponent implements OnInit {
  id!: string | number;
  mode!: string | number;
  submitted!: boolean;
  employee$!: Observable<Employee>;
  DepList: string[] = [];
  UnfilteredDepList: string[] = [];
  newEmployee: AddModifyEmpReq = { name:'', email:'', phone:'' };
  editEmployeeForm: FormGroup  = this.fb.group({
    id: [{ value:'', disabled:true },Validators.required],
    name: [null, [Validators.required, Validators.pattern(/^[a-zA-Z]{1,15}\s[a-zA-Z]{1,15}$/)]],
    phone: [null, [Validators.required, Validators.pattern(/^[- +()0-9]{10,15}$/)]],
    email: [null, [Validators.required, Validators.email]]
  });
  employeeForm: FormGroup  = this.fb.group({
    name: [null, [Validators.required, Validators.pattern(/^[a-zA-Z]{1,15}\s[a-zA-Z]{1,15}$/)]],
    phone: [null, [Validators.required, Validators.pattern(/^[- +()0-9]{10,15}$/)]],
    email: [null, [Validators.required, Validators.email]]
  });
  constructor(private fb:FormBuilder,private router:Router,private route:ActivatedRoute,private employeeService:EmployeeService)
  { }
  ngOnInit(): void {
    this.submitted = false;
    this.id = this.route.snapshot.paramMap.get('id') ?? '0';
    this.mode = Number(this.id) == 0 ? 'add' : 'edit';
    this.employee$ = this.employeeService.GetEmployee(this.id);
  }
  SortResult2(x: number, asc: boolean = true): void {
    this.DepList = this.UnfilteredDepList.sort(function (a, b) {
      return asc ? (a[x] > b[x]) ? 1 : ((a[x] < b[x]) ? -1 : 0) : (b[x] > a[x]) ? 1 : ((b[x] < a[x]) ? -1 : 0);
    });
  }
  onSubmitAdd(): void {
    if (this.employeeForm.valid) {
        this.newEmployee.name = this.employeeForm.get('name')!.value;
        this.newEmployee.email = this.employeeForm.get('email')!.value;
        this.newEmployee.phone = this.employeeForm.get('phone')!.value;
        this.employeeService.AddEmployee(this.newEmployee).subscribe(res => console.info(res));
        this.router.navigateByUrl('/employees');
    } else console.warn('Form Invalid');
  }
  onSubmitEdit(): void {
    this.employeeService.EditEmployee(this.editEmployeeForm.getRawValue().id,this.editEmployeeForm.value).subscribe(res => console.info(res));
    // this.router.navigateByUrl('/employees');
  }
}
