import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AccountService } from './_services/account.service';
import { OnlineUsersService } from './_services/hubs/online-users.service';
import { ChatService } from './_services/hubs/chat.service';
import { NotificationsHubService } from './_services/hubs/notifications-hub.service';
import { NavbarComponent } from "./navbar/navbar.component";


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    NavbarComponent
],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit{
  private accountService = inject(AccountService);
  private chatService = inject(ChatService);
  private onlineUsersService = inject(OnlineUsersService);
  private notificationHubService = inject(NotificationsHubService);
  
  ngOnInit(): void {
    this.setCurrentUser();
  }

  setCurrentUser(){
    var userString = localStorage.getItem('user');

    if(!userString) return;
    const user = JSON.parse(userString);
    this.accountService.setCurrentUser(user);
    this.chatService.startConnection(user);
    this.onlineUsersService.startConnection(user);
    this.notificationHubService.startConnection(user);
  }

}
