import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { User } from '../_models/User';
import { map } from 'rxjs';
import { Interest } from '../_models/Interest';
import { OnlineUsersService } from './hubs/online-users.service';
import { ChatService } from './hubs/chat.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private onlineUsersService = inject(OnlineUsersService);
  private chatService = inject(ChatService);
  baseUrl = environment.apiUrl;
  currentUser = signal<User | null>(null);
  imageCache = new Map();


  register(model: any){
    return this.http.post<User>(this.baseUrl + "account/register", model).pipe(
      map(user => {
        if(user){
          console.log(user);
          this.setCurrentUser(user);
          this.chatService.startConnection(user);
          this.onlineUsersService.startConnection(user);
        }
      })
    );
  }

  login(model: any){
    return this.http.post<User>(this.baseUrl + "account/login", model).pipe(
      map( user => {
        if(user){
          this.setCurrentUser(user);
          this.chatService.startConnection(user);
          this.onlineUsersService.startConnection(user);
        }
      })
    );
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.onlineUsersService.stopConnection();
  }

  uploadImage(model: any){
    console.log(this.currentUser());
    return this.http.post<{registerStep: number, profilePhoto: string}>(this.baseUrl + "data/upload-image/" + this.currentUser()?.userName , model);
  }

  getSignedUrl(resourceId: string){
    return this.http.get<{signedUrl: string}>(this.baseUrl + "data/sign-url/" + resourceId);
  }

  getInterests(){
    return this.http.get<Interest[]>(this.baseUrl + "data/interests");
  }

  setInterests(interests: Interest[]){
    return this.http.post<{registerStep: number}>(this.baseUrl + "data/interests/add/" + this.currentUser()?.userName, interests);
  }

  setCurrentUser(user: User){
    localStorage.setItem('user', JSON.stringify(user));
    console.log(localStorage.getItem('user'));
    this.currentUser.set(user);
  }  

  updateRegisterStep(step: number){
    var user = this.currentUser();
    
    if(user){
        user.registerStep = step;
        this.setCurrentUser(user);
    }
  }

  updateProfilePhoto(profilePhoto: string){
    var user = this.currentUser();
    
    if(user){
        user.profilePhoto = profilePhoto;
        this.setCurrentUser(user);
    }
  }

  getRegisterStep(): number{
    return this.currentUser()!.registerStep;
  }
  
}
