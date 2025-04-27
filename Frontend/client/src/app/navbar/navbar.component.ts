import { Component, inject } from '@angular/core';
import { faDoorClosed, faMessage } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  faMessage = faMessage;
  faDoorClosed = faDoorClosed;
  accountService = inject(AccountService);
  private router = inject(Router);

  logout(): void{
    this.accountService.logout();
    this.router.navigateByUrl("/");
  }
}
