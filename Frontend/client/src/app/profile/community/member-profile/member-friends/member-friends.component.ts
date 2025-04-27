import { Component, inject, Input, OnInit } from '@angular/core';
import { FriendService } from '../../../../_services/friend.service';
import { Member } from '../../../../_models/Member';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { AccountService } from '../../../../_services/account.service';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpEventType, HttpHeaders, HttpRequest } from '@angular/common/http';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-member-friends',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './member-friends.component.html',
  styleUrl: './member-friends.component.css'
})
export class MemberFriendsComponent implements OnInit{
  private friendService = inject(FriendService);
  private accountService = inject(AccountService);
  private http = inject(HttpClient);
  private router = inject(Router);
  @Input() username: string = "";
  friends : Member[] = [];
  imageUrl: SafeUrl | null = null;
  constructor(private sanitizer: DomSanitizer) {}

  ngOnInit() {
    this.getFriends();
  }

  getFriends(){
    this.friendService.getUserFriends(this.username).subscribe({
      next: (response) =>  {this.friends = response.friends.map(friend => ({
        ...friend,
        imageLoaded: false,
        safeImageUrl: null,
      }));
      this.loadImages();
    },
      error: (err) => console.log(err)
    });

  }

  loadImages(){
    this.friends.forEach(friend => {
      if(friend.profilePhoto){
        this.accountService.getSignedUrl(friend.profilePhoto).subscribe({
          next: (response) => {
            const objectUrl = response.signedUrl;
            const img = new Image();
            img.src = objectUrl;

            img.onload = () => {
              friend.safeImageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
              friend.imageLoaded = true;
            }
          }
        });
    }});
  }

  redirectProfile(username: string){
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate(['/community', username], { replaceUrl: true });
    });
  }
}
