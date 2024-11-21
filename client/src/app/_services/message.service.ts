import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Message } from '../_models/message';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  httpClient = inject(HttpClient);
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);

  constructor() { }

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

  sendMessage(userName: string, content: string){
    return this.httpClient.post<Message>(this.baseUrl + "messages", {recipientUsername: userName, content})
  }

  deleteMessage(message: Message){
    return this.httpClient.delete(this.baseUrl + 'messages/' + message.id);
  }
}
