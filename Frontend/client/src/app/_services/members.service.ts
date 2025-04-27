import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AccountService } from './account.service';
import { environment } from '../../environments/environment.development';
import { Member } from '../_models/Member';
import { AboutMember } from '../_models/AboutMember';
import { Interest } from '../_models/Interest';
import { query } from '@angular/animations';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;
  memberCache = new Map();
  user = this.accountService.currentUser();

  getMembers(){
    return this.http.get<Member[]>(this.baseUrl + "user/members");
  }

  getMember(username: string){
    return this.http.get<Member>(this.baseUrl + "user/member/" + username);
  }

  getAboutMember(username: string){
    return this.http.get<AboutMember>(this.baseUrl + "user/member/about/" + username);
  }

  getMemberInterests(username: string){
    return this.http.get<Interest[]>(this.baseUrl + "user/member/interests/" + username);
  }

  getOnlineMembers(usernames: string[]){
    let params = new HttpParams();

    usernames.forEach(username => {
      params = params.append("usernames", username);
    })
    return this.http.get<Member[]>(this.baseUrl + "user/online-users", { params });
  }
}
