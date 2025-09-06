import { Component, inject, OnInit } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDialog } from '@angular/material/dialog';
import { WritePostComponent } from '../write-post/write-post.component';
import { AddPhotoComponent } from '../add-photo/add-photo.component';
import { Feed } from '../../_models/Feed';
import { PostingService } from '../../_services/posting.service';
import { FeedItemComponent } from "./feed-item/feed-item.component";
import { CommonModule } from '@angular/common';
import { LoaderComponent } from "../../loader/loader.component";

@Component({
  selector: 'app-feed',
  standalone: true,
  imports: [MatDialogModule, FeedItemComponent, CommonModule, LoaderComponent],
  templateUrl: './feed.component.html',
  styleUrl: './feed.component.css'
})
export class FeedComponent implements OnInit{
  private accountService = inject(AccountService);
  private postingService = inject(PostingService);
  private sanitizer = inject(DomSanitizer);
  private addPost = inject(MatDialog);
  private addPhoto = inject(MatDialog);
  currentUser = this.accountService.currentUser();
  imageUrl: SafeUrl | null = null;
  photoLoaded: boolean = false;
  feed: Feed[] = [];

  ngOnInit(): void {
    this.postingService.feed$.subscribe(feed => {
      this.feed = feed;
    })

    this.getProfilePhoto();
    this.postingService.getFeed();
  }

  getProfilePhoto(): void{
    this.photoLoaded = false;
    var profilePhoto = this.accountService.currentUser()!.profilePhoto;

    if(profilePhoto !== null){
      this.accountService.getSignedUrl(profilePhoto).subscribe({
        next: (response) => {
          const objectUrl = response.signedUrl;
          const img = new Image();
          img.src = objectUrl;

          img.onload = () => {
            this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
            this.photoLoaded = true;
          }
    
        },
        error: _ => {
          this.imageUrl = null;
          this.photoLoaded = true;
        }
      })
    }
  }

  openPostDialog(): void{
    this.addPost.open(WritePostComponent, {
      width: '500px',
      data: { 
        userPhoto: this.imageUrl,
        firstname: this.currentUser?.firstname,
        lastname: this.currentUser?.lastname
      }
    });
  }

  openPhotoDialog(): void{
    this.addPhoto.open(AddPhotoComponent, {
      width: "500px",
      data: {
        userPhoto: this.imageUrl,
        firstname: this.currentUser?.firstname,
        lastname: this.currentUser?.lastname
      }
    })
  }

}
