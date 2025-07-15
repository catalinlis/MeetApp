import { Component, inject, OnInit } from '@angular/core';
import { NavbarComponent } from "../../../navbar/navbar.component";
import { Member } from '../../../_models/Member';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { AccountService } from '../../../_services/account.service';
import { CommonModule } from '@angular/common';
import { MembersService } from '../../../_services/members.service';
import { AboutMember } from '../../../_models/AboutMember';
import { StringProcess } from '../../../utils/StringProcess';
import { MemberAboutComponent } from "./member-about/member-about.component";
import { MemberInterestsComponent } from "./member-interests/member-interests.component";
import { Location } from '@angular/common';
import { faUserFriends } from '@fortawesome/free-solid-svg-icons';
import { faAdd } from '@fortawesome/free-solid-svg-icons';
import { faCheck } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { FriendService } from '../../../_services/friend.service';
import { MemberFriendsComponent } from "./member-friends/member-friends.component";
import { Interest } from '../../../_models/Interest';
import { LoaderComponent } from "../../../loader/loader.component";
import { MemberPhotosComponent } from "./member-photos/member-photos.component";
import { MemberPostsComponent } from "./member-posts/member-posts.component";
import { User } from '../../../_models/User';

@Component({
  selector: 'app-member-profile',
  standalone: true,
  imports: [NavbarComponent, CommonModule, MemberAboutComponent, MemberInterestsComponent, FontAwesomeModule, MemberFriendsComponent, LoaderComponent, MemberPhotosComponent, MemberPostsComponent],
  templateUrl: './member-profile.component.html',
  styleUrl: './member-profile.component.css'
})
export class MemberProfileComponent implements OnInit{
  faAdd = faAdd;
  faUserFriend = faUserFriends;
  faCheck = faCheck;
  private route = inject(ActivatedRoute);
  private accountService = inject(AccountService);
  private memberService = inject(MembersService);
  private friendsService = inject(FriendService); 
  private router = inject(Router);
  currentUser: User | null = null;
  profilePhoto: SafeUrl | null = null;
  currentProfilePhoto: SafeUrl | null = null;
  member = {} as Member;
  aboutMember = {} as AboutMember;
  interests: Interest[] = [];
  imageUrl: SafeUrl | null = null;
  buttons: string[] =  ["About", "Posts","Friends", "Photos", "Interests"];
  selectedTab: string = 'About';
  tabContent: string = '';
  photoLoaded: boolean = false;
  friends = false;
  friendRequestSent = false;
  friendRequestReceived = false;
  
  constructor(private sanitizer: DomSanitizer, private location: Location) {
    this.currentUser = this.accountService.currentUser()!;
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.member.username = params['username'];
      this.initStack();
    });
  }

  initStack(){
    this.getMember().then((member) => {
      this.member = member

      const currentUsername = this.currentUser!.userName;
      const memberUsername = this.member.username;

      this.getProfilePhoto(this.currentUser!.profilePhoto).then(img => { this.currentProfilePhoto = img; });
      this.loadImage(member);
      this.areFriends(currentUsername, memberUsername);
      this.isfriendRequestSent(currentUsername, memberUsername);
      this.isFriendRequestReceived(currentUsername, memberUsername);
      this.getInitialTab();
    });
  }

  getMember() : Promise<Member>{
    return new Promise<Member>((resolve, reject) => {
      this.route.data.subscribe({
        next: (data) => {
          const member = data['member'];
          resolve(member);
        },
        error: (err) => {
          console.log(err.error);
          reject();
        }
      });
    })
  }

  loadImage(member: Member){

    this.photoLoaded = false;
    if(member.profilePhoto !== null){
      this.accountService.getSignedUrl(member.profilePhoto).subscribe((response) => {
        const objectUrl = response.signedUrl;
        const img = new Image();
        img.src = objectUrl;

        img.onload = () => {
          this.profilePhoto = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
          this.photoLoaded = true;
        }
        
      });
    }
  }

  getProfilePhoto(photoId: string): Promise<SafeUrl>{
    return new Promise<SafeUrl>((resolve, reject) => {
      this.accountService.getSignedUrl(photoId).subscribe({
        next: (response) => {
          const objectUrl = response.signedUrl;
          const img = new Image();
          img.src = objectUrl;
          img.onload = () => {
            const imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
            resolve(imageUrl);
          }
        },
        error: (err) => {
          console.log(err.error);
          reject();
        }
      })
    })
  }

  getInitialTab(){
    this.route.queryParams.subscribe((params) => {
      var tab = params['tab'] || 'about';
      tab = tab.toString();
      this.selectedTab = tab.charAt(0).toUpperCase() + tab.slice(1);
      this.buttons.indexOf(this.selectedTab);
    });
  }

  areFriends(currentUser: string, memberUsername: string){
    this.friendsService.areFriends(currentUser, memberUsername).subscribe({
      next: (response) => {
        this.friends = response.areFriends;
      },
      error: (err) => console.log(err.error)
    });
  }

  isfriendRequestSent(currentUser: string, memberUsername: string){
    this.friendsService.isFriendRequestSent(currentUser, memberUsername).subscribe({
      next: (response) => {
        this.friendRequestSent = response.friendRequest;
      },
      error: (err) => {
        console.log(err.error);
      }
    });
  }

  isFriendRequestReceived(currentUser: string, memberUsername: string){
    this.friendsService.isFriendRequestReceived(memberUsername, currentUser).subscribe({
      next: (response) => {
        this.friendRequestReceived = response.friendRequest;
      },
      error: (err) => console.log(err.error)
    });
  }

  sendFriendRequest(currentUser: string, memberUsername: string){
    this.friendsService.sendFriendRequest(currentUser, memberUsername).subscribe({
      next: _ => {
        this.friendRequestSent = true;
      },
      error: (err) => {
        console.log(err.error);
      }
    })
  }

  answerFriendRequest(currentUser: string, memberUsername: string){
    this.friendsService.answerFriendRequest(currentUser, memberUsername).subscribe({
      next: _ => this.friends = true,
      error: (err) => console.log(err.error)
    });
  }

  getMemberInterests(memberUsername: string){
    this.memberService.getMemberInterests(memberUsername).subscribe({
      next: (response) => {
        this.interests = response;
      },
      error: (err) => {
        console.log(err.error);
      }
    })
  }

  selectTab(tab: string) {
    this.selectedTab = tab;
    var param = tab.charAt(0).toLowerCase() + tab.slice(1);

    // Update the URL with the selected tab's query parameter
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: param },
      queryParamsHandling: 'merge', // Merge with existing query params
    });


    const url = this.router.createUrlTree([],{
      relativeTo: this.route,
      queryParams: { tab: param }
    });

    this.location.replaceState(url.toString());
  }
}
