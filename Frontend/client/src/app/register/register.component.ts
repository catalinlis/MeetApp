import { Component, Inject, OnDestroy, OnInit, Renderer2 } from '@angular/core';
import { NavbarComponent } from "../navbar/navbar.component";
import { RegisterFormComponent } from "../forms/register-form/register-form.component";
import { DOCUMENT } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [NavbarComponent, RegisterFormComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit, OnDestroy{

  constructor(@Inject(DOCUMENT) private document: Document) {}

  ngOnDestroy(): void {
    this.document.body.style.backgroundColor = '';
  }
  
  ngOnInit(): void {
    this.document.body.style.backgroundColor = '#EBEBEB';
  }

}
