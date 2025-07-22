import { Component, inject } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { Router } from '@angular/router';
import { Member } from '../../_models/Member';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { OnlineUsersService } from '../../_services/hubs/online-users.service';
import { AccountService } from '../../_services/account.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-online-users',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './online-users.component.html',
  styleUrl: './online-users.component.css'
})
export class OnlineUsersComponent {
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
}
