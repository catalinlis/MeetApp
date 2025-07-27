import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AccountService } from './account.service';
import { BehaviorSubject } from 'rxjs';
import { Feed } from '../_models/Feed';

@Injectable({
  providedIn: 'root'
})
export class PostingService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  private baseUrl = environment.apiUrl;
  private feedSubject = new BehaviorSubject<Feed[]>([]);
  feed$ = this.feedSubject.asObservable();
  
  uploadPost(model: any){
    var username =  this.accountService.currentUser()?.userName;
    this.http.post<{feedItem: Feed}>(this.baseUrl + "post/" + username, model).subscribe({
      next: (response) => {
        const currentFeed = this.feedSubject.value;
        const updatedFeed = [response.feedItem, ...currentFeed];
        this.feedSubject.next(updatedFeed);
      },
      error: (err) => {
        console.log(err.error);
      }
    })
  }

  uploadPhoto(model: any){
    var username = this.accountService.currentUser()?.userName;
    this.http.post<{feedItem: Feed}>(this.baseUrl + "post/photo/" + username, model).subscribe({
      next: (response) => {
        const currentFeed = this.feedSubject.value;
        const updatedFeed = [response.feedItem, ...currentFeed];
        this.feedSubject.next(updatedFeed);
      },
      error: (err) => {
        console.log(err.error);
      }
    });
  }

  getFeed(){
    var username = this.accountService.currentUser()?.userName;
    this.http.get<Feed[]>(this.baseUrl + "post/" + username).subscribe(feedItems => {
      console.log(feedItems);
      this.feedSubject.next(feedItems);
    });
  }

  getPostPhoto(feed: Feed){
    switch(feed.type){
      case 'Post':{
        return this.http.get<{signedUrl: string}>(this.baseUrl + "post/post/" + feed.imageUrl);
      }
      case 'Photo':{
        return this.http.get<{signedUrl: string}>(this.baseUrl + "post/photo/" + feed.imageUrl);
      }
      default:
        return null;
    }
  }

  like(feedItem: Feed){
    switch(feedItem.type){
      case 'Post': {
        return this.http.put<{hasLiked : boolean}>(this.baseUrl + "post/like/post/" + feedItem.postId, null);
      }
      case 'Photo': {
        return this.http.put<{hasLiked : boolean}>(this.baseUrl + "post/like/photo/" + feedItem.postId, null);
      }
      default:
        return null;
    }
  }
  
}
