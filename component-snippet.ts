enum UserType {Trainee='Trainee',Reviewer='Reviewer',Admin='Admin',Unauthorized='Unauthorized'}
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class HOMEComponent implements OnInit, AfterViewInit{
  @ViewChild('tdElements', { static: false }) tdElementsRef!: ElementRef[];
  @ViewChild('sc') sc!: Scroller;

  user!: Observable<any>;
  items: string[][] = [];
  date!: Date[];
  weekRange!:string[];
  rowSelected!: string;
  barVisible!: boolean;
  userType$: BehaviorSubject<UserType> = new BehaviorSubject<UserType>(UserType.Unauthorized);
  constructor(private renderer: Renderer2, private elementRef: ElementRef) {
    [this.rowSelected,this.barVisible] = ['cal',true];
  }
  ngOnInit(): void {
    this.items = Array.from({ length: 1000 }).map((_, i) =>
    Array.from({ length: 1000 }).map((_j, j) => `Item #${i}_${j}`));
    setTimeout(() => {
      this.userType$.next(UserType.Trainee);
    }, 1000);
  }
  selected(num: number): void {
    let month!: number;
    let monthNumber: number[] = [1,2,3,4,5,6,7,8,9,10,11,12];
    let weekRange!: string[];
    let year: string =  this.elementRef.nativeElement.querySelectorAll('.p-datepicker-year')
      [0].innerHTML.replace(/\s/g,'').slice(-2);
    let monthName: string[] = ['January','February','March','April','May','June','July','August','September','October','November','December'];
    for(let i = 0; i < monthName.length; i++){
      if(this.elementRef.nativeElement.querySelectorAll('.p-datepicker-month')[0].innerHTML.replace(/\s/g,'') === monthName[i]){
        month = monthNumber[i]}
    }
    switch(num){
      case 1:
        let [day1w1,day2w1]: [number,number] = [this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[0].innerHTML.replace(/\D/g,''),
          this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[6].innerHTML.replace(/\D/g,'')
        ];
        let startMonth:number = Number(day1w1) < Number(day2w1) ? month : month - 1;
        [weekRange,this.rowSelected] = [[`${day1w1}/${startMonth}/${year}`,`${day2w1}/${month}/${year}`],'cal1'];
          break;
        case 2:
          [weekRange,this.rowSelected] = [[`${this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[7].innerHTML.replace(/\D/g,'')}/${month}/${year}`,
        `${this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[13].innerHTML.replace(/\D/g,'')}/${month}/${year}`],'cal2'];
          break
        case 3:
          [weekRange,this.rowSelected] = [[`${this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[14].innerHTML.replace(/\D/g,'')}/${month}/${year}`,
        `${this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[20].innerHTML.replace(/\D/g,'')}/${month}/${year}`],'cal3'];
          break;
        case 4:
          [weekRange,this.rowSelected] = [[`${this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[21].innerHTML.replace(/\D/g,'')}/${month}/${year}`,
        `${this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[27].innerHTML.replace(/\D/g,'')}/${month}/${year}`],'cal4'];
          break;
        case 5:
          let [day1w4,day2w4]: [number,number] = [this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[28]
          .innerHTML.replace(/\D/g,''),this.elementRef.nativeElement.querySelectorAll('.p-ripple.p-disabled')[34].innerHTML.replace(/\D/g,'')];
          let endMonth:number = Number(day1w4) < Number(day2w4) ? month : month + 1;
          [weekRange,this.rowSelected] = [[`${day1w4}/${month}/${year}`,`${day2w4}/${endMonth}/${year}`],'cal5'];
          break;
      }
      console.log(weekRange + '' + this.rowSelected);
  }
  reset(): void {
    this.sc.scrollToIndex(0,'smooth')
  }
  ngAfterViewInit(): void {
    setTimeout(() => {
      //
    },0);
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
  }                                                               //sort expression
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
