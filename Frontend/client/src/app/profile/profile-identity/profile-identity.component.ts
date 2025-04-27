import { Component, inject } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faAddressCard, faCog, faEnvelope, faUserFriends } from '@fortawesome/free-solid-svg-icons';
import { AccountService } from '../../_services/account.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-profile-identity',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './profile-identity.component.html',
  styleUrl: './profile-identity.component.css'
})
export class ProfileIdentityComponent {
  faEnvelope = faEnvelope;
  faUserFriends = faUserFriends;
  faAddressCard = faAddressCard;
  faCog = faCog;
  private accountService = inject(AccountService);
  currentUser = this.accountService.currentUser();
  imageUrl: SafeUrl | null = null;

  constructor(private sanitizer: DomSanitizer) {}
  
  ngOnInit(): void {
    var profilePhoto = this.accountService.currentUser()!.profilePhoto;
    if(profilePhoto !== null)
      this.accountService.getSignedUrl(profilePhoto).subscribe({
        next: (response) => {
          const objectUrl = response.signedUrl;
          this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
        },
        error: _ => this.imageUrl = null
      });
  }
}
