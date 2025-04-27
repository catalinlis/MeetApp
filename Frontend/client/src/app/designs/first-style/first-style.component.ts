import { Component } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { LoginFormComponent } from "../../forms/login-form/login-form.component";
import { NavbarComponent } from "../../navbar/navbar.component";

@Component({
  selector: 'app-first-style',
  standalone: true,
  imports: [
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatDividerModule,
    FontAwesomeModule,
    LoginFormComponent,
    NavbarComponent
],
  templateUrl: './first-style.component.html',
  styleUrl: './first-style.component.css'
})
export class FirstStyleComponent {

}
