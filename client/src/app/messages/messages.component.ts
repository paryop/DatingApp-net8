import { Component, inject, OnInit } from '@angular/core';
import { MessageService } from '../_services/message.service';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { FormsModule } from '@angular/forms';
import { TimeAgoPipe } from '../_pipes/timeago.pipe';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { Message } from '../_models/message';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [ButtonsModule, FormsModule, TimeAgoPipe, PaginationModule, RouterLink],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.css'
})
export class MessagesComponent implements OnInit {
  messageService = inject(MessageService);
  container = "Unread";
  pageNumber = 1;
  pageSize = 5;

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages(){
    console.log(this.container);
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container)
  }

  getRoute(message: Message) {
    if (this.container === 'Outbox') return `/members/${message.recipientUsername}`;
    else return `/members/${message.senderUsername}`;
  }

  deleteMessage(message: Message){
    this.messageService.deleteMessage(message).subscribe({
      next: _ => {
        this.messageService.paginatedResult.update(prev => {
          if(prev && prev.items) {
            prev.items.splice(prev.items.findIndex(m => m.id == message.id), 1);
            return prev;
          }
          return prev;
        })
      }
    })
  }

  pageChanged(event: any){
    if(this.pageNumber != event.page){
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }
}
