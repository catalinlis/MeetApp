import { Component, inject, Input, input } from '@angular/core';
import { Member } from '../../../_models/Member';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { AccountService } from '../../../_services/account.service';
import { Router } from '@angular/router';
import { LoaderComponent } from "../../../loader/loader.component";
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [LoaderComponent, CommonModule, RouterModule],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {
  private accountService = inject(AccountService);
  private router = inject(Router);
  @Input() member!: Member;
  imageUrl: SafeUrl | null = null;
  photoLoaded: boolean = false;
  
  constructor(private sanitizer: DomSanitizer) {}
    
  ngOnInit() {
    this.loadProfilePhoto();
  }

  private loadProfilePhoto(){
    this.photoLoaded = false;
    if(this.member?.profilePhoto){
      this.accountService.getSignedUrl(this.member.profilePhoto).subscribe(
        (response) => {
          const signedUrl = response.signedUrl;
          const img = new Image();
          img.src = signedUrl;

          img.onload = () => {
            this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(signedUrl);
            this.photoLoaded = true;
          }
        },
        (error) => {
          console.error('Error fetching signed URL:', error);
          this.photoLoaded = true;
        }
      )
    }
  }

  redirectProfile(){
    this.router.navigateByUrl("/community/"+this.member.username);
  }
}
