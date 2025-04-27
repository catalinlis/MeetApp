import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TextInputComponent } from "../../checks/text-input/text-input.component";
import { AccountService } from '../../_services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login-form',
  standalone: true,
  imports: [ReactiveFormsModule, TextInputComponent],
  templateUrl: './login-form.component.html',
  styleUrl: './login-form.component.css'
})
export class LoginFormComponent implements OnInit{
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);
  loginForm: FormGroup = new FormGroup({});
  validatorsErrors: string[] | undefined;

  login(): void{
    const { username, password } = this.loginForm.value;
    console.log(`Username: ${username} Password: ${password}`);
    this.accountService.login(this.loginForm.value).subscribe({
      next: response => {
        this.router.navigateByUrl("/profile");
      },
      error: error => {
        console.log(error);
      }
    })
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(){
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    })
  }

}
