import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import { AccountService } from './account.service';
import { RealtimeNotification } from '../_models/RealtimeNotification';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private accountService = inject(AccountService);
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  getRealtimeNotifications(){
    return this.http.get<RealtimeNotification[]>(this.baseUrl + "notification/" + this.accountService.currentUser()?.userName);
  }

  markAsSeenAllNotifications(){
    return this.http.put(this.baseUrl + "notification/seen/" + this.accountService.currentUser()?.userName, null);
  }

  getNotificationsCount(){
    return this.http.get<number>(this.baseUrl + "notification/unseen/" + this.accountService.currentUser()?.userName);
  }
}
