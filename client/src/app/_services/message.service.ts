import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Message } from '../_models/message';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubsUrl;
  httpClient = inject(HttpClient);
  hubConnection?: HubConnection;
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);
  messageThread = signal<Message[]>([]);

  constructor() { }

  createHubConnection(user: User, otherUsername: string){
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + "message?user=" + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on("ReceiveMessageThread", messages => {
      console.log(messages);
      this.messageThread.set(messages);
    });

    this.hubConnection.on("NewMessage", message => {
      this.messageThread.update(messages => [...messages, message])
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if(group.connections.some(x => x.username == otherUsername)){
        this.messageThread.update(messages => {
          messages.forEach(message => {
            if(!message.dateRead){
              message.dateRead = new Date(Date.now())
            }
          })
          return messages;
        })
      }
    })
  }

  stopHubConnection(){
    if(this.hubConnection?.state == HubConnectionState.Connected){
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }

  getMessages(pageNo: number, pageSize: number, container: string){
    let params = setPaginationHeaders(pageNo, pageSize);

    params = params.append('Container', container);

    return this.httpClient.get<Message[]>(this.baseUrl + 'messages', {observe: 'response', params})
    .subscribe({
      next: response => setPaginatedResponse(response, this.paginatedResult)
    });
  }

  getMessageThread(userName: string){
    return this.httpClient.get<Message[]>(this.baseUrl + 'messages/thread/' + userName);
  }

  addMessage(message: Message){
    this.httpClient.post(this.baseUrl + 'messages',message);
  }

  async sendMessage(userName: string, content: string){
    // return this.httpClient.post<Message>(this.baseUrl + "messages", {recipientUsername: userName, content})
    return this.hubConnection?.invoke("SendMessage", {recipientUsername: userName, content});
  }

  deleteMessage(message: Message){
    return this.httpClient.delete(this.baseUrl + 'messages/' + message.id);
  }
}
