import { inject, Injectable } from '@angular/core';
import { AccountService } from './account.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import { Member } from '../_models/Member';

@Injectable({
  providedIn: 'root'
})
export class FriendService {
  private http = inject(HttpClient);
  friends: string[] = [];
  baseUrl = environment.apiUrl;
  
  sendFriendRequest(currentUser: string, targetUser: string){
    return this.http.post(this.baseUrl + `friend/request/${currentUser}/${targetUser}`, null);
  }

  areFriends(currentUser: string, targetUser: string){
    return this.http.get<{ areFriends: boolean }>(this.baseUrl + `friend/${currentUser}/${targetUser}`);
  }

  isFriendRequestSent(currentUser: string, targetUser: string){
    return this.http.get<{ friendRequest: boolean }>(this.baseUrl + `friend/requests/${currentUser}/${targetUser}`);
  }

  isFriendRequestReceived(currentUser: string, targetUser: string){
    return this.http.get<{ friendRequest: boolean }>(this.baseUrl + `friend/requests/${currentUser}/${targetUser}`);
  }

  answerFriendRequest(currentUser: string, targetUser: string){
    return this.http.post(this.baseUrl + `friend/requests/approve/${currentUser}/${targetUser}`, null);
  }

  getUserFriends(username: string){
    return this.http.get<{count: number, friends: Member[]}>(this.baseUrl + `friend/${username}`);
  }

  getFriendsUsernames(username: string){
    this.getUserFriends(username).subscribe((response) => {
      response.friends.forEach(friend => {
        this.friends.push(friend.username);
      });
    });
    console.log("FriendService "+ this.friends);
  }
}
