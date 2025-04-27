import { inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { User } from '../../_models/User';
import { FriendService } from '../friend.service';

@Injectable({
  providedIn: 'root'
})
export class OnlineUsersService {
  private hubConnection!: signalR.HubConnection;
  onlineUsers$ = new BehaviorSubject<string[]>([]);

  startConnection(user: User): Promise<void>{
    console.log("Start connection");
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5100/userStatusHub", {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();
    
    this.hubConnection.on("UserOnline", (userId: string) => {
      this.onlineUsers$.next([...this.onlineUsers$.value, userId]);
      this.persistOnlineUsers(this.onlineUsers$.value);
    });

    this.hubConnection.on("UserOffline", (userId: string) => {
      this.onlineUsers$.next(this.onlineUsers$.value.filter(id => id !== userId));
      this.persistOnlineUsers(this.onlineUsers$.value);
    });

    this.hubConnection.on("ReceiveOnlineUsers", (users: string[]) => {
      this.onlineUsers$.next(users);
      this.persistOnlineUsers(this.onlineUsers$.value);
    });
  
    return this.hubConnection.start()
      .then(() => {
          if(this.hubConnection.state === signalR.HubConnectionState.Connected){
            this.requestOnlineUsers(); // ✅ Ensure we request online users after connection is stable
            this.startHeartbeat();
          }
      })
      .catch(err => console.log("Error while starting connection" + err));
  }

  stopConnection(){
    if(this.hubConnection){
      this.hubConnection.stop().then(() => {
        this.onlineUsers$ = new BehaviorSubject<string[]>([]);
        this.deleteOnlineUsers();
      });
    }
  }

  requestOnlineUsers(){
    try{
      this.hubConnection.invoke("GetOnlineUsers");
    } catch(err){
      console.log("Error fetching users:", err);
    }
  }  

  persistOnlineUsers(users: string[]){
    localStorage.setItem("online-users", JSON.stringify(users));
  }

  deleteOnlineUsers(){
    localStorage.removeItem("online-users");
  }

  retreiveOnlineUsers(): string[]{
    const users = JSON.parse(localStorage.getItem("online-users") || "[]");
    return users;
  }

  private startHeartbeat(){
    setInterval(() => {
      this.hubConnection.invoke("Heartbeat")
        .then(() => console.log("Heartbeat sent!"))
        .catch(err => console.error("Heartbeat error:", err));
    }, 45000);
  }
}
