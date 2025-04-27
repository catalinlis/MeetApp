import { Component, inject, OnInit } from '@angular/core';
import { NavbarComponent } from "../../navbar/navbar.component";
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { ProfileIdentityComponent } from "../profile-identity/profile-identity.component";
import { OnlineUsersService } from '../../_services/hubs/online-users.service';
import { MembersService } from '../../_services/members.service';
import { Member } from '../../_models/Member';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { AccountService } from '../../_services/account.service';
import { forkJoin } from 'rxjs';
import { images } from '../../constants/interest-resources';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [NavbarComponent, FontAwesomeModule, ProfileIdentityComponent],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.css'
})
export class ProfilePageComponent implements OnInit{
  //private onlineUsersService = inject(OnlineUsersService);
  private memberService = inject(MembersService);
  private accountService = inject(AccountService);
  private router = inject(Router);
  onlineUsers: string[] = [];
  onlineMembers: Member[] = [];
  urlMap: Map<string, SafeUrl | null> = new Map();

  constructor(private sanitizer: DomSanitizer, private onlineUsersService: OnlineUsersService) {}

  ngOnInit(): void {
    this.onlineUsersService.onlineUsers$.subscribe(users => {
      this.onlineUsers = users;
      this.memberService.getOnlineMembers(this.onlineUsers).subscribe((users) => {
        this.onlineMembers = users;
        
        const onlineUsersRequests = users.map(user => {
          this.accountService.getSignedUrl(user.profilePhoto).subscribe({
            next: (response) => {
              const objectUrl = response.signedUrl;
              const imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
              this.urlMap.set(user.username, imageUrl);
            },
            error: _ => {
              const imageUrl = null;
              this.urlMap.set(user.username, imageUrl);
            }
          });

        });
      })
    })
  }

  redirectToChat(user: Member){
    this.router.navigate(['/messages', user.username]);
  }

}
