import { Component, inject, Input, OnInit } from '@angular/core';
import { FriendService } from '../../../../_services/friend.service';
import { Member } from '../../../../_models/Member';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { AccountService } from '../../../../_services/account.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';


@Component({
  selector: 'app-member-friends',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './member-friends.component.html',
  styleUrl: './member-friends.component.css'
})
export class MemberFriendsComponent implements OnInit{
  private friendService = inject(FriendService);
  private accountService = inject(AccountService);
  @Input() user!: Member;
  friends : Member[] = [];
  imageUrl: SafeUrl | null = null;
  loaded: boolean = false;

  constructor(private sanitizer: DomSanitizer) {}

  ngOnInit() {
    this.getFriends();
  }

  getFriends(){
    this.friendService.getUserFriends(this.user.username).subscribe({
      next: (response) =>  {
        this.loaded = true
        this.friends = response.friends.map(friend => ({
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
}
