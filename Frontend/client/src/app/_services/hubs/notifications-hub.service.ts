import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { User } from '../../_models/User';
import * as signalR from '@microsoft/signalr';
import { RealtimeNotification } from '../../_models/RealtimeNotification';
import { NotificationService } from '../notification.service';
import { environment } from '../../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class NotificationsHubService {
  private hubConnection!: signalR.HubConnection;
  receivedNotification$ = new Subject<RealtimeNotification>();
  notificationHubUrl = environment.notificationUrl;

  startConnection(user: User): Promise<void>{
    if(this.hubConnection && this.hubConnection.state !== signalR.HubConnectionState.Disconnected){
      return Promise.resolve();
    };

    this.hubConnection = new signalR.HubConnectionBuilder()
                          .withUrl(this.notificationHubUrl, {
                            accessTokenFactory: () => user.token
                          })
                          .withAutomaticReconnect()
                          .build();

    this.hubConnection.on("notification",(data: RealtimeNotification) => {
      this.receivedNotification$.next(data);
    });

    return this.hubConnection.start().then(() => {
      if(this.hubConnection.state === signalR.HubConnectionState.Connected){
        
      }
    });
  }

  stopConnection(){
   if(this.hubConnection){
     this.hubConnection.stop().then(() => {
       this.receivedNotification$ = new Subject<RealtimeNotification>();
     });
   }
  }

}
