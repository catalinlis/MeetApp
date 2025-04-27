import { Component, inject, Input, input } from '@angular/core';
import { Member } from '../../../_models/Member';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { AccountService } from '../../../_services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {
  private accountService = inject(AccountService);
  private router = inject(Router);
  @Input() member!: Member;
  imageUrl: SafeUrl | null = null;
  
  constructor(private sanitizer: DomSanitizer) {}
    
  ngOnInit() {
    this.loadProfilePhoto();
  }

  private loadProfilePhoto(){
    if(this.member?.profilePhoto){
      this.accountService.getSignedUrl(this.member.profilePhoto).subscribe(
        (response) => {
          const signedUrl = response.signedUrl;
          this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(signedUrl);
        },
        (error) => {
          console.error('Error fetching signed URL:', error);
        }
      )
    }
  }

  redirectProfile(){
    this.router.navigateByUrl("/community/"+this.member.username);
  }
}
