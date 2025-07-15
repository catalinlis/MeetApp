import { Component, inject, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Feed } from '../../../_models/Feed';
import { AccountService } from '../../../_services/account.service';
import { MembersService } from '../../../_services/members.service';
import { Member } from '../../../_models/Member';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { PostingService } from '../../../_services/posting.service';
import { LoaderComponent } from "../../../loader/loader.component";
import { DateProcessing } from '../../../utils/DateProcessing';
import { MatDialog } from '@angular/material/dialog';
import { SeePostComponent } from '../../../see-post/see-post.component';
import { User } from '../../../_models/User';
import { CommentPostComponent } from '../../../comment-post/comment-post.component';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-feed-item',
  standalone: true,
  imports: [CommonModule, LoaderComponent, RouterModule],
  templateUrl: './feed-item.component.html',
  styleUrl: './feed-item.component.css'
})
export class FeedItemComponent implements OnInit, OnChanges{
  private memberService = inject(MembersService);
  private accountService = inject(AccountService);
  private postService = inject(PostingService);
  private sanitizer = inject(DomSanitizer);
  private seePostMat = inject(MatDialog);
  private commentPostMat = inject(MatDialog);
  @Input() feedItem!: Feed;
  @Input() username!: string;
  @Input() currentUserProfilePhoto!: SafeUrl | null; 
  @Input() currentUser!: User | null;
  user: Member | null = null;
  imageUrl: SafeUrl | null = null;
  postImage: SafeUrl | null = null;
  isLiked: boolean = false;
  profilePhotoLoaded: boolean = false;
  postPhotoLoaded: boolean = false;
  createdAt: string = '';
  

  ngOnInit(): void {  
    this.getUserInfo(this.feedItem.author).then(() => {
      var profilePhoto = this.user!.profilePhoto;
      this.getProfilePhoto(profilePhoto);
      this.getPostPhoto(this.feedItem);
      this.isLiked = this.feedItem.likedByMe;
      this.createdAt = DateProcessing.formatPostDate(this.feedItem.createdAt);
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if(changes['feedItem'] && changes['feedItem'].currentValue){
      this.getUserInfo(this.feedItem.author).then(() => {
        var profilePhoto = this.user!.profilePhoto;
        this.getProfilePhoto(profilePhoto);
        this.getPostPhoto(this.feedItem);
        this.createdAt = DateProcessing.formatPostDate(this.feedItem.createdAt);
    });
    }
  }

  getUserInfo(username: string): Promise<any>{

    return new Promise((resolve, reject) => {
      this.memberService.getMember(username).subscribe({
        next: (response) => {
            this.user = response;
            resolve(this.user);
        },
        error: (err) => {
          console.log(err.error);
          reject(err);
        }
      })
    });
  }

  getProfilePhoto(profilePhoto: string): void{
    this.profilePhotoLoaded = false;
    if(profilePhoto !== null){
      this.accountService.getSignedUrl(profilePhoto).subscribe({
        next: (response) => {
          const objectUrl = response.signedUrl;
          const img = new Image();
          img.src = objectUrl;

          img.onload = () => {
            this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
            this.profilePhotoLoaded = true;
          }
        },
        error: _ => {
          this.imageUrl = null;
          this.profilePhotoLoaded = true;
        }
      });
    }
  }

  getPostPhoto(feed: Feed){
    this.postPhotoLoaded = false;
    if(feed.imageUrl !== null){
      this.postService.getPostPhoto(feed)?.subscribe({
        next: (response) => {
          const objectUrl = response.signedUrl;
          const img = new Image();
          img.src = objectUrl;

          img.onload = () => {
            this.postImage = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
            this.postPhotoLoaded = true;
          }
        }
      });
    }
    else{
      this.postPhotoLoaded = true;
    }
  }

  like(){
    this.postService.like(this.feedItem)?.subscribe({
      next: (response) => {
        this.isLiked = response.hasLiked;
        if(this.isLiked)
          this.feedItem.likesCount++;
        else
          this.feedItem.likesCount--;
      },
      error: (err) => {
        console.log(err.error);
      }
    })
  }

  seePost(): void{
    const feedback = this.seePostMat.open(SeePostComponent, {
      width: "90%",
      height: "90%",
      disableClose: true,
      data: {
        photo: this.postImage,
        profilePhoto: this.imageUrl,
        postItem: this.feedItem,
        currentUserProfilePhoto: this.currentUserProfilePhoto, 
        user: this.user,
        currentUser: this.currentUser,
        createdAt: this.createdAt,
        isLiked: this.isLiked,
      }
    });

    feedback.afterClosed().subscribe((result) => {
      this.feedItem = result.postItem;
      this.isLiked = result.isLiked;
    });
  }

  commentPost(): void{
    const feedback = this.commentPostMat.open(CommentPostComponent, {
      width: "800px",
      height: "90%",
      disableClose: true,
      data: {
        photo: this.postImage,
        profilePhoto: this.imageUrl,
        postItem: this.feedItem,
        currentUserProfilePhoto: this.currentUserProfilePhoto, 
        user: this.user,
        currentUser: this.currentUser,
        createdAt: this.createdAt,
        isLiked: this.isLiked,
      }
    });

    feedback.afterClosed().subscribe((result) => {
      this.feedItem = result.postItem;
      this.isLiked = result.isLiked;
    });
  }

  

}
