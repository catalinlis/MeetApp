import { inject, Injectable } from '@angular/core';
import { AccountService } from './account.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { IncomingComment } from '../_models/IncomingComment';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;

  getComments(type: string, id: number){
    switch(type){
      case 'Photo': {
        return this.http.get<{comments: IncomingComment[]}>(this.baseUrl + 'comment/photo/' + id);
      }
      case 'Post': {
        return this.http.get<{comments: IncomingComment[]}>(this.baseUrl + 'comment/post/' + id);
      }
      default:
        return null;
    }
  }
  
  addComment(type: string, id: number, form: FormData){
    const username = this.accountService.currentUser()?.userName;
    switch(type){
      case 'Photo': {
        return this.http.post<{comment: IncomingComment}>(this.baseUrl + 'comment/photo/' + id, form);
      }
      case 'Post': {
        return this.http.post<{comment: IncomingComment}>(this.baseUrl + 'comment/post/' + id, form);
      }
      default: 
        return null;
    }
  }
}
