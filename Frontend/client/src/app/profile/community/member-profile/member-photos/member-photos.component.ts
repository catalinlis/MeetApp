import { Component, inject, Input, OnInit } from '@angular/core';
import { LoaderComponent } from "../../../../loader/loader.component";
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { MembersService } from '../../../../_services/members.service';
import { Member } from '../../../../_models/Member';
import { DateProcessing } from '../../../../utils/DateProcessing';
import { PostingService } from '../../../../_services/posting.service';
import { Feed } from '../../../../_models/Feed';
import { SeePostComponent } from '../../../../see-post/see-post.component';
import { MatDialog } from '@angular/material/dialog';
import { AccountService } from '../../../../_services/account.service';


interface ExtendedPhoto{ 
  feed: Feed, 
  image: SafeUrl | null, 
  createdAt: string
}

@Component({
  selector: 'app-member-photos',
  standalone: true,
  imports: [LoaderComponent, CommonModule],
  templateUrl: './member-photos.component.html',
  styleUrl: './member-photos.component.css'
})
export class MemberPhotosComponent implements OnInit{
  private memberService = inject(MembersService);
  private postingService = inject(PostingService);
  private accountService = inject(AccountService);
  private sanitizer = inject(DomSanitizer);
  private seePostMat = inject(MatDialog);
  @Input() member!: Member;
  @Input() profilePhoto!: SafeUrl | null;
  @Input() currentProfilePhoto!: SafeUrl | null;
  currentUser = this.accountService.currentUser();
  loading: boolean = false;
  photos: ExtendedPhoto[]= [];
  imageUrl: SafeUrl | null = null;

  ngOnInit(): void {
    this.getMemberPhotos(this.member.username).then(photos => {
      const imgPromises = photos.map(photo => {
        return this.getPhoto(photo).then((img) => ({photoId: photo.postId, image: img}))
          .catch(_ => ({photoId: photo.postId, image: null}));
      });

      Promise.all(imgPromises).then(results => {
        var images = results.filter(({image}) => image !== null);

        photos.forEach(photo => {
          const match = images.find(({photoId}) => photoId === photo.postId);
          const image = match ? match.image : null;

          if(image !== null){
            const photoDTO = {
              feed: photo,
              image: image,
              createdAt: DateProcessing.formatPostDate(photo.createdAt)
            }

            console.log(photoDTO);

            this.photos.push(photoDTO);
          }
        })

        this.loading = true;
      })
    })
  }

  getMemberPhotos(username: string): Promise<Feed[]>{
    return new Promise<Feed[]>((resolve, reject) => {
      this.memberService.getMemberPhotos(username).subscribe({
        next: (response) => {
          resolve(response.photos);
        },
        error: (err) => { 
          console.log(err.error);
          reject();
        }
      })
    })
  }

  getPhoto(photo: Feed): Promise<SafeUrl>{
    return new Promise<SafeUrl>((resolve, reject) => {
      this.postingService.getPostPhoto(photo)?.subscribe({
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

  seePost(extendedFeedItem: ExtendedPhoto): void{
      console.log(extendedFeedItem);

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

  
}
