import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AccountService } from './_services/account.service';
import { OnlineUsersService } from './_services/hubs/online-users.service';
import { ChatService } from './_services/hubs/chat.service';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet
],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit{
  private accountService = inject(AccountService);
  private chatService = inject(ChatService);
  private onlineUsersService = inject(OnlineUsersService);
  
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
  }

}
