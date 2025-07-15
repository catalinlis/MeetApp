import { Component, Inject, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faClose } from '@fortawesome/free-solid-svg-icons';
import { Feed } from '../_models/Feed';
import { User } from '../_models/User';
import { CommonModule } from '@angular/common';
import { PostingService } from '../_services/posting.service';
import { LoaderComponent } from "../loader/loader.component";
import { IncomingComment } from '../_models/IncomingComment';
import { AccountService } from '../_services/account.service';
import { MembersService } from '../_services/members.service';
import { CommentService } from '../_services/comment.service';
import { DateProcessing } from '../utils/DateProcessing';
import { Member } from '../_models/Member';
import { Comment } from '../_models/Comment';
import { CdkTextareaAutosize, TextFieldModule } from '@angular/cdk/text-field';
import { ViewChild } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-comment-post',
  standalone: true,
  imports: [FontAwesomeModule, CommonModule, LoaderComponent, MatFormFieldModule, MatSelectModule, MatInputModule, TextFieldModule, FormsModule, RouterModule],
  templateUrl: './comment-post.component.html',
  styleUrl: './comment-post.component.css'
})
export class CommentPostComponent {
  faClose = faClose;
  private commentDialog = inject(MatDialogRef);
  private postService = inject(PostingService);
  private accountService = inject(AccountService);
  private memberService = inject(MembersService);
  private commentService = inject(CommentService);
  comments: Comment[] = [];
  loadedComments: boolean = false;
  rawComments: IncomingComment[] = [];
  commentContent: string = "";
  profilePhotos: Map<string, SafeUrl> = new Map<string, SafeUrl>();

  @ViewChild('autosize') autosize!: CdkTextareaAutosize;
  @ViewChild('commentTextarea') commentTextarea!: ElementRef<HTMLTextAreaElement>;

  constructor(@Inject(MAT_DIALOG_DATA) 
    public data: { 
      photo?: SafeUrl | null,  
      profilePhoto: SafeUrl | null, 
      postItem: Feed, 
      currentUserProfilePhoto: SafeUrl | null,
      user: User, 
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

  onClose(){
    const feedback = {
      isLiked: this.data.isLiked,
      postItem: this.data.postItem
    };

    this.commentDialog.close(feedback);
  }

  focusTextarea(){
    this.commentTextarea.nativeElement.focus();
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
          console.log(response);
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
