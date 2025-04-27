import { inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { ReceiverMessage } from '../../_models/SenderMessage';
import { User } from '../../_models/User';
import { ChatMessage } from '../../_models/ChatMessage';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private hubConnection!: signalR.HubConnection;
  private privateMessagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  privateMessage$ = this.privateMessagesSubject.asObservable();

  startConnection(user: User){
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5100/chatHub", {
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
    console.log(this.privateMessagesSubject.value);
  }
}
