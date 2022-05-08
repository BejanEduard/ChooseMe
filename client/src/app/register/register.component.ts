import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();

  registerForm!: FormGroup;
  maxDate!: Date;
  validationErrors: string[] = [];

  constructor(private accountService: AccountService, private toastr: ToastrService,
     private fb: FormBuilder, private router: Router) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm(){
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: [null, Validators.required],
      knowsAs: [null, Validators.required],
      dateOfBirth: [null, Validators.required],
      city: [null, Validators.required],
      country: [null, Validators.required],      
      password: [null, [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: [null, [Validators.required, this.matchValues('password')]]
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      if (control.parent && control.parent.controls){
        return control.value === (control.parent.controls as {[key: string]: AbstractControl})[matchTo].value 
        ? null : {isMatching: true}
      }
      return null;
    }
  }

register(){  
  this.accountService.register(this.registerForm.value).subscribe(
    response => {
      this.router.navigateByUrl('/members');
    }, error => {
      this.validationErrors = error;
    }
  )
}

cancel(){
  this.cancelRegister.emit(false);
}

}
