import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import { HttpParams } from '@angular/common/http';
import { ChatMessage } from '../_models/ChatMessage';
import { Member } from '../_models/Member';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  sendMessage(sender: string, receiver: string, message: string){
    let params = new HttpParams().set("message", message);

    return this.http.post(`${this.baseUrl}message/${sender}/${receiver}`, null, { params });
  }

  getMessages(sender: string, receiver: string, fetch: number, lastKey: string = ""){
    let params;
    if(lastKey !== "")
      params = new HttpParams().set("fetch", fetch).set("sortKey", lastKey);
    else
      params = new HttpParams().set("fetch", fetch);

    return this.http.get<{messages: ChatMessage[], lastSortKey: string, end: boolean }>(`${this.baseUrl}message/${sender}/${receiver}`, { params });
  }

  getChats(username: string){
    return this.http.get<Member[]>(`${this.baseUrl}message/chats/${username}`);
  }

  getNumberOfUnreadMessages(receiver: string, sender: string){
    return this.http.get<{count: number}>(`${this.baseUrl}message/unread/${receiver}/${sender}`);
  }
  
  setUnreadMessages(receiver: string, sender: string){
    return this.http.put(`${this.baseUrl}message/set/read/${receiver}/${sender}`, {});
  }
}
