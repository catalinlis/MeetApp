import { Component, inject, Input } from '@angular/core';
import { LoaderComponent } from "../../../../loader/loader.component";
import { MembersService } from '../../../../_services/members.service';
import { AccountService } from '../../../../_services/account.service';
import { PostingService } from '../../../../_services/posting.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { MatDialog } from '@angular/material/dialog';
import { Member } from '../../../../_models/Member';
import { DateProcessing } from '../../../../utils/DateProcessing';
import { Feed } from '../../../../_models/Feed';
import { CommonModule } from '@angular/common';
import { SeePostComponent } from '../../../../see-post/see-post.component';
import { CommentPostComponent } from '../../../../comment-post/comment-post.component';
import { RouterModule } from '@angular/router';

interface ExtendedFeed{ 
  feed: Feed, 
  image: SafeUrl | null, 
  createdAt: string
}

@Component({
  selector: 'app-member-posts',
  standalone: true,
  imports: [LoaderComponent, CommonModule, RouterModule],
  templateUrl: './member-posts.component.html',
  styleUrl: './member-posts.component.css'
})
export class MemberPostsComponent {
 private memberService = inject(MembersService);
  private accountService = inject(AccountService);
  private postService = inject(PostingService);
  private sanitizer = inject(DomSanitizer);
  private seePostMat = inject(MatDialog);
  private commentPostMat = inject(MatDialog);
  @Input() profilePhoto!: SafeUrl | null; 
  @Input() currentProfilePhoto!: SafeUrl | null;
  @Input() member!: Member;
  currentUser = this.accountService.currentUser();
  feed: ExtendedFeed[]= [];
  postPhotoLoaded: boolean = false;
  createdAt: string = '';
  isLiked: boolean = false;
  loaded: boolean = false;
  

  ngOnInit(): void {  
    this.getMemberFeed(this.member.username).then(feed => {

      const imgPromises = feed.map((feedItem) => {
        if(feedItem.imageUrl !== null )
          return this.getPostPhoto(feedItem).then((img) => ({type: feedItem.type, id: feedItem.postId, image: img}));
        else
          return ({type: feedItem.type, id: feedItem.postId, image: null})
      });

      Promise.all(imgPromises).then(results => {
      
        feed.forEach(feedItem => {
          const match = results.find(({type, id}) => type === feedItem.type && id === feedItem.postId);
          const image = match ? match.image : null;
          const createdAt = DateProcessing.formatPostDate(feedItem.createdAt);
          
          this.feed.push({feed: feedItem, image: image, createdAt: createdAt});
        })

        this.loaded = true;
      })
    });
  }

  getMemberFeed(username: string): Promise<Feed[]>{
    return new Promise<Feed[]>((resolve, reject) => {
      this.memberService.getMemberFeed(username).subscribe({
        next: (response) => {
          resolve(response);
        },
        error: (err) => {
          reject();
        } 
      })
    });
  }

  getPostPhoto(feed: Feed): Promise<SafeUrl>{
    return new Promise<SafeUrl>((resolve, reject) => {
      if(feed.imageUrl !== null){
        this.postService.getPostPhoto(feed)?.subscribe({
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
      })}
      else
        reject();
    });
  }

  like(feedItem: Feed){
    this.postService.like(feedItem)?.subscribe({
      next: (response) => {
        feedItem.likedByMe = response.hasLiked;
        if(response.hasLiked)
          feedItem.likesCount++;
        else
          feedItem.likesCount--;
      },
      error: (err) => {
        console.log(err.error);
      }
    });
  }

  seePost(extendedFeedItem: ExtendedFeed): void{
    const feedback = this.seePostMat.open(SeePostComponent, {
      width: "90%",
      height: "90%",
      disableClose: true,
      data: {
        photo: extendedFeedItem.image,
        profilePhoto: this.profilePhoto,
        postItem: extendedFeedItem.feed,
        currentUserProfilePhoto: this.currentProfilePhoto, 
        createdAt: extendedFeedItem.createdAt,
        isLiked: extendedFeedItem.feed.likedByMe,
        user: this.member,
        currentUser: this.currentUser,
      }
    });
  
    feedback.afterClosed().subscribe((result) => {
      extendedFeedItem.feed = result.postItem;
      extendedFeedItem.feed.likedByMe = result.isLiked;
    });
  }

  commentPost(extendedFeedItem: ExtendedFeed): void{
      const feedback = this.commentPostMat.open(CommentPostComponent, {
        width: "800px",
        height: "90%",
        disableClose: true,
        data: {
          photo: extendedFeedItem.image,
          profilePhoto: this.profilePhoto,
          postItem: extendedFeedItem.feed,
          currentUserProfilePhoto: this.currentProfilePhoto, 
          user: this.member,
          currentUser: this.currentUser,
          createdAt: extendedFeedItem.createdAt,
          isLiked: extendedFeedItem.feed.likedByMe,
        }
      });
  
      feedback.afterClosed().subscribe((result) => {
        extendedFeedItem.feed = result.postItem;
        extendedFeedItem.feed.likedByMe = result.isLiked;
      });
    }

}
