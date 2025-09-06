import { inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { User } from '../../_models/User';
import { ChatMessage } from '../../_models/ChatMessage';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private hubConnection!: signalR.HubConnection;
  private privateMessagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  privateMessage$ = this.privateMessagesSubject.asObservable();
  baseUrl = environment.chatHubUrl;

  startConnection(user: User){
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.baseUrl, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on("ReceivePrivateMessage", (receiver: string, sender: string, message: string, sentDate: Date) => {
      let incomingMessage: ChatMessage = { sender: sender, receiver: receiver, message: message, sentDate: sentDate};
      this.privateMessagesSubject.next([...this.privateMessagesSubject.value, incomingMessage]);
    });

    return this.hubConnection.start()
            .catch(err => console.log("Error while starting connection"));
  }

  stopConnection(){
    if(this.hubConnection){
      this.hubConnection.stop().then(() => {
        this.privateMessagesSubject = new BehaviorSubject<ChatMessage[]>([]);
      });
    }
  }

  sendMessage(message: ChatMessage){
    try{
      this.hubConnection.invoke("SendPrivateMessageToUser", message.sender, message.receiver, message.message);
    } catch(err){
      console.log("Error sending message to chat hub:", err);
    }
  }

  messageConsumed(index: number){
    const currentItems = this.privateMessagesSubject.value;
    const updatedMessages = [...currentItems.slice(0, index), ...currentItems.slice(index+1)];
    this.privateMessagesSubject.next(updatedMessages);
  }
}
