import { Component, inject, input, model, OnInit, output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { TextInputComponent } from '../_forms/text-input/text-input.component';
import { NgIf } from '@angular/common';
import { DatePickerComponent } from "../_forms/date-picker/date-picker.component";
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

function equalValues(ctrl1: string, ctrl2: string){
  return (control: AbstractControl) => {
  const val1=control.get(ctrl1)?.value
  const val2=control.get(ctrl2)?.value

  if(val1 == val2)
    return null;

  return {misMatch: true}
}
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, TextInputComponent, NgIf, DatePickerComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {  
  private accountService = inject(AccountService);
  private router = inject(Router)
  private toastr = inject(ToastrService)
  fb = inject(FormBuilder)
  cancelRegister = output<boolean>();
  registerForm: FormGroup = new FormGroup({})
  maxDate= new Date;
  validationErrors: [] = [];
  model: any;

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18)
  }

  private initializeForm() {
    this.registerForm = this.fb.group({
      username: ['', { validators: [Validators.required] }],
      gender: ['male', { validators: [Validators.required] }],
      dateOfBirth: ['', { validators: [Validators.required] }],
      knownAs: ['', { validators: [Validators.required] }],
      city: ['', { validators: [Validators.required] }],
      country: ['', { validators: [Validators.required] }],
      password: ['', {
        validators: [
          Validators.required,
          Validators.minLength(4),
          Validators.maxLength(8)
        ]
      }],
      confirmPassword: ['', { validators: [Validators.required] }]
    }, { validators: [equalValues('password', 'confirmPassword')] });
  }

  register(){
    this.model = this.registerForm.value;
    const dob = this.getDateOnly(this.registerForm.get('dateOfBirth')?.value);
    this.model.dateOfBirth = dob;
    this.accountService.register(this.model)
    .subscribe({
      next: ()=> {
        this.router.navigateByUrl("/members");
      },
      error: err => this.validationErrors = err
    })
  }

  getDateOnly(dob: string | undefined){
    if(!dob) return;

    return new Date(dob).toISOString().slice(0,10)
  }
  cancel(){
    this.cancelRegister.emit(false);
  }    
}

  

