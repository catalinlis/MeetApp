import { Component, inject, OnInit } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faAddressCard, faCog, faEnvelope, faUserFriends, faImages } from '@fortawesome/free-solid-svg-icons';
import { AccountService } from '../../_services/account.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { LoaderComponent } from "../../loader/loader.component";
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-profile-identity',
  standalone: true,
  imports: [FontAwesomeModule, CommonModule, LoaderComponent, RouterModule],
  templateUrl: './profile-identity.component.html',
  styleUrl: './profile-identity.component.css'
})
export class ProfileIdentityComponent implements OnInit{
  faEnvelope = faEnvelope;
  faImages = faImages;
  faUserFriends = faUserFriends;
  faAddressCard = faAddressCard;
  faCog = faCog;
  private accountService = inject(AccountService);
  currentUser = this.accountService.currentUser();
  imageUrl: SafeUrl | null = null;
  photoLoaded: boolean = false;

  constructor(private sanitizer: DomSanitizer) {}
  
  ngOnInit(): void {
    this.loadProfilePhoto()
  }

  loadProfilePhoto(){
    this.photoLoaded = false;
    var profilePhoto = this.accountService.currentUser()!.profilePhoto;
    if(profilePhoto !== null)
      this.accountService.getSignedUrl(profilePhoto).subscribe({
        next: (response) => {
          const objectUrl = response.signedUrl;
          const img = new Image();
          img.src = objectUrl;

          img.onload = () => {
            this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
            this.photoLoaded = true;
          }
        },
        error: _ => {
          this.imageUrl = null;
          this.photoLoaded = true;
        }
      });
  }
}
