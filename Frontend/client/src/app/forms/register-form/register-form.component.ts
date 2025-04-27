import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { TextInputComponent } from "../../checks/text-input/text-input.component";
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faQuestionCircle } from '@fortawesome/free-solid-svg-icons';
import { DatePickerComponent } from "../date-picker/date-picker.component";
import {MatRadioModule} from '@angular/material/radio';
import { AccountService } from '../../_services/account.service';
import { Router } from '@angular/router';


@Component({
  selector: 'app-register-form',
  standalone: true,
  imports: [ReactiveFormsModule, TextInputComponent, FontAwesomeModule, DatePickerComponent, MatRadioModule],
  templateUrl: './register-form.component.html',
  styleUrl: './register-form.component.css'
})
export class RegisterFormComponent implements OnInit{
  questionMark = faQuestionCircle;
  private accountService = inject(AccountService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  registerForm: FormGroup = new FormGroup({});
  maxDate = new Date();
  validationErrors: string[] | undefined;


  ngOnInit(): void {
      this.initializeForm();
      this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }
  
  initializeForm(){
    this.registerForm = this.fb.group({
      firstname: ['', Validators.required],
      lastname: ['', Validators.required],
      username: ['', Validators.required],
      email: ['', Validators.required],
      dateOfBirthday: ['', Validators.required],
      country: ['', Validators.required],
      city: ['', Validators.required],
      gender: ['', Validators.required],
      password: ['', Validators.required],
      repeatpassword: ['', [Validators.required, this.matchValues('password')]],
    })

    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () => this.registerForm.controls['repeatpassword'].updateValueAndValidity()
    })
  }

  matchValues(matchTo: string) : ValidatorFn{
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : {isMatching: true}
    };
  }

  private getDateOnly(dob: string | undefined){
    if(!dob) return;

    const date = new Date(dob);
    
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0"); // Months are 0-indexed
    const day = String(date.getDate()).padStart(2, "0");

    const parsedDateString = `${year}-${month}-${day}`;
    const parsedDate = new Date(parsedDateString);
    const formattedDate = parsedDate.toISOString();
    
    return formattedDate;
  }

  register(){
    const dob = this.getDateOnly(this.registerForm.get('dateOfBirthday')?.value);
    this.registerForm.patchValue({dateOfBirthday: dob});
    this.accountService.register(this.registerForm.value).subscribe({
      next: response => {
        this.router.navigateByUrl("/register/image-upload");
      },
      error: error => {
        console.log(error);
      }
    })
  }
}
