import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { User } from '../../_models/User';
import * as signalR from '@microsoft/signalr';
import { RealtimeNotification } from '../../_models/RealtimeNotification';
import { NotificationService } from '../notification.service';

@Injectable({
  providedIn: 'root'
})
export class NotificationsHubService {
  private hubConnection!: signalR.HubConnection;
  receivedNotification$ = new Subject<RealtimeNotification>();

  startConnection(user: User): Promise<void>{
    if(this.hubConnection && this.hubConnection.state !== signalR.HubConnectionState.Disconnected){
      return Promise.resolve();
    };

    this.hubConnection = new signalR.HubConnectionBuilder()
                          .withUrl("http://localhost:5104/notificationsHub", {
                            accessTokenFactory: () => user.token
                          })
                          .withAutomaticReconnect()
                          .build();

    this.hubConnection.on("notification",(data: RealtimeNotification) => {
      console.log(data);
      this.receivedNotification$.next(data);
    });

    return this.hubConnection.start().then(() => {
      if(this.hubConnection.state === signalR.HubConnectionState.Connected){
        
      }
    });
  }

  stopConnection(){
   if(this.hubConnection)
     this.hubConnection.stop();
  }

}
