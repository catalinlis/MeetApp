import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { NavbarComponent } from "../../navbar/navbar.component";
import { Member } from '../../_models/Member';
import { AccountService } from '../../_services/account.service';
import { FriendService } from '../../_services/friend.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { FormsModule, NgForm } from '@angular/forms';
import { NgClass } from '@angular/common';
import { MessagesService } from '../../_services/messages.service';
import { ChatMessage } from '../../_models/ChatMessage';
import { NgIf } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ChatService } from '../../_services/hubs/chat.service';
import { OnlineUsersService } from '../../_services/hubs/online-users.service';
import { DateProcessing } from '../../utils/DateProcessing';
import { lastValueFrom } from 'rxjs';
import { faE, faEnvelope } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { MembersService } from '../../_services/members.service';

@Component({
  selector: 'app-messages-page',
  standalone: true,
  imports: [NavbarComponent, FormsModule, NgClass, NgIf, FontAwesomeModule],
  templateUrl: './messages-page.component.html',
  styleUrl: './messages-page.component.css'
})
export class MessagesPageComponent implements OnInit{
  @ViewChild('messagesBlock') private messageBlock!: ElementRef;
  faEnvelope = faEnvelope;
  private friendService = inject(FriendService);
  private accountService = inject(AccountService);
  private messageService = inject(MessagesService);
  private memberService = inject(MembersService);
  private chatService = inject(ChatService);
  private onlineUsersService = inject(OnlineUsersService);
  private route = inject(ActivatedRoute);
  private location = inject(Location);
  private sanitizer = inject(DomSanitizer);


  Friends: Member [] = [];
  onlineStatusMark: Map<string, boolean> = new Map();
  chatUsers: Member[] = [];
  chatMessagesMap: Map<string, number> = new Map();
  urlMap: Map<string, SafeUrl | null> = new Map();
  selectedUser: Member | null = null;
  toggleUserSelected: boolean = false;
  isUserSelected: boolean = false;
  spinFlag: boolean = false;
  fetchedChatUsers: boolean = false;
  message: string = "";
  messages: ChatMessage[] = [];
  fetchMessages: number = 20;
  sortKey: string = "";
  currentUsername = this.accountService.currentUser()?.userName;

  ngOnInit(): void {
    this.getFriends().then(() => {
      this.retrieveOnlineFriends();
    });
    this.getChats().then(() => {
      this.chatUsers.forEach(user => {
        this.setMapOfUnreadMessages(user.username);
      })
      if(this.hasParam()){
        this.checkUserChat(this.selectedUser!);
        this.getMessages();
      }
    });
    this.subscribeToIncomingMessages();
    this.subscribeToOnlineUsers();
  }

  async getFriends(): Promise<void>{

    return new Promise(async (resolve, reject) => {
      try{
        if(!this.currentUsername)  resolve();
      
        let response = await lastValueFrom(this.friendService.getUserFriends(this.currentUsername!));
        this.Friends = response.friends;
        
        await Promise.all(this.Friends.map(async (friend) => {
          try {
            const signedUrlResponse = await lastValueFrom(this.accountService.getSignedUrl(friend.profilePhoto));
            const objectUrl = signedUrlResponse.signedUrl;
            const image = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
            this.urlMap.set(friend.username, image);
          }catch (err) {
            this.urlMap.set(friend.username, null);
          }
        }));
        resolve();
      } catch(err){ reject(); }
      });
  }

  async getChats(): Promise<void>{
    return new Promise(async (resolve, reject) => {
      try{
        if(!this.currentUsername) resolve();

        let response =  await lastValueFrom(this.messageService.getChats(this.currentUsername!));
        response.forEach((chatUser) => {
            this.rearangeUserChat(chatUser);
        });
        this.fetchedChatUsers = true;
        resolve();
      } catch(err){ reject(); }
    });
  }

  retrieveOnlineFriends(){
    const onlineUsers = this.onlineUsersService.retreiveOnlineUsers();
    this.markUsersStatus(onlineUsers);
  }

  rearangeUserChat(user: Member){
    if(user !== null){
      let sameUser = this.chatUsers.some((user1) => user1.username === user.username);
      
      if(sameUser){
        this.chatUsers = this.chatUsers.filter(item => item.username !== user.username);
        this.chatUsers.unshift(user);
      }
      else
        this.chatUsers.unshift(user);
    }
  }

  checkUserChat(user: Member){
    if(user !== null){
      let sameUser = this.chatUsers.some((user1) => user1.username === user.username);
      if(!sameUser)
        this.chatUsers.unshift(user);
    }
  }

  readAllMessages(sender: string){
    if(this.currentUsername){
      this.messageService.setUnreadMessages(this.currentUsername, sender).subscribe({
        next: _ => {}
      });
      this.chatMessagesMap.delete(sender);
    }
  }

  sendMessage(){
    if(this.currentUsername && this.selectedUser){
      if(this.message !== ""){
        const chatUsername = this.selectedUser.username;
        this.messageService.sendMessage(this.currentUsername, chatUsername, this.message).subscribe({
          next: _ => {
            const pushMessage: ChatMessage = { sender: this.currentUsername!, 
                                               receiver: this.selectedUser!.username, 
                                               message: this.message,
                                               sentDate: new Date() };
            this.messages.push(pushMessage);
            this.chatService.sendMessage(pushMessage);
            this.rearangeUserChat(this.selectedUser!);
            setTimeout(() => this.scrollToBottom(), 0);
            this.message = "";
          }
        });
        this.readAllMessages(this.selectedUser.username);
      }
    }
  }

  getMessages(){
    
    this.spinFlag = true;
    if(this.currentUsername && this.selectedUser){
      const chatUsername = this.selectedUser.username;
      this.messageService.getMessages(this.currentUsername, chatUsername, this.fetchMessages, this.sortKey).subscribe({
        next: (response) => {
          if(!response.end){
            this.messages = response.messages.reverse().concat(this.messages);
            this.sortKey = response.lastSortKey;
          }
            this.spinFlag = false;
          this.readAllMessages(this.selectedUser!.username);
          if(this.toggleUserSelected){
            setTimeout(() => this.scrollToBottom(), 0);
            this.toggleUserSelected = false;
          }
        },
        error: (err) => {
          console.log(err.error);
          this.spinFlag = false;
        }
      });
    }
  }
  setMapOfUnreadMessages(sender: string){
    this.messageService.getNumberOfUnreadMessages(this.currentUsername!, sender).subscribe({
      next: (response) => {
        let lastNumberOfMessages = this.chatMessagesMap.get(sender);
        if(lastNumberOfMessages)
          this.chatMessagesMap.set(sender, lastNumberOfMessages + response.count);
        else  
          this.chatMessagesMap.set(sender, response.count);
      },
      error: (err) => {
        console.error(err);
      }
    })
  }

  hasParam(): boolean{
    this.route.data.subscribe(data => {
      this.selectedUser = data["username"];
    });

    if(this.selectedUser !== null && this.selectedUser !== undefined){
      this.isUserSelected = true;
      return true;
    }
    else return false;
  }

  initializeChat(){
    this.messages = [];
    this.sortKey = "";
    this.toggleUserSelected = true;
  }

  redirectedToChat(user: Member){
    this.selectedUser = user;
    if(this.selectedUser === null || this.selectedUser){
      this.location.replaceState(`/messages/${user.username}`);
      this.initializeChat();
      this.checkUserChat(user);
      this.getMessages();
      this.isUserSelected = true;
    }
  }

  updateMessageNotification(username: string){
    let lastValueChatUserMap = this.chatMessagesMap.get(username);
   
    if(!lastValueChatUserMap)
      this.chatMessagesMap.set(username, 1);
    else
      this.chatMessagesMap.set(username, lastValueChatUserMap+1);

    let foundUser: Member | null = null;
    this.chatUsers.some((user1) =>{ 
      if(user1.username === username){
         foundUser = user1; 
      }
    });
    if(foundUser !== null)
      this.rearangeUserChat(foundUser);
    else{
      this.memberService.getMember(username).subscribe((member) => {
        this.checkUserChat(member);
        console.log(member);
        this.accountService.getSignedUrl(member.profilePhoto).subscribe({
          next: (response) => {
            const objectUrl = response.signedUrl;
            const image = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
            this.urlMap.set(member.username, image);
            this.checkUserChat(member);
          },
          error: (err) => {
            this.urlMap.set(member.username, null);
            this.checkUserChat(member);
          }
        });
      });
    }
    
  }

  subscribeToIncomingMessages(){
    this.chatService.privateMessage$.subscribe((incomingMessages) => {
        incomingMessages.forEach((message) => {
          if(this.selectedUser){
            if(message.sender === this.selectedUser?.username){
              this.messages.push(message);
              setTimeout(() => this.scrollToBottom(), 0);
            }
          }
          this.chatService.messageConsumed(incomingMessages.indexOf(message));
          this.updateMessageNotification(message.sender);
        });
    })
  }

  markUsersStatus(users: string[]){
    this.Friends.forEach((friend) => {
      if(users.includes(friend.username))
        this.onlineStatusMark.set(friend.username, true);
      else  
        this.onlineStatusMark.set(friend.username, false);
    });

    this.onlineStatusMark.forEach((state, user) => {
      if(state === true){
        const saveUser = this.Friends.filter(i => i.username == user);
        this.Friends = this.Friends.filter(u => u.username !== user);
        this.Friends.unshift(saveUser[0]);
      }
    });
  }

  subscribeToOnlineUsers(){
    this.onlineUsersService.onlineUsers$.subscribe(users => {
        this.markUsersStatus(users);
    });
  }

  checkScrollTop(): void {
    const scrollContainer = this.messageBlock.nativeElement;

    if(scrollContainer.scrollTop === 0 && this.messages.length >= this.fetchMessages)
      this.getMessages();
  }

  scrollToBottom(): void{
    try{
      this.messageBlock.nativeElement.scrollTop = this.messageBlock.nativeElement.scrollHeight;
    } catch(err) {
      console.log("Scrolling failed", err);
    }
  }

  submitOnEnter(event: Event, form: NgForm ){
    event.preventDefault();
    if(form.valid)
      form.ngSubmit.emit();
  }

  fromDateToDDMMYYYY(date: Date): string{
    return DateProcessing.getDateDDMMYYYY(date);
  }

  fromDateToHHMM(date: Date): string{
    return DateProcessing.getTimeHHMM(date);
  }

  isDifferenceTimeBigger(date1: Date, date2: Date, difference: number){
    return DateProcessing.differenceBetweenDates(date1, date2, difference);
  }

  isToday(date: Date): boolean{
    return DateProcessing.isToday(date);
  }
}
