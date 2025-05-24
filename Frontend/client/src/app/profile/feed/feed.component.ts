import { Component, inject, OnInit } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDialog } from '@angular/material/dialog';
import { WritePostComponent } from '../write-post/write-post.component';
import { AddPhotoComponent } from '../add-photo/add-photo.component';

@Component({
  selector: 'app-feed',
  standalone: true,
  imports: [ MatDialogModule ],
  templateUrl: './feed.component.html',
  styleUrl: './feed.component.css'
})
export class FeedComponent implements OnInit{
  private accountService = inject(AccountService);
  private sanitizer = inject(DomSanitizer);
  private addPost = inject(MatDialog);
  private addPhoto = inject(MatDialog);
  currentUser = this.accountService.currentUser();
  imageUrl: SafeUrl | null = null;

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

  ngOnInit(): void {
      var profilePhoto = this.accountService.currentUser()!.profilePhoto;

      if(profilePhoto !== null){
        this.accountService.getSignedUrl(profilePhoto).subscribe({
          next: (response) => {
            const objectUrl = response.signedUrl;
            this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
          },
          error: _ => this.imageUrl = null
        })
      }
  }

}
