import { Component, inject, Inject, OnInit } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { Feed } from '../_models/Feed';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faClose } from '@fortawesome/free-solid-svg-icons';
import { User } from '../_models/User';
import { CommonModule } from '@angular/common';
import { PostingService } from '../_services/posting.service';
import { CdkTextareaAutosize, TextFieldModule } from '@angular/cdk/text-field';
import { ViewChild } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Comment } from '../_models/Comment';
import { CommentService } from '../_services/comment.service';
import { IncomingComment } from '../_models/IncomingComment';
import { MembersService } from '../_services/members.service';
import { AccountService } from '../_services/account.service';
import { Member } from '../_models/Member';
import { LoaderComponent } from "../loader/loader.component";
import { DateProcessing } from '../utils/DateProcessing';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-see-post',
  standalone: true,
  imports: [FontAwesomeModule, CommonModule, MatFormFieldModule, MatSelectModule, MatInputModule, TextFieldModule, FormsModule, LoaderComponent, RouterModule],
  templateUrl: './see-post.component.html',
  styleUrl: './see-post.component.css'
})
export class SeePostComponent implements OnInit{
  faClose = faClose;
  private postDialog = inject(MatDialogRef);
  private postService = inject(PostingService);
  private commentService = inject(CommentService);
  private memberService = inject(MembersService);
  private accountService = inject(AccountService);
  comments: Comment[] = [];
  loadedComments: boolean = true;
  rawComments: IncomingComment[] = [];
  commentContent: string = "";
  profilePhotos: Map<string, SafeUrl> = new Map<string, SafeUrl>();

  @ViewChild('autosize') autosize!: CdkTextareaAutosize;
  @ViewChild('commentTextarea') commentTextarea!: ElementRef<HTMLTextAreaElement>;

  constructor(@Inject(MAT_DIALOG_DATA) 
    public data: { 
      photo: SafeUrl, 
      profilePhoto: SafeUrl, 
      postItem: Feed, 
      currentUserProfilePhoto: SafeUrl | null,
      user: Member, 
      currentUser: User,
      createdAt: string,
      isLiked: boolean,
    }, private sanitizer: DomSanitizer) { }
  
  ngOnInit(): void {

    this.getComments().then((comments) => {
      
      this.data.postItem.commentsCount = comments.length;
      
      const uniqueUsers = [...new Set(comments.map(comment => comment.username))];

      const imgPromises = uniqueUsers.map((username) => {
        return this.getMember(username).then((member) => {
          return this.getProfilePhoto(member.profilePhoto).then((img) => ({ username: username, image: img , member: member}))
            .catch(_ => ({ username: username, image: null, member: member}))
          })
      });

      Promise.all(imgPromises).then(results => {
        var images = results.filter(({image}) => image !== null); 
     
        comments.forEach((comment) => {
          
          const match = images.find(({username}) => username === comment.username);
          const image = match ? match.image : null;
          const member = match ? match.member : null;

          if(member !== null){
            const pushComment = {
              profilePhoto: image,
              username: comment.username,
              firstname: member.firstname,
              lastname: member.lastname,
              comment: comment.content,
              addedAt: DateProcessing.formatPostDate(comment.addedAt)
            };

            this.comments.push(pushComment);
          }
        });

        this.loadedComments = true;
      });
    });
  }

  closeDialog(): void {
    const feedback = {
      isLiked: this.data.isLiked,
      postItem: this.data.postItem
    };

    this.postDialog.close(feedback);
  }

  like(){
    this.postService.like(this.data.postItem)?.subscribe({
      next: (response) => {
        this.data.isLiked = response.hasLiked;
        if(this.data.isLiked)
          this.data.postItem.likesCount++;
        else
          this.data.postItem.likesCount--;
      },
      error: (err) => {
        console.log(err.error);
      }
    })
  }

  focusTextarea(){
    this.commentTextarea.nativeElement.focus();
  }

  addComment(){
    this.loadedComments = false;

    const type = this.data.postItem.type;
    const id = this.data.postItem.postId;
    const formData = new FormData();
    formData.append("username", this.data.currentUser.userName);
    formData.append("text", this.commentContent);
    
    this.commentService.addComment(type, id, formData)?.subscribe({
      next: (response) => {
        const comment = {
          profilePhoto: this.data.currentUserProfilePhoto,
          username: response.comment.username,
          firstname: this.data.currentUser.firstname,
          lastname: this.data.currentUser.lastname,
          comment: response.comment.content,
          addedAt: DateProcessing.formatPostDate(response.comment.addedAt)
        };

        this.comments.unshift(comment);
        this.data.postItem.commentsCount++;
        this.commentContent = "";
        this.loadedComments = true;
      },
      error: (err) => {
        console.log(err.error);
      }
    });
  }

  async getComments(): Promise<IncomingComment[]>{

    return new Promise((resolve, reject) => {
      let id = this.data.postItem.postId;
      let type = this.data.postItem.type;
      this.commentService.getComments(type, id)?.subscribe({
        next: (response) => {
          this.loadedComments = false;
          resolve(response.comments);
        },
        error: (err) => {
          console.log(err.error);
          reject();
        }
      });
    });
  }

  async getMember(username: string): Promise<Member>{
    return new Promise((resolve, reject) => {
      this.memberService.getMember(username).subscribe({
        next: (response) => {
          resolve(response);
        },
        error: (err) => {
          console.log(err.error);
          reject();
        }
      });
    });
  }

  async getProfilePhoto(photoId: string): Promise<SafeUrl>{
    return new Promise((resolve, reject) => {
      this.accountService.getSignedUrl(photoId).subscribe({
        next: (response) => {
          const objectUrl = response.signedUrl;
          const img = new Image();
          img.src = objectUrl;
          console.log(objectUrl);
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

}
