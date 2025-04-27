import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Interest } from '../_models/Interest';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class InterestService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;

  setInterests(interests: Interest[]){
    return this.http.post<{registerStep: number}>(this.baseUrl + "data/interests/add/" + this.accountService.currentUser()?.userName, interests);
  }

  getInterests(){
    return this.http.get<Interest[]>(this.baseUrl + "data/interests");
  }

  getInterestData(interest: string){
    return this.http.get<Interest>(this.baseUrl + `interest/${interest}`);
  }

  addUserInterest(username: string, interest: string){
    return this.http.post(this.baseUrl + `interest/${interest}/${username}`, null);
  }

  isUserSubscribed(username: string, interest: string){
    return this.http.get<{subscribed: boolean}>(this.baseUrl + `interest/subscribed/${interest}/${username}`);
  }

}
