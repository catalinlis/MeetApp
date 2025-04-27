import { Component, inject, OnChanges, OnInit, SimpleChanges } from '@angular/core';
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

@Component({
  selector: 'app-member-profile',
  standalone: true,
  imports: [NavbarComponent, CommonModule, MemberAboutComponent, MemberInterestsComponent, FontAwesomeModule, MemberFriendsComponent],
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
  
  currentUser: string = "";
  member = {} as Member;
  aboutMember = {} as AboutMember;
  interests: Interest[] = [];
  imageUrl: SafeUrl | null = null;
  buttons: string[] =  ["About", "Posts","Friends", "Photos", "Interests"];
  selectedTab: string = 'About'; // Default selected tab
  tabContent: string = ''; // Content for the selected tab
  loading: boolean = false; // Show loading indicator
  photoLoading: boolean = true;
  friends = false;
  friendRequestSent = false;
  friendRequestReceived = false;
  error: string | null = null; // Handle errors
  
    
  constructor(private sanitizer: DomSanitizer, private location: Location) {
    this.currentUser = this.accountService.currentUser()!.userName;
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.member.username = params['username']; // Update the username
      this.imageUrl = null;
      this.initStack(); // Reinitialize the data
    });
  }

  initStack(){
    this.getMember();
    this.areFriends();
    this.isfriendRequestSent();
    this.isFriendRequestReceived();
    this.getInitialTab();
  }

  getMember(){
    this.route.data.subscribe({
      next: (data) => {
        this.member = data['member'];
        this.imageUrl = null;
        this.loadImage();
      }
    });
  }

  loadImage(){

    this.imageUrl = null;
    this.photoLoading = true
    if(this.member.profilePhoto !== null){
      this.accountService.getSignedUrl(this.member.profilePhoto).subscribe((response) => {
        const objectUrl = response.signedUrl;
        this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
        this.photoLoading = false;
      });
    }
  }

  getInitialTab(){
    this.route.queryParams.subscribe((params) => {
      var tab = params['tab'] || 'about'; // Default to tab 1 if no query param
      tab = tab.toString();
      this.selectedTab = tab.charAt(0).toUpperCase() + tab.slice(1);
      this.buttons.indexOf(this.selectedTab);
      this.fetchTabContent(tab);
    });
  }

  fetchTabContent(tabId: string) {
    this.loading = true;
    this.error = null;
    console.log(tabId);
    switch(tabId){
      case 'about': {
        this.getAboutMember();
        break;
      }

      case 'interests': {
        this.getMemberInterests();
        break;
      }

      case 'friends': {
        this.getMemberFriends();
        break;
      }

      default: break;
    }
  }

  areFriends(){
    this.friendsService.areFriends(this.currentUser, this.member.username).subscribe({
      next: (response) => {
        this.friends = response.areFriends;
      },
      error: (err) => console.log(err.error)
    });
  }

  isfriendRequestSent(){
    this.friendsService.isFriendRequestSent(this.currentUser, this.member.username).subscribe({
      next: (response) => {
        this.friendRequestSent = response.friendRequest;
      },
      error: (err) => {
        console.log(err.error);
      }
    });
  }

  isFriendRequestReceived(){
    this.friendsService.isFriendRequestReceived(this.currentUser, this.member.username).subscribe({
      next: (response) => {
        this.friendRequestReceived = response.friendRequest;
      },
      error: (err) => console.log(err.error)
    });
  }

  sendFriendRequest(){
    this.friendsService.sendFriendRequest(this.currentUser, this.member.username).subscribe({
      next: _ => {
        this.friendRequestSent = true;
      },
      error: (err) => {
        console.log(err.error);
      }
    })
  }

  answerFriendRequest(){
    this.friendsService.answerFriendRequest(this.currentUser, this.member.username).subscribe({
      next: _ => this.friends = true,
      error: (err) => console.log(err.error)
    });
  }

  getAboutMember(){
    this.memberService.getAboutMember(this.member.username).subscribe({
      next: (response) => {
        this.aboutMember = response;
        this.aboutMember.gender = StringProcess.capitalizeFirstLetter(this.aboutMember.gender);
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load the data.';
        this.loading = false;
      }
    });
  }

  getMemberInterests(){
    this.memberService.getMemberInterests(this.member.username).subscribe({
      next: (response) => {
        console.log(response);
        this.interests = response;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load the data.';
        this.loading = false;
      }
    })
  }

  getMemberFriends(){
    this.loading = false;
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

    this.fetchTabContent(tab);
  }
}
